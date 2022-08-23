namespace UserBagMicroservice.Data.Settings
{
    public interface IKafkaSettings
    {
        public string GroupId { get; set; }
        public string BootstrapServers { get; set; }
        public string[] Topicks { get; set; }
    }
}
