using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using TenentManagement.Models.Property;
using TenentManagement.Services.Property;
using TenentManagement.Services.Property.Unit;
using TenentManagement.Services.Utilities;
using TenentManagement.ViewModel;


namespace TenentManagement.Controllers
{
    [BlockDirectAccess]
    [Authorize]
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;
        private readonly UnitService _unitService;
        private readonly PropertyImageService _propertyImageService;
        public PropertyController
            (
            PropertyService propertyService
            , UnitService unitService
            , PropertyImageService propertyImageService
            )
        {
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _propertyImageService = propertyImageService ?? throw new ArgumentNullException(nameof(propertyImageService));
        }
        [HttpGet]
        public IActionResult Create(int id)
        {
            var propertyTypes = _propertyService.GetAllPropertyTypes();

            PropertyModel model = new()
            {
                UserId = id,
                PropertyTypes = propertyTypes
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(PropertyModel property, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        PropertyImageModel propertyImageModel = new PropertyImageModel
                        {
                            ImageType = imageFile.ContentType,
                        };
                        using (var ms = new MemoryStream())
                        {
                            imageFile.CopyTo(ms);
                            propertyImageModel.ImageData = ms.ToArray();
                        }
                        property.PropertyImage = propertyImageModel;
                    }
                    string result = _propertyService.CreateProperty(property);
                    if (result == "success")
                    {
                        TempData["Message"] = "Property created successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["Message"] = result;
                        ViewData["MessageType"] = "error";
                        property.PropertyTypes = _propertyService.GetAllPropertyTypes();
                        return View(property);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while creating the property.";
                    ViewData["MessageType"] = "error";
                    property.PropertyTypes = _propertyService.GetAllPropertyTypes();
                    return View(property);
                }
            }
            else
            {
                property.PropertyTypes = _propertyService.GetAllPropertyTypes();
                return View(property);
            }
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            try
            {
                PropertyDetailViewModel result = _propertyService.GetPropertyAndUnitDetail(id,null);
                if (result != null)
                {
                    return View(result);
                }
                else
                {
                    ViewData["Message"] = "Couldn't get the detail.";
                    ViewData["MessageType"] = "error";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ViewData["Message"] = "Couldn't get the detail.";
                ViewData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var property = _propertyService.GetPropertyDetail(id);
            if (property == null) return NotFound();
            property.PropertyImage = _propertyImageService.GetPropertyImage(id);
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PropertyModel model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        PropertyImageModel propertyImageModel = new PropertyImageModel
                        {
                            ImageType = imageFile.ContentType,
                        };
                        using (var ms = new MemoryStream())
                        {
                            imageFile.CopyTo(ms);
                            propertyImageModel.ImageData = ms.ToArray();
                        }
                        model.PropertyImage = propertyImageModel;
                    }
                    int row = _propertyService.UpdateProperty(model);
                    if (row == 1)
                    {
                        TempData["Message"] = "Property detail updated successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["Message"] = "An error occurred while updating the property detail.";
                        ViewData["MessageType"] = "error";
                        return View(model);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while updating the property detail.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }

        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var property = _propertyService.GetPropertyDetail(id);
            if (property == null) return NotFound();
            try
            {
                int row = _propertyService.DeleteProperty(id);
                if (row == 1)
                {
                    TempData["Message"] = "Property deleted successfully!";
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
        public IActionResult RentedPropertyDetail(int propertyId, int renterId)
        {
            try
            {
                PropertyDetailViewModel result = _propertyService.GetPropertyAndUnitDetail(propertyId, renterId);
                if (result != null)
                {
                    if(result.Property.OwnerName != null)
                    {
                        result.Property.OwnerName = result.Property.OwnerName.ToTitleCase();
                    }
                    return View(result);
                }
                else
                {
                    ViewData["Message"] = "Couldn't get the detail.";
                    ViewData["MessageType"] = "error";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ViewData["Message"] = "Couldn't get the detail.";
                ViewData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
