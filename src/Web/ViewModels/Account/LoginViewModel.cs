// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Contoso.FraudProtection.Web.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Contoso.FraudProtection.Web.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public DeviceFingerPrintingViewModel DeviceFingerPrinting { get; set; }
    }
}
