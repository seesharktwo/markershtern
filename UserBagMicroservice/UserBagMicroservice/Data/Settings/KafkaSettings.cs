namespace UserBagMicroservice.Data.Settings
{
    public class KafkaSettings : IKafkaSettings
    {
        public string BootstrapServers { get; set; } = null!;
        public string GroupId { get; set; } = null!;
        public string[] Topicks { get; set; } = null!;
    }
}
