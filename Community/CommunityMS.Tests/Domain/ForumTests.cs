using System;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommunityMS.Tests.Domain
{
    public class ForumTests
    {
        [Fact]
        public void Thread_Lifecycle_Tests()
        {
            var eventId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var thread = new ForumThread("Test Thread", "Hello world", eventId, authorId, "Author Name");

            thread.Title.Should().Be("Test Thread");
            thread.Content.Should().Be("Hello world");
            thread.IsLocked.Should().BeFalse();
            thread.Posts.Should().BeEmpty();

            var post = new ForumPost("First post", thread.Id, authorId, "Author Name");
            thread.AddPost(post);
            thread.Posts.Should().HaveCount(1);

            thread.Lock();
            thread.IsLocked.Should().BeTrue();

            Action actPostFail = () => thread.AddPost(new ForumPost("Fail post", thread.Id, authorId, "Name"));
            actPostFail.Should().Throw<ThreadLockedException>();

            thread.Unlock();
            thread.AddPost(new ForumPost("Success post", thread.Id, authorId, "Name"));
            thread.Posts.Should().HaveCount(2);

            thread.Pin();
            thread.IsPinned.Should().BeTrue();
            thread.Unpin();
            thread.IsPinned.Should().BeFalse();
        }

        [Fact]
        public void Post_Lifecycle_Tests()
        {
            var threadId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = new ForumPost("Something", threadId, authorId, "Author");

            post.IsHidden.Should().BeFalse();
            post.Hide();
            post.IsHidden.Should().BeTrue();
            post.Unhide();
            post.IsHidden.Should().BeFalse();
        }
    }
}
