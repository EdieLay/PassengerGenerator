using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator.SimulationObjects
{
    internal class Simulation
    {
        DateTime _time = DateTime.Now;
        int _interval = 1000; // за сколько миллисекунд реального времени проходит 1 минута симуляции

        public List<Passenger> passengers { get; set; }
        public List<Flight> flights { get; set; }



        public void Execute()
        {
            while (true)
            {
                var delay = Task.Delay(_interval);
                
                for (int i = 0; i < passengers.Count; i++)
                {
                    Passenger passenger = passengers[i];
                    PassengerState curState = passenger.State;
                    switch (curState)
                    {
                        case PassengerState.CameToAirport:
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

                _time = _time.AddMinutes(1);
                delay.Wait();
            }
        }
    }
}
