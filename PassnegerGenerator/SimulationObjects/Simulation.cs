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
        Rabbit _rabbit = Rabbit.GetInstance();
        DateTime _time = DateTime.Now;
        int _interval = 1000; // за сколько миллисекунд реального времени проходит 1 минута симуляции

        public List<Passenger> passengers { get; set; }
        public List<Flight> flights { get; set; }



        public void Execute()
        {
            while (true)
            {
                var delay = Task.Delay(_interval);
                // прочитать сообщения из рэббита
                
                for (int i = 0; i < passengers.Count; i++)
                {
                    Passenger passenger = passengers[i];
                    PassengerState curState = passenger.State;
                    ProccessState(passenger, curState);
                }

                _time = _time.AddMinutes(1);
                delay.Wait();
            }
        }

        void ProccessState(Passenger passenger, PassengerState curState)
        {
            switch (curState)
            {
                case PassengerState.CameToAirport:
                    int baggage = passenger.HasBaggage ? 1 : 0;
                    string message = $"{passenger.Flight.GUID}\r\n{passenger.GUID}\r\n{baggage}";
                    _rabbit.PutMessage(_rabbit.TicketsWQ, message);
                    passenger.State = PassengerState.RequestedTicket;
                    passenger.NextState = PassengerState.GotTicket;
                    break;
                case PassengerState.RequestedTicket:
                    break;
                case PassengerState.GotTicket:
                    
                    break;
                case PassengerState.RequstedRegistration:
                    break;
                case PassengerState.Registered:
                    break;
                case PassengerState.GotIntoBus:
                    break;
                case PassengerState.AFK:
                    passenger.AFKTimer--;
                    break;
                case PassengerState.GotRejected:
                    break;
            }
        }
    }
}
