using PassengerGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    internal class Simulation
    {
        Rabbit _rabbit;
        DateTime _curTime = DateTime.Now;
        int _interval = 1000; // за сколько миллисекунд реального времени проходит 1 минута симуляции

        public List<Passenger> Passengers { get; set; }
        public List<Flight> Flights { get; set; }

        public Simulation()
        {
            Passengers = new List<Passenger>();
            Flights = new List<Flight>();
            _rabbit = Rabbit.GetInstance();
        }


        public void Execute()
        {
            while (true)
            {
                var delay = Task.Delay(_interval);

                UpdateSimulation();
                
                for (int i = 0; i < Passengers.Count; i++)
                {
                    Passenger passenger = Passengers[i];
                    PassengerState curState = passenger.State;
                    ProccessState(passenger, curState);
                }

                _curTime = _curTime.AddMinutes(1);
                delay.Wait();
            }
        }

        void UpdateSimulation() // считывание сообщений с очередей и их обработка
        {
            string mes = _rabbit.GetMessage(_rabbit.TicketsRQ);
            if (mes != String.Empty)
            {
                // в зависимости от ответа менять состояние пассажира
            }

            mes = _rabbit.GetMessage(_rabbit.RegistrationRQ); 
            if (mes != String.Empty)
            {
                // в зависимости от ответа менять состояние пассажира
            }
        }

        void ProccessState(Passenger passenger, PassengerState curState)
        {
            switch (curState)
            {
                case PassengerState.CameToAirport:
                    ProccessCameToAirport(passenger);
                    break;
                case PassengerState.RequestedTicket:
                    break;
                case PassengerState.GotTicket:
                    passenger.RollToGoAFK(_curTime);
                    if (passenger.State != PassengerState.AFK)
                    {
                        ProccessGotTicket(passenger);
                    }
                    break;
                case PassengerState.RequstedRegistration:
                    break;
                case PassengerState.Registered:
                    passenger.RollToGoAFK(_curTime);
                    if (passenger.State != PassengerState.AFK)
                    {
                        ProccessRegistered(passenger);
                    }
                    break;
                case PassengerState.GotIntoBus:
                    Passengers.Remove(passenger);
                    break;
                case PassengerState.AFK:
                    passenger.AFKTimer--;
                    break;
                case PassengerState.GotRejected:
                    Passengers.Remove(passenger);
                    break;
            }
        }

        void ProccessCameToAirport(Passenger passenger)
        {
            int baggage = passenger.HasBaggage ? 1 : 0;
            string message = $"{passenger.Flight.GUID}\r\n{passenger.GUID}\r\n{baggage}";
            _rabbit.PutMessage(_rabbit.TicketsWQ, message);
            passenger.State = PassengerState.RequestedTicket;
            passenger.NextState = PassengerState.GotTicket;
        }

        void ProccessGotTicket(Passenger passenger)
        {
            string message = $"{passenger.Flight.GUID}\r\n{passenger.GUID}";
            _rabbit.PutMessage(_rabbit.RegistrationWQ, message);
            passenger.State = PassengerState.Registered;
            passenger.NextState = PassengerState.GotIntoBus;
        }

        void ProccessRegistered(Passenger passenger)
        {
            string message = $"{passenger.Flight.GUID}\r\n{passenger.GUID}";
            _rabbit.PutMessage(_rabbit.BusWQ, message);
            passenger.State = PassengerState.GotIntoBus;
        }
    }
}
