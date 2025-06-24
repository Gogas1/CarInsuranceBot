using CarInsuranceBot.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsuranceBot.WebApi.Controllers
{
    [Route("/")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly TestService testService;

        public IndexController(TestService testService)
        {
            this.testService = testService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Use our telegram bot @TestTask_CarInsuranceBot");
        }

        [HttpPost("/license")]
        public async Task<IActionResult> SetVehicleDocument(long userId)
        {
            await testService.AddVehicleDocument(userId, new Core.Models.Documents.DriverLicenseDocument());
            return Ok();
        }

        [HttpPost("/state")]
        public async Task<IActionResult> SetState(long userId)
        {
            await testService.GotoVehicleDataCorrectingState(userId);
            return Ok();
        }
    }
}
