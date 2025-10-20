using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Parky.Application.Interfaces
{
    public interface IKafkaProducerService
    {
        Task<PersistenceStatus> ProduceAsync<T>(T message, string? topic = null, CancellationToken cancellationToken = default);
        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
