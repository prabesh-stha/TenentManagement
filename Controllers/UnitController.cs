using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Property.Unit;
using TenentManagement.Services.Property;
using TenentManagement.Services.Property.Unit;
using TenentManagement.Services.User;

namespace TenentManagement.Controllers
{
    public class UnitController : Controller
    {
        private readonly UnitService _unitService;
        private readonly UserImageService _userImageService;
        public UnitController(UnitService unitService, UserImageService userImageService)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _userImageService = userImageService ?? throw new ArgumentNullException(nameof(userImageService));

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
                    var userImage = _userImageService.GetUserImage(result.OwnerId);
                    result.UserImage = userImage;
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

        public IActionResult Detail(int id)
        {
            try
            {
                var result = _unitService.GetOwnerUnitDetail(id);
                if (result != null)
                {
                    var userImage = _userImageService.GetUserImage(result.RenterId ?? 0);
                    result.UserImage = userImage;
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

        [HttpPost]
        public IActionResult RemoveTenent(int id)
        {
            var unit = _unitService.GetUnitById(id);
            if (unit == null) return NotFound();
            try
            {
                var updatedUnit = new UnitModel
                {
                    Id = id,
                    RentStartDate = null,
                    RentEndDate = null,
                    RenterId = null,
                    IsVacant = true
                };
                int row = _unitService.UpdateUnit(updatedUnit);
                if (row > 0)
                {
                    TempData["Message"] = "Tenent removed successfully!";
                    TempData["MessageType"] = "success";
                    return Ok();
                }
                else
                {
                    ViewData["Message"] = "An error occurred while removing tenent.";
                    ViewData["MessageType"] = "error";
                    return View();
                }
            }
            catch
            {
                ViewData["Message"] = "An error occurred while removing tenent.";
                ViewData["MessageType"] = "error";
                return View();
            }
        }


        [HttpGet]
        public JsonResult UnitSelection(int propertyId)
        {
            var units = _unitService.GetAllUnits(propertyId)
                .Select(u => new {
                    id = u.Id,
                    unitName = u.Name,
                    renterId = u.RenterId
                }).ToList();

            return Json(units);
        }
    }
}
