using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KafkaProducer.Producer
{
    public class KafkaProducer : IDisposable
    {
        private readonly ILogger<KafkaProducer> logger;
        private readonly string topic;
        private readonly IProducer<string, string> kafkaProducer;

        public KafkaProducer(IConfiguration config, ILogger<KafkaProducer> logger)
        {
            var producerConfig = new ProducerConfig();
            config.GetSection("Kafka:ProducerSettings").Bind(producerConfig);
            this.topic = config.GetValue<string>("Kafka:Topic");
            this.kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
            this.logger = logger;
        }

        public async Task ProduceAsync(Message<string, string> message)
        {
            try
            {
                var dr = await this.kafkaProducer.ProduceAsync(topic, message);
                logger.LogInformation($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                logger.LogInformation($"Delivery failed: {e.Error.Reason}");
            }
        }

        public void Dispose()
        {
            kafkaProducer.Flush();
            kafkaProducer.Dispose();
        }
    }
}
