// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace DoAnMon.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
		private readonly IWebHostEnvironment _environment;

		public IndexModel(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager,
			IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
			_environment = environment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

		public string Email { get; set; }
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

			
			[DataType(DataType.Date)]
			[Display(Name = "Date of Birth")]
			public DateTime? DateOfBirth { get; set; }

			
			[Display(Name = "Profile Picture URL")]
			public string? ProfilePictureUrl { get; set; }

			[ValidateNever]
			[Display(Name = "Profile Picture")]
			public IFormFile ProfilePicture { get; set; }
		}

        private async Task LoadAsync(CustomUser user)
        {
			var userName = await _userManager.GetUserNameAsync(user);
            var fullName = user.Name;
			var email = user.Email;

			Username = userName;
			Email = email;

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

			if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
			{
				var uploadsFolder = Path.Combine(_environment.WebRootPath, "Imgs_avtUser");
				//đổi tên sau khi upload
				var filePath = Path.Combine(uploadsFolder, user.Mssv + ".jpg");
				
				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await Input.ProfilePicture.CopyToAsync(fileStream);
				}
				user.UrlAvt = user.Mssv + ".jpg";
			}

			// Update user properties
			user.Name = Input.FullName;
			user.NgaySinh = Input.DateOfBirth.ToString();
			

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
			TempData["StatusMessage"] = StatusMessage;
			return RedirectToAction("Index", "ClassRooms");


		}
	}
}
