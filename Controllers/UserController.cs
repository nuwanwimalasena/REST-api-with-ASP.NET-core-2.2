using System;
using System.Collections.Generic;
using BankApi.Models;
using BankApi.Services;
using Microsoft.AspNetCore.Mvc;
using BankApi.Helpers;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;


namespace BankApi.Controllers
{   
    [Authorize(Roles="admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        public UsersController(
            UserService userService, 
            IMapper mapper,
            IOptions<AppSettings> appSettings
        )
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            var user = _userService.Authenticate(userDto.Username, userDto.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Role = user.Role,
                Token = tokenString
            });
        }

        [HttpGet]
        public ActionResult<List<User>> Get()
        {
            return _userService.Get();
        }

        [HttpGet("{id:guid}", Name = "GetUser")]
        public ActionResult<User> Get(Guid id)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult<UserDto> Register([FromBody]UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            try{
                _userService.Create(user, userDto.Password);
                return CreatedAtRoute("GetUser", new { id = user.Id }, user);
 
            }catch(AppException ex){
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, [FromBody]UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.Id = id;
            var found = _userService.Get(id);

            if (found == null)
            {
                return NotFound();
            }
            try{
                _userService.Update(user, userDto.Password);
                return NoContent();
            }catch(AppException ex){
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            _userService.Remove(user.Id);

            return NoContent();
        }
    }
}