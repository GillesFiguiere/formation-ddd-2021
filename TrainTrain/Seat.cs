namespace TrainTrain
{
    public class Seat
    {
        public string CoachName { get; }
        public int SeatNumber { get; }
        public bool IsReserved { get; private set; }

        public Seat(string coachName, int seatNumber, bool isReserved)
        {
            CoachName = coachName;
            SeatNumber = seatNumber;
            IsReserved = isReserved;
        }
        
        public void Reserve()
        {
            IsReserved = true;
        }
    }
}