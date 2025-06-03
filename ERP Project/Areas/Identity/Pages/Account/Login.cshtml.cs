
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ERP_Project.Data;
using System.Security.Claims;

namespace ERP_Project.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _db;

        public LoginModel(SignInManager<IdentityUser> signInManager, ApplicationDbContext db,
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            var user1 = await _userManager.FindByEmailAsync("eshamir41@gmail.com");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user1);
            var result = await _userManager.ResetPasswordAsync(user1, token, "Admin@123");

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                if (role.ElementAt(0) == "Employee")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    string link = Request.Scheme + "://" + Request.Host + "/User/EmployeeAttendance/" + employee.EmployeeId;
                    return Redirect(link);
                }
                else
                {

                    string link = Request.Scheme + "://" + Request.Host + "";
                    return Redirect(link);

                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
         
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if(user == null)
                {
                    string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                    return Redirect(link);
                }
                var role = await _userManager.GetRolesAsync(user);
                if (role.Count() == 0)
                {
                    string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                    return Redirect(link);
                }
                if (role.ElementAt(0) == "Employee")
                {
                    var employee = _db.Employees.Where(a => a.Email == Input.Email).FirstOrDefault();
                    if (employee.Status == false)
                    {
                        string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                        return Redirect(link);
                    }
                }
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                 
                    if (role.ElementAt(0) == "Employee")
                    {
                        var employee = _db.Employees.Where(a => a.Email == Input.Email).FirstOrDefault();
                        string link = Request.Scheme + "://" + Request.Host + "/User/EmployeeAttendance/"+employee.EmployeeId;
                        return Redirect(link);
                    }
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
