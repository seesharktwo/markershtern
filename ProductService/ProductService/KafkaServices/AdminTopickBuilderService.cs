using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using ProductService.Configs;

namespace ProductService.KafkaServices
{
    public class AdminTopickBuilderService
    {
        WebApplication _webHost;
        public AdminTopickBuilderService(WebApplication webHost)
        {
            _webHost = webHost;
        }

        /// <summary>
        /// Расширение для инициализации топиков
        /// </summary>
        /// <param name="webHost"></param>
        /// <returns></returns>
        public async void TopicsBuildAsync()
        {
            KafkaSettings kafkaSettings = _webHost.Services.GetService<IOptions<KafkaSettings>>().Value;
            ILogger<WebApplication> logger = _webHost.Services.GetService<ILogger<WebApplication>>();
            if (kafkaSettings is null)
                throw new ArgumentNullException("kafkaSettings");
            if (logger is null)
                throw new ArgumentNullException("logger");
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = kafkaSettings.BootstrapServers }).Build())
            {
                foreach (var item in kafkaSettings.Topicks)
                {
                    try
                    {
                        await adminClient.CreateTopicsAsync(new TopicSpecification[]
                        {
                        new TopicSpecification
                        {
                            Name = item,
                            ReplicationFactor = 1,
                            NumPartitions = 1
                        }
                        });

                        logger.LogInformation($"Was created topic {item}");
                    }
                    catch (CreateTopicsException e)
                    {
                        logger.LogError($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }

            return;
        }
     }
}
