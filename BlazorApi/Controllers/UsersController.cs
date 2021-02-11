using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BlazorApi.Contracts;
using BlazorApi.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace BlazorApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userInManager;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _iconfig;  
        public UsersController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userInManager, ILoggerService logger,
                IConfiguration iconfig)
        {
            _signInManager = signInManager;
            _userInManager = userInManager;
            _logger = logger;
            _iconfig = iconfig;
        }

        private string GetControllerNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something is wrong, please contact Admin");
        }

        private async Task<string> GenerateJWT(IdentityUser user)
        {
            JwtSecurityToken token; 

            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iconfig["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),

                };

                var roles = await _userInManager.GetRolesAsync(user);

                claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));

                token = new JwtSecurityToken(_iconfig["Jwt:Issuer"],
                    _iconfig["Jwt:Issuer"],
                    claims,
                    null,
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: credentials
                );

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ExternalLoginInfo([FromBody] UserDTO userDTO)
        {
            var location = GetControllerNames();

            try
            {

                var userName = userDTO.UserName;
                var password = userDTO.Password;

                _logger.LogInfo($"Login attempt for user {userName}");
                var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);

                if (result != null)
                {
                    var user = await _userInManager.FindByNameAsync(userName);
                    _logger.LogInfo($"Login attempt is successful for user {userName}");
                    var tokenString = GenerateJWT(user);
                    return Ok(new {token = tokenString});
                }
                _logger.LogInfo($"Login attempt is failed for user {userName}");
                return Unauthorized(userDTO);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }

    }
}
