using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Property.Unit;
using TenentManagement.Services.Property;
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
                    if (model.IsVacant == true)
                    {
                        model.RenterId = null;
                        model.RentEndDate = null;
                        model.RentAmount = null;
                        model.RentStartDate = null;
                    }
                    else
                    {
                        model.RentEndDate = model.RentStartDate?.AddMonths(model.RentDurationMonths) ?? null;
                    }

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
        public IActionResult RenterDetail(int id)
        {
            try
            {
                var result = _unitService.GetRentalDetail(id);
                if (result != null)
                {
                    return View(result);
                }
                else
                {
                    ViewData["Message"] = "Couldn't get the renter detail.";
                    ViewData["MessageType"] = "error";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ViewData["Message"] = "Couldn't get the renter detail.";
                ViewData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var unit = _unitService.GetUnitById(id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UnitModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsVacant == true)
                {
                    model.RenterId = null;
                    model.RentEndDate = null;
                    model.RentAmount = null;
                    model.RentStartDate = null;
                }
                else
                {
                    model.RentEndDate = model.RentEndDate?.AddMonths(model.RentDurationMonths) ?? null;
                }
                    try
                    {
                        int row = _unitService.UpdateUnit(model);
                        if (row > 0)
                        {
                            TempData["Message"] = "Unit detail updated successfully!";
                            TempData["MessageType"] = "success";
                            return RedirectToAction("Detail", "Property", new { id = model.PropertyId });
                        }
                        else
                        {
                            ViewData["Message"] = "An error occurred while updating the unit detail.";
                            ViewData["MessageType"] = "error";
                            return View(model);
                        }
                    }
                    catch
                    {
                        ViewData["Message"] = "An error occurred while updating the unit detail.";
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
            var unit = _unitService.GetUnitById(id);
            if (unit == null) return NotFound();
            try
            {
                int row = _unitService.DeleteUnit(id);
                if (row > 0)
                {
                    TempData["Message"] = "Unit deleted successfully!";
                    TempData["MessageType"] = "success";
                    return Ok();
                }
                else
                {
                    ViewData["Message"] = "An error occurred while deleting the unit.";
                    ViewData["MessageType"] = "error";
                    return View();
                }
            }
            catch
            {
                ViewData["Message"] = "An error occurred while deleting the unit.";
                ViewData["MessageType"] = "error";
                return View();
            }
        }
    }
}
