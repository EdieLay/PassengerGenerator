using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public Flight(string guid, DateTime date, int totalSeats, DateTime curTime)
        {
            _guid = guid;
            _date = date;
            _totalSeats = totalSeats;
            int minutesToFlight = (int)date.Subtract(curTime).TotalMinutes;
            _genRate = (int)(_totalSeats * 100 / minutesToFlight * 1.15) + 100;
        }

        public int GenerateNumOfPassengers()
        {
            var rand = new Random();
            return rand.Next(PassengerGenerationRate + 1) / 100;
        }

        public override string ToString()
        {
            return $"Flight info:\nID: {GUID}\nDate: {Date}";
        }
    }
}
