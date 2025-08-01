using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Property;
using TenentManagement.Models.Property.Utility;
using TenentManagement.Services.Payment;
using TenentManagement.Services.Property;
using TenentManagement.Services.Property.UtilityBill;
using TenentManagement.Services.Utilities;

namespace TenentManagement.Controllers
{

    [Authorize]
    [BlockDirectAccess]
    public class UtilityBillController : Controller
    {
        private readonly UtilityBillService _utilityBillService;
        private readonly PropertyService _propertyService;
        private readonly UtilityBillImageService _utilityBillImageService;
        public UtilityBillController(UtilityBillService utilityBillService, PropertyService propertyService, UtilityBillImageService utilityBillImageService)
        {
            _utilityBillService = utilityBillService;
            _propertyService = propertyService;
            _utilityBillImageService = utilityBillImageService;
        }


        [HttpGet]
        public IActionResult UtilityBills()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Authentication");
            var utilityBills = _utilityBillService.GetAllUtilityBill(userId.Value);
            return View(utilityBills);
        }
        [HttpGet]
        public IActionResult UploadBill()
        {
            var utilityTypes = _utilityBillService.GetUtilityBills();
            UtilityBillModel utilityBillModel = new UtilityBillModel();
            var userId = HttpContext.Session.GetInt32("UserId");
            if(!userId.HasValue) return NotFound();
            utilityBillModel.OwnerPropertyName = _propertyService.GetPropertiesOfUser(userId.Value);
            utilityBillModel.UtilityTypes = utilityTypes;
            utilityBillModel.UserId = userId.Value;
            return View(utilityBillModel);
        }

        [HttpPost]
        public IActionResult UploadBill(UtilityBillModel utilityBillModel, IFormFile? billImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (billImage != null && billImage.Length > 0)
                    {
                        UtilityBillImageModel utilityBillImageModel = new UtilityBillImageModel
                        {
                            ImageType = billImage.ContentType,
                        };
                        using (var ms = new MemoryStream())
                        {
                            billImage.CopyTo(ms);
                            utilityBillImageModel.ImageData = ms.ToArray();
                        }
                        utilityBillModel.UtilityBillImage = utilityBillImageModel;
                    }
                    else
                    {
                        utilityBillModel.UtilityBillImage = null;
                    }
                    int rowAffected = _utilityBillService.UploadUtilityBill(utilityBillModel);
                    if (rowAffected == 1)
                    {
                        TempData["Message"] = "Utility bill uploaded successfully.";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {
                    ViewData["Message"] = $"An error occurred while uploading the bill: {ex.Message}";
                    ViewData["MessageType"] = "error";
                    utilityBillModel.UtilityTypes = _utilityBillService.GetUtilityBills();
                    utilityBillModel.OwnerPropertyName = _propertyService.GetPropertiesOfUser(utilityBillModel.UserId);
                    return View(utilityBillModel);
                }
            }
            utilityBillModel.UtilityTypes = _utilityBillService.GetUtilityBills();
            utilityBillModel.OwnerPropertyName = _propertyService.GetPropertiesOfUser(utilityBillModel.UserId);
            return View(utilityBillModel);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                int row = _utilityBillService.DeleteUtilityBill(id);
                if (row == 1)
                {
                    TempData["Message"] = "Utility bill deleted successfully!";
                    TempData["MessageType"] = "success";
                    return Ok();
                }
                else
                {
                    ViewData["Message"] = "An error occurred while deleting the utility bill.";
                    ViewData["MessageType"] = "error";
                    return View();
                }
            }
            catch
            {
                ViewData["Message"] = "An error occurred while deleting the utility bill.";
                ViewData["MessageType"] = "error";
                return View();
            }
        }

        [HttpGet]
        public IActionResult EditBill(int id)
        {
            var utilityBill = _utilityBillService.GetUtilityBillById(id);
            if (utilityBill == null) return NotFound();
            utilityBill.UtilityBillImage = _utilityBillImageService.GetUtilityBillImageById(0,id);
            return View(utilityBill);
        }

        [HttpPost]
        public IActionResult EditBill(UtilityBillModel utilityBill, IFormFile? billImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (billImage != null && billImage.Length > 0)
                    {
                        UtilityBillImageModel billImageModel = new UtilityBillImageModel
                        {
                            ImageType = billImage.ContentType,
                        };
                        using (var ms = new MemoryStream())
                        {
                            billImage.CopyTo(ms);
                            billImageModel.ImageData = ms.ToArray();
                        }
                        utilityBill.UtilityBillImage = billImageModel;
                    }
                    int row = _utilityBillService.UpdateUtilityBill(utilityBill);
                    if (row == 1)
                    {
                        TempData["Message"] = "Utility bill updated successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("UtilityBills", "UtilityBill");
                    }
                    else
                    {
                        ViewData["Message"] = "An error occurred while updating the utility bill.";
                        ViewData["MessageType"] = "error";
                        return View(utilityBill);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while updating the utility bill.";
                    ViewData["MessageType"] = "error";
                    return View(utilityBill);
                }
            }
            else
            {
                return View(utilityBill);
            }
        }


    }
}
