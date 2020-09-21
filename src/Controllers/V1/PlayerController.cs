using Microsoft.AspNetCore.Mvc;
using Sketch.DTOs;
using Sketch.Services.Interfaces;
using System.Threading.Tasks;

namespace Sketch.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<PlayerViewModel>> Login([FromBody] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { Message = "Login should be provided!" });
            }

            var (success, player) = await _playerService.Login(username);
            return success ? Ok(player) : (ActionResult<PlayerViewModel>)BadRequest("Login is taken!");
        }
    }
}
