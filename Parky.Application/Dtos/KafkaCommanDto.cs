using Parky.Domain.Enums;

namespace Parky.Application.Dtos
{
    public class KafkaCommanDto
    {
        public string TopicName { get; set; }
        public KafkaCommand Command { get; set; }
    }
}
