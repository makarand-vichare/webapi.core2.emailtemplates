﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Net.Core.Utility;
using Net.Core.ViewModels.Core;

namespace Net.Core.ViewModels
{
    [Serializable]
    public class LoginViewModel : BaseViewModel
    {
        [Required]
        [DisplayName("Email")]
        [StringLength(50, ErrorMessage = AppMessages.Input_MaxCharsAllowed)]
        [RegularExpression(@"^.{1,}$", ErrorMessage = AppMessages.Input_MaxCharsAllowed)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string Password { get; set; }

        [Required]
        [DisplayName("RememberMe")]
        public bool RememberMe { get; set; }

    }

}
