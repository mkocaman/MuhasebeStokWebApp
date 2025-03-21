using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MuhasebeStokWebApp.Services
{
    public class UserManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            
            return null;
        }

        public string GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
} 