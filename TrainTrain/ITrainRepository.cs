using System.Threading.Tasks;

namespace TrainTrain
{
    public interface ITrainRepository
    {
        Task<Train> Get(string trainId);
    }
}