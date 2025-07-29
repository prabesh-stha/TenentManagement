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
        private readonly PaymentQRImageService _paymentQRService;
        private readonly PaymentProofService _paymentProofService;
        public PaymentInvoiceController(UnitService unitService, PaymentInvoiceService paymentInvoiceService, PaymentService paymentService, PaymentQRImageService paymentQRService, PaymentProofService paymentProofService)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _paymentInvoiceService = paymentInvoiceService ?? throw new ArgumentNullException(nameof(paymentInvoiceService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentQRService = paymentQRService ?? throw new ArgumentNullException(nameof(paymentQRService));
            _paymentProofService = paymentProofService ?? throw new ArgumentNullException(nameof(paymentProofService));
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
            var invoice = _paymentInvoiceService.GetLatestMonth(unitId);
            var excludedMonths = new List<DateTime>();

            if (invoice != null)
            {
                var latestInvoiceMonth = new DateTime(invoice.ToMonth.Year, invoice.ToMonth.Month, 1);

                for (var dt = rentStart; dt <= latestInvoiceMonth; dt = dt.AddMonths(1))
                {
                    excludedMonths.Add(dt);
                }
            }

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
                model.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                model.PaymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
                return View(model);
                }


            //var payment = _paymentService.GetAllPaymentByUnit(model.UnitId);
            //var paidMonths = payment
            //    .Select(p => new DateTime(p.PaidMonth.Year, p.PaidMonth.Month, 1))
            //    .ToHashSet();

            //// Generate range of months to invoice
            //var invoiceMonths = new List<DateTime>();

            //for (var dt = model.FromMonth; dt <= model.ToMonth; dt = dt.AddMonths(1))
            //{
            //    if (paidMonths.Contains(dt))
            //    {
            //        ViewData["Message"] = $"Month {dt:yyyy-MM} is already paid.";
            //        ViewData["MessageType"] = "error";
            //        return View(model);
            //    }
            //    invoiceMonths.Add(dt);
            //}

            // Valid invoice
            //model.AmountDue = invoiceMonths.Count - 1 * model.AmountPerMonth;


                try {
                if (model.PaymentMethodId == 1 && model.StatusId == 2)
                {
                    model.IsVerified = true;
                    model.VerifiedAt = DateTime.UtcNow;
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
                                PaymentDate = DateTime.UtcNow,
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
                    model.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                    model.PaymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
                    return View(model);
                    }
                }
                catch(Exception ex)
                {
                    ViewData["Message"] = $"An error occurred while creating the invoice: {ex.Message}";
                    ViewData["MessageType"] = "error";
                model.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                model.PaymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
                return View(model);
                }
        }

        [HttpGet]
        public IActionResult UnitInvoices(int id, int page = 1)
        {
            //var paymentInvoice = _paymentInvoiceService.GetAllInvoiceOfUnit(id);
            const int pageSize = 10;

            var allInvoices = _paymentInvoiceService.GetAllInvoiceOfUnit(id);
            var totalInvoices = allInvoices.Count;
            var unitName = _unitService.GetUnitName(id);
            var invoices = allInvoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var model = new PaymentInvoiceListViewModel
            {
                UnitId = id,
                UnitName = unitName ?? "",
                PaymentInvoices = invoices,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalInvoices / (double)pageSize)
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {

            var paymentInvoice = _paymentInvoiceService.GetPaymentInvoiceById(id);
            var paymentProof = _paymentProofService.GetPaymentProofImage(id);
            if(paymentInvoice == null)
            {
                return NotFound();
            }
            var invoiceForLatestMonth = _paymentInvoiceService.GetLatestMonth(paymentInvoice.UnitId);
            if(invoiceForLatestMonth != null)
            {
                if (invoiceForLatestMonth.FromMonth > paymentInvoice.FromMonth)
                {
                    return BadRequest("Cannot delete this invoice before deleting the latest invoice.");
                }
            }
            if(paymentProof != null)
            {
                return BadRequest("Cannot delete this invoice once payment proof is uploaded.");
            }
            try
            {
                int row = _paymentInvoiceService.DeletePaymentInvoice(id);
                if (row > 0)
                {
                    TempData["Message"] = "Payment invoice deleted successfully!";
                    TempData["MessageType"] = "success";
                    return Ok();
                }
                else
                {
                    ViewData["Message"] = "An error occurred while deleting the property.";
                    ViewData["MessageType"] = "error";
                    return View();
                }
            }
            catch
            {
                ViewData["Message"] = "An error occurred while deleting the property.";
                ViewData["MessageType"] = "error";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, bool all = false)
        {

            var payInvoice = _paymentInvoiceService.GetPaymentInvoiceById(id);
            if (payInvoice == null) return NotFound();
            var unit = _unitService.GetUnitById(payInvoice.UnitId);
            var paymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
            var paymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();    
            payInvoice.AmountPerMonth = unit?.RentAmount ?? 0;
            payInvoice.PaymentMethods = paymentMethods;
            payInvoice.PaymentStatuses = paymentStatuses;
            payInvoice.PaymentProof = _paymentProofService.GetPaymentProofImage(id);
            ViewData["All"] = all;
            return View(payInvoice);
        }

        [HttpPost]
        public IActionResult Edit(PaymentInvoiceModel model, bool all = false)
        {
                try
                {
                    int row = _paymentInvoiceService.UpdatePaymentInvoice(model);
                    if(row > 0)
                    {
                        if (model.StatusId == 2)
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
                                    PaymentDate = DateTime.UtcNow,
                                    PaidMonth = month,
                                    Amount = model.AmountDue / months.Count,
                                    InvoiceId = model.Id
                                });
                            }
                        }
                        TempData["Message"] = "Invoice updated successfully!";
                        TempData["MessageType"] = "success";
                        if (all)
                        {
                        return RedirectToAction("AllInvoice", "PaymentInvoice", new { id = model.OwnerId });
                        }
                        else
                        {
                        return RedirectToAction("UnitInvoices", "PaymentInvoice", new { id = model.UnitId });
                        }
                    }
                    else
                    {
                        ViewData["Message"] = "An error occurred while updating the payment invoice.";
                        ViewData["MessageType"] = "error";
                        model.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                        model.PaymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
                        ViewData["All"] = all;
                        return View(model);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while updating the payment invoice.";
                    ViewData["MessageType"] = "error";
                model.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                model.PaymentMethods = _paymentInvoiceService.GetAllPaymentMethod();
                ViewData["All"] = all;
                return View(model);
                }
            }

        [HttpGet]
        public IActionResult RenterInvoices(int unitId, int renterId, int page = 1)
        {
            const int pageSize = 10;
            var allInvoices = _paymentInvoiceService.GetAllInvoiceOfRenter(unitId, renterId);
            var totalInvoices = allInvoices.Count;
            var unitName = _unitService.GetUnitName(unitId);
            var invoices = allInvoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var model = new RenterPaymentInvoiceListViewModel
            {
                UnitId = unitId,
                UnitName = unitName ?? "",
                RenterId = renterId,
                PaymentInvoices = invoices,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalInvoices / (double)pageSize)
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult RenterInvoicePayment(int id, bool all = false)
        {
            var payInvoice = _paymentInvoiceService.GetPaymentInvoiceById(id);
            if (payInvoice == null) return NotFound();
            var unit = _unitService.GetUnitById(payInvoice.UnitId);
            var payMethods = _paymentInvoiceService.GetAllPaymentMethod();
            payInvoice.AmountPerMonth = unit?.RentAmount ?? 0;
            payInvoice.PaymentMethods = payMethods;
            payInvoice.PaymentQRImage = _paymentQRService.GetPaymentQRByOwnerIdAndPaymentId(payInvoice.OwnerId, payInvoice.PaymentMethodId);
            payInvoice.PaymentProof = _paymentProofService.GetPaymentProofImage(payInvoice.Id);
            ViewData["All"] = all;
            return View(payInvoice);
        }
        [HttpPost]
        public IActionResult RenterInvoicePayment(PaymentInvoiceModel model, IFormFile imageFile, bool all = false)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var image = new PaymentProofModel();
                image.ImageType = imageFile.ContentType;

                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    image.ImageData = ms.ToArray();
                }
                image.InvoiceId = model.Id;
                image.OwnerId = model.OwnerId;
                image.PaymentMethodId = model.PaymentMethodId;
                try
                {
                    int row = _paymentProofService.CreatePaymentProofImage(image);
                    if (row < 0)
                    {
                        ViewData["Message"] = "Error while uploading payment proof";
                        ViewData["MessageType"] = "error";
                        ViewData["All"] = all;
                        return View(model);
                    }
                }
                catch
                {
                    ViewData["Message"] = "Error while uploading payment proof";
                    ViewData["MessageType"] = "error";
                    ViewData["All"] = all;
                    return View(model);
                }
            }
            else
            {
                ViewData["Message"] = "Please upload valid payment proof!";
                ViewData["MessageType"] = "error";
                ViewData["All"] = all;
                return View(model);
            }
                try
                {
                    model.StatusId = 1;
                    int row = _paymentInvoiceService.UpdatePaymentInvoice(model);
                    if (row > 0)
                    {
                        TempData["Message"] = "Payment Invoice sent for verification!";
                        TempData["MessageType"] = "success";
                    if (all)
                    {
                        return RedirectToAction("AllInvoice", "PaymentInvoice", new { id = model.RenterId, tab = "rented" });

                    }
                    else
                    {
                        return RedirectToAction("RenterInvoices", "PaymentInvoice", new { unitId = model.UnitId, renterId = model.RenterId });
                    }
                    }
                    else
                    {
                        ViewData["Message"] = "An error occurred while updating the payment invoice.";
                        ViewData["MessageType"] = "error";
                        return View(model);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while updating the payment invoice.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
        }

        [HttpGet]
        public IActionResult ValidatePayment(int id, bool all = false)
        {
            var payInvoice = _paymentInvoiceService.GetPaymentInvoiceById(id);
            if (payInvoice == null) return NotFound();
            var unit = _unitService.GetUnitById(payInvoice.UnitId);
            var paymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
            payInvoice.AmountPerMonth = unit?.RentAmount ?? 0;
            payInvoice.PaymentStatuses = paymentStatuses;
            payInvoice.PaymentProof = _paymentProofService.GetPaymentProofImage(id);
            ViewData["All"] = all;
            return View(payInvoice);
        }

        [HttpPost]
        public IActionResult ValidatePayment(PaymentInvoiceModel payInvoice, bool all = false)
        {
            try
            {
                int row = _paymentInvoiceService.UpdatePaymentInvoice(payInvoice);
                if(row > 0)
                {
                    if (payInvoice.IsVerified)
                    {
                        TempData["Message"] = "Payment validated successfully!";
                        TempData["MessageType"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = "Payment rejected successfully!";
                        TempData["MessageType"] = "success";
                    }
                    if (all)
                    {
                        return RedirectToAction("AllInvoice", "PaymentInvoice", new { id = payInvoice.OwnerId, tab = "owned" });

                    }
                    else
                    {
                        return RedirectToAction("UnitInvoices", "PaymentInvoice", new { id = payInvoice.UnitId });
                    }
                }
                else
                {
                    ViewData["Message"] = "Failed while updating payment invoice!";
                    ViewData["MessageType"] = "error";
                    payInvoice.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                    ViewData["All"] = all;
                    return View(payInvoice);
                }
            }
            catch
            {
                ViewData["Message"] = "Failed while updating payment invoice!";
                ViewData["MessageType"] = "error";
                payInvoice.PaymentStatuses = _paymentInvoiceService.GetAllPaymentStatus();
                ViewData["All"] = all;
                return View(payInvoice);
            }
        }

        [HttpGet]
        public IActionResult Detail(int id, bool all = false)
        {
            var payInvoice = _paymentInvoiceService.GetPaymentInvoiceById(id);
            if (payInvoice == null) return NotFound();
            payInvoice.PaymentProof = _paymentProofService.GetPaymentProofImage(id);
            ViewData["All"] = all;
            return View(payInvoice);
        }


        public IActionResult AllInvoice(int id, int page = 1, string tab = "owned")
        {
            const int pageSize = 10;
            bool isOwner = false;

            List<PaymentInvoiceModel> invoiceQuery;

            switch (tab.ToLowerInvariant())
            {
                case "owned":
                    invoiceQuery = _paymentInvoiceService.GetAllOwnedPropertyInvoice(id);
                    isOwner = true;
                    break;

                case "rented":
                    invoiceQuery = _paymentInvoiceService.GetAllRentedPropertyInvoice(id);
                    isOwner = false;
                    break;

                default:
                    invoiceQuery = new List<PaymentInvoiceModel>();
                    break;
            }

            var totalInvoices = invoiceQuery.Count();
            var invoices = invoiceQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PaymentInvoiceListViewModel
            {
                PaymentInvoices = invoices,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalInvoices / (double)pageSize),
                IsOwner = isOwner
            };

            ViewBag.ActiveTab = tab;
            return View(model);
        }
    }
}
