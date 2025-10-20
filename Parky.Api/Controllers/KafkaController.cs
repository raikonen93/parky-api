using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Parky.Application.Dtos;
using Parky.Application.Interfaces;
using Parky.Domain.Enums;

namespace Parky.Api.Controllers
{
    /// <summary>
    /// Kafka message broker test controller.
    /// Demonstrates publish/subscribe, delayed messages, retries, and stream replay.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly ILogger<KafkaController> _logger;
        private readonly IKafkaProducerService _kafka;

        public KafkaController(ILogger<KafkaController> logger, IKafkaProducerService kafka)
        {
            _logger = logger;
            _kafka = kafka;
        }

        /// <summary>
        /// Publishes an event to a Kafka topic (fire-and-forget).
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> PublishEvent([FromBody] string message)
        {
            _logger.LogInformation("[Kafka] Publishing event: {@Message}", message);
            return await PublishToKafkaAsync(message, "parky.publish_event", "Publishing event");
        }

        /// <summary>
        /// Simulates message replay from a Kafka topic using offsets.
        /// </summary>
        [HttpPost("replay")]
        public async Task<IActionResult> ReplayMessages()
        {
            _logger.LogInformation("[Kafka] Replaying messages based on offset request");

            var command = new KafkaCommanDto
            {
                Command = KafkaCommand.Reply,
                TopicName = "parky.publish_event"
            };

            return await PublishToKafkaAsync(command, "parky.command", "Replaying messages based on offset request");
        }

        /// <summary>
        /// Simulates a consumer error (for testing DLQ and retries).
        /// </summary>
        [HttpPost("simulate-error")]
        public async Task<IActionResult> SimulateError()
        {
            _logger.LogWarning("[Kafka] Simulating consumer failure...");

            var command = new KafkaCommanDto
            {
                Command = KafkaCommand.SimulateError,
                TopicName = "parky.publish_event"
            };
            return await PublishToKafkaAsync("error", "parky.command", "Replaying messages based on offset request");
        }

        /// <summary>
        /// Health check for Kafka controller.
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var result = await _kafka.CheckHealthAsync();
            return result.Status == HealthStatus.Healthy
                ? Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow })
                : StatusCode(503, new { Status = "Unhealthy", result.Description });
        }

        private async Task<IActionResult> PublishToKafkaAsync<T>(T message, string topic, string logAction)
        {
            _logger.LogInformation("[Kafka] {Action}: {@Message}", logAction, message);
            var result = await _kafka.ProduceAsync(message, topic);

            return result == Confluent.Kafka.PersistenceStatus.Persisted
                ? Ok(new { Status = "Published", Topic = topic, Message = message })
                : BadRequest(new { Status = "Failed", Topic = topic, PersistenceStatus = result.ToString() });
        }
    }
}
