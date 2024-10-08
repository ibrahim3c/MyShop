﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.Entities.Models;
using MyShop.Services.Interfaces;
using MyShop.Web.Settings;

namespace MyShop.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IFileService fileService;

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.fileService = fileService;
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
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            // my custom fields
            [Required, MaxLength(100)]
            public string FirstName { get; set; }

            [Required, MaxLength(100)]
            public string LastName { get; set; }

            public string? Address { get; set; } = default!;
            public IFormFile? ProfilePicture { get; set; }
            public string? ProfilePictureURL { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var firstName =  user.FirstName;
            var lastName =  user.LastName;
            var address =  user.Address;
            var profilePictureURL = user.ProfilePicture;
            var profilePicture = await fileService.GetFileAsIFormFileAsync(profilePictureURL);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
                ,
                FirstName = firstName,
                LastName = lastName,
                Address = address,
                ProfilePictureURL = profilePictureURL,
                ProfilePicture = profilePicture
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var firstName = user.FirstName;
            if(firstName != Input.FirstName)
            {
                user.FirstName = Input.FirstName;
                await _userManager.UpdateAsync(user);
            }


            var lastName = user.LastName;
            if (lastName != Input.LastName)
            {
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
            }

            var address = user.Address;
            if (address != Input.Address)
            {
                user.Address = Input.Address;
                await _userManager.UpdateAsync(user);
            }

           // add image
           if(Input.ProfilePicture!=null && Input.ProfilePicture != fileService.GetFileAsIFormFileAsync( user.ProfilePicture))
            {
                var fileExtension = Path.GetExtension(Input.ProfilePicture.FileName).ToLowerInvariant();
                var FileSize = Input.ProfilePicture.Length;

                if (!FileSettings.AllowedExtensions.Contains(fileExtension)) ModelState.AddModelError("ProfilePicture", "you should put valid extension");
                if (FileSettings.MaxFileSizeInBytes < FileSize) ModelState.AddModelError("ProfilePicture", "you should put valid size");

                if (ModelState.IsValid)
                {
                var image = await fileService.UploadFileAsync(Input.ProfilePicture, FileSettings.ImagePath);
                if (string.IsNullOrEmpty(image)) ModelState.AddModelError("ProfilePicture", "The profile Picture is Required");
                    else
                    {

                    user.ProfilePicture = image;    
                    }
                }
            
            }

           
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
