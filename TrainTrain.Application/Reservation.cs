using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainTrain.Application
{
    public class Reservation
    {
        private readonly ITrainCaching _trainCaching;
        private readonly ITrainDataService _trainDataService;
        private readonly IBookingReferenceService _bookingReferenceService;

        public Reservation(ITrainDataService trainDataService, IBookingReferenceService bookingReferenceService, ITrainCaching trainCaching)
        {
            _trainDataService = trainDataService;
            _bookingReferenceService = bookingReferenceService;
            _trainCaching = trainCaching;
            _trainCaching.Clear();
        }
        public async Task<string> Do(string trainId, int nbSeatRequested)
        {
            var train = await _trainDataService.GetTrain(trainId);
            var reservedSeats = train.ReserveSeats(nbSeatRequested).ToList();
            if (!reservedSeats.Any()) return FormatEmptyReservation(trainId);

            var bookingRef = await _bookingReferenceService.GetBookingReference(); 
            await _trainCaching.Save(trainId, train, bookingRef);
            await _trainDataService.BookSeats(trainId, bookingRef, reservedSeats);

            return FormatReservation(trainId, reservedSeats, bookingRef);
        }

        private string FormatReservation(string trainId, IEnumerable<Seat> reservedSeats, string bookingRef) =>
            $"{{\"train_id\": \"{trainId}\", \"booking_reference\": \"{bookingRef}\", \"seats\": {dumpSeats(reservedSeats)}}}";


        private static string FormatEmptyReservation(string trainId) =>
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