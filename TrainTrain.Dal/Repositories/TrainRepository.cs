using System.Threading.Tasks;

namespace TrainTrain.Dal.Repositories
{
    public class TrainRepository : ITrainRepository
    {
        private readonly ITrainDataService _trainDataService;

        public TrainRepository(ITrainDataService trainDataService)
        {
            _trainDataService = trainDataService;
        }

        public async Task<Train> Get(string trainId)
        {
            return new Train(await _trainDataService.GetTrain(trainId));
        }
    }
}