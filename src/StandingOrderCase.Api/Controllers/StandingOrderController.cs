using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StandingOrderCase.Api.Records;
using StandingOrderCase.Api.Services;

namespace StandingOrderCase.Api.Controllers;

[ApiController]
[Route("standing-orders")]
public class StandingOrderController : ControllerBase
{
    private readonly StandingOrderService _standingOrderService;

    public StandingOrderController(StandingOrderService standingOrderService)
    {
        _standingOrderService = standingOrderService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetStandingOrder))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId([FromQuery] Guid userId)
    {
        var standingOrder = await _standingOrderService.GetByUserId(userId);

        if (standingOrder == null)
        {
            return NotFound();
        }

        return Ok(standingOrder);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetStandingOrder))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var standingOrder = await _standingOrderService.Get(id);

        if (standingOrder == null)
        {
            return NotFound();
        }

        return Ok(standingOrder);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateStandingOrder model,
        [FromServices] IValidator<CreateStandingOrder> validator)
    {
        var validationResult = await validator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var standingOrderId = await _standingOrderService.Create(model);

        return Created(nameof(Get), new IdResponse(standingOrderId));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id)
    {
        var result = await _standingOrderService.Cancel(id);

        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }
}