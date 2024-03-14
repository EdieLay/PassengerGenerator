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

        readonly string _guid;
        readonly string _name;
        readonly bool _hasBaggage;
        readonly Flight _flight;
        // предпочтения по еде (возможно)

        PassengerState _state;
        PassengerState _nextState;
        int _afkTimer;

        public string GUID { get => _guid; }
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
                //Log.Debug("NextState of passenger {Name} set to {NextState}", Name, value.ToString());
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
                    State = NextState;
                    NextState = NextState + 1;
                }
            }
        }

        public Passenger(Flight flight)
        {
            _guid = Guid.NewGuid().ToString();
            _name = NameGenerator.GetRandomName();
            _hasBaggage = rand.NextDouble() < 0.85;
            _flight = flight;
            _state = PassengerState.CameToAirport;
            _nextState = PassengerState.RequestedTicket;
            _afkTimer = 0;
        }

        public void RollToGoAFK(DateTime curTime)
        {
            if (rand.NextDouble() < 0.1) // шанс встать афк
            {
                TimeSpan timeToFlight = Flight.Date.Subtract(curTime);
                int minutesToFlight = (int)timeToFlight.TotalMinutes;
                if (minutesToFlight < 1)
                    AFKTimer = 1;
                else
                    AFKTimer = Math.Max(1, (int)(rand.Next(minutesToFlight) * rand.NextDouble())); // время, на которое встал афк
                _nextState = _state; // следующее состояние после выхода из афк - это текущее состояние пассажира
                                    // то есть мы вернёмся в тот же обработчик, в котором вызвали этот афк
            }
        }

        public void GotTicket()
        {
            State = PassengerState.GotTicket;
            NextState = PassengerState.RequstedRegistration;
        }

        public void Registered()
        {
            State = PassengerState.Registered;
            NextState = PassengerState.GotIntoBus;
        }

        public void GotRejected()
        {
            State = PassengerState.GotRejected;
        }

        public override string ToString()
        {
            string buggage = HasBaggage ? "With buggage" : "Without buggage";
            return $"Passenger info:\nID: {GUID}\nName: {Name}\n{buggage}\n{Flight}";
        }
    }
}
