using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new List<string>();
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string PhoneNumber { get; set; } = "";
        public bool PhoneNumberConfirmed { get; set; }
        public List<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> UserRoles { get; set; } = new List<string>();
        public IList<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
    }
} 