using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarInsuranceBot.WebApi.Controllers
{
    [Route("api/")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("index");
        }
    }
}
