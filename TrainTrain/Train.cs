using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainTrain
{
    public class Train
    {
        public const int CoachCapacity = 10;
        public Train(List<Seat> seats)
        {
            Seats = seats;
        }

        public List<Seat> Seats { get; }


        private bool WillNotExceed70PercentReservation(int nbSeatRequested) =>
            Seats.Count(s => s.IsReserved) + nbSeatRequested <= Math.Floor(ThreasholdManager.GetMaxRes() * Seats.Count);

        public IEnumerable<Seat> ReserveSeats(int nbSeatRequested)
        {
            if (WillNotExceed70PercentReservation(nbSeatRequested))
            {
                var availableSeats = Seats.Where(seat => !seat.IsReserved).ToList();

                var reservedSeats = FindAndReserveSeats(nbSeatRequested, availableSeats);

                if (reservedSeats.Count != nbSeatRequested) return new List<Seat>();
                
                return reservedSeats;
            }

            return new List<Seat>();
        }
        
        private static List<Seat> FindAndReserveSeats(int nbSeatRequested, List<Seat> availableSeats)
        {
            var reservedSeats = new List<Seat>();
            if (nbSeatRequested > CoachCapacity) return reservedSeats;
            
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
                availableSeats[index].Reserve();
                reservedSeats.Add(availableSeats[index]);
            }

            return reservedSeats;
        }
    }
}