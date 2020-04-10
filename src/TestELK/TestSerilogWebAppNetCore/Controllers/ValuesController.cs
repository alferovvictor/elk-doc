using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TestSerilogWebAppNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private ILogger<ValuesController> logger;

        public ValuesController(ILogger<ValuesController> logger) => this.logger = logger;

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            logger.LogTrace("+");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{count}")]
        public ActionResult<string> Get(int count = 1)
        {
            logger.LogTrace("+");

            for (int i = 0; i < count; i++)
            {
                logger.LogInformation("iteration: {i}", i);
                Thread.Sleep(10);
            }

            logger.LogTrace("-");

            return "value " + count;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
