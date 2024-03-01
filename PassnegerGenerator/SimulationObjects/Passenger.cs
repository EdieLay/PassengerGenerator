using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    enum PassengerState
    {
        CameToAirport,
        RequestedTicket,
        GotTicket,
        RequstedRegistration,
        Registered,
        GotIntoBus,
        AFK,
        GotRejected
    }
    internal class Passenger
    {
        static int FREE_ID = 1; // заменить на GUID
        static Random rand = new Random(Environment.TickCount);

        readonly int _id;
        readonly string _name;
        readonly bool _hasBaggage;
        readonly Flight _flight;
        // предпочтения по еде (возможно)

        PassengerState _state;
        PassengerState _nextState;
        int _afkTimer;

        public int Id { get => _id; }
        public string Name { get => _name; }
        public bool HasBaggage { get => _hasBaggage; }
        public Flight Flight { get => _flight; }
        public PassengerState State { get => _state; }
        public PassengerState NextState { get => _nextState; }
        public int AFKTimer 
        { 
            get
            {
                return _afkTimer;
            }
            set
            {
                _afkTimer = value;
                if (_afkTimer <= 0)
                {
                    _state = _nextState;
                    _nextState = _nextState + 1;
                }
            }
        }

        public Passenger(Flight flight)
        {
            _id = FREE_ID++;
            _name = NameGenerator.GetRandomName();
            _hasBaggage = rand.NextDouble() < 0.8;
            _flight = flight;
            _state = PassengerState.CameToAirport;
            _nextState = PassengerState.RequestedTicket;
            _afkTimer = 0;
        }

        public void GoAFK(DateTime curTime)
        {
            TimeSpan timeToFlight = Flight.Date.Subtract(curTime);
            int minutesToFlight = (int)timeToFlight.TotalMinutes;
            if (minutesToFlight < 1) 
                AFKTimer = 1;
            else
                AFKTimer = Math.Max(1, (int)(rand.Next(minutesToFlight) * rand.NextDouble()));
        }


        public override string ToString()
        {
            string buggage = HasBaggage ? "With buggage" : "Without buggage";
            return $"Passenger info:\nID: {Id}\nName: {Name}\n{buggage}\n{Flight}";
        }
    }
}
