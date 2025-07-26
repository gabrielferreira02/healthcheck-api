using System;
using HealthCheckApi.Dto;
using MassTransit;

namespace HealthCheckApi.Consumers;

public class SimulateSendEmailConsumer : IConsumer<EmailPayload>
{
    private readonly ILogger<SimulateSendEmailConsumer> _logger;

    public SimulateSendEmailConsumer(ILogger<SimulateSendEmailConsumer> logger)
    {
        _logger = logger;
    }
    public Task Consume(ConsumeContext<EmailPayload> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending Email ------\nUser Id: {userId}\nStatus: {status}\nUrl: {url}\nVerified At: {date}", message.UserId, message.Status, message.Url, message.VerifiedAt);
        return Task.CompletedTask;
    }
}
