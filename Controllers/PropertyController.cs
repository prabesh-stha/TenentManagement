using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Property;
using TenentManagement.Services.Property;
using TenentManagement.ViewModel;

namespace TenentManagement.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;
        public PropertyController(PropertyService propertyService)
        {
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
        }
        [HttpGet]
        public IActionResult Create(int id)
        {
            PropertyModel model = new()
            {
                UserId = id
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
                        return View(property);
                    }
                }
                catch
                {
                    ViewData["Message"] = "An error occurred while creating the property.";
                    ViewData["MessageType"] = "error";
                    return View(property);
                }
            }
            else
            {
                return View(property);
            }
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            try
            {
                PropertyDetailViewModel result = _propertyService.GetPropertyDetail(id);
                if(result != null)
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
