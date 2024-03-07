using System.Text.RegularExpressions;
using Confluent.Kafka;
using LinqToDB;
using Microsoft.Extensions.Options;
using Mxm.Kafka;
using Popug.Tasks.Api.Data;

namespace Popug.Tasks.Api.Logic;

public class AuthAdminEventsConsumer : KafkaConsumer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Regex _useRegex = new("(?>users\\/)(?'UserId'.*)(?>\\/groups)", RegexOptions.Compiled);

    public AuthAdminEventsConsumer(
        ILogger<KafkaConsumer> logger, 
        IOptionsMonitor<KafkaConsumerOptions> options, 
        IServiceProvider serviceProvider) : base(logger, options)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Обработка админский сообщений keycloak из топика auth-admin-events.
    /// Ловим смену групп, т.к. через них задаётся роль попугу.
    /// </summary>
    public override async Task OnMessage(Message<string, string> message)
    {
        var msgJson = message.Value.FromJson<AuthAdminData>();

        // Реагирую только на присвоение группы, удаление решил не делать.
        if (msgJson is { ResourceType: "GROUP_MEMBERSHIP", OperationType: "CREATE" })
        {
            var group = msgJson.Representation.FromJson<GroupMembershipRepresentation>();
            var userId = _useRegex.Match(msgJson.ResourcePath).Groups["UserId"].Value;

            await using var scope = _serviceProvider.CreateAsyncScope();

            var rowsCount = await scope.ServiceProvider.GetRequiredService<TasksDb>()
                .Popugs
                .Where(w => w.UserId == userId)
                .Set(s => s.UserRole, () => group.Name)
                .UpdateAsync();

            if (rowsCount == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); // Чтобы кафку не насиловать, сделаем паузу на секунду.
                throw new Exception($"Очередь с попугами задерживается. Уже роль ему ({userId}) меняют, а его всё нет в БД.");
            }
        }
    }

    /* Пример ожидаемого сообщения в топике
       {
	        "id": "09af717a-c346-4096-ab49-6ec13b6b71ba",
	        "time": 1709326589567,
	        "realmId": "393de734-2ae6-4d87-8f13-512d50f8d87c",
	        "authDetails": {
		        "realmId": "cb7685a0-4b2c-43d7-ba0e-527a75b960a0",
		        "clientId": "80396e8c-3f9b-4034-8350-9500e8ec9e85",
		        "userId": "642713ca-8fbe-4fe1-aa33-1c2e95efa10f",
		        "ipAddress": "172.23.0.1"
	        },
	        "resourceType": "GROUP_MEMBERSHIP",
	        "operationType": "CREATE",
	        "resourcePath": "users/58bccc5e-abff-4a01-a7be-fb8d3895ce52/groups/71c03dec-6d6a-4adb-a205-6b53021a445f",
	        "representation": "{\"id\":\"71c03dec-6d6a-4adb-a205-6b53021a445f\",\"name\":\"worker\",\"path\":\"/worker\",\"subGroups\":[],\"attributes\":{},\"realmRoles\":[\"worker\"],\"clientRoles\":{}}",
	        "error": null,
	        "resourceTypeAsString": "GROUP_MEMBERSHIP"
        }
     */

    public override string MessageType => "auth-admin-events";
}