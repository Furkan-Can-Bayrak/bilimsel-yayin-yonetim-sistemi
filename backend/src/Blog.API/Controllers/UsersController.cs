using Blog.API.Infrastructure.Authorization;
using Blog.Application.Users.Commands.CreateUser;
using Blog.Application.Users.Commands.UpdateUserAcademicTitle;
using Blog.Application.Users.Commands.UpdateUserActiveStatus;
using Blog.Application.Users.Commands.UpdateUserRoles;
using Blog.Application.Users.Queries.GetUsers;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Users.View)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(items);
    }

    [HasPermission(Permissions.Users.Manage)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Created($"/api/users/{result.Id}", result);
    }

    [HasPermission(Permissions.Users.Manage)]
    [HttpPut("{id:int}/roles")]
    public async Task<IActionResult> UpdateRoles(
        int id,
        [FromBody] UpdateUserRolesRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateUserRolesCommand(id, body.RoleIds), cancellationToken);
        return NoContent();
    }

    [HasPermission(Permissions.Users.Manage)]
    [HttpPut("{id:int}/academic-title")]
    public async Task<IActionResult> UpdateAcademicTitle(
        int id,
        [FromBody] UpdateUserAcademicTitleRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateUserAcademicTitleCommand(id, body.AcademicTitle),
            cancellationToken);
        return NoContent();
    }

    [HasPermission(Permissions.Users.Manage)]
    [HttpPut("{id:int}/active")]
    public async Task<IActionResult> UpdateActiveStatus(
        int id,
        [FromBody] UpdateUserActiveStatusRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateUserActiveStatusCommand(id, body.IsActive),
            cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateUserRolesRequest(IReadOnlyList<int> RoleIds);

public sealed record UpdateUserAcademicTitleRequest(AcademicTitle AcademicTitle);

public sealed record UpdateUserActiveStatusRequest(bool IsActive);
