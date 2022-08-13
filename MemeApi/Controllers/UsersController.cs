using MemeApi.Models;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MemeApi.library.repositories;
using MemeApi.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace MemeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public UsersController(IConfiguration config, SignInManager<User> signInManager, UserManager<User> userManager, UserRepository userRepository)
        {
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _userRepository.GetUser(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, UserUpdateDTO updateDto)
        {
            if (!(await _userRepository.UpdateUser(id, updateDto))) return NotFound();

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("[controller]/register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<User>> Register(UserCreationDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = new User
            {
                UserName = userDTO.Username,
                Email = userDTO.Email
            };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            if (!result.Succeeded)
                return BadRequest(userDTO);

            await _signInManager.SignInAsync(user, false);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(long id)
        {
            if (await _userRepository.DeleteUser(id)) return NotFound();

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login/")]
        public IActionResult Login(UserLoginDTO login)
        {
            var user = _userManager.FindByNameAsync(login.Username);

            if (user == null)
                return Unauthorized();

            return Ok();
        }
    }
}
