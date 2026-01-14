using System;
using System.Collections.Generic;
using CommunityMS.Domain.Exceptions;

namespace CommunityMS.Domain.Entities
{
    public class ForumThread
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public Guid EventId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string AuthorName { get; private set; } 
        public DateTime CreatedAt { get; private set; }
        public bool IsPinned { get; private set; }
        public bool IsLocked { get; private set; }

        private readonly List<ForumPost> _posts = new();
        public IReadOnlyCollection<ForumPost> Posts => _posts.AsReadOnly();

        protected ForumThread() 
        { 
            Title = null!;
            Content = null!;
            AuthorName = null!;
        }

        public ForumThread(string title, string content, Guid eventId, Guid authorId, string authorName)
        {
            Id = Guid.NewGuid();
            Title = title;
            Content = content;
            EventId = eventId;
            AuthorId = authorId;
            AuthorName = authorName;
            CreatedAt = DateTime.UtcNow;
            IsPinned = false;
            IsLocked = false;
        }

        public void AddPost(ForumPost post)
        {
            if (IsLocked) throw new ThreadLockedException();
            _posts.Add(post);
        }

        public void Pin() => IsPinned = true;
        public void Unpin() => IsPinned = false;
        public void Lock() => IsLocked = true;
        public void Unlock() => IsLocked = false;
    }
}
