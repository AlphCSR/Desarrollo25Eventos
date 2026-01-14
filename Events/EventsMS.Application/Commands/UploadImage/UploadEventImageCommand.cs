using MediatR;
using System;
using System.IO;

namespace EventsMS.Application.Commands.UploadImage
{
    public record UploadEventImageCommand(Guid EventId, Stream FileStream, string FileName) : IRequest<string>;
}
