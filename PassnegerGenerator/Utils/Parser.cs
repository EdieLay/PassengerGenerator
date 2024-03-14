using PassnegerGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassengerGenerator
{
    internal class Parser
    {
        public string PassengerKey { get => "Passenger"; }
        public string FlightKey { get => "Flight"; }
        public string BaggageKey { get => "Baggage"; }
        public string FoodKey { get => "Food"; }
        public string ResponseKey { get => "Response"; }
        public string TimeKey { get => "Time"; }
        public string FailValue { get => "0"; }
        public string SuccessValue { get => "1"; }
        public string RegistrationNotStartedValue { get => "2"; }

        public JsonDocument ParseMessage(string json)
        {
            return JsonDocument.Parse(json);
        }

        public string ParseDict(Dictionary<string, string> dict)
        {
            string json = JsonSerializer.Serialize(dict);
            return json;
        }

        public string ParsePassengerForTickets(Passenger passenger)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data[PassengerKey] = passenger.GUID;
            data[FlightKey] = passenger.Flight.GUID;
            string baggage = passenger.HasBaggage ? "1" : "0";
            data[BaggageKey] = baggage;
            data[FoodKey] = "meat";
            return ParseDict(data);
        }

        public string ParsePassengerForRegAndBus(Passenger passenger)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data[PassengerKey] = passenger.GUID;
            data[FlightKey] = passenger.Flight.GUID;
            return ParseDict(data);
        }
    }
}
