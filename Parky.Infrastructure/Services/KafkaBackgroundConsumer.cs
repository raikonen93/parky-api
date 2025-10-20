using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parky.Application.Dtos;
using Parky.Application.Interfaces;
using System.Text.Json;

namespace Parky.Infrastructure.Services
{
    public class KafkaBackgroundConsumer : BackgroundService
    {
        private readonly ConsumerConfig _config;
        private readonly string[] _topics;
        private readonly ILogger<KafkaBackgroundConsumer> _logger;
        private readonly IKafkaCommandHandler _handler;

        public KafkaBackgroundConsumer(IConfiguration config, ILogger<KafkaBackgroundConsumer> logger, IKafkaCommandHandler handler)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9094",
                GroupId = config["Kafka:GroupId"] ?? "parky-consumer-api",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                SecurityProtocol = SecurityProtocol.Plaintext,
                //Debug = "consumer,cgrp,topic,fetch"
            };

            _handler = handler;
            _topics = config.GetSection("Kafka:Topics").Get<string[]>() ?? new[] { "parky.events" };
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Connecting to Kafka at {Servers}", _config.BootstrapServers);
            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            consumer.Subscribe(_topics);

            _logger.LogInformation("Kafka consumer started for all topics");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);

                        if (result?.Message != null)
                        {
                            if (result.Topic == "parky.command")
                            {
                                var command = JsonSerializer.Deserialize<KafkaCommanDto>(result.Message.Value);
                                await _handler.HandleAsync(command, consumer);
                            }
                            else
                            {
                                _logger.LogInformation("Received message: {Message}", result.Message.Value);
                            }

                            consumer.Commit(result);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consume error: {Reason}", ex.Error.Reason);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Kafka consumer stopping...");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
