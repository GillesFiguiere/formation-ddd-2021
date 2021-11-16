using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainTrain
{
    public class WebTicketManager
    {
        private readonly ITrainCaching _trainCaching;
        private readonly ITrainDataService _trainDataService;
        private readonly IBookingReferenceService _bookingReferenceService;

        public WebTicketManager(ITrainDataService trainDataService, IBookingReferenceService bookingReferenceService, ITrainCaching trainCaching)
        {
            _trainDataService = trainDataService;
            _bookingReferenceService = bookingReferenceService;
            _trainCaching = trainCaching;
            _trainCaching.Clear();
        }
        public async Task<string> Reserve(string trainId, int nbSeatRequested)
        {
            // TODO repository
            var jsonTrain = await _trainDataService.GetTrain(trainId);
            var train = new Train(jsonTrain);

            if (train.WillNotExceed70PercentReservation(nbSeatRequested))
            {
                var availableSeats = train.Seats.Where(seat => seat.IsNotReserved()).ToList();

                var reservedSeats = ReserveSeats(nbSeatRequested, availableSeats);

                if (reservedSeats.Count != nbSeatRequested) return EmptyReservation(trainId);

                var bookingRef = await _bookingReferenceService.GetBookingReference();

                AddBookingRefToSeats(reservedSeats, bookingRef);

                await _trainCaching.Save(trainId, train, bookingRef);

                await _trainDataService.BookSeats(trainId, bookingRef, reservedSeats);
                return
                    $"{{\"train_id\": \"{trainId}\", \"booking_reference\": \"{bookingRef}\", \"seats\": {dumpSeats(reservedSeats)}}}";
            }
            return EmptyReservation(trainId);
        }

        private static void AddBookingRefToSeats(List<Seat> reservedSeats, string bookingRef)
        {
            foreach (var reservedSeat in reservedSeats)
            {
                reservedSeat.BookingRef = bookingRef;
            }
        }

        private static string EmptyReservation(string trainId) =>
            $"{{\"train_id\": \"{trainId}\", \"booking_reference\": \"\", \"seats\": []}}";

        private static List<Seat> ReserveSeats(int nbSeatRequested, List<Seat> availableSeats)
        {
            var reservedSeats = new List<Seat>();
            if (nbSeatRequested > Train.CoachCapacity) return reservedSeats;
            
            var firstSeatIndex = 0;
            for (int index = 0; index < availableSeats.Count; index++)
            {
                var seat = availableSeats[index];
                if (seat.CoachName == availableSeats[index + nbSeatRequested - 1].CoachName)
                {
                    firstSeatIndex = index;
                    break;
                }
            }

            for (int index = firstSeatIndex; index < firstSeatIndex + nbSeatRequested; index++)
            {
                reservedSeats.Add(availableSeats[index]);
            }

            return reservedSeats;
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