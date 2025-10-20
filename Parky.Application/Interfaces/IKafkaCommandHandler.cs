using Confluent.Kafka;
using Parky.Application.Dtos;
using Parky.Domain.Enums;

namespace Parky.Application.Interfaces
{
    public interface IKafkaCommandHandler
    {
        Task HandleAsync(KafkaCommanDto cmd, IConsumer<Ignore, string> consumer);
    }
}
