using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using datingApp2.API.Data;
using datingApp2.API.Dtos;
using datingApp2.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;

namespace datingApp2.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _config = config;
            _repo = repo;

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegistorDto userForRegistorDto)
        {

            userForRegistorDto.Username = userForRegistorDto.Username.ToLower();

            if (await _repo.UserExists(userForRegistorDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = _mapper.Map<User>(userForRegistorDto);

            var createdUser = await _repo.Register(userToCreate, userForRegistorDto.Password);
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);
            return CreatedAtRoute("GetUser", new {Controller = "Users", id = createdUser.Id}, userToReturn);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {


            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }

    }

}
