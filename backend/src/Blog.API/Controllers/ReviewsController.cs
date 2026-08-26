using Blog.API.Infrastructure.Authorization;
using Blog.Application.Reviews.Commands.AssignReview;
using Blog.Application.Reviews.Commands.SubmitReview;
using Blog.Application.Reviews.Commands.WithdrawReview;
using Blog.Application.Reviews.Queries.GetMyReviews;
using Blog.Application.Reviews.Queries.GetReviewById;
using Blog.Application.Reviews.Queries.GetReviewCandidates;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Reviews.Assign)]
    [HttpGet("candidates")]
    public async Task<IActionResult> Candidates(
        [FromQuery] int manuscriptId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReviewCandidatesQuery(manuscriptId), cancellationToken);
        return Ok(result);
    }

    [HasPermission(Permissions.Reviews.Submit)]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyReviewsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReviewByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HasPermission(Permissions.Reviews.Assign)]
    [HttpPost]
    public async Task<IActionResult> Assign(
        [FromBody] AssignReviewCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HasPermission(Permissions.Reviews.Assign)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new WithdrawReviewCommand(id), cancellationToken);
        return NoContent();
    }

    [HasPermission(Permissions.Reviews.Submit)]
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(
        int id,
        [FromBody] SubmitReviewRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SubmitReviewCommand(id, body.Recommendation, body.Comments),
            cancellationToken);
        return NoContent();
    }
}

public sealed record SubmitReviewRequest(ReviewRecommendation Recommendation, string Comments);
