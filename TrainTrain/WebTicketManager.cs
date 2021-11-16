using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TrainTrain
{
    public class WebTicketManager
    {
        private const string UriBookingReferenceService = "http://localhost:51691/";
        private const string UriTrainDataService = "http://localhost:50680";
        private readonly ITrainCaching _trainCaching;
        private readonly ITrainDataService _trainDataService;
        private readonly IBookingReferenceService _bookingReferenceService;

        public WebTicketManager():this(new TrainDataService(UriTrainDataService), new BookingReferenceService(UriBookingReferenceService))
        {

        }

        public WebTicketManager(ITrainDataService trainDataService, IBookingReferenceService bookingReferenceService)
        {

            _trainDataService = trainDataService;
            _bookingReferenceService = bookingReferenceService;
            _trainCaching = new TrainCaching();
            _trainCaching.Clear();
    
        }
        public async Task<string> Reserve(string trainId, int nbSeatRequested)
        {
            List<Seat> availableSeats = new List<Seat>();

            // get the train
            var jsonTrain = await _trainDataService.GetTrain(trainId);

            var train = new Train(jsonTrain);
            if (train.ReservedSeats + nbSeatRequested <= Math.Floor(ThreasholdManager.GetMaxRes() * train.GetMaxSeat()))
            {
                // find seats to reserve
                for (int index = 0; index < train.Seats.Count; index++)
                {
                    var seat = train.Seats[index];
                    if (seat.BookingRef == "")
                    {
                        availableSeats.Add(seat);
                    }
                }
                
                // reserve seat
                var reservedSeats = new List<Seat>();
                var firstSeatIndex = 0;
                for(int index = 0; index < availableSeats.Count; index++)
                {
                    var seat = availableSeats[index];
                    if(seat.SeatNumber + nbSeatRequested < 10)
                    {
                        firstSeatIndex = index;
                        break;
                    }
                }

                for (int index = firstSeatIndex; index < firstSeatIndex + nbSeatRequested; index++)
                {
                    reservedSeats.Add(availableSeats[index]);
                }

                string bookingRef;
                if (reservedSeats.Count != nbSeatRequested)
                {
                    return $"{{\"train_id\": \"{trainId}\", \"booking_reference\": \"\", \"seats\": []}}";
                }

                bookingRef = await _bookingReferenceService.GetBookingReference();

                foreach (var reservedSeat in reservedSeats)
                {
                    reservedSeat.BookingRef = bookingRef;
                }

                if (reservedSeats.Count == nbSeatRequested)
                {
                    await _trainCaching.Save(trainId, train, bookingRef);

                    await _trainDataService.BookSeats(trainId, bookingRef, reservedSeats);
                    return
                            $"{{\"train_id\": \"{trainId}\", \"booking_reference\": \"{bookingRef}\", \"seats\": {dumpSeats(reservedSeats)}}}";
                    
                }
            }
            return $"{{\"train_id\": \"{trainId}\", \"booking_reference\": \"\", \"seats\": []}}";
        }

        private string dumpSeats(IEnumerable<Seat> seats)
        {
            var sb = new StringBuilder("[");

            var firstTime = true;
            foreach (var seat in seats)
            {
                if (!firstTime)
                {
                    sb.Append(", ");
                }
                else
                {
                    firstTime = false;
                }

                sb.Append(string.Format("\"{0}{1}\"", seat.SeatNumber, seat.CoachName));
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}