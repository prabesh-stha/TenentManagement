using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TenentManagement.Models;
using TenentManagement.Services.Property;
using TenentManagement.Services.Property.Unit;
using TenentManagement.Services.User;
using TenentManagement.ViewModel;

namespace TenentManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PropertyService _propertyService;
        private readonly UserService _userService;

        public HomeController
            (
            ILogger<HomeController> logger
            ,PropertyService propertyService
            , UserService userService
            )
        {
            _logger = logger;
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning("Unauthorized access attempt - no user ID in session");
                return RedirectToAction("Login", "Authentication");
            }
            var owned = _propertyService.GetAllProperty(userId.Value);
            var rented = _propertyService.GetAllRentedProperty(userId.Value);
            return View(Tuple.Create(owned, rented));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning("Unauthorized access attempt - no user ID in session");
                return RedirectToAction("Login", "Authentication");
            }
            var user = _userService.GetProfile(userId.Value);
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
