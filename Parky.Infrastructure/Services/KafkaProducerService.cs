using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Parky.Application.Interfaces;
using System.Text.Json;

namespace Parky.Infrastructure.Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _defaultTopic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
        {
            var bootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9094";

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.Leader,
                MessageTimeoutMs = 2000
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _defaultTopic = config["Kafka:Topic"] ?? "parky.events";
            _logger = logger;
        }

        //Publish message
        public async Task<PersistenceStatus> ProduceAsync<T>(T message, string? topic = null, CancellationToken cancellationToken = default)
        {
            var targetTopic = topic ?? _defaultTopic;

            try
            {
                string json = JsonSerializer.Serialize(message,
                       new JsonSerializerOptions { WriteIndented = false });

                var result = await _producer.ProduceAsync(
                    targetTopic,
                    new Message<Null, string> { Value = json },
                    cancellationToken);

                _logger.LogInformation(
                    "[KafkaProducer] Sent message to {Topic} (offset: {Offset})",
                    result.Topic, result.Offset);

                return result.Status;
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "[KafkaProducer] Delivery failed: {Reason}", ex.Error.Reason);
                throw;
            }
        }

        //Health check
        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var testMessage = new Message<Null, string> { Value = $"Kafka ping {DateTime.UtcNow:O}" };

                var result = await _producer.ProduceAsync("check_health", testMessage, cancellationToken);

                if (result.Status == PersistenceStatus.NotPersisted)
                {
                    _logger.LogWarning("[KafkaProducer] Health ping not persisted to {Topic}", _defaultTopic);
                    return HealthCheckResult.Unhealthy("Kafka broker did not persist message");
                }

                _logger.LogInformation("[KafkaProducer] Health check successful");
                return HealthCheckResult.Healthy("Kafka broker reachable and producing messages.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KafkaProducer] Health check failed");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
