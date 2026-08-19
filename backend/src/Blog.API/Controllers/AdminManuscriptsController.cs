using Blog.Application.Manuscripts.Queries.GetAdminManuscripts;
using Blog.Application.Manuscripts.Queries.GetManuscriptById;
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
    /// ViewAll ise tüm kayıtlar; aksi halde yalnızca giriş yapanın yazdığı veya atandığı makaleler.
    /// Public <c>GET /api/manuscripts/{slug}</c> ile çakışmaması için ayrı önek.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var manuscript = await _mediator.Send(new GetManuscriptByIdQuery(id), cancellationToken);
        return manuscript is null ? NotFound() : Ok(manuscript);
    }
}
