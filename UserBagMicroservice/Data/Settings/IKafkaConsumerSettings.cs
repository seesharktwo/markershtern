namespace UserBagMicroservice.Data.Settings
{
    public interface IKafkaConsumerSettings
    {
        public string GroupId { get; set; }
        public string BootstrapServers { get; set; }
    }
}
