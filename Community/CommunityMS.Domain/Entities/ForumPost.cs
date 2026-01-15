using System;

namespace CommunityMS.Domain.Entities
{
    public class ForumPost
    {
        public Guid Id { get; private set; }
        public string Content { get; private set; }
        public Guid ThreadId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string AuthorName { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsHidden { get; private set; }

        protected ForumPost() 
        { 
            Content = null!;
            AuthorName = null!;
        }

        public ForumPost(string content, Guid threadId, Guid authorId, string authorName)
        {
            Id = Guid.NewGuid();
            Content = content;
            ThreadId = threadId;
            AuthorId = authorId;
            AuthorName = authorName;
            CreatedAt = DateTime.UtcNow;
            IsHidden = false;
        }

        public void Hide() => IsHidden = true;
        public void Unhide() => IsHidden = false;
    }
}
