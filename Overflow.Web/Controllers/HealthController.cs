using Microsoft.AspNetCore.Mvc;

namespace Overflow.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController: ControllerBase
{
    [HttpGet]
    public ActionResult Index()
    {
        return Ok();
    }
}