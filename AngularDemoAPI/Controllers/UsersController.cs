using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Entities;
using WebApi.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return Ok(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [HttpGet("getalluser")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Role.SuperAdmin)]
        public IActionResult GetById(int id)
        {
            // only allow admins to access other user records
            var currentUserId = int.Parse(User.Identity.Name);
            if (id != currentUserId && !User.IsInRole(Role.SuperAdmin))
                return Forbid();

            var user = _userService.GetById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("createupdateuser")]
        public async Task<IActionResult> CreateUpdateUser([FromBody] CreateUserRequest model)
        {
            if (model.Id > 0)
            {
                if (ModelState.ContainsKey("password"))
                    ModelState.Remove("password");
            }

            if (ModelState.IsValid)
            {
                var createduser = await _userService.CreateUpdateUser(model);

                return Ok(createduser);
            }
            else
            {
                var Errors = ModelState.Keys.Where(i => ModelState[i].Errors.Count > 0)
                .Select(k => new string(ModelState[k].Errors.First().ErrorMessage));

                return BadRequest(string.Join(',', Errors));
            }
        }

        [Authorize(Roles = Role.SuperAdmin)]
        [HttpDelete("deleteuser")]
        public IActionResult DeleteUser(long id)
        {
            var users = _userService.DeleteUser(id);
            return Ok(users);
        }

        [HttpGet("getuserdetails")]
        public IActionResult GetLoginUserDetails()
        {
            var currentUserId = int.Parse(User.Identity.Name);

            var users = _userService.GetById(currentUserId);
            return Ok(users);
        }
    }
}
