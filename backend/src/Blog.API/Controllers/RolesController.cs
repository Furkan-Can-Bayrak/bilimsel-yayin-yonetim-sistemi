using Blog.API.Infrastructure.Authorization;
using Blog.Application.Roles.Queries.GetRoles;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Roles.View)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new GetRolesQuery(), cancellationToken);
        return Ok(items);
    }
}
