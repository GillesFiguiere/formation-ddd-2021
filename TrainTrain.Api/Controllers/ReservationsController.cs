using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainTrain.Api.Models;
using TrainTrain.Api.Services;
using TrainTrain.Application;
using TrainTrain.Dal.Repositories;

namespace TrainTrain.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController : Controller
    {
        private const string UriBookingReferenceService = "http://localhost:51691/";
        private const string UriTrainDataService = "http://localhost:50680";
        
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new [] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/reservations
        [HttpPost]
        public async Task<string> Post([FromBody]ReservationRequestDto reservationRequest)
        {
            var trainDataService = new TrainDataService(UriTrainDataService);
            
            var manager = new Reservation(
                trainDataService,
                new BookingReferenceService(UriBookingReferenceService), 
                new TrainCaching());
            
            return await manager.Do(reservationRequest.train_id, reservationRequest.number_of_seats);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
