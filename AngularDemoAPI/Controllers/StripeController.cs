using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private IStripeService _stripeService;

        public StripeController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [AllowAnonymous]
        [HttpPost("createcard")]
        public IActionResult Authenticate([FromBody] createcardrequest model)
        {
            var card = _stripeService.CreateVirtualCard(model);

            if (card == null)
                return Ok(new { message = "Please fill valid details" });

            return Ok(card);
        }
    }
}
