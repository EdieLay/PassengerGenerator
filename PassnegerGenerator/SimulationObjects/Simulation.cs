using PassengerGenerator;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    internal class Simulation
    {
        Rabbit _rabbit; // класс для работы с RabbitMQ
        DateTime _curTime; // текущее время симуляции
        int _interval = 2000; // за сколько миллисекунд реального времени проходит 1 минута симуляции
        Parser _parser; // класс для парсинга сообщений из RabbitMQ

        public Dictionary<string, Passenger> Passengers { get; set; } // словарь всех пассжиров, ключ - GUID
        public Dictionary<string, Flight> Flights { get; set; } // словарь всех рейсов, ключ - GUID

        public Simulation()
        {
            _curTime = DateTime.Now;
            Passengers = new Dictionary<string, Passenger>();
            Flights = new Dictionary<string, Flight>();
            _rabbit = Rabbit.GetInstance();
            _parser = new Parser();
        }


        public void Execute() // основной метод для запуска симуляции
        {
            while (true)
            {
                var delay = Task.Delay(_interval); // для обеспечения выполнения цикла не меньше, чем за _interval мс

                Log.Information("-----CURRENT TIME: {CurTime}-----", _curTime);

                UpdateSimulation(); // считывание сообщение и обновление информации
                GeneratePassengersOnFlights(); // генерация пассажиров на рейсы
                
                foreach (KeyValuePair<string, Passenger> passengerItem in Passengers)
                {
                    Passenger passenger = passengerItem.Value;
                    PassengerState curState = passenger.State;
                    ProccessState(passenger, curState); // конечный автомат для каждого пассажира
                }

                _curTime = _curTime.AddMinutes(1); // увеличиваем время на 1 минуту
                delay.Wait(); // тут ждём, когда пройдут оставшиеся от _interval мс
            }
        }
     
        void UpdateSimulation() // считывание сообщений с очередей и их обработка
        {
            var data = new Dictionary<string, string>();

            string mes = _rabbit.GetMessage(_rabbit.TicketsRQ);
            if (mes != String.Empty)
            {
                data = _parser.ParseMessage(mes);
                string guid = data[_parser.PassengerKey];
                string response = data[_parser.ResponseKey];
                if (response == _parser.SuccessValue) // если пришёл успех от кассы
                {
                    Passengers[guid].GotTicket(); // то получили билет
                }
                else // если нет
                {
                    Passengers[guid].GotRejected(); // то уходим из аэропорта
                }
            }

            mes = _rabbit.GetMessage(_rabbit.RegistrationRQ); 
            if (mes != String.Empty)
            {
                data = _parser.ParseMessage(mes);
                string guid = data[_parser.PassengerKey];
                string response = data[_parser.ResponseKey];
                if (response == _parser.SuccessValue) // если пришёл успех от регистрации
                {
                    Passengers[guid].Registered(); // то зарегистрировались
                }
                else if (response == _parser.RegistrationNotStartedValue) // если регистрация не открыта
                {
                    Passengers[guid].RollToGoAFK(_curTime); // идём афк
                }
                else // если просто не смогли зарегаться
                {
                    Passengers[guid].GotRejected(); // то уходим из аэропорта
                }
            }

            mes = _rabbit.GetMessage(_rabbit.FlightsRQ);
            if (mes != String.Empty)
            {
                // добавить полёт в симуляцию
                data = _parser.ParseMessage(mes);
            }
        }

        void GeneratePassengersOnFlights() // генерирует пассажиров и удаляет прошедшие рейсы
        {
            int i = 0;
            while (i < Flights.Count)
            {
                var flightItem = Flights.ElementAt(i);
                Flight flight = flightItem.Value;
                if (flight.Date <= _curTime) // если текущее время больше, чем время вылета
                {
                    Flights.Remove(flightItem.Key); // то удаляем рейс
                    continue;
                }
                int numOfPassengers = flight.GenerateNumOfPassengers(); // генерируем количество пассажиров на рейс
                for (int j = 0; j < numOfPassengers; j++)
                {
                    Passenger newPassenger = new Passenger(flight);
                    Passengers[newPassenger.GUID] = newPassenger; // добавляем нового пассажира
                }
                i++;
            }
        }

        void ProccessState(Passenger passenger, PassengerState curState) // конечный автомат
        {
            switch (curState)
            {
                case PassengerState.CameToAirport: // только сгенерированный пассажир
                    ProccessCameToAirport(passenger); // обработка состояния, запрос билета
                    break;
                case PassengerState.RequestedTicket: // билет запрошен
                    break; // ничего не происходит, пока билет или отказ не получен
                case PassengerState.GotTicket: // билет получен
                    passenger.RollToGoAFK(_curTime); // возможность встать афк
                    if (passenger.State != PassengerState.AFK) // если не встали афк
                    {
                        ProccessGotTicket(passenger); // обработка состояния, запрос регистрации
                    }
                    break;
                case PassengerState.RequstedRegistration: // регистрация запрошена
                    break; // ничего не делаем до ответа
                case PassengerState.Registered: // зарегистрирован
                    passenger.RollToGoAFK(_curTime); // возможность встать афк
                    if (passenger.State != PassengerState.AFK) // если не встал афк
                    {
                        ProccessRegistered(passenger); // обработка состояния, отправка пассажира в автобус
                    }
                    break;
                case PassengerState.GotIntoBus: // отправился в автобус
                    Passengers.Remove(passenger.GUID); // удаляем его
                    break;
                case PassengerState.AFK: // уменьшаем таймер афк
                    passenger.AFKTimer--;
                    break;
                case PassengerState.GotRejected: // был отвергнут на каком-то этапе
                    Passengers.Remove(passenger.GUID); // удаляем его
                    break;
            }
        }

        void ProccessCameToAirport(Passenger passenger) // обработка только сгенерированного пассажира
        {
            string message = _parser.ParsePassengerForTickets(passenger); // парсим пассажира в строку для кассы
            _rabbit.PutMessage(_rabbit.TicketsWQ, message); // кладём сообщение в очередь кассы
            passenger.State = PassengerState.RequestedTicket; // изменяем состояния
            passenger.NextState = PassengerState.GotTicket;
        }

        void ProccessGotTicket(Passenger passenger)
        {
            string message = _parser.ParsePassengerForRegAndBus(passenger); // парсим пассажира в строку для регистрации
            _rabbit.PutMessage(_rabbit.RegistrationWQ, message); // кладём сообщение в очередь регистрации
            passenger.State = PassengerState.Registered; // изменяем состояния
            passenger.NextState = PassengerState.GotIntoBus;
        }

        void ProccessRegistered(Passenger passenger)
        {
            string message = _parser.ParsePassengerForRegAndBus(passenger); // парсим пассажира в строку для автобуса
            _rabbit.PutMessage(_rabbit.BusWQ, message); // кладём сообщение в очередь автобуса
            passenger.State = PassengerState.GotIntoBus; // изменяем состояния
        }
    }
}
