using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProjectEXE.Hubs;
using ProjectEXE.Services.Interfaces;

namespace ProjectEXE.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayOsService _payOsService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<PaymentHub> _hubContext;
        public PaymentController(IPayOsService payOsService, IConfiguration configuration, IHubContext<PaymentHub> hubContext)
        {
            _payOsService = payOsService;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment()
        {
            return View();
        }
    }
}
