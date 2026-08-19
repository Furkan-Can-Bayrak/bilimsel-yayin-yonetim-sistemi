using Blog.API.Infrastructure.Authorization;
using Blog.Application.ResearchAreas.Commands.CreateResearchArea;
using Blog.Application.ResearchAreas.Commands.DeleteResearchArea;
using Blog.Application.ResearchAreas.Commands.UpdateResearchArea;
using Blog.Application.ResearchAreas.Queries.GetResearchAreaById;
using Blog.Application.ResearchAreas.Queries.GetResearchAreas;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/research-areas")]
public class ResearchAreasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResearchAreasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new GetResearchAreasQuery(), cancellationToken);
        return Ok(items);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetResearchAreaByIdQuery(id), cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HasPermission(Permissions.ResearchAreas.Manage)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateResearchAreaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HasPermission(Permissions.ResearchAreas.Manage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateResearchAreaRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateResearchAreaCommand(id, body.Name), cancellationToken);
        return NoContent();
    }

    [HasPermission(Permissions.ResearchAreas.Manage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteResearchAreaCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateResearchAreaRequest(string Name);
