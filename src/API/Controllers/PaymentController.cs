using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Payment.Commands.ProcessPayOSWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/payment")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("payos/webhook")]
    public IActionResult PayOSWebhookVerify() => Ok();

    [HttpPost("payos/webhook")]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookPayload? payload)
    {
        if (payload is null)
            return Ok();

        try
        {
            await _mediator.Send(new ProcessPayOSWebhookCommand(payload));
        }
        catch (InvalidWebhookSignatureException)
        {
            // Silently ignore — invalid signature, not a legitimate PayOS call
        }
        catch
        {
            // always 200
        }

        return Ok();
    }
}
