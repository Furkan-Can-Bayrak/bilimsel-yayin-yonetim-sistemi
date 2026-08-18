using Blog.Application.Categories.Commands.CreateCategory;
using Blog.Application.Categories.Commands.DeleteCategory;
using Blog.Application.Categories.Commands.UpdateCategory;
using Blog.Application.Categories.Queries.GetCategories;
using Blog.Application.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(items);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateCategoryCommand(id, body.Name, body.Slug), cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateCategoryRequest(string Name, string? Slug);

