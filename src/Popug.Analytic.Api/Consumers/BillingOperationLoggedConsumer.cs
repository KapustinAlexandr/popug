using Confluent.Kafka;
using LinqToDB;
using Microsoft.Extensions.Options;
using Mxm.Kafka;

using Popug.Analytic.Api.Common;
using Popug.Analytic.Api.Data;

namespace Popug.Analytic.Api.Consumers;

public class BillingOperationLoggedConsumer : KafkaConsumer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string[] _acceptedTypes = { "write-off", "accrual" };

    public BillingOperationLoggedConsumer(ILogger<KafkaConsumer> logger, IOptionsMonitor<KafkaConsumerOptions> options, IServiceProvider serviceProvider) : base(logger, options)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnMessage(Message<string, string> message)
    {
        var eventData = await message.ValidateAndGetData<OperationLoggedData>(MessageType, version: 1);
        
        if (!_acceptedTypes.Contains(eventData.Type))
        {
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AnalyticDb>();

        await db.InsertAsync(new OperationEntity
        {
            PublicUserId = eventData.PopugPublicId,
            Credit = eventData.Type == "write-off" ? eventData.Amount : 0m,
            Debt = eventData.Type == "accrual" ? eventData.Amount : 0m,
            OperationDate = eventData.OperationDate
        });
    }

    public override string MessageType => KafkaTopic.Billing.OperationLogged;
}