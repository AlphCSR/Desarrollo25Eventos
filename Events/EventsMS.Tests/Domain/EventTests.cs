using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Enums;
using EventsMS.Domain.Exceptions;

namespace EventsMS.Tests.Domain
{
    public class EventTests
    {
        [Fact]
        public void Event_Lifecycle_Tests()
        {
            var idUser = Guid.NewGuid();
            var now = DateTime.UtcNow.AddDays(1);
            
            var evt = new Event(idUser, "Concierto Rock", "Gran concierto", now, now.AddHours(3), "Estadio Libertad", new List<string> { "Musica" });
            
            evt.Status.Should().Be(EventStatus.Draft);
            evt.Title.Should().Be("Concierto Rock");

            Action actConstruct = () => new Event(idUser, "", "D", now, now.AddHours(2), "V", new List<string> { "C" });
            actConstruct.Should().Throw<InvalidEventDataException>().WithMessage("El título es requerido.");

            Action actPublishFail = () => evt.Publish();
            actPublishFail.Should().Throw<InvalidOperationException>().WithMessage("No se puede publicar un evento sin localidades.");

            evt.AddSection("VIP", 150.00m, 100, true);
            evt.Publish();
            evt.Status.Should().Be(EventStatus.Published);

            Action actAddSectionFail = () => evt.AddSection("General", 50, 500, false);
            actAddSectionFail.Should().Throw<InvalidOperationException>().WithMessage("No se pueden agregar secciones a un evento publicado.");

            evt.UpdateDetails("Rock Festival", "Updated", now, now.AddHours(4), "New Venue", new List<string> { "Rock" }, EventType.Physical, null);
            evt.Title.Should().Be("Rock Festival");

            evt.Cancel();
            evt.Status.Should().Be(EventStatus.Cancelled);

            Action actUpdateFail = () => evt.UpdateDetails("Fail", "D", now, now.AddHours(2), "V", new List<string> { "C" }, EventType.Physical, null);
            actUpdateFail.Should().Throw<InvalidEventDataException>().WithMessage("No se puede modificar un evento cancelado.");

            var finishedEvt = new Event(idUser, "End", "D", now, now.AddHours(2), "V", new List<string> { "C" });
            finishedEvt.UpdateStatus(EventStatus.Finished);
            Action actCancelFail = () => finishedEvt.Cancel();
            actCancelFail.Should().Throw<InvalidOperationException>().WithMessage("No se puede cancelar un evento finalizado.");
        }
    }
}
