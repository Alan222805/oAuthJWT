using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using oAuthJWT.Models;
using oAuthJWT.Services;

namespace oAuthJWT.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly oAuthJTWContext _dbContext;
        private readonly IUserService _userService;

        public AuthController(IConfiguration config, oAuthJTWContext context, IUserService userService)
        {
            _config = config;
            _dbContext = context;
            _userService = userService;
        }


        [HttpGet]
        [Route("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Route("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if(!authenticateResult.Succeeded)
            {
                return Unauthorized();
            }
            var user = authenticateResult.Principal;
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var name = user.FindFirst(ClaimTypes.Name)?.Value;

            var userExist = _dbContext.Users.FirstOrDefault(u => u.GoogleId == googleId);
            
            // Si el usuario existe y tiene un token válido, lo devolvemos
            if(userExist != null && userExist.CurrentToken != null && userExist.TokenExpiration > DateTime.Now)
            {
                return Ok(new { token = userExist.CurrentToken });
            }

            // Si no existe el usuario o el token expiró, generamos uno nuevo
            var token = GenerateJWTToken(authenticateResult.Principal);
            
            if(userExist == null)
            {
                userExist = new User
                {
                    GoogleId = googleId,
                    Name = name,
                    Email = email,
                    CurrentToken = token,
                    TokenExpiration = DateTime.Now.AddMinutes(30)
                };
                _dbContext.Users.Add(userExist);
            }
            else
            {
                userExist.CurrentToken = token;
                userExist.TokenExpiration = DateTime.Now.AddMinutes(30);
                _dbContext.Users.Update(userExist);
            }
            
            await _dbContext.SaveChangesAsync();
            return Ok(new {token = token});
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

            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userExist.GoogleId),
                new Claim(ClaimTypes.Name, userExist.Name),
                new Claim(ClaimTypes.Email, userExist.Email)
            });

            var claimsPrincipal = new ClaimsPrincipal(claims);
            var tokenString = GenerateJWTToken(claimsPrincipal);
            return Ok(new {token = tokenString});
        }
        
        public string GenerateJWTToken(ClaimsPrincipal user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new [] 
            {
                new Claim(JwtRegisteredClaimNames.Jti, user.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                new Claim(JwtRegisteredClaimNames.Sub, user.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                new Claim(JwtRegisteredClaimNames.Name, user.FindFirst(ClaimTypes.Name)?.Value),
                new Claim(JwtRegisteredClaimNames.Email, user.FindFirst(ClaimTypes.Email)?.Value)
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