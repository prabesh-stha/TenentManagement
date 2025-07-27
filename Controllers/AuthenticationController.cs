using Microsoft.AspNetCore.Mvc;
using TenentManagement.Models.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using AuthenticationService = TenentManagement.Services.Authentication.AuthenticationService;
using TenentManagement.Services.Authentication;
using TenentManagement.Models.Authentication.EmailVerification;
using TenentManagement.Services.Mail;
using Microsoft.AspNetCore.Authorization;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Payment;
using TenentManagement.Models.User;
using TenentManagement.Services.User;

namespace TenentManagement.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly AuthenticationService _authenticationService;
        private readonly MailService _mailService;
        private readonly UserImageService _userImageService;
        public AuthenticationController(AuthenticationService authenticationService, MailService mailService, UserImageService userImageService)
        {
            _authenticationService = authenticationService;
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _userImageService = userImageService ?? throw new ArgumentNullException(nameof(userImageService));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterModel model, IFormFile imageFile)
        {
            // Validate image first
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewData["Message"] = "Please upload a valid user image";
                ViewData["MessageType"] = "error";
                return View(model);
            }

            // Process image
            byte[] imageData;
            string imageType;
            try
            {
                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    imageData = ms.ToArray();
                }
                imageType = imageFile.ContentType;
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "Error processing image: " + ex.Message;
                ViewData["MessageType"] = "error";
                return View(model);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    model.UserImage = new UserImageModel
                    {
                        ImageData = imageData,
                        ImageType = imageType
                    };
                    var registrationResult = _authenticationService.RegisterUser(model);
                    if (registrationResult.STATUS == "SUCCESS")
                    {
                        try
                        {
                            TokenModel? emailVerificationModel = _authenticationService.GenerateAndStoreEmailVerificationToken(model.Email.ToLower());
                            if (emailVerificationModel != null)
                            {

                                var verificationLink = Url.Action("VerifyEmail", "Authentication", new { token = emailVerificationModel.Token, email = model.Email.ToLower() }, Request.Scheme);
                                _mailService.Send(model.Email.ToLower(), "Verify your email.", $"Click here: {verificationLink}");
                                TempData["Message"] = $"{registrationResult.MSG} Check your {model.Email.ToLower()} email for verification.";
                                TempData["MessageType"] = "success";
                                return RedirectToAction("Login");
                            }
                            else
                            {
                                ViewData["Message"] = "Invalid email.";
                                ViewData["MessageType"] = "error";
                            }
                        }
                        catch (Exception e)
                        {
                            ViewData["Message"] = "Error while sending email." + e.Message.ToString();
                            ViewData["MessageType"] = "error";
                        }
                        return View();
                    }
                    else
                    {
                        ViewData["Message"] = registrationResult.MSG;
                        ViewData["MessageType"] = "error";
                    }
                }
                catch (Exception e)
                {
                    ViewData["Message"] = e.Message;
                    ViewData["MessageType"] = "error";
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token, string email)
        {
            try
            {
                var verifyEmailModel = _authenticationService.ValidateToken(token);
                if (verifyEmailModel == null)
                {
                    TempData["Message"] = "Invalid or expired token.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Login", "Authentication");
                }

                var result = _authenticationService.UpdateEmailVerificationStatus(verifyEmailModel.Token, verifyEmailModel.Email);
                if (result == "success")
                {
                    TempData["Message"] = "Email verified successfully.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    TempData["Message"] = "Invalid or expired token.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index", "Authentication");
                }
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Error while validating token." + e.Message.ToString();
                ViewData["MessageType"] = "error";
                return RedirectToAction("Login", "Authentication");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loginResult = _authenticationService.Login(model);

                    if (loginResult != null && loginResult.Role != null)
                    {
                        if (!loginResult.IsVerified)
                        {
                            ViewData["Message"] = "Please verify your email.";
                            ViewData["MessageType"] = "warning";
                            return View(model);
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, loginResult.UserName),
                            new Claim(ClaimTypes.Role, loginResult.Role)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            new AuthenticationProperties
                            {
                                IsPersistent = true
                            });

                        HttpContext.Session.SetInt32("Id", loginResult.Id);
                        HttpContext.Session.SetInt32("UserId", loginResult.UserId);
                        HttpContext.Session.SetString("Username", loginResult.UserName);
                        HttpContext.Session.SetString("Role", loginResult.Role);
                        if(loginResult.UserImage != null)
                        {
                            HttpContext.Session.SetString("UserImage", loginResult.UserImage.Base64Image);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["Message"] = "Invalid username or password.";
                        ViewData["MessageType"] = "error";
                    }
                }
                catch (Exception e)
                {
                    ViewData["Message"] = e.Message;
                    ViewData["MessageType"] = "error";
                }
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Clear the authentication cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Clear session values
                HttpContext.Session.Clear();
                TempData["Message"] = "Logged out";
                TempData["MessageType"] = "success";

                return RedirectToAction("Login", "Authentication");
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Couldn't able to logout at the moment." + e.Message.ToString();
                ViewData["MessageType"] = "error";
            }
            return View();
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            try
            {
                TokenModel? reset = _authenticationService.GenerateAndStorePasswordToken(email);
                if (reset != null)
                {

                    var resetLink = Url.Action("ResetPassword", "Authentication", new { token = reset.Token }, Request.Scheme);
                    _mailService.Send(email, "Reset your password", $"Click here: {resetLink}");

                    TempData["Message"] = "Check your email for reset instructions.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    ViewData["Message"] = "Invalid email.";
                    ViewData["MessageType"] = "error";
                }
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Error while sending email." + e.Message.ToString();
                ViewData["MessageType"] = "error";
            }
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            try
            {
                var resetModel = _authenticationService.ValidateToken(token);
                if (resetModel == null)
                {
                    TempData["Message"] = "Invalid or expired token.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Login", "Authentication");
                }

                return View(new ResetPasswordModel { Token = token });
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Error while sending email." + e.Message.ToString();
                ViewData["MessageType"] = "error";
            }
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var status = _authenticationService.ResetPassword(model);
                TempData["Message"] =
                    status == "success"
                    ? "Password reset complete."
                    : "Couldn't reset password at the moment.";
                TempData["MessageType"] = status;
                return RedirectToAction("Logout", "Authentication");
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Error while reseting password." + e.Message.ToString();
                ViewData["MessageType"] = "error";
            }
            return View();

        }

        [HttpGet]
        public IActionResult ChangeEmail()
        {
            var authId = HttpContext.Session.GetInt32("Id");
            if (!authId.HasValue)
            {
                TempData["Message"] = "You need to login first.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Authentication");
            }
            var email = _authenticationService.GetEmail(authId.Value);
            if(email.Equals(string.Empty))
            {
                TempData["Message"] = "Couldn't find your email.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Profile", "User");
            }
            var model = new ChangeEmailModel
            {
                OldEmail = email
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult ChangeEmail(ChangeEmailModel model)
        {
            int row = _authenticationService.CheckExistingEmail(model.NewEmail);
            if(row > 0)
            {
                ViewData["Message"] = "Email already existed.";
                ViewData["MessageType"] = "error";
                return View(model);
            }
            try
            {
                TokenModel? tokenModel = _authenticationService.GenerateAndStoreEmailVerificationToken(model.OldEmail);
                if (tokenModel != null)
                {

                    var verificationLink = Url.Action("VerifyNewEmail", "Authentication", new {newEmail = model.NewEmail, token = tokenModel.Token }, Request.Scheme);
                    _mailService.Send(model.NewEmail, "Verify your email", $"Please verify your new email by clicking this link: {verificationLink}");

                    TempData["Message"] = "Verification email sent. Please check your inbox.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Profile", "User");
                }
                else
                {
                    ViewData["Message"] = "Invalid email.";
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Error while sending email." + e.Message.ToString();
                ViewData["MessageType"] = "error";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyNewEmail(string newEmail, string token)
        {
            try
            {
                var tokenModel = _authenticationService.ValidateToken(token);
                if (tokenModel == null)
                {
                    TempData["Message"] = "Invalid or expired token.";
                    TempData["MessageType"] = "error";
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Authentication");
                }
                int row = _authenticationService.ChangeEmail(newEmail, token);
                if(row > 0)
                {
                    TempData["Message"] = "Email changed successfully.";
                    TempData["MessageType"] = "success";
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    TempData["Message"] = "Couldn't change email at the moment.";
                    TempData["MessageType"] = "error";
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Authentication");
                }
            }
            catch
            {
                ViewData["Message"] = "Error while changing email.";
                ViewData["MessageType"] = "error";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Authentication");
            }
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var authId = HttpContext.Session.GetInt32("Id");
                    if (authId.HasValue)
                    {
                        model.AuthId = authId ?? 0;
                        AuthResponse authResponse = _authenticationService.ChangePassword(model);
                        if (authResponse.STATUS == "success")
                        {
                            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            HttpContext.Session.Clear();
                            TempData["Message"] = authResponse.MSG;
                            TempData["MessageType"] = "success";
                            return RedirectToAction("Login", "Authentication");
                        }
                        else
                        {
                            ViewData["Message"] = authResponse.MSG;
                            ViewData["MessageType"] = "error";
                            return View(model);
                        }
                    }
                }
                catch (Exception e)
                {
                    ViewData["Message"] = "Error while changing password." + e.Message.ToString();
                    ViewData["MessageType"] = "error";
                    return View(model);
                }
            }
            return View(model);
        }



        [HttpGet]
        public JsonResult ValidateUsername(string username)
        {
            var id = _authenticationService.GetIdByUsername(username);
            var currentUserId = HttpContext.Session.GetInt32("Id");
            if (currentUserId != null)
            {

            }
            if (id.HasValue && currentUserId != null)
            {
                if(id.Value == currentUserId)
                {
                    return Json(new { success = false });
                }
                else
                {

                    return Json(new { success = true, id = id.Value });
                }
            }
            return Json(new { success = false });
        }

    }
}
