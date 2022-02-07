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
