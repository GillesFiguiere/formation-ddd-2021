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


        public bool WillNotExceed70PercentReservation(int nbSeatRequested) =>
            Seats.Count(s => s.IsReserved()) + nbSeatRequested <= Math.Floor(ThreasholdManager.GetMaxRes() * Seats.Count);
    }
}