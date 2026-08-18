using Blog.Application.Posts.Commands.CreatePost;
using Blog.Application.Posts.Commands.DeletePost;
using Blog.Application.Posts.Commands.UpdatePost;
using Blog.Application.Posts.Queries.GetAdminPosts;
using Blog.Application.Posts.Queries.GetPostById;
using Blog.Application.Posts.Queries.GetPostBySlug;
using Blog.Application.Posts.Queries.GetPosts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPostsQuery(page, pageSize, search, categoryId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Admin: tüm yazılar (taslak dahil). {slug} ile çakışmasın diye literal "admin".</summary>
    [Authorize]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isPublished = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminPostsQuery(page, pageSize, search, categoryId, isPublished),
            cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("admin/{id:int}")]
    public async Task<IActionResult> GetAdminById(int id, CancellationToken cancellationToken)
    {
        var post = await _mediator.Send(new GetPostByIdQuery(id), cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var post = await _mediator.Send(new GetPostBySlugQuery(slug), cancellationToken);

        if (post is null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug }, result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePostRequest body,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePostCommand(
            id,
            body.Title,
            body.Content,
            body.Summary,
            body.CategoryId,
            body.IsPublished,
            body.Slug);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeletePostCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdatePostRequest(
    string Title,
    string Content,
    string? Summary,
    int CategoryId,
    bool IsPublished,
    string? Slug);
