namespace ProductService.Configs
{
    public class KafkaSettings
    {
        public string GroupId { get; set; }

        public string BootstrapServers { get; set; }

        public string[] Topicks { get; set; }
    }
}
