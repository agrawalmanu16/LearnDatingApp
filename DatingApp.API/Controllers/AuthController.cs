using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTO;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController (IAuthRepository repo, IConfiguration config) {
            _config = config;
            _repo = repo;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserForRegisterDto userForRegisterDTO) {
            //Validate request
            userForRegisterDTO.username = userForRegisterDTO.username.ToLower ();
            if (await _repo.UserExists (userForRegisterDTO.username))
                 return BadRequest("Username already exists");

            var userToCreate = new User {
                UserName = userForRegisterDTO.username
            };

            var createdUser = await _repo.Register (userToCreate, userForRegisterDTO.password);

            return StatusCode (201);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserForLoginDTO userForLoginDto) {
           
            var userFromRepo = await _repo.Login (userForLoginDto.username.ToLower(), userForLoginDto.password);

            if (userFromRepo == null)
                return Unauthorized ();

            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim (ClaimTypes.GivenName, userFromRepo.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config.GetSection("AppSettings:token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });


        }

    }
}