using System.Threading.Tasks;

namespace TrainTrain
{
    public interface ITrainCaching
    {
        Task Save(string train, Train trainInst, string bookingRef);
        void Clear();
    }
}