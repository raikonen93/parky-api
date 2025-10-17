using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        public KafkaController(ILogger<KafkaController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Publishes an event to a Kafka topic (fire-and-forget).
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> PublishEvent([FromBody] object message)
        {
            _logger.LogInformation("[Kafka] Publishing event: {@Message}", message);
            // TODO: Use Confluent.Kafka Producer to send to topic
            await Task.CompletedTask;
            return Ok(new { Status = "Published", Message = message });
        }

        /// <summary>
        /// Simulates consuming messages from a Kafka topic.
        /// </summary>
        [HttpGet("consume")]
        public async Task<IActionResult> ConsumeTopic([FromQuery] string topic)
        {
            _logger.LogInformation("[Kafka] Subscribing to topic: {Topic}", topic);
            // TODO: Use Kafka consumer (poll loop)
            await Task.Delay(200);
            return Ok(new { Topic = topic, Status = "Consumed sample messages" });
        }

        /// <summary>
        /// Simulates delayed (scheduled) message publishing.
        /// </summary>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleMessage([FromBody] object message)
        {
            _logger.LogInformation("[Kafka] Scheduling message: {@Message}", message);
            // TODO: Implement delayed message (store + produce later)
            await Task.CompletedTask;
            return Ok(new { Status = "Scheduled", Message = message });
        }

        /// <summary>
        /// Simulates message replay from a Kafka topic using offsets.
        /// </summary>
        [HttpPost("replay")]
        public async Task<IActionResult> ReplayMessages([FromBody] object replayRequest)
        {
            _logger.LogInformation("[Kafka] Replaying messages based on offset request: {@Request}", replayRequest);
            // TODO: Use KafkaConsumer.Assign() and Seek() to reprocess messages
            await Task.CompletedTask;
            return Ok(new { Status = "Replayed messages", Replay = replayRequest });
        }

        /// <summary>
        /// Simulates a consumer error (for testing DLQ and retries).
        /// </summary>
        [HttpPost("simulate-error")]
        public IActionResult SimulateError()
        {
            _logger.LogWarning("[Kafka] Simulating consumer failure...");
            throw new InvalidOperationException("Simulated Kafka consumer failure");
        }

        /// <summary>
        /// Health check for Kafka controller.
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Broker = "Kafka",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
