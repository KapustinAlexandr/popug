﻿using Confluent.Kafka;
using LinqToDB;
using Microsoft.Extensions.Options;
using Mxm.Kafka;
using Popug.Tasks.Api.Data;

namespace Popug.Tasks.Api.Logic;

public class AuthEventsConsumer : KafkaConsumer
{
    private readonly IServiceProvider _serviceProvider;

    public AuthEventsConsumer(
        ILogger<KafkaConsumer> logger, 
        IOptionsMonitor<KafkaConsumerOptions> options, 
        IServiceProvider serviceProvider) : base(logger, options)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Обработка пользовательских сообщений keycloak из топика auth-events.
    /// Регистрация, входы, выходы и все такое сервер шлёт в этот топик.
    /// </summary>
    public override async Task OnMessage(Message<string, string> message)
    {
        var data = message.Value.FromJson<AuthEventData>();

        // Нас интересует только появление нового попуга.
        // Отслеживание изменений в следующей версии продукта.
        if (data?.Type == "REGISTER")
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            
            await scope.ServiceProvider.GetRequiredService<TasksDb>().InsertAsync(new PopugEntity
            {
                UserId = data.UserId,
                UserName = $"{data.Details.Email} ({string.Join(" ", data.Details.FirstName, data.Details.LastName)}",
                // так как keycloak не отдаёт нам какую группу или роль мы получили автоматом при регистрации,
                // просто знаем что все изначально в worker попадают.
                // Вот такой лайфхак для домашки
                UserRole = "worker" 
            });
        }
    }

    /*  Пример ожидаемого сообщения
        {
	        "id": "f8383bec-86a7-4dd7-b9a9-051afed0bd5d",
	        "time": 1709325106837,
	        "type": "REGISTER",
	        "realmId": "393de734-2ae6-4d87-8f13-512d50f8d87c",
	        "clientId": "account-console",
	        "userId": "ac264f15-7bb4-4b68-8408-5fa2d55f1460",
	        "sessionId": null,
	        "ipAddress": "172.23.0.1",
	        "error": null,
	        "details": {
		        "auth_method": "openid-connect",
		        "auth_type": "code",
		        "register_method": "form",
		        "last_name": "Manager",
		        "redirect_uri": "http://localhost:8080/realms/popug/account/#/",
		        "first_name": "Jhon",
		        "code_id": "3097b753-f98a-4926-94db-63811e6fa750",
		        "email": "jhon.manager@popug.com",
		        "username": "jhon.manager@popug.com"
	        }
        }     
     */

    public override string MessageType => "auth-events";
}