﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.UI.Areas.Identity.Pages.Account.Manage
{
    public class GenerateRecoveryCodesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<GenerateRecoveryCodesModel> _logger;

        public GenerateRecoveryCodesModel(
            UserManager<IdentityUser> userManager,
            ILogger<GenerateRecoveryCodesModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"'{_userManager.GetUserId(User)}' Kimliğine sahip kullanıcı yüklenemiyor.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                throw new InvalidOperationException($"2FA etkin olmadığı için '{userId}' kimliğine sahip kullanıcı için kurtarma kodları oluşturulamıyor.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"'{_userManager.GetUserId(User)}' Kimliğine sahip kullanıcı yüklenemiyor.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException($"2FA etkin olmadığı için '{userId}' kimliğine sahip kullanıcı için kurtarma kodları oluşturulamıyor.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation("'{UserId}' kimliğine sahip kullanıcı, yeni 2FA kurtarma kodları oluşturdu.", userId);
            StatusMessage = "Yeni kurtarma kodları oluşturdunuz.";
            return RedirectToPage("./ShowRecoveryCodes");
        }
    }
}