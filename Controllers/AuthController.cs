using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using oAuthJWT.Models;

namespace oAuthJWT.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly oAuthJTWContext _dbContext;

        public AuthController(IConfiguration config, oAuthJTWContext context)
        {
            _config = config;
            _dbContext = context;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(Guid userId)
        {
            var userExist = _dbContext.Users.Find(userId);

            if(userExist == null)
            {
                return NotFound();
            }
                var tokenString = GenerateJWTToken(userExist);
                return Ok(new {token = tokenString});
        }
        
        public string GenerateJWTToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new [] 
            {
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.GoogleId),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires:DateTime.Now.AddMinutes(30),
                signingCredentials : credentials

            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}