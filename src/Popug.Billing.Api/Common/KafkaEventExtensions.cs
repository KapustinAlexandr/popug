using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Json.Schema;
using Mxm.Kafka;

namespace Popug.Tasks.Api.Common;

public static class KafkaEventExtensions
{
    // Confluent клиент для совместимого хранилища от apicurio.
    private static readonly CachedSchemaRegistryClient SchemaRegistry = new(new SchemaRegistryConfig { Url = "http://apicurio:8080/apis/ccompat/v6" });
    
    // Валидация схемы
    private static async Task ValidateSchema<T>(this T source, string topicName, int version)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        // Получаем из хранилища схему для топика нужной версии
        var schema = await SchemaRegistry.GetRegisteredSchemaAsync($"{topicName}-value", version);

        // Переводим объект в json
        var dataJson = JsonSerializer.SerializeToNode(source, JsonSerializerOptions.Default);

        // Проверяем получившийся json на соотв. схеме.
        var result = JsonSchema.FromText(schema.SchemaString).Evaluate(dataJson, new EvaluationOptions { AddAnnotationForUnknownKeywords = true, OutputFormat = OutputFormat.Hierarchical });

        if (!result.IsValid)
        {
            throw new Exception(
                $"Validation failed for schema {topicName}-value version {version}.\n{string.Join("\n", result.Errors!.Values)}");
        }
    }
    
    // Мета-данные события в Header кафки. Id и версия схемы.
    private static Headers GetEventMeta(int version)
    {
        return new Headers
        {
            { "mxg-event-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
            { "mxg-event-schema-version", Encoding.UTF8.GetBytes(version.ToString()) }
        };
    }

    // Отправка события в кафку
    public static async Task SendEvent<TData>(this KafkaProducer producer, string topicName, TData eventData, int version, object? key = null)
    {
        await eventData.ValidateSchema(topicName, version);

        await producer.SendMessage(topicName, eventData, key?.ToString(), GetEventMeta(version));
    }


    public static async Task<T> ValidateAndGetData<T>(this Message<string, string> source, string topicName, int version)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        // Получаем из хранилища схему для топика нужной версии
        var schema = await SchemaRegistry.GetRegisteredSchemaAsync($"{topicName}-value", version);

        // Переводим объект в json
        var dataJson = JsonNode.Parse(source.Value);

        // Проверяем получившийся json на соотв. схеме.
        var result = JsonSchema.FromText(schema.SchemaString).Evaluate(dataJson, new EvaluationOptions { AddAnnotationForUnknownKeywords = true, OutputFormat = OutputFormat.Hierarchical });

        if (!result.IsValid)
        {
            throw new Exception(
                $"Validation failed for schema {topicName}-value version {version}.\n{string.Join("\n", result.Errors!.Values)}");
        }

        return source.Value.FromJson<T>();
    }

    public static int GetSchemaVersion(this Message<string, string> source)
    {
        return int.Parse(source.Headers.First(w => w.Key == "mxg-event-schema-version").GetValueBytes());
    }
}