﻿namespace BEWebtoon.DataTransferObject.UserProfilesDto
{
    public class UserProfileDto
    {
        public string? FistName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Sex { get; set; }
        public string? ImagePath { get; set; }
        public int? AuthorId { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
    }
}