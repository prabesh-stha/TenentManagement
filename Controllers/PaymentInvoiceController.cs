using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;
using TenentManagement.Services.Payment;
using TenentManagement.Services.Property.Unit;
using TenentManagement.Services.Utilities;
using TenentManagement.ViewModel;

namespace TenentManagement.Controllers
{
    [BlockDirectAccess]
    [Authorize]
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
            var paymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
            var paymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
            if (unit == null)
            {
                TempData["Message"] = "Unit not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            //var payment = _paymentService.GetAllPaymentByUnit(unitId);
            //var paidMonths = payment
            //    .Select(p => new DateTime(p.PaidMonth.Year, p.PaidMonth.Month, 1))
            //    .ToList();
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

            //var monthsExceptPaidMonth = allMonths.Except(paidMonths).ToList();
            var invoice = _paymentInvoiceService.GetLatestMonth(unitId);
            var excludedMonths = new List<DateTime>();

            if (invoice != null)
            {
                // Get all months up to the latest invoice's ToMonth
                var latestInvoiceMonth = new DateTime(invoice.ToMonth.Year, invoice.ToMonth.Month, 1);

                // Generate all months from rent start to the latest invoice month
                for (var dt = rentStart; dt <= latestInvoiceMonth; dt = dt.AddMonths(1))
                {
                    excludedMonths.Add(dt);
                }
            }

            // Now exclude these months from allMonths
            var monthsExcludingExisting = allMonths
                .Except(excludedMonths)
                .OrderBy(m => m)
                .ToList();

            var availableMonths = monthsExcludingExisting.OrderBy(m => m).Where(m => m > monthsExcludingExisting.First()).ToList();
            if(availableMonths.Count <= 0)
            {
                TempData["Message"] = "Already created available invoices";
                TempData["MessageType"] = "error";
                return RedirectToAction("Detail", "Unit", new {id = unitId} );
            }

            var model = new PaymentInvoiceModel
            {
                FromMonth = monthsExcludingExisting.First(),
                UnitId = unitId,
                AmountPerMonth = unit.RentAmount ?? 0,
                RenterId = unit.RenterId ?? 0,
                OwnerId = userId,
                AvailableMonth = availableMonths,
                DueDate = new DateTime(unit.RentStartDate.Value.Year, unit.RentStartDate.Value.Month, unit.RentStartDate.Value.Day),
                PaymentMethods = paymentMethods,
                PaymentStatuses = paymentStatuses
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
                if (model.PaymentMethodId == 1 && model.StatusId == 2)
                {
                    model.IsVerified = true;
                    model.VerifiedAt = DateTime.Now;
                }
                    int row = _paymentInvoiceService.CreatePaymentInvoice(model);
                    if (row > 0)
                    {
                        TempData["Message"] = "Invoice created successfully!";
                        TempData["MessageType"] = "success";

                    if (model.PaymentMethodId == 1 && model.StatusId == 2)
                    {
                        var months = new List<DateTime>();
                        for (var dt = model.FromMonth; dt < model.ToMonth; dt = dt.AddMonths(1))
                        {
                            months.Add(new DateTime(dt.Year, dt.Month, 1));
                        }

                        foreach (var month in months)
                        {
                            _paymentService.CreatePayment(new PaymentModel
                            {
                                UnitId = model.UnitId,
                                PaymentDate = DateTime.Now,
                                PaidMonth = month,
                                Amount = model.AmountDue / months.Count,
                                InvoiceId = row
                            });
                        }
                    }
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

        [HttpGet]
        public IActionResult AllInvoice(int id)
        {
            var paymentInvoice = _paymentInvoiceService.GetAllInvoiceOfUnit(id);
            var model = new PaymentInvoiceListViewModel
            {
                UnitId = id,
                PaymentInvoices = paymentInvoice
            };
            return View(model);
        }
    }
}
