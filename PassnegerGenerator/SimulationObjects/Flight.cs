using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    internal class Flight
    {
        readonly int _id;
        readonly DateTime _date;
        readonly int _totalSeats;

        public int Id {  get => _id; }
        public DateTime Date { get => _date; }
        public int TotalSeats { get => _totalSeats; }


        public Flight(int id, DateTime date, int totalSeats)
        {
            _id = id;
            _date = date;
            _totalSeats = totalSeats;
        }

        public override string ToString()
        {
            return $"Flight info:\nID: {Id}\nDate: {Date}";
        }
    }
}
