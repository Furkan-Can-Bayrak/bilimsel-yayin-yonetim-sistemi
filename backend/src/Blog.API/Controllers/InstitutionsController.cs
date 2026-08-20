using Blog.API.Infrastructure.Authorization;
using Blog.Application.Institutions.Queries.GetInstitutions;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/institutions")]
public class InstitutionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstitutionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Users.Manage)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new GetInstitutionsQuery(), cancellationToken);
        return Ok(items);
    }
}
