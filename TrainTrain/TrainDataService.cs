using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrainTrain
{
    public interface ITrainDataService
    {
        Task<string> GetTrain(string train);
        Task BookSeats(string trainId, string bookingRef, List<Seat> availableSeats);
    }
}