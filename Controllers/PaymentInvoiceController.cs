using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;
using TenentManagement.Services.Payment;
using TenentManagement.Services.Property.Unit;

namespace TenentManagement.Controllers
{
    public class PaymentInvoiceController : Controller
    {
        private readonly UnitService _unitService;
        private readonly PaymentInvoiceService _paymentInvoiceService;
        private readonly PaymentService _paymentService;
        public PaymentInvoiceController(UnitService unitService, PaymentInvoiceService paymentInvoiceService, PaymentService paymentService)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _paymentInvoiceService = paymentInvoiceService ?? throw new ArgumentNullException(nameof(paymentInvoiceService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));


        }

        [HttpGet]
        public IActionResult CreateInvoice(int unitId, int userId)
        {
            var unit = _unitService.GetUnitById(unitId);
            if (unit == null)
            {
                TempData["Message"] = "Unit not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            var payment = _paymentService.GetAllPaymentByUnit(unitId);
            var paidMonths = payment
                .Select(p => new DateTime(p.PaidMonth.Year, p.PaidMonth.Month, 1))
                .ToList();
            if (!unit.RentStartDate.HasValue || !unit.RentEndDate.HasValue)
            {
                ViewData["Message"] = "Rent duration is not configured.";
                ViewData["MessageType"] = "error";
                return View();
            }

            var rentStart = new DateTime(unit.RentStartDate.Value.Year, unit.RentStartDate.Value.Month, 1);
            var rentEnd = new DateTime(unit.RentEndDate.Value.Year, unit.RentEndDate.Value.Month, 1);

            var allMonths = new List<DateTime>();
            for (var dt = rentStart; dt <= rentEnd; dt = dt.AddMonths(1))
            {
                allMonths.Add(dt);
            }

            var monthsExceptPaidMonth = allMonths.Except(paidMonths).ToList();
            var availableMonths = monthsExceptPaidMonth.OrderBy(m => m).Where(m => m > monthsExceptPaidMonth.First()).ToList();

            //ViewBag.AvailableMonths = availableMonths;
            var model = new PaymentInvoiceModel
            {
                FromMonth = monthsExceptPaidMonth.First(),
                UnitId = unitId,
                AmountPerMonth = unit.RentAmount ?? 0,
                RenterId = unit.RenterId ?? 0,
                OwnerId = userId,
                AvailableMonth = availableMonths,
                DueDate = new DateTime(unit.RentStartDate.Value.Year, unit.RentStartDate.Value.Month, unit.RentStartDate.Value.Day)
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateInvoice(PaymentInvoiceModel model)
        {
            // Normalize dates to 1st of month
            model.FromMonth = new DateTime(model.FromMonth.Year, model.FromMonth.Month, 1);
            model.ToMonth = new DateTime(model.ToMonth.Year, model.ToMonth.Month, 1);

            // Get rent range
            var unit = _unitService.GetUnitById(model.UnitId);
            if (unit == null)
            {
                TempData["Message"] = "Unit not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            if (!unit.RentStartDate.HasValue || !unit.RentEndDate.HasValue)
            {
                ViewData["Message"] = "Rent duration is not configured.";
                ViewData["MessageType"] = "error";
                return View(model);
            }

            var rentStart = unit.RentStartDate.Value;
            var rentEnd = unit.RentEndDate.Value;

            // Normalize RentStartDate (round up to next 1st if not already 1st)
            DateTime startLimit = new DateTime(rentStart.Year, rentStart.Month, 1);

                // Normalize RentEndDate (round down to 1st of that month)
                DateTime endLimit = new DateTime(rentEnd.Year, rentEnd.Month, 1);

            if (model.FromMonth < startLimit || model.ToMonth > endLimit)
                {
                    ViewData["Message"] = $"You must select months between {startLimit:yyyy-MM} and {endLimit:yyyy-MM}.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }


            var payment = _paymentService.GetAllPaymentByUnit(model.UnitId);
            var paidMonths = payment
                .Select(p => new DateTime(p.PaidMonth.Year, p.PaidMonth.Month, 1))
                .ToHashSet();

            // Generate range of months to invoice
            var invoiceMonths = new List<DateTime>();

            for (var dt = model.FromMonth; dt <= model.ToMonth; dt = dt.AddMonths(1))
            {
                if (paidMonths.Contains(dt))
                {
                    ViewData["Message"] = $"Month {dt:yyyy-MM} is already paid.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
                invoiceMonths.Add(dt);
            }

            // Valid invoice
            model.AmountDue = invoiceMonths.Count * model.AmountPerMonth;

                try {
                    int row = _paymentInvoiceService.CreatePaymentInvoice(model);
                    if (row > 0)
                    {
                        TempData["Message"] = "Invoice created successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Detail", "Unit", new { id = model.UnitId });
                    }
                    else
                    {
                        ViewData["Message"] = "An error occurred while creating the invoice.";
                        ViewData["MessageType"] = "error";
                        return View(model);
                    }
                }
                catch(Exception ex)
                {
                    ViewData["Message"] = $"An error occurred while creating the invoice: {ex.Message}";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
        }
    }
}
