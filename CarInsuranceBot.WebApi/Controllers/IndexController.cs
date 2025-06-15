using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarInsuranceBot.WebApi.Controllers
{
    [Route("api/")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly DocumentsService _documentsService;
        private readonly TestService _testService;

        public IndexController(DocumentsService documentsService, TestService testService)
        {
            _documentsService = documentsService;
            _testService = testService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("index");
        }

        [HttpPost("documents")]
        public async Task<IActionResult> SetUserDocuments([FromBody] SetUserDocumentsModel model)
        {
            await _documentsService.SetDocumentsForUser(model.UserId, model.IdDocument, model.LicenseDocument, default);
            return Ok();
        }

        [HttpPost("test")]
        public async Task<IActionResult> SetUserTestState(long userId)
        {
            await _testService.ToTestState(userId);
            return Ok();
        }

        public class SetUserDocumentsModel
        {
            public long UserId { get; set; }
            public IdDocument IdDocument { get; set; }
            public DriverLicenseDocument LicenseDocument { get; set; }
        }
    }
}
