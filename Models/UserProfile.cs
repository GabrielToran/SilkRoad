using System;
using System.Collections.Generic;
using System.Text;

namespace Multiplatoform_Project.Models
{
    public class UserProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; } = false;

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Initials =>
            $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}".Trim().ToUpper();
        public string MemberSinceFormatted => $"Member since {MemberSince:MMM yyyy}";
    }
}
