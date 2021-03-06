using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrainTrain.Dal.Repositories;

namespace TrainTrain.Api.Services
{
    public class TrainDataService : ITrainDataService
    {
        private readonly string _uriTrainDataService;

        public TrainDataService(string uriTrainDataService)
        {
            _uriTrainDataService = uriTrainDataService;
        }

        public async Task<Train> GetTrain(string train)
        {
            string jsonTrainTopology;
            using (var client = new HttpClient())
            {
                var value = new MediaTypeWithQualityHeaderValue("application/json");
                client.BaseAddress = new Uri(_uriTrainDataService);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(value);
                // HTTP GET
                var response = await client.GetAsync($"api/data_for_train/{train}");
                response.EnsureSuccessStatusCode();
                jsonTrainTopology = await response.Content.ReadAsStringAsync();
            }
            
            return ParseJsonTrain(jsonTrainTopology);
        }

        private static Train ParseJsonTrain(string jsonTrainTopology)
        {
            var parsed = JsonConvert.DeserializeObject(jsonTrainTopology);

            var seats = new List<Seat>();
            foreach (var token in ((Newtonsoft.Json.Linq.JContainer)parsed))
            {
                var allStuffs = ((Newtonsoft.Json.Linq.JObject)((Newtonsoft.Json.Linq.JContainer)token).First);

                foreach (var stuff in allStuffs)
                {
                    var seat = stuff.Value.ToObject<SeatJsonPoco>();
                    seats.Add(new Seat(seat.coach, int.Parse(seat.seat_number), seat.booking_reference != ""));
                }
            }

            return new Train(seats);
        }

        public async Task BookSeats(string trainId, string bookingRef, IEnumerable<Seat> availableSeats)
        {
            using (var client = new HttpClient())
            {
                var value = new MediaTypeWithQualityHeaderValue("application/json");
                client.BaseAddress = new Uri(_uriTrainDataService);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(value);
                // HTTP POST
                HttpContent resJson = new StringContent(buildPostContent(trainId, bookingRef, availableSeats),
                    Encoding.UTF8, "application/json");
                var response = await client.PostAsync("reserve", resJson);

                response.EnsureSuccessStatusCode();
            }
        }

        public static string buildPostContent(string trainId, string bookingRef, IEnumerable<Seat> availableSeats)
        {
            var seats = new StringBuilder("[");
            bool firstTime = true;

            foreach (var s in availableSeats)
            {
                if (!firstTime)
                {
                    seats.Append(", ");
                }
                else
                {
                    firstTime = false;
                }

                seats.Append(String.Format("\"{0}{1}\"", s.SeatNumber, s.CoachName));
            }
            seats.Append("]");

            var result = String.Format(
                "{{\r\n\t\"train_id\": \"{0}\",\r\n\t\"seats\": {1},\r\n\t\"booking_reference\": \"{2}\"\r\n}}",
                trainId, seats.ToString(), bookingRef);

            return result;
        }
    }
}