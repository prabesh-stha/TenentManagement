using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.User;
using TenentManagement.Services.User;

namespace TenentManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly UserImageService _userImageService;
        private readonly UserService _userService;
        public UserController(UserImageService userImageService, UserService userService)
        {
            _userImageService = userImageService;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }
            var user = _userService.GetProfile(userId.Value);
            user.UserImage = _userImageService.GetUserImage(userId.Value);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfileImage(UserModel model, IFormFile profileImage)
        {
            try
            {
                if (profileImage == null || profileImage.Length == 0)
                {
                    TempData["Message"] = "No image file provided";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Profile", "User"); // Or whatever your profile view action is
                }

                // Validate file type and size
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Message"] = "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Profile", "User");
                }

                if (profileImage.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    TempData["Message"] = "File size exceeds 5MB limit";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Profile", "User");
                }
                //if (string.IsNullOrEmpty(model.Id))
                //{
                //    TempData["Message"] = "User not authenticated";
                //    TempData["MessageType"] = "error";
                //    return RedirectToAction("Profile", "User");
                //}
                
                if(profileImage != null && profileImage.Length > 0)
                {
                    var userImage = new UserImageModel();
                    userImage.ImageType = profileImage.ContentType;

                    using (var ms = new MemoryStream())
                    {
                        profileImage.CopyTo(ms);
                        userImage.ImageData = ms.ToArray();
                    }
                    userImage.UserId = model.Id;
                    int row = _userImageService.UploadUserImage(userImage);
                    if(row > 0)
                    {
                        HttpContext.Session.SetString("UserImage", $"data:{userImage.ImageType};base64,{Convert.ToBase64String(userImage.ImageData)}");
                        TempData["Message"] = "Profile image updated successfully";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Profile", "User");
                    }
                    else
                    {
                        TempData["Message"] = "Failed to update profile image in database";
                        TempData["MessageType"] = "error";
                        return RedirectToAction("Profile", "User");
                    }

                }
                else
                {
                    TempData["Message"] = "Failed to update profile image in database";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Profile", "User");
                }
            }
            catch
            {
                TempData["Message"] = "Failed to update profile image in database";
                TempData["MessageType"] = "error";
                return RedirectToAction("Profile", "User");
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _userService.GetProfile(id);
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(UserModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    int row = _userService.UpdateProfile(model);
                    if (row > 0)
                    {
                        TempData["Message"] = "Profile updated successfully";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Profile", "User");
                    }
                    else
                    {
                        ViewData["Message"] = "Failed to update profile";
                        ViewData["MessageType"] = "error";
                        return View(model);
                    }
                }
                catch
                {
                    ViewData["Message"] = "Failed to update profile";
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
