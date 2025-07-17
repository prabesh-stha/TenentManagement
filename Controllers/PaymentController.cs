using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Payment;
using TenentManagement.Models.Property.Unit;
using TenentManagement.Services.Payment;
using TenentManagement.Services.Property.Unit;
using TenentManagement.Services.Utilities;

namespace TenentManagement.Controllers
{
    [Authorize]
    [BlockDirectAccess]
    public class PaymentController : Controller
    {
        private readonly UnitService _unitService;
        private readonly PaymentInvoiceService _paymentService;
        public PaymentController(UnitService unitService, PaymentInvoiceService paymentService)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }
        [HttpGet]
        public IActionResult Create(int unitId)
        {
            UnitModel? unitModel = _unitService.GetUnitById(unitId);
            if (unitModel == null) {
                ViewData["Message"] = "Unit not found.";
                ViewData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            var paymentMethods = _paymentService.GetAllPaymentMethod();
            var paymentStatues = _paymentService.GetAllPaymentStatus();
            PaymentInvoiceModel model = new PaymentInvoiceModel
            {
                UnitId = unitId,
                
                PaymentMethods = paymentMethods,
                PaymentStatuses = paymentStatues
            };
            return View(unitModel);
        }
    }
}
