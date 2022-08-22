namespace ProductService.Configs
{
    public class KafkaConsumerSettings
    {
        public string GroupId { get; set; }

        public string BootstrapServers { get; set; }
    }
}
