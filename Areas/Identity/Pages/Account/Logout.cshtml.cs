using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuplicateLogin.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DuplicateLogin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly LoginHelper _loginHelper;


        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, LoginHelper loginHelper)
        {
            _signInManager = signInManager;
            _logger = logger;
            _loginHelper = loginHelper;

        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            //remove the user from the memory cache
            var user = await _signInManager.UserManager.GetUserAsync(User);
            _loginHelper.RemoveLogin(user.Email);

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
