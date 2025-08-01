using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TenentManagement.Models;
using TenentManagement.Services.Payment;
using TenentManagement.Services.Property;
using TenentManagement.Services.Property.Unit;
using TenentManagement.Services.User;
using TenentManagement.ViewModel;

namespace TenentManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PropertyService _propertyService;
        private readonly PaymentInvoiceService _paymentInvoiceService;


        public HomeController
            (
            ILogger<HomeController> logger
            ,PropertyService propertyService
            ,PaymentInvoiceService paymentInvoiceService
            )
        {
            _logger = logger;
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _paymentInvoiceService = paymentInvoiceService ?? throw new ArgumentNullException(nameof(paymentInvoiceService));

        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning("Unauthorized access attempt - no user ID in session");
                return RedirectToAction("Login", "Authentication");
            }
            var owned = await _propertyService.GetAllProperty(userId.Value);
            var rented = await _propertyService.GetAllRentedProperty(userId.Value);

            var invoiceCreateAlert = _paymentInvoiceService.GetInvoiceCreationAlert(userId.Value);
            var dueDateAlert = _paymentInvoiceService.GetDueDateAlert(userId.Value);
            ViewBag.InvoiceAlerts = invoiceCreateAlert;
            ViewBag.DueDateAlerts = dueDateAlert;
            return View(Tuple.Create(owned, rented));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
