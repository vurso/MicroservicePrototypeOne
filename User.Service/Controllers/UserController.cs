using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using User.Domain.Constants;
using User.Domain.Contracts;
using User.Domain.Models;

namespace User.Service.Controllers
{
    [ProducesResponseType(500)]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Policy = "Administrator")]
        [HttpPost("v1/user")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> PostUserPreferencesAsync([FromBody] PreferencesModel preferenceModel)
        {
            var authorizedUserId = Guid.Parse(HttpContext.User.Claims.Single(c => c.Type == "userId").Value);

            var preference = await _userService.GetUserPreferenceAsync(preferenceModel.UserId);
            if (preference != null)
                return BadRequest(UserConstants.UserExists);

            await _userService.PostUserPreferenceAsync(preferenceModel, authorizedUserId);
            await _userService.CompleteTransactionAsync();

            return Ok(preferenceModel.UserId);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Policy = "User")]
        [HttpGet("v1/user/{userId}")]
        [ProducesResponseType(typeof(PreferencesModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserPreferencesAsync(Guid userId)
        {
            var authorizedUserId = Guid.Parse(HttpContext.User.Claims.Single(c => c.Type == "userId").Value);
            var elevatedRights = bool.Parse(HttpContext.User.Claims.Single(c => c.Type == "ElevatedRights").Value);

            if (!elevatedRights && userId != authorizedUserId)
                return Forbid();

            var preference = await _userService.GetUserPreferenceAsync(userId);
            if (preference is null)
                return NotFound();

            return Ok((PreferencesModel)preference);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Policy = "User")]
        [HttpPut("v1/user/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutUserPreferencesAsync(Guid userId, [FromBody] PutPreferencesModel preferenceModel)
        {
            var authorizedUserId = Guid.Parse(HttpContext.User.Claims.Single(c => c.Type == "userId").Value);
            var elevatedRights = bool.Parse(HttpContext.User.Claims.Single(c => c.Type == "ElevatedRights").Value);

            if (!elevatedRights && userId != authorizedUserId)
                return Forbid();

            var preference = await _userService.GetUserPreferenceAsync(userId);
            if (preference is null)
                return NotFound();

            _userService.PutUserPreference(preference, preferenceModel, authorizedUserId);
            await _userService.CompleteTransactionAsync();

            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Policy = "Administrator")]
        [HttpDelete("v1/user/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteUserPreferencesAsync(Guid userId)
        {
            var authorizedUserId = Guid.Parse(HttpContext.User.Claims.Single(c => c.Type == "userId").Value);
            
            var preference = await _userService.GetUserPreferenceAsync(userId);
            if (preference is null)
                return Ok();

            _userService.DeleteUserPreference(preference, authorizedUserId);
            await _userService.CompleteTransactionAsync();

            return Ok();
        }
    }
}
