﻿namespace DigitalBlog.Models
{
    public class UserEdit
    {
        public short UserId { get; set; }

        public string LoginName { get; set; } = null!;

        public string LoginPassword { get; set; } = null!;

        public string UserProfile { get; set; } = null!;

        public bool LoginStatus { get; set; }

        public string UserRole { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string EmailAddress { get; set; } = null!;

        public string? Phone { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
