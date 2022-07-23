using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetClient
{
  public  class Client
    {
        private IMqttClient _mqttClient;

        /// 客户端连接MQTT服务器
        /// </summary>
        public async Task MqttClientAsync(string ServiceIp,int Prot,string UseName,string Password, string Topic, string Message)
        {


            if (_mqttClient == null)
            {
                try
                {
                    var factory = new MqttFactory();
                    _mqttClient = factory.CreateMqttClient();

                    var options = new MqttClientOptionsBuilder()
                        .WithTcpServer(ServiceIp, Prot).WithCredentials(UseName, Password).WithClientId(Guid.NewGuid().ToString("D")) // Port is optional
                        .Build();


                    //连接服务器
                    await _mqttClient.ConnectAsync(options, CancellationToken.None);



                    //接收消息
                    _mqttClient.UseApplicationMessageReceivedHandler(e =>
                    {



                    });

                    //关闭连接
                    _mqttClient.UseDisconnectedHandler(e =>
                    {


                    });
                    //发送消息
                    await Publish(Topic, Message);
                    //关闭连接
                    Close();
                }
                catch (Exception ex)
                {
                    _mqttClient = null;
                  
                }
            }
        }

        private async Task Publish(string topic,string inputString)
        {

            try
            {

                var message = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(inputString)
        .WithExactlyOnceQoS()
        .WithRetainFlag()
        .Build();

                await _mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {

                
            }




        }

        private async void Close()
        {
            if (null != _mqttClient && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
                _mqttClient = null;

            }


        }


    }
}
