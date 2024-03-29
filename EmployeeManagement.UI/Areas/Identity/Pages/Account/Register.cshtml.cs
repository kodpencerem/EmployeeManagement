﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EmployeeManagement.Common.ConstantsModels;
using EmployeeManagement.Data.DbModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.UI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Employee> _signInManager;
        private readonly UserManager<Employee> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<Employee> userManager,
            SignInManager<Employee> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "E-Posta Alanı Boş Geçilemez!")]
            [EmailAddress]
            [Display(Name = "Email:")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Şifre Alanı Boş Geçilemez!")]
            [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Şifreniz:")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "Şifre Onay Alanı Boş Geçilemez!")]
            [Display(Name = "Şifre Tekrarı:")]
            [Compare("Password", ErrorMessage = "Şifre ve onay şifresi eşleşmiyor.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "İsim Alanı Boş Geçilemez!")]
            [DataType(DataType.Text)]
            [Display(Name = "Adınız:")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Soyisim Alanı Boş Geçilemez!")]
            [DataType(DataType.Text)]
            [Display(Name = "Soyadınız:")]
            public string LastName { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new Employee
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user, ResultConstant.Employee_Role).Wait();
                    _logger.LogInformation("Kullanıcı şifre ile yeni bir hesap oluşturdu.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "E-postanızı onaylayın",
                        $"Lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>linkine tıklayarak hesabınızı onaylayın!</a>.");
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
