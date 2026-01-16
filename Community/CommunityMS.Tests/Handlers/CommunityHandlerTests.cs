using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Application.Commands;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Interfaces;
using CommunityMS.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityMS.Tests.Handlers
{
    public class CommunityHandlerTests
    {
        private readonly Mock<IForumRepository> _repositoryMock;

        public CommunityHandlerTests()
        {
            _repositoryMock = new Mock<IForumRepository>();
        }

        [Fact]
        public async Task Handle_CreateThread_Tests()
        {
            var handler = new CreateThreadCommandHandler(_repositoryMock.Object);
            var eventId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var command = new CreateThreadCommand 
            { 
                Title = "Title", 
                Content = "Content", 
                EventId = eventId, 
                AuthorId = authorId, 
                AuthorName = "Author" 
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _repositoryMock.Verify(x => x.AddThreadAsync(It.IsAny<ForumThread>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AddPost_Tests()
        {
            var handler = new AddPostCommandHandler(_repositoryMock.Object);
            var threadId = Guid.NewGuid();
            var thread = new ForumThread("Title", "Content", Guid.NewGuid(), Guid.NewGuid(), "Author");
            typeof(ForumThread).GetProperty("Id")?.SetValue(thread, threadId);

            _repositoryMock.Setup(x => x.GetThreadByIdAsync(threadId, It.IsAny<CancellationToken>())).ReturnsAsync(thread);

            var command = new AddPostCommand 
            { 
                ThreadId = threadId, 
                AuthorId = Guid.NewGuid(), 
                AuthorName = "Author", 
                Content = "Content" 
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            thread.Posts.Should().HaveCount(1);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            thread.Lock();
            Func<Task> act1 = async () => await handler.Handle(command, CancellationToken.None);
            await act1.Should().ThrowAsync<ThreadLockedException>();

            _repositoryMock.Setup(x => x.GetThreadByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ForumThread?)null);
            var commandNotFound = new AddPostCommand { ThreadId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), AuthorName = "A", Content = "C" };
            Func<Task> act2 = async () => await handler.Handle(commandNotFound, CancellationToken.None);
            await act2.Should().ThrowAsync<Exception>().WithMessage("Thread not found");
        }
    }
}
