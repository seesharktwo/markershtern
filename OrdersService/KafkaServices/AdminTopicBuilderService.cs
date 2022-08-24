using Confluent.Kafka;
using Confluent.Kafka.Admin;
using OrdersService.Services.KafkaSettings;

namespace OrdersService.KafkaServices
{
    public class AdminTopicBuilderService
    {
        WebApplication _webHost;
        public AdminTopicBuilderService(WebApplication webHost)
        {
            _webHost = webHost;
        }

        public async void TopicBuildAsync()
        {
            IKafkaSettings kafkaSettings = _webHost.Services.GetService<IKafkaSettings>();
            ILogger<WebApplication> logger = _webHost.Services.GetService<ILogger<WebApplication>>();

            if (kafkaSettings is null)
                throw new ArgumentNullException("kafkaSettings");
            if (logger is null)
                throw new ArgumentNullException("logger");

            using(var adminClient = new AdminClientBuilder(new AdminClientConfig { 
                BootstrapServers = kafkaSettings.BootstrapServers }).Build())
            {
                foreach(var item in kafkaSettings.Topicks)
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
                    catch(CreateTopicsException e)
                    {
                        logger.LogError($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                }
            }

            return;
        }
    }
}
