using PassnegerGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string SuccessValue { get => "1"; }
        public string FailValue { get => "0"; }
        public string RegistrationNotStartedValue { get => "2"; }
        public Dictionary<string, string> ParseMessage(string message)
        {
            Dictionary<string, string> parsedMessage = new Dictionary<string, string>();
            string[] pairs = message.Split(';', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] key_value = pairs[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                string key = key_value[0].Trim();
                string value = key_value[1].Trim();
                parsedMessage[key] = value;
            }
            return parsedMessage;
        }

        public string ParseDict(Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in dict)
            {
                string key = pair.Key;
                string value = pair.Value;
                string strPair = $"{key}:{value};";
                sb.Append(strPair);
            }
            return sb.ToString();
        }

        public string ParsePassengerForTickets(Passenger passenger)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{PassengerKey}:{passenger.GUID};");
            sb.Append($"{FlightKey}:{passenger.Flight.GUID};");
            string baggage = passenger.HasBaggage ? "1" : "0";
            sb.Append($"{BaggageKey}:{baggage};");
            sb.Append($"{FoodKey}:meat;");
            return sb.ToString();
        }

        public string ParsePassengerForRegAndBus(Passenger passenger)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{PassengerKey}:{passenger.GUID};");
            sb.Append($"{FlightKey}:{passenger.Flight.GUID};");
            return sb.ToString();
        }
    }
}
