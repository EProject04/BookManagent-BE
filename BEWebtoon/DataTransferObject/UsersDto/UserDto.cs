﻿namespace BEWebtoon.DataTransferObject.UsersDto
{
    public class UserDto
    {
        public int? Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Username { get; set; }
        public string? RoleName { get; set; }
        public string? CreatedDate { get; set; }
        public string? LastModifiedDate { get; set; }
    }
}
