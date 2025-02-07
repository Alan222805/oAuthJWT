using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace oAuthJWT.Controllers
{
    public class DataBaseController : ControllerBase
    {
        private readonly oAuthJTWContext _dbContext;
        public DataBaseController(oAuthJTWContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        [Route("createdDb")]
        public IActionResult CreatedDatabase()
        {
            _dbContext.Database.EnsureCreated();
            return Ok();
        }
    }
}