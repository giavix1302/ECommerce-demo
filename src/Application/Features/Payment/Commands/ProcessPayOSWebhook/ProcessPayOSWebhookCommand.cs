using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Payment.Commands.ProcessPayOSWebhook;

public record ProcessPayOSWebhookCommand(PayOSWebhookPayload Payload) : IRequest;
