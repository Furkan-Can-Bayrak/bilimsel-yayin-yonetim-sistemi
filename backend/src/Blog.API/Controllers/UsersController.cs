using Blog.API.Infrastructure.Authorization;
using Blog.Application.Users.Commands.CreateUser;
using Blog.Application.Users.Queries.GetUsers;
using Blog.Domain.Authorization;
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
}
