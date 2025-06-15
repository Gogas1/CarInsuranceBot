using Microsoft.AspNetCore.Mvc;

namespace CarInsuranceBot.WebApi.Controllers
{
    [Route("/")]
    [ApiController]
    public class IndexController : ControllerBase
    {

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Use our telegram bot @TestTask_CarInsuranceBot");
        }
    }
}
