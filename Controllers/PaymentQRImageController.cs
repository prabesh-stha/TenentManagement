using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Payment;
using TenentManagement.ViewModel;


namespace TenentManagement.Controllers
{
    public class PaymentQRImageController : Controller
    {

        private readonly PaymentQRImageService _paymentQRImageService;
        public PaymentQRImageController(PaymentQRImageService paymentQRImageService)
        {
            _paymentQRImageService = paymentQRImageService ?? throw new ArgumentNullException(nameof(paymentQRImageService));
        }
        [HttpGet]
        public IActionResult UploadQRImage(int ownerId)
        {

            var paymentMethods = _paymentQRImageService.GetAllAvailabeMethod(ownerId);
            if(paymentMethods.Count == 0)
            {
                TempData["Message"] = "All payments qr already exist!";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Profile", "Home");
            }
            var model = new UploadQRImageViewModel
            {
                PaymentMethods = paymentMethods,
                PaymentQRImage = new PaymentQRImageModel
                {
                    OwnerId = ownerId,
                }
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult UploadQRImage(UploadQRImageViewModel model, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if(!_paymentQRImageService.ContainsQRCode(imageFile))
                {
                    ViewData["Message"] = "The uploaded file is not a valid QR code image.";
                    ViewData["MessageType"] = "error";
                    model.PaymentMethods = _paymentQRImageService.GetAllAvailabeMethod(model.PaymentQRImage.OwnerId);
                    return View(model);
                }
                model.PaymentQRImage.ImageType = imageFile.ContentType;

                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    model.PaymentQRImage.ImageData = ms.ToArray();
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        int row = _paymentQRImageService.CreatePaymentQRImage(model.PaymentQRImage);
                        if (row <= 0)
                        {
                            ViewData["Message"] = "Failed to upload QR Image.";
                            ViewData["MessageType"] = "error";
                            model.PaymentMethods = _paymentQRImageService.GetAllAvailabeMethod(model.PaymentQRImage.OwnerId);
                            return View(model);
                        }
                        TempData["Message"] = "QR Image uploaded successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("ManageQRImage", "PaymentQRImage", new {ownerId = model.PaymentQRImage.OwnerId});
                    }
                    catch (Exception ex)
                    {
                        ViewData["Message"] = ex.Message;
                        ViewData["MessageType"] = "error";
                        model.PaymentMethods = _paymentQRImageService.GetAllAvailabeMethod(model.PaymentQRImage.OwnerId);
                        return View(model);
                    }
                }
                else
                {
                    model.PaymentMethods = _paymentQRImageService.GetAllAvailabeMethod(model.PaymentQRImage.OwnerId);
                    return View(model);
                }
            }
            else
            {
                ViewData["Message"] = "Please select a valid image file.";
                ViewData["MessageType"] = "error";
                return View(model);
            }
        }

        public IActionResult ManageQRImage(int ownerId)
        {
            var paymentInfo = _paymentQRImageService.GetAllAvailabeMethod(ownerId);
            ViewBag.ShowButton = paymentInfo.Count > 0;
            var model = _paymentQRImageService.GetAllQRImages(ownerId);
            return View(model);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var qr = _paymentQRImageService.GetPaymentQRImageById(id);
            if (qr == null)
            {
                return NotFound();
            }
            try
            {
                int row = _paymentQRImageService.DeleteQRImage(id);
                if (row > 0)
                {
                    TempData["Message"] = "QR image deleted successfully!";
                    TempData["MessageType"] = "success";
                    return Ok();
                }
                else
                {
                    ViewData["Message"] = "An error occurred while deleting the qr image.";
                    ViewData["MessageType"] = "error";
                    return View();
                }
            }
            catch
            {
                ViewData["Message"] = "An error occurred while deleting the qr image.";
                ViewData["MessageType"] = "error";
                return View();
            }
        }

        [HttpGet]
        public IActionResult UpdateQRImage(int id)
        {
            var qrImage = _paymentQRImageService.GetPaymentQRImageById(id);
            if (qrImage == null)
            {
                return NotFound();
            }
            return View(qrImage);
        }
        [HttpPost]
        public IActionResult UpdateQRImage(PaymentQRImageModel model, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!_paymentQRImageService.ContainsQRCode(imageFile))
                {
                    ViewData["Message"] = "The uploaded file is not a valid QR code image.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
                model.ImageType = imageFile.ContentType;
                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    model.ImageData = ms.ToArray();
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    int row = _paymentQRImageService.UpdateQRImage(model);
                    if (row <= 0)
                    {
                        ViewData["Message"] = "Failed to update QR Image.";
                        ViewData["MessageType"] = "error";
                        return View(model);
                    }
                    TempData["Message"] = "QR Image updated successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("ManageQRImage", new { ownerId = model.OwnerId });
                }
                catch (Exception ex)
                {
                    ViewData["Message"] = ex.Message;
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

    }
}
