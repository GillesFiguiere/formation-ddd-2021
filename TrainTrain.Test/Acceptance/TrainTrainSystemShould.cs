using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using TrainTrain.Application;
using TrainTrain.Dal.Repositories;

namespace TrainTrain.Test.Acceptance
{
    public class TrainTrainSystemShould
    {
        private const string TrainId = "9043-2017-09-22";
        private const string BookingReference = "75bcd15";

        [Test]
        public void Reserve_seats_when_train_is_empty()
        {
            const int seatsRequestedCount = 3;
            
            var trainDataService = BuildTrainDataService(TrainId, TrainTopologyGenerator.With_10_available_seats());
            
            var reservation = new Reservation(
                trainDataService, 
                BuildBookingReferenceService(BookingReference),
                new TrainCaching());

            var jsonReservation = reservation.Do(TrainId, seatsRequestedCount).Result;

            Check.That(jsonReservation)
                .IsEqualTo($"{{\"train_id\": \"{TrainId}\", \"booking_reference\": \"{BookingReference}\", \"seats\": [\"1A\", \"2A\", \"3A\"]}}");
        }

        [Test]
        public void Not_reserve_seats_when_it_exceed_max_capacty_threshold()
        {
            const int seatsRequestedCount = 3;

            var trainDataService = BuildTrainDataService(TrainId, TrainTopologyGenerator.With_10_seats_and_6_already_reserved());
            
            var webTicketManager = new Reservation(
                trainDataService, 
                BuildBookingReferenceService(BookingReference),
                new TrainCaching());
            
            var jsonReservation = webTicketManager.Do(TrainId, seatsRequestedCount).Result;

            Check.That(jsonReservation)
                .IsEqualTo($"{{\"train_id\": \"{TrainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }

        [Test]
        public void Reserve_all_seats_in_the_same_coach()
        {
            const int seatsRequestedCount = 2;

            var trainDataService = BuildTrainDataService(TrainId, TrainTopologyGenerator.With_2_coaches_and_9_seats_already_reserved_in_the_first_coach());
            var webTicketManager = new Reservation(
                trainDataService, 
                BuildBookingReferenceService(BookingReference),
                new TrainCaching());

            var jsonReservation = webTicketManager.Do(TrainId, seatsRequestedCount).Result;

            Check.That(jsonReservation)
                .IsEqualTo($"{{\"train_id\": \"{TrainId}\", \"booking_reference\": \"{BookingReference}\", \"seats\": [\"1B\", \"2B\"]}}");
        }
        
        [Test]
        public void ReserveNothingWhenNotEnoughSeatsInTheSameCoach()
        {
            const int seatsRequestedCount = 11;

            var trainDataService = BuildTrainDataService(TrainId, TrainTopologyGenerator.With_2_coaches_and_all_seats_available());
            var webTicketManager = new Reservation(
                trainDataService, 
                BuildBookingReferenceService(BookingReference),
                new TrainCaching());
            
            var jsonReservation = webTicketManager.Do(TrainId, seatsRequestedCount).Result;

            Check.That(jsonReservation)
                .IsEqualTo($"{{\"train_id\": \"{TrainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }

        private static IBookingReferenceService BuildBookingReferenceService(string bookingReference)
        {
            var bookingReferenceService = Substitute.For<IBookingReferenceService>();
            bookingReferenceService.GetBookingReference().Returns(Task.FromResult(bookingReference));
            return bookingReferenceService;
        }

        private static ITrainDataService BuildTrainDataService(string trainId, string trainTopology)
        {
            var trainDataService = Substitute.For<ITrainDataService>();
            trainDataService.GetTrain(trainId)
                .Returns(Task.FromResult(ParseJsonTrain(trainTopology)));
            return trainDataService;
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
    }
}
