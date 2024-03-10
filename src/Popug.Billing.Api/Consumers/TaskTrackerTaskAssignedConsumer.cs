using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Mxm.Kafka;
using Popug.Billing.Api.Common;
using Popug.Billing.Api.Logic;
using Popug.Tasks.Api.Common;

namespace Popug.Billing.Api.Consumers;

public class TaskTrackerTaskAssignedConsumer : KafkaConsumer
{
    private readonly IServiceProvider _serviceProvider;

    public TaskTrackerTaskAssignedConsumer(ILogger<KafkaConsumer> logger, IOptionsMonitor<KafkaConsumerOptions> options, IServiceProvider serviceProvider) : base(logger, options)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnMessage(Message<string, string> message)
    {
        var eventData = await message.ValidateAndGetData<TaskAssignedData>(MessageType, version: 1);
        
        await using var scope = _serviceProvider.CreateAsyncScope();
        var logic = scope.ServiceProvider.GetRequiredService<BillingLogic>();

        await logic.WriteOffAssignedTask(eventData.TaskId, eventData.AssignTo);
    }

    public override string MessageType => KafkaTopic.TaskTracker.TaskAssigned;
}