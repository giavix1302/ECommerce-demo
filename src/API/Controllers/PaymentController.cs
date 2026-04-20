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

    [HttpPost("payos/webhook")]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookPayload payload)
    {
        try
        {
            Console.WriteLine($"Received PayOS webhook: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            await _mediator.Send(new ProcessPayOSWebhookCommand(payload));
        }
        catch (InvalidWebhookSignatureException)
        {
            // Silently ignore — invalid signature, not a legitimate PayOS call
        }

        return Ok();
    }
}
