// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoAnMon.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;

        public IndexModel(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[Display(Name = "Full Name")]
			public string FullName { get; set; }

			[Required]
			[EmailAddress]
			[Display(Name = "Email")]
			public string Email { get; set; }

			
			[DataType(DataType.Date)]
			[Display(Name = "Date of Birth")]
			public DateTime? DateOfBirth { get; set; }

			[Url]
			[Display(Name = "Profile Picture URL")]
			public string? ProfilePictureUrl { get; set; }
		}

        private async Task LoadAsync(CustomUser user)
        {
			var userName = await _userManager.GetUserNameAsync(user);
            var fullName = user.Name;

			Username = userName;

			DateTime? dateOfBirth = null;
			if (user.NgaySinh != null)
			{
				if (DateTime.TryParse(user.NgaySinh.Trim(), out var parsedDate))
				{
					dateOfBirth = parsedDate;
				}
				else
				{
					dateOfBirth = null;
				}
			}

			Input = new InputModel
			{
				FullName = fullName,
				Email = user.Email,
				DateOfBirth = dateOfBirth,
				ProfilePictureUrl = user.UrlAvt
			};

		}

		public async Task<IActionResult> OnGetAsync()
        {
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			await LoadAsync(user);
			return Page();
		}

        public async Task<IActionResult> OnPostAsync()
        {
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			if (!ModelState.IsValid)
			{
				await LoadAsync(user);
				return Page();
			}

			// Update user properties
			user.Email = Input.Email;
			user.Name = Input.FullName;
			user.NgaySinh = Input.DateOfBirth.ToString();
			user.UrlAvt = Input.ProfilePictureUrl;

			// Save changes to the user
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				await LoadAsync(user);
				return Page();
			}

			await _signInManager.RefreshSignInAsync(user);
			StatusMessage = "Your profile has been updated";
			return RedirectToPage();
		}
    }
}
