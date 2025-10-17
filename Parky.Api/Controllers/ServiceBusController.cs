using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Parky.Api.Controllers
{
    /// <summary>
    /// Azure Service Bus controller.
    /// Demonstrates queue/command messaging, topic publishing, RPC and DLQ testing.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceBusController : ControllerBase
    {
        private readonly ILogger<ServiceBusController> _logger;

        public ServiceBusController(ILogger<ServiceBusController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends a command message to a specific queue (point-to-point).
        /// </summary>
        [HttpPost("send-command")]
        public async Task<IActionResult> SendCommand([FromBody] object command)
        {
            _logger.LogInformation("[ServiceBus] Sending command: {@Command}", command);
            // TODO: Use MassTransit or Azure.Messaging.ServiceBus to send
            await Task.CompletedTask;
            return Ok(new { Status = "Command sent", Command = command });
        }

        /// <summary>
        /// Publishes an event to a Service Bus topic (broadcast).
        /// </summary>
        [HttpPost("publish-event")]
        public async Task<IActionResult> PublishEvent([FromBody] object evt)
        {
            _logger.LogInformation("[ServiceBus] Publishing event: {@Event}", evt);
            // TODO: Use TopicClient or MassTransit Publish()
            await Task.CompletedTask;
            return Ok(new { Status = "Event published", Event = evt });
        }

        /// <summary>
        /// Performs an RPC-style request/response operation.
        /// </summary>
        [HttpPost("rpc")]
        public async Task<IActionResult> RpcRequest([FromBody] object request)
        {
            _logger.LogInformation("[ServiceBus] RPC request sent: {@Request}", request);
            // TODO: Use RequestClient<T> in MassTransit or direct reply queue
            await Task.Delay(300);
            var response = new { Reply = "Simulated RPC response from Service Bus", Original = request };
            return Ok(response);
        }

        /// <summary>
        /// Tests delayed/scheduled message delivery (native Service Bus feature).
        /// </summary>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleMessage([FromBody] object message)
        {
            _logger.LogInformation("[ServiceBus] Scheduling message: {@Message}", message);
            // TODO: ServiceBusSender.ScheduleMessageAsync()
            await Task.CompletedTask;
            return Ok(new { Status = "Scheduled", Message = message });
        }

        /// <summary>
        /// Simulates an error for DLQ testing.
        /// </summary>
        [HttpPost("simulate-error")]
        public IActionResult SimulateError()
        {
            _logger.LogWarning("[ServiceBus] Simulating processing failure...");
            throw new InvalidOperationException("Simulated Service Bus consumer failure");
        }

        /// <summary>
        /// Health check for Service Bus controller.
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Broker = "Azure Service Bus",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
