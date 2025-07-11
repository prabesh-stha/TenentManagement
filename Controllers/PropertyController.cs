using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Property;
using TenentManagement.Services.Property;
using TenentManagement.Services.Property.Unit;
using TenentManagement.ViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TenentManagement.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;
        private readonly UnitService _unitService;
        public PropertyController
            (
            PropertyService propertyService
            , UnitService unitService
            )
        {
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService)); 
        }
        [HttpGet]
        public IActionResult Create(int id)
        {
            var propertyTypes = _propertyService.GetAllPropertyTypes();

            PropertyModel model = new()
            {
                UserId = id,
                // Store the property types in ViewData to pass to the view
                PropertyTypes = propertyTypes
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(PropertyModel property)
        {
            if (ModelState.IsValid)
            {
                try
                {
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
                        var propertyTypes = _propertyService.GetAllPropertyTypes();
                        return View(property);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while creating the property.";
                    ViewData["MessageType"] = "error";
                    var propertyTypes = _propertyService.GetAllPropertyTypes();
                    return View(property);
                }
            }
            else
            {
                var propertyTypes = _propertyService.GetAllPropertyTypes();
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

        //[HttpGet]
        //public IActionResult Detail(int id)
        //{
        //    try
        //    {
        //        var userId = HttpContext.Session.GetInt32("UserId");
        //        if (userId == null)
        //        {
        //            TempData["Message"] = "Please log in to view property details.";
        //            TempData["MessageType"] = "error";
        //            return RedirectToAction("Login", "Authentication");
        //        }

        //        PropertyDetailViewModel result = _propertyService.GetPropertyDetail(id);

        //        if (result == null)
        //        {
        //            TempData["Message"] = "Couldn't get the property detail.";
        //            TempData["MessageType"] = "error";
        //            return RedirectToAction("Index", "Home");
        //        }

        //        // 🚫 Ownership check
        //        if (result.Property.UserId != userId)
        //        {
        //            TempData["Message"] = "You are not authorized to view this property.";
        //            TempData["MessageType"] = "error";
        //            return RedirectToAction("Index", "Home");
        //        }

        //        return View(result);
        //    }
        //    catch
        //    {
        //        TempData["Message"] = "An error occurred while getting the property detail.";
        //        TempData["MessageType"] = "error";
        //        return RedirectToAction("Index", "Home");
        //    }
        //}




        [HttpGet]
        public IActionResult Edit(int id)
        {
            var property = _propertyService.GetPropertyDetail(id);
            if (property == null) return NotFound();
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PropertyModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int row = _propertyService.UpdateProperty(model);
                    if (row > 0)
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
                if (row > 0)
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
