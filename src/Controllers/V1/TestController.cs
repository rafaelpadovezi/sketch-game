using Microsoft.AspNetCore.Mvc;

namespace Sketch.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestController : ControllerBase
    {
        public IActionResult Get() => Ok("5");
    }
}
