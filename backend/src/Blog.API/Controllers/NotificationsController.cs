using Blog.API.Infrastructure.Authorization;
using Blog.Application.Notifications.Commands.MarkNotificationRead;
using Blog.Application.Notifications.Queries.GetNotifications;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[HasPermission(Permissions.Notifications.View)]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var items = await _mediator.Send(new GetNotificationsQuery(take), cancellationToken);
        return Ok(items);
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }
}
