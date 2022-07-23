namespace MqttNetClient
{
    internal class MQTTMessageModel
    {
        public MQTTMessageModel()
        {
        }

        public string Payload { get; set; }
        public string Topic { get; set; }
        public bool Retain { get; set; }
    }
}