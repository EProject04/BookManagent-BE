﻿using BEWebtoon.Models.Domains.Interfaces;
using BEWebtoon.Models.Domains;

namespace BEWebtoon.Models
{
    public class Following : EntityAuditBase<int>, IUserTracking
    {
        public int? UserId { get; set; }
        public int? BookId { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public virtual UserProfile? UserProfiles { get; set; }
        public virtual Book? Books { get; set; }
    }
}
