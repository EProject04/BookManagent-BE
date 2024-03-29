﻿using BEWebtoon.Models.Domains.Interfaces;
using BEWebtoon.Models.Domains;

namespace BEWebtoon.Models
{
    public class Comment : EntityAuditBase<int>
    {
        public string? CommentText { get; set; }
        public int? Rate { get; set; }
        public int? UserId { get; set; }
        public int? BookId { get; set; }
        public virtual UserProfile? UserProfiles { get; set; }
        public virtual Book? Books { get; set; }

    }
}
