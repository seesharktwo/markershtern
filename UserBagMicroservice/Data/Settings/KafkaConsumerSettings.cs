namespace UserBagMicroservice.Data.Settings
{
    public class KafkaConsumerSettings : IKafkaConsumerSettings
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
    }
}
