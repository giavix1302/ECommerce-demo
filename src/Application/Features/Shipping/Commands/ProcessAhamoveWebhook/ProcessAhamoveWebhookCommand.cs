using Application.Common.DTOs.Ahamove;
using MediatR;

namespace Application.Features.Shipping.Commands.ProcessAhamoveWebhook;

public record ProcessAhamoveWebhookCommand(AhamoveWebhookPayload Payload) : IRequest;
