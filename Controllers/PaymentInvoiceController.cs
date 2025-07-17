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
        public PaymentInvoiceController(UnitService unitService, PaymentInvoiceService paymentInvoiceService)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _paymentInvoiceService = paymentInvoiceService ?? throw new ArgumentNullException(nameof(paymentInvoiceService));

        }

        [HttpGet]
        public IActionResult CreateInvoice(int unitId, int userId)
        {
            var unit = _unitService.GetUnitById(unitId);
            if (unit == null)
            {
                ViewData["Message"] = "Unit not found.";
                ViewData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            //        var paidMonths = _db.Payments
            //.Where(p => p.UnitId == unitId)
            //.Select(p => new DateTime(p.PaidMonth.Year, p.PaidMonth.Month, 1))
            //.ToList();
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

            //var availableMonths = allMonths.Except(paidMonths).ToList();
            var availableMonths = allMonths; // Assuming no months are paid yet for simplicity

            ViewBag.AvailableMonths = availableMonths;
            var model = new PaymentInvoiceModel
            {
                UnitId = unitId,
                AmountPerMonth = unit.RentAmount ?? 0,
                RenterId = unit.RenterId ?? 0,
                OwnerId = userId,
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
                return BadRequest("Tenant rent info not found.");

            if (!unit.RentStartDate.HasValue || !unit.RentEndDate.HasValue)
            {
                ViewData["Message"] = "Rent duration is not configured.";
                ViewData["MessageType"] = "error";
                return View(model);
            }

            var rentStart = unit.RentStartDate.Value;
            var rentEnd = unit.RentEndDate.Value;

            // Normalize RentStartDate (round up to next 1st if not already 1st)
            DateTime startLimit = rentStart.Day == 1
                    ? new DateTime(rentStart.Year, rentStart.Month, 1)
                    : new DateTime(rentStart.Year, rentStart.Month, 1).AddMonths(1);

                // Normalize RentEndDate (round down to 1st of that month)
                DateTime endLimit = new DateTime(rentEnd.Year, rentEnd.Month, 1);

            if (model.FromMonth < startLimit || model.ToMonth > endLimit)
                {
                    ViewData["Message"] = $"You must select months between {startLimit:yyyy-MM} and {endLimit:yyyy-MM}.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }

            // Get paid months for this unit
            //var paidMonths = .Payments
            //    .Where(p => p.UnitId == UnitId)
            //    .Select(p => new DateTime(p.PaidMonth.Year, p.PaidMonth.Month, 1))
            //    .ToHashSet();

            // Generate range of months to invoice
            var invoiceMonths = new List<DateTime>();

            for (var dt = model.FromMonth; dt <= model.ToMonth; dt = dt.AddMonths(1))
            {
                //if (paidMonths.Contains(dt))
                //{
                //    ViewData["Message"] = $"Month {dt:yyyy-MM} is already paid.";
                //    ViewData["MessageType"] = "error";
                //    return View(model);
                //}
                invoiceMonths.Add(dt);
            }

            // Valid invoice
            model.AmountDue = invoiceMonths.Count * model.AmountPerMonth;
            model.DueDate = DateTime.Now;

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
