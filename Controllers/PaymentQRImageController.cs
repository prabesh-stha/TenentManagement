using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Payment;


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
        public IActionResult UploadQRImage(int ownerId, int paymentId)
        {
            var model = new PaymentQRImageModel
            {
                OwnerId = ownerId,
                PaymentMethodId = paymentId
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult UploadQRImage(PaymentQRImageModel model, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                model.ImageType = imageFile.ContentType;

                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    model.ImageData = ms.ToArray();
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        int row = _paymentQRImageService.CreatePaymentQRImage(model);
                        if (row <= 0)
                        {
                            ViewData["Message"] = "Failed to upload QR Image.";
                            ViewData["MessageType"] = "error";
                            return View(model);
                        }
                        TempData["Message"] = "QR Image uploaded successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Profile", "Home");
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
            else
            {
                ViewData["Message"] = "Please select a valid image file.";
                ViewData["MessageType"] = "error";
                return View(model);
            }
        }

        public IActionResult GetImage(int ownerId)
        {
            var image = _paymentQRImageService.GetPaymentQRImage(ownerId);
            return View(image);
        }
    }
}
