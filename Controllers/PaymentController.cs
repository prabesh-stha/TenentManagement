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

    }
}
