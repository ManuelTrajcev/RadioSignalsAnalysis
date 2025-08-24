using Microsoft.AspNetCore.Mvc;

namespace RadioSignalsWeb.Controllers;

[ApiController]
[Route("api/home")]
public class HomePageController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetUserFromToken()
    {
        try
        {
            var helloMsg  = "Hello World";
            return await Task.FromResult<ActionResult<string>>(Ok("Hello World"));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}