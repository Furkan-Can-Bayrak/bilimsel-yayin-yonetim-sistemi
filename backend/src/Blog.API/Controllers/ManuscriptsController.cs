using Blog.API.Infrastructure.Authorization;
using Blog.Application.Manuscripts.Commands.CreateManuscript;
using Blog.Application.Manuscripts.Commands.DeleteManuscript;
using Blog.Application.Manuscripts.Commands.PublishManuscript;
using Blog.Application.Manuscripts.Commands.UnpublishManuscript;
using Blog.Application.Manuscripts.Commands.UpdateManuscript;
using Blog.Application.Manuscripts.Queries.GetAdminManuscripts;
using Blog.Application.Manuscripts.Queries.GetManuscriptById;
using Blog.Application.Manuscripts.Queries.GetManuscriptBySlug;
using Blog.Application.Manuscripts.Queries.GetManuscripts;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/manuscripts")]
public class ManuscriptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManuscriptsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? researchAreaId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetManuscriptsQuery(page, pageSize, search, researchAreaId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ViewAll ise tüm kayıtlar; aksi halde yalnızca giriş yapanın yazdığı makaleler.
    /// {slug} ile çakışmasın diye literal "admin".
    /// </summary>
    [HttpGet("admin")]
    public async Task<IActionResult> GetAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? researchAreaId = null,
        [FromQuery] bool? isPublished = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminManuscriptsQuery(page, pageSize, search, researchAreaId, isPublished),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("admin/{id:int}")]
    public async Task<IActionResult> GetAdminById(int id, CancellationToken cancellationToken)
    {
        var manuscript = await _mediator.Send(new GetManuscriptByIdQuery(id), cancellationToken);
        return manuscript is null ? NotFound() : Ok(manuscript);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var manuscript = await _mediator.Send(new GetManuscriptBySlugQuery(slug), cancellationToken);
        return manuscript is null ? NotFound() : Ok(manuscript);
    }

    [HasPermission(Permissions.Manuscripts.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateManuscriptCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug }, result);
    }

    [HasPermission(Permissions.Manuscripts.Update)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateManuscriptRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateManuscriptCommand(
                id,
                body.Title,
                body.Content,
                body.Summary,
                body.ResearchAreaId,
                body.Slug),
            cancellationToken);

        return NoContent();
    }

    [HasPermission(Permissions.Manuscripts.Publish)]
    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PublishManuscriptCommand(id), cancellationToken);
        return NoContent();
    }

    [HasPermission(Permissions.Manuscripts.Unpublish)]
    [HttpPost("{id:int}/unpublish")]
    public async Task<IActionResult> Unpublish(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnpublishManuscriptCommand(id), cancellationToken);
        return NoContent();
    }

    [HasPermission(Permissions.Manuscripts.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteManuscriptCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateManuscriptRequest(
    string Title,
    string Content,
    string? Summary,
    int ResearchAreaId,
    string? Slug);
