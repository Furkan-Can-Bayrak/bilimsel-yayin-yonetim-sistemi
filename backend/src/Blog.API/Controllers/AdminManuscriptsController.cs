using Blog.API.Infrastructure.Authorization;
using Blog.Application.Manuscripts.Queries.GetAdminManuscripts;
using Blog.Application.Manuscripts.Queries.GetManuscriptById;
using Blog.Application.Manuscripts.Queries.GetMyManuscripts;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/admin/manuscripts")]
public class AdminManuscriptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminManuscriptsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Editör kuyruğu: taslak yok, giriş yapanın kendi yazıları yok.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Manuscripts.ViewAll)]
    public async Task<IActionResult> GetEditorialQueue(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? researchAreaId = null,
        [FromQuery] ManuscriptStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminManuscriptsQuery(page, pageSize, search, researchAreaId, status),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Makalelerim: yalnızca current user yazarı, taslak dahil.</summary>
    [HttpGet("mine")]
    [HasPermission(Permissions.Manuscripts.Create)]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? researchAreaId = null,
        [FromQuery] ManuscriptStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyManuscriptsQuery(page, pageSize, search, researchAreaId, status),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var manuscript = await _mediator.Send(new GetManuscriptByIdQuery(id), cancellationToken);
        return manuscript is null ? NotFound() : Ok(manuscript);
    }
}
