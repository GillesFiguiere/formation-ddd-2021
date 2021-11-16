using System;
using System.Threading.Tasks;
using TrainTrain.Dal.Entities;

namespace TrainTrain.Dal.Repositories
{
    public class TrainCaching : ITrainCaching
    {
        public async Task Save(string train, Train trainInst, string bookingRef)
        {
            await Task.Run((Action) (() => Cache(trainInst, train, bookingRef)));
        }

        public void Clear()
        {
            TrainRepository.RemoveAll();
        }

        private static void Cache(Train trainInst, string trainId, string bookingRef)
        {
            var trainEntity = new TrainEntity { TrainId = trainId };
            foreach (var seat in trainInst.Seats)
            {
                trainEntity.Seats.Add(new SeatEntity { TrainId = trainId, BookingRef = bookingRef, CoachName = seat.CoachName, SeatNumber = seat.SeatNumber });
            }
            TrainRepository.Save(trainEntity);
        }
    }
}