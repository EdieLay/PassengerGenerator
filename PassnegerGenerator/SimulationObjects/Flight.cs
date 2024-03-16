using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PassnegerGenerator
{
    internal class Flight
    {
        readonly string _guid;
        readonly DateTime _date;
        readonly int _totalSeats;
        readonly int _genRate;

        public string GUID {  get => _guid; }
        public DateTime Date { get => _date; }
        public int TotalSeats { get => _totalSeats; }
        public int PassengerGenerationRate { get => _genRate; } // 0-99 - 0 passengers; 100-199 - 1 passenger; 200-299 - 2... 
        public int GeneratedOnFlight { get; set; }


        public Flight(string guid, DateTime date, int totalSeats, DateTime curTime)
        {
            _guid = guid;
            _date = date;
            _totalSeats = totalSeats;
            int secondsToFlight = (int)date.Subtract(curTime).TotalSeconds;
            _genRate = (int)((double)_totalSeats * 100.0 / (double)secondsToFlight * 1.5) + 100;
            GeneratedOnFlight = 0;
            Log.Information("Created flight: {@Flight}", this);
        }

        public int GenerateNumOfPassengers(DateTime curTime)
        {
            int secondsToFlight = (int)_date.Subtract(curTime).TotalSeconds;
            //Log.Debug("Flight time: {FlightTime}. Current time: {CurrentTime}. Difference: {MinutesDif}", Date, curTime, minutesToFlight);
            if (_date.Subtract(curTime).TotalSeconds > 240)
                return -1;
            var rand = new Random();
            return rand.Next(PassengerGenerationRate + 1) / 100;
        }

        public override string ToString()
        {
            return $"Flight info:\nID: {GUID}\nDate: {Date}";
        }
    }
}
