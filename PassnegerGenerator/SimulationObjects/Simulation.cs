using PassengerGenerator;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    internal class Simulation
    {
        Rabbit _rabbit; // класс для работы с RabbitMQ
        DateTime _curTime; // текущее время симуляции
        int _interval = 1000; // за сколько миллисекунд реального времени проходит 1 Секунда симуляции
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

                _curTime = _curTime.AddSeconds(1); // увеличиваем время на 1 секунду
                delay.Wait(); // тут ждём, когда пройдут оставшиеся от _interval мс
            }
        }
     
        void UpdateSimulation() // считывание сообщений с очередей и их обработка
        {
            Log.Debug("UpdateSimulation()");
            string mes = _rabbit.GetMessage(_rabbit.TicketsRQ);
            if (mes != String.Empty)
            {
                try
                {
                    using (JsonDocument jsonDoc = _parser.ParseMessage(mes))
                    {
                        JsonElement data = jsonDoc.RootElement;
                        string guid = data.GetProperty(_parser.PassengerKey).ToString();
                        string response = data.GetProperty(_parser.TicketKey).ToString();
                        if (response == _parser.SuccessValue) // если пришёл успех от кассы
                        {
                            Passengers[guid].GotTicket(); // то получили билет
                        }
                        else // если нет
                        {
                            Passengers[guid].GotRejected(); // то уходим из аэропорта
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Couldn't parse JSON from {QueueName}", _rabbit.TicketsRQ);
                }
            }

            mes = _rabbit.GetMessage(_rabbit.RegistrationRQ); 
            if (mes != String.Empty)
            {
                try
                {
                    using (JsonDocument jsonDoc = _parser.ParseMessage(mes))
                    {
                        JsonElement data = jsonDoc.RootElement;
                        string guid = data.GetProperty(_parser.PassengerKey).ToString();
                        string response = data.GetProperty(_parser.ResponseKey).ToString();
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
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Couldn't parse JSON from {QueueName}", _rabbit.RegistrationRQ);
                }
                
            }

            if (_curTime.Second % 10 == 0) // запрашиваем инфу о самолётах раз в 15 секунд, чтобы снизить нагрузку программы
            {
                mes = _rabbit.GetMessage(_rabbit.FlightsRQ);
                if (mes != String.Empty)
                {
                    try
                    {
                        using (JsonDocument jsonDoc = _parser.ParseMessage(mes))
                        {
                            JsonElement data = jsonDoc.RootElement;
                            string guid = data.GetProperty(_parser.FlightKey).ToString();
                            string date = data.GetProperty(_parser.DateKey).ToString();
                            string totalSeats = data.GetProperty(_parser.TotalSeatsKey).ToString();
                            DateTime dateTime = DateTime.Parse(date);
                            int seats = int.Parse(totalSeats);
                            Flights[guid] = new Flight(guid, dateTime, seats, _curTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Couldn't parse JSON or DateTime from {QueueName}", _rabbit.FlightsRQ);
                    }
                }
            }
        }

        void GeneratePassengersOnFlights() // генерирует пассажиров и удаляет прошедшие рейсы
        {
            Log.Debug("GeneratePassengersOnFlights()");
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
                int numOfPassengers = flight.GenerateNumOfPassengers(_curTime); // генерируем количество пассажиров на рейс
                if (numOfPassengers > 0)
                {
                    for (int j = 0; j < numOfPassengers; j++)
                    {
                        Passenger newPassenger = new Passenger(flight);
                        Log.Information("Generated {Name} on flight {Flight}", newPassenger.Name, flight.GUID);
                        Passengers[newPassenger.GUID] = newPassenger; // добавляем нового пассажира
                    }
                    flight.GeneratedOnFlight += numOfPassengers;
                    Log.Information("Generated totally {GenereatedOnFlight} passengers on flight {Flight}", flight.GeneratedOnFlight, flight.GUID);
                }
                i++;
            }
        }

        void ProccessState(Passenger passenger, PassengerState curState) // конечный автомат
        {
            switch (curState)
            {
                case PassengerState.CameToAirport: // только сгенерированный пассажир
                    ProccessCameToAirport(passenger); // обработка состояния, запрос билета\
                    //passenger.State = PassengerState.GotTicket;
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
                    passenger.State = PassengerState.GotIntoBus;
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
            string message = _parser.ParsePassengerForRegistration(passenger); // парсим пассажира в строку для регистрации
            _rabbit.PutMessage(_rabbit.RegistrationWQ, message); // кладём сообщение в очередь регистрации
            passenger.State = PassengerState.RequstedRegistration; // изменяем состояния
            passenger.NextState = PassengerState.Registered;
        }
    }
}
