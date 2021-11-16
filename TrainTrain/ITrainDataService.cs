using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrainTrain
{
    public interface ITrainDataService
    {
        Task<Train> GetTrain(string train);
        Task BookSeats(string trainId, string bookingRef, IEnumerable<Seat> availableSeats);
    }
}