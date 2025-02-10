using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oAuthJWT.Models;
using oAuthJWT.Services;

namespace oAuthJWT.Controllers
{
    [ApiController]
    [Route("user/")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("get")]
        public IActionResult Get()
        {
            return Ok(_userService.GetUsers());
        }

        [HttpPost]
        [Route("post")]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            await _userService.PostUser(user);
            return Ok();
        }

        [HttpDelete]
        [Route("delete/{userId}")]
        public IActionResult Delete(Guid userId)
        {
            _userService.DeleteUser(userId);
            return Ok();
        }
    }
}