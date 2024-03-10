using Confluent.Kafka;
using LinqToDB;
using Microsoft.Extensions.Options;
using Mxm.Kafka;
using Popug.Billing.Api.Common;
using Popug.Billing.Api.Data;
using Popug.Billing.Api.Logic;
using Popug.Tasks.Api.Common;

namespace Popug.Billing.Api.Consumers;

public class TaskTrackerTaskStreamingConsumer : KafkaConsumer
{
    private readonly IServiceProvider _serviceProvider;

    public TaskTrackerTaskStreamingConsumer(ILogger<KafkaConsumer> logger, IOptionsMonitor<KafkaConsumerOptions> options, IServiceProvider serviceProvider) : base(logger, options)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnMessage(Message<string, string> message)
    {
        var eventData = await message.ValidateAndGetData<StreamingEvent<TaskCreatedData>>(MessageType, version: 1);

        if (eventData.Type != "created")
        {
            return;
        }

        var eventTask = eventData.Data;

        await using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<BillingDb>();
        var logic = scope.ServiceProvider.GetRequiredService<BillingLogic>();
        
        // Создадим задачу и назначим стоимость. Если задача уже есть, то добавим название.
        await db.Tasks.InsertOrUpdateAsync(
            () => new TaskEntity
            {
                TaskId = eventTask.TaskId,
                Description = eventTask.Description,
                AssignCharge = logic.GetAssignCharge(),
                CompleteCharge = logic.GetCompleteCharge()
            },
            exist => new TaskEntity
            {
                Description = eventTask.Description
            });

        // Спишем бабло с назначенного попуга
        await logic.WriteOffAssignedTask(eventTask.TaskId, eventTask.AssignTo);
    }

    public override string MessageType => KafkaTopic.TaskTracker.TaskStreaming;
}