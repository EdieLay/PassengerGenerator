using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    internal class Flight
    {
        readonly Guid _guid;
        readonly DateTime _date;
        readonly int _totalSeats;

        public Guid GUID {  get => _guid; }
        public DateTime Date { get => _date; }
        public int TotalSeats { get => _totalSeats; }


        public Flight(string guid, DateTime date, int totalSeats)
        {
            _guid = Guid.Parse(guid);
            _date = date;
            _totalSeats = totalSeats;
        }

        public override string ToString()
        {
            return $"Flight info:\nID: {GUID}\nDate: {Date}";
        }
    }
}
