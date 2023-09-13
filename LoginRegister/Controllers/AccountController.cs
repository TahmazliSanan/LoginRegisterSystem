using LoginRegister.Entities;
using LoginRegister.Entities.Db;
using LoginRegister.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LoginRegister.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly DatabaseContext _dc;
        private readonly IConfiguration _config;

        public AccountController(DatabaseContext dc, IConfiguration config)
        {
            _dc = dc;
            _config = config;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginViewModel lvm)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = DoMD5HashedString(lvm.Password);

                User u = _dc.Users.SingleOrDefault(x => x.Username.ToLower() == lvm.Username.ToLower()
                    && x.Password == hashedPassword);

                if (u != null)
                {
                    if (u.Locked)
                    {
                        ModelState.AddModelError(nameof(lvm.Username), "User is locked!");
                        return View(lvm);
                    }

                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, u.FullName ?? string.Empty));
                    claims.Add(new Claim(ClaimTypes.Role, u.Role));
                    claims.Add(new Claim("Username", u.Username));

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username or password is incorrect!");
                }
            }
            return View(lvm);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(RegisterViewModel rvm)
        {
            if (ModelState.IsValid)
            {
                if (_dc.Users.Any(u => u.Username.ToLower() == rvm.Username.ToLower()))
                {
                    ModelState.AddModelError(nameof(rvm.Username), "Username is already exists!");
                    return View(rvm);
                }

                string hashedPassword = DoMD5HashedString(rvm.Password);

                User u = new()
                {
                    Username = rvm.Username,
                    Password = hashedPassword
                };

                _dc.Users.Add(u);
                int affectedRowCount = _dc.SaveChanges();

                if (affectedRowCount == 0)
                {
                    ModelState.AddModelError("", "User cannot be added!");
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            return View(rvm);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Profile()
        {
            ProfileInfoLoader();
            return View();
        }

        [HttpPost]
        public IActionResult ProfileChangeFullName([Required] [StringLength(50)] string? fullname)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = _dc.Users.SingleOrDefault(x => x.Id == userId);
                user.FullName = fullname;
                _dc.SaveChanges();
                ViewData["Result"] = "FullNameChanged";
            }
            ProfileInfoLoader();
            return View("Profile");
        }

        [HttpPost]
        public IActionResult ProfileChangePassword([Required] [MinLength(6)] [MaxLength(16)] string? newpassword)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = _dc.Users.SingleOrDefault(x => x.Id == userId);
                string hashedPassword = DoMD5HashedString(newpassword);
                user.Password = hashedPassword;
                _dc.SaveChanges();
                ViewData["Result"] = "PasswordChanged";
            }
            ProfileInfoLoader();
            return View("Profile");
        }

        private void ProfileInfoLoader()
        {
            Guid userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = _dc.Users.SingleOrDefault(x => x.Id == userId);
            ViewData["FullName"] = user.FullName;
        }

        private string DoMD5HashedString(string s)
        {
            string md5Salt = _config.GetValue<string>("AppSettings:MD5Salt");
            string salted = s + md5Salt;
            string hashed = salted.MD5();
            return hashed;
        }
    }
}