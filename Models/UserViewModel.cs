using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> UserRoles { get; set; }
        public IList<IdentityRole> AllRoles { get; set; }
    }
} 