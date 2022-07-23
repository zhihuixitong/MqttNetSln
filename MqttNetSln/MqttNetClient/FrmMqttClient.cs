using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using ServiceStack.Text;

namespace MqttNetClient
{
    public partial class FrmMqttClient : Form
    {
        private IMqttClient _mqttClient;

        private Action<string> _updateListBoxAction;

        private List<IManagedMqttClient> managedMqttClients = new List<IManagedMqttClient>(); 
        public FrmMqttClient()
        {
            InitializeComponent();

            MqttNetGlobalLogger.LogMessagePublished += (o, args) =>
            {
                var s = new StringBuilder();
                s.Append($"{args.TraceMessage.Timestamp} ");
                s.Append($"{args.TraceMessage.Level} ");
                s.Append($"{args.TraceMessage.Source} ");
                s.Append($"{args.TraceMessage.ThreadId} ");
                s.Append($"{args.TraceMessage.Message} "); 
                s.Append($"{args.TraceMessage.Exception}");
                s.Append($"{args.TraceMessage.LogId} ");
            };
        }

        private void FrmMqttClient_Load(object sender, EventArgs e)
        {
            var ips = Dns.GetHostAddressesAsync(Dns.GetHostName());
            TxbServer.Text = ips.Result[1].ToString();
            foreach (var ip in ips.Result)
            {
                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        TxbServer.Text = ip.ToString();
                        break;
                    case AddressFamily.InterNetworkV6:
                        break;
                }
            }

            foreach (var value in Enum.GetValues(typeof(MqttQualityOfServiceLevel)))
            {
                CmbPubMqttQuality.Items.Add((int) value);
                CmbSubMqttQuality.Items.Add((int) value);
            }
            CmbPubMqttQuality.SelectedItem = 0;
            CmbSubMqttQuality.SelectedIndex = 0;


            _updateListBoxAction = new Action<string>((s) =>
            {
                listBox1.Items.Add(s);
                if (listBox1.Items.Count > 100)
                {
                    listBox1.Items.RemoveAt(0);
                }
            });

            
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            MqttClient();

           BtnConnect.Enabled = false;
           BtnDisConnect.Enabled = true;
           // MqttCAsync();
        }


        public async void MqttCAsync()
        {
            // Create a new MQTT client.
            var factory = new MqttFactory();
          var   mqttClient = factory.CreateMqttClient();
            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId("fdsdfsf")
                .WithCredentials("admin", "passwod")//用户名 密码
                .WithCleanSession()
                  .WithTcpServer("127.0.0.1", 3007) // Port is optional TCP 服务
                                                    //      .WithTls(
                                                    //new MqttClientOptionsBuilderTlsParameters
                                                    //{
                                                    //    UseTls = true,
                                                    //    CertificateValidationCallback = (X509Certificate x, X509Chain y, SslPolicyErrors z, IMqttClientOptions o) =>
                                                    //    {
                                                    //        // TODO: Check conditions of certificate by using above parameters.
                                                    //        return true;
                                                    //    },

                //})//类型 TCPS
                .Build();

            //重连机制
            mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(3));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            //消费消息
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");//主题
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");//页面信息
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");//消息等级
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");//是否保留
                Console.WriteLine();

