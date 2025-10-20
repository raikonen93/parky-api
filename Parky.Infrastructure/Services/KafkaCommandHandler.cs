using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parky.Application.Dtos;
using Parky.Application.Interfaces;
using Parky.Domain.Enums;

namespace Parky.Infrastructure.Services
{
    public class KafkaCommandHandler : IKafkaCommandHandler
    {
        private readonly ILogger<KafkaCommandHandler> _logger;
        private readonly string _bootstrapServers;

        public KafkaCommandHandler(ILogger<KafkaCommandHandler> logger, IConfiguration config)
        {
            _logger = logger;
            _bootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9094";
        }

        public async Task HandleAsync(KafkaCommanDto cmd, IConsumer<Ignore, string> consumer)
        {
            switch (cmd.Command)
            {
                case KafkaCommand.Reply:
                    await ReplayAsync(consumer, cmd.TopicName);
                    break;

                case KafkaCommand.SimulateError:
                    await SimulateErrorAsync();
                    break;

                default:
                    _logger.LogWarning("Unknown command received: {Cmd}", cmd);
                    break;
            }
        }

        private async Task ReplayAsync(IConsumer<Ignore, string> consumer, string topicName)
        {
            _logger.LogInformation("Replaying messages from offset 0...");

            var partition = new Partition(0);
            consumer.Assign(new TopicPartition(topicName, partition));
            consumer.Seek(new TopicPartitionOffset(topicName, partition, new Offset(0)));
            await Task.CompletedTask;
        }

        private async Task SimulateErrorAsync()
        {
            const string dlqTopic = "parky.publish_event.DLQ";
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    throw new Exception("Simulated exception occurred");
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "Error attempt {Attempt}", retryCount);
                    if (retryCount >= maxRetries)
                    {
                        await ProduceToDlqAsync(dlqTopic, $"Failed after {maxRetries} attempts");
                    }
                    else
                    {
                        _logger.LogInformation("Retrying...");
                        await Task.Delay(1000);
                    }
                }
            }
        }

        private async Task ProduceToDlqAsync(string topic, string message)
        {
            using var producer = new ProducerBuilder<Null, string>(
                new ProducerConfig { BootstrapServers = _bootstrapServers }).Build();

            await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            _logger.LogInformation("Message moved to DLQ topic {Topic}", topic);
        }
    }
}
