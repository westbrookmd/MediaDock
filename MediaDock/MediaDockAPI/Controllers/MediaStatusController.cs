using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MediaDockAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MediaStatusController : ControllerBase
    {
        // GET: api/<MediaStatusController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<MediaStatusController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "id " + id;
        }

        [HttpGet]
        public string GetPlayingStatus()
        {
            // get from db for authorized user
            int playing = 0;

            // calculate response
            string status = "";
            if (playing == 0)
            {
                status = "paused";
            }
            else
            {
                status = "playing";
            }
            return status;
        }

        [HttpPost]
        public void SetPlayingStatus(string status)
        {
            int playing;
            // calculate response
            if (status == "paused")
            {
                playing = 0;
            }
            else
            {
                playing = 1;
            }
            //db call to set playing status
        }

        // POST api/<MediaStatusController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<MediaStatusController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MediaStatusController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
