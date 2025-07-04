using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Property.Unit;
using TenentManagement.Services.Property.Unit;

namespace TenentManagement.Controllers
{
    public class UnitController : Controller
    {
        private readonly UnitService _unitService;
        public UnitController(UnitService unitService)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
        }
        [HttpGet]
        public IActionResult Create(int id)
        {
            UnitModel model = new()
            {
                PropertyId = id
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(UnitModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitService.CreateUnit(model);
                    
                        TempData["Message"] = "Unit created successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Detail", "Property", new { id = model.PropertyId });
                    
                }
                catch(Exception e)
                {
                    ViewData["Message"] = e.Message;
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
