using Serilog;
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
        static Random rand = new Random(Environment.TickCount);

        readonly Guid _guid;
        readonly string _name;
        readonly bool _hasBaggage;
        readonly Flight _flight;
        // предпочтения по еде (возможно)

        PassengerState _state;
        PassengerState _nextState;
        int _afkTimer;

        public Guid GUID { get => _guid; }
        public string Name { get => _name; }
        public bool HasBaggage { get => _hasBaggage; }
        public Flight Flight { get => _flight; }
        public PassengerState State
        {
            get => _state;
            set 
            { 
                _state = value;
                Log.Information("State of passenger {Name} set to {State}", Name, value.ToString());
            }
        }
        public PassengerState NextState 
        { 
            get => _nextState;
            set 
            {
                _nextState = value;
                Log.Debug("NextState of passenger {Name} set to {NextState}", Name, value.ToString());
            }
        }
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
            _guid = Guid.NewGuid();
            _name = NameGenerator.GetRandomName();
            _hasBaggage = rand.NextDouble() < 0.8;
            _flight = flight;
            _state = PassengerState.CameToAirport;
            _nextState = PassengerState.RequestedTicket;
            _afkTimer = 0;
        }

        public void RollToGoAFK(DateTime curTime)
        {
            if (rand.NextDouble() < 0.1)
            {
                TimeSpan timeToFlight = Flight.Date.Subtract(curTime);
                int minutesToFlight = (int)timeToFlight.TotalMinutes;
                if (minutesToFlight < 1)
                    AFKTimer = 1;
                else
                    AFKTimer = Math.Max(1, (int)(rand.Next(minutesToFlight) * rand.NextDouble()));
                _nextState = _state;
            }
        }


        public override string ToString()
        {
            string buggage = HasBaggage ? "With buggage" : "Without buggage";
            return $"Passenger info:\nID: {GUID}\nName: {Name}\n{buggage}\n{Flight}";
        }
    }
}