                // Task.Run(() => mqttClient.PublishAsync("hello/world"));
            });



            //连接成功触发订阅主题
            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine($"### 成功连接 ###");

                // Subscribe to a topic
                //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("my/topic").Build());


                //Console.WriteLine("### SUBSCRIBED ###");
            });

            try
            {

                await mqttClient.ConnectAsync(options, CancellationToken.None);
                bool isExit = false;
                while (!isExit)
                {
                    Console.WriteLine(@"请输入
                    1.订阅主题
                    2.取消订阅
                    3.发送消息
                    4.退出输入exit");
                    var input = Console.ReadLine();
                    switch (input)
                    {
                        case "1":
                            Console.WriteLine(@"请输入主题名称：");
                            var topicName = Console.ReadLine();
                            await SubscribedTopic(topicName);
                            break;
                        case "2":
                            Console.WriteLine(@"请输入需要取消订阅主题名称：");
                            topicName = Console.ReadLine();
                            await UnsubscribedTopic(topicName);
                            break;
                        case "3":
                            Console.WriteLine("请输入需要发送的主题名称");
                            topicName = Console.ReadLine();
                            Console.WriteLine("请输入需要发送的消息");
                            var message = Console.ReadLine();

                            await PublishMessageAsync(new MQTTMessageModel() { Payload = message, Topic = topicName, Retain = true });
                            break;
                        case "exit":
                            isExit = true;
                            break;
                        default:
                            Console.WriteLine("请输入正确指令！");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }

        }

        private Task UnsubscribedTopic(string topicName)
        {
            throw new NotImplementedException();
        }

        private Task SubscribedTopic(string topicName)
        {
            throw new NotImplementedException();
        }

        private Task PublishMessageAsync(MQTTMessageModel mQTTMessageModel)
        {
            throw new NotImplementedException();
        }

        private async void BtnDisConnect_Click(object sender, EventArgs e)
        {
            if (null != _mqttClient && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
                _mqttClient = null;

            }


                



            BtnConnect.Enabled = true;
            BtnDisConnect.Enabled = false;
        }



        private void MqttClient_Disconnected(object sender, EventArgs e)
        {


         ////   _mqttClient.DisconnectAsync();

         //   Invoke((new Action(() =>
         //   {
         //       txtReceiveMessage.Clear();
         //       DateTime curTime = new DateTime();
         //       curTime = DateTime.UtcNow;
         //       txtReceiveMessage.AppendText($">> [{curTime.ToLongTimeString()}]");
         //       txtReceiveMessage.AppendText("已断开MQTT连接！" + Environment.NewLine);
         //   })));

         //   //Reconnecting
         //   if (isReconnect)
         //   {
         //       Invoke((new Action(() =>
         //       {
         //           txtReceiveMessage.AppendText("正在尝试重新连接" + Environment.NewLine);
         //       })));

         //       var options = new MqttClientOptionsBuilder()
         //           .WithClientId(txtClientId.Text)
         //           .WithTcpServer(txtIp.Text, Convert.ToInt32(txtPort.Text))
         //           .WithCredentials(txtUsername.Text, txtPsw.Text)
         //           //.WithTls()
         //           .WithCleanSession()
         //           .Build();
         //       Invoke((new Action(async () =>
         //       {
         //           await Task.Delay(TimeSpan.FromSeconds(5));
         //           try
         //           {
         //               await mqttClient.ConnectAsync(options);
         //           }
         //           catch
         //           {
         //               txtReceiveMessage.AppendText("### RECONNECTING FAILED ###" + Environment.NewLine);
         //           }
         //       })));
         //   }
         //   else
         //   {
         //       Invoke((new Action(() =>
         //       {
         //           txtReceiveMessage.AppendText("已下线！" + Environment.NewLine);
         //       })));
         //   }
        }



        private void BtnSubscribe_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    Task.Factory.StartNew(async () =>
            //    {
            //        await _mqttClient.SubscribeAsync(
            //            new List<TopicFilter>
            //            {
            //                new TopicFilter(
            //                    txbSubscribe.Text,
            //                    (MqttQualityOfServiceLevel)
            //                        Enum.Parse(typeof (MqttQualityOfServiceLevel), CmbSubMqttQuality.Text))
            //            });
            //    });
            //}
            //catch (Exception)
            //{
            //    throw;
            //

            Subscribe();
        }


        private async Task Subscribe()
        {
            string topic = txbSubscribe.Text.Trim();

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("订阅主题不能为空！");
                return;
            }

            if (!_mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT客户端尚未连接！");
                return;
            }

            // Subscribe to a topic
            await _mqttClient.SubscribeAsync(new TopicFilterBuilder()
                .WithTopic(topic)
                .WithAtMostOnceQoS()
                .Build()
                );
            Invoke((new Action(() =>
            {
                string dd = ($"已订阅[{topic}]主题{Environment.NewLine}");
                listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $"" + dd
                                    );
            })));

        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {

                Publish2();
                //Publish();
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task Publish()
        {
            string topic = TxbTopic.Text.Trim();

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("发布主题不能为空！");
                return;
            }

            string inputString = TxbPayload.Text;
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

                Invoke((new Action(() =>
                {
                   string dd=($"发布主题失败！" + Environment.NewLine + ex.Message + Environment.NewLine);
                    listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $"" + dd
                                    );
                })));
            }




        }

        private async Task Publish2()
        {

            try
            {
                string topic = TxbTopic.Text.Trim();
                while (1 == 1)
                {
                    if (_mqttClient != null)
                    {
                        Thread.Sleep(1000);//睡眠500毫秒，也就是0.5秒
                        var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload("{\"EventName\":\"carriage_pull_data_refresh\",\"TagValue\":\"\"}")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();
                        await _mqttClient.PublishAsync(message);

                        Thread.Sleep(1000);//睡眠500毫秒，也就是0.5秒
                        var message2 = new MqttApplicationMessageBuilder()
                     .WithTopic(topic)
                     .WithPayload("{\"EventName\":\"carriage_next_class\",\"TagValue\":{seats_state:[]}}")
                     .WithExactlyOnceQoS()
                      .WithRetainFlag()
                       .Build();
                        await _mqttClient.PublishAsync(message2);
                    }
                }
               
            }
            catch (Exception ex)
            {

                Invoke((new Action(() =>
                {
                    string dd = ($"发布主题失败！" + Environment.NewLine + ex.Message + Environment.NewLine);
                    listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $"" + dd
                                    );
                })));
            }




        }


        private void BtnMultiConnect_Click(object sender, EventArgs e)
        {
            MqttMultiClient(Convert.ToInt32(TxbConnectCount.Text));
        }

        private void BtnMultiDisConnect_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                foreach (var client in managedMqttClients)
                {
                    await  client.StopAsync();
                    client.Dispose();
                    Thread.Sleep(100);
                }
            });
        }

        /// <summary>
        /// 客户端连接MQTT服务器
        /// </summary>
        private async void MqttClient()
        {
            

            if (_mqttClient == null)
            {
                try
                {
                    var factory = new MqttFactory();
                    _mqttClient = factory.CreateMqttClient();

                    var options = new MqttClientOptionsBuilder()
                        .WithTcpServer("mqqt.onecatplay.com",1883).WithCredentials("root@server", "root2020").WithClientId(Guid.NewGuid().ToString("D")) // Port is optional
                        .Build();

                    
                    //连接服务器
                    await _mqttClient.ConnectAsync(options, CancellationToken.None);
                    Invoke((new Action(() =>
                    {
                        string dd = ($"连接到MQTT服务器成功！" + TxbServer.Text+":"+ TxbPort.Text);
                        listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $""+dd
                                    );

                       
                    })));


                    //接收消息
                    _mqttClient.UseApplicationMessageReceivedHandler(e =>
                    {

                        Invoke((new Action(() =>
                        {
                            string dd = ($"收到订阅消息！" + Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                            listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $"" + dd
                                    );
                        })));

                    });

                   //关闭连接
                    _mqttClient.UseDisconnectedHandler(e=> 
                    {

                        Invoke((new Action(() =>
                        {
                        string dd = ($"连接关闭！" );
                            listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $"" + dd
                                    );
                        })));
                    });


                }
                catch (Exception ex)
                {
                    _mqttClient = null;
                    Invoke((new Action(() =>
                    {
                        string dd=($"连接到MQTT服务器失败！" + Environment.NewLine + ex.Message + Environment.NewLine);
                        listBox1.BeginInvoke(
                                    _updateListBoxAction,
                                    $"" + dd
                                    );
                    })));
                }
            }
        }

        private async void MqttMultiClient( int clientsCount)
        {
            //await Task.Factory.StartNew(async () =>
            // {
            //     for (int i = 0; i < clientsCount; i++)
            //     {
            //         var options = new ManagedMqttClientOptionsBuilder()
            //         .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            //         .WithClientOptions(new MqttClientOptionsBuilder()
            //             .WithClientId(Guid.NewGuid().ToString().Substring(0, 13))
            //             .WithTcpServer(TxbServer.Text, Convert.ToInt32(TxbPort.Text))
            //             .WithCredentials("admin", "public")
            //             .Build()
            //         )
            //         .Build();

            //         var c = new MqttFactory().CreateManagedMqttClient();
            //         await c.SubscribeAsync(
            //             new TopicFilterBuilder().WithTopic(txbSubscribe.Text)
            //                 .WithQualityOfServiceLevel(
            //                     (MqttQualityOfServiceLevel)
            //                         Enum.Parse(typeof(MqttQualityOfServiceLevel), CmbSubMqttQuality.Text)).Build());

            //         await c.StartAsync(options);

            //         managedMqttClients.Add(c);

            //         Thread.Sleep(200);
            //     }
            // });
            
            
        }
    }
}
