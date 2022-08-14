using Confluent.Kafka.Admin;
using Confluent.Kafka;
using UserBalanceMicroservice.Configs;

namespace UserBalanceMicroservice.Services
{
    public static class AdminTopickBuilderService
    {
        public async static void Build()
        {
            var config = new ConfigurationBuilder();
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json");
            IConfigurationRoot root = config.Build();
            var settings = new KafkaSettings();
            root.GetSection("BootstrapServerKafka").Bind(settings);
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = settings.BootstrapServers }).Build())
            {
                foreach (var item in settings.Topicks)
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
                    }
                    catch (CreateTopicsException e)
                    {
                        Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
        }
    }
}
