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

        public int GetMaxSeat()
        {
            return this.Seats.Count;
        }

        public List<Seat> Seats { get; }


        public bool WillNotExceed70PercentReservation(int nbSeatRequested) =>
            Seats.Count(s => s.IsReserved()) + nbSeatRequested <= Math.Floor(ThreasholdManager.GetMaxRes() * Seats.Count);
    }

    public class TrainJsonPoco
    {
        public List<SeatJsonPoco> seats { get; set;  }

        public TrainJsonPoco()
        {
            this.seats = new List<SeatJsonPoco>();
        }
    }

    public class SeatJsonPoco
    {
        public string booking_reference { get; set; }
        public string seat_number { get; set; }
        public string coach { get; set; }

        public SeatJsonPoco()
        {
        }
    }
}