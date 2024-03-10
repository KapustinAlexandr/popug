using System.Text.Json.Nodes;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Json.Schema;
using Mxm.Kafka;

namespace Popug.Analytic.Api.Common;

public static class KafkaEventExtensions
{
    // Confluent клиент для совместимого хранилища от apicurio.
    private static readonly CachedSchemaRegistryClient SchemaRegistry = new(new SchemaRegistryConfig { Url = "http://apicurio:8080/apis/ccompat/v6" });
    
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