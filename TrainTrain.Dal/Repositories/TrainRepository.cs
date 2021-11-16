using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrainTrain.Dal.Repositories
{
    public class TrainRepository : ITrainRepository
    {
        private readonly ITrainDataService _trainDataService;

        public TrainRepository(ITrainDataService trainDataService)
        {
            _trainDataService = trainDataService;
        }

        public async Task<Train> Get(string trainId)
        {
            var trainJson = await _trainDataService.GetTrain(trainId);

            var parsed = JsonConvert.DeserializeObject(trainJson);

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
    }
}