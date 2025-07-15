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

namespace TenentManagement.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly AuthenticationService _authenticationService;
        private readonly MailService _mailService;

        public AuthenticationController(AuthenticationService authenticationService, MailService mailService)
        {
            _authenticationService = authenticationService;
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
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
                return RedirectToAction("Login", "Authentication");
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Error while reseting password." + e.Message.ToString();
                ViewData["MessageType"] = "error";
            }
            return View();

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
