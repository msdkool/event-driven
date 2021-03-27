using System;
namespace KafkaCDCClass.Configs
{
    public class ConsumerSettings
    {
        public string BootstrapServers { get;}
        public string GroupId { get; }
        public string SaslMechanism { get; }
        public string SecurityProtocol { get; }
    }
}
