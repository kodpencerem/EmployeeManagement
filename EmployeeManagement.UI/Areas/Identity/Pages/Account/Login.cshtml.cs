﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using EmployeeManagement.Data.DbModels;
using EmployeeManagement.Common.SessionOperations;
using EmployeeManagement.Data.Contracts;
using Microsoft.AspNetCore.Http;
using EmployeeManagement.Common.ConstantsModels;
using Newtonsoft.Json;

namespace EmployeeManagement.UI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        //private readonly IEmailSender _emailSender;

        public LoginModel(SignInManager<Employee> signInManager,
            ILogger<LoginModel> logger,
            UserManager<Employee> userManager,
             IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "E-Posta Alanı Boş Geçilemez!")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Şifre Alanı Boş Geçilemez!")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Beni Hatırla?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = _unitOfWork.employeeRepository.GetFirstOrDefault(u=>u.Email ==Input.Email.ToLower());

                    var userInfo = new SessionContext()
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        //TODO:Admın Bilgisini dinamic olarak getir
                        IsAdmin=false,
                        LastName = user.LastName,
                        LoginId = user.Id
                    };

                    //Set To User ınfo Session
                    HttpContext.Session.SetString(ResultConstant.LoginUserInfo, JsonConvert.SerializeObject(userInfo));

                    _logger.LogInformation("Kullanıcı giriş yaptı.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kullanıcı hesabı kilitlendi.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Doğrulama e-postası gönderildi. Lütfen emailinizi kontrol edin.");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            //await _emailSender.SendEmailAsync(
            //    Input.Email,
            //    "Confirm your email",
            //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Doğrulama e-postası gönderildi. Lütfen emailinizi kontrol edin.");
            return Page();
        }
    }
}
