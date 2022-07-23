//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Windows.Forms;

//using MQTTnet;
//using MQTTnet.Server;
//using MQTTnet.Protocol;
//using ServiceStack;
//using ServiceStack.Text.Common;
//using MQTTnet.Client.Receiving;
//using System.Threading.Tasks;
//using MQTTnet.Client.Options;

//namespace MqttNetServer
//{
//    public partial class FrmMqttServer : Form
//    {

//        private IMqttServer _mqttServer = null;

//        private Action<string> _updateListBoxAction;
//        public FrmMqttServer()
//        {
//            InitializeComponent();
//        }

//        private void FrmMqttServer_Load(object sender, EventArgs e)
//        {
//            var ips = Dns.GetHostAddressesAsync(Dns.GetHostName());

//            foreach (var ip in ips.Result)
//            {
//                switch (ip.AddressFamily)
//                {
//                    case AddressFamily.InterNetwork:
//                        TxbServer.Text = ip.ToString();
//                        break;
//                    case AddressFamily.InterNetworkV6:
//                        break;
//                }
//            }

//            _updateListBoxAction = new Action<string>((s) =>
//            {
//                listBox1.Items.Add(s);
//                if (listBox1.Items.Count > 1000)
//                {
//                    listBox1.Items.RemoveAt(0);
//                }
//                var visibleItems = listBox1.ClientRectangle.Height/listBox1.ItemHeight;

//                listBox1.TopIndex = listBox1.Items.Count - visibleItems + 1;
//            });


//            listBox1.KeyPress += (o, args) =>
//            {
//                if (args.KeyChar == 'c' || args.KeyChar=='C')
//                {
//                    listBox1.Items.Clear();
//                }
//            };

//            BtnStart.Enabled = true;
//            BtnStop.Enabled = false;
//            TxbServer.Enabled = true;
//            TxbPort.Enabled = true;
//        }

//        private void BtnStart_Click(object sender, EventArgs e)
//        {
//            MqttServer();
//            //MqttServiceV3();
//            BtnStart.Enabled = false;
//            BtnStop.Enabled = true;
//            TxbServer.Enabled = false;
//            TxbPort.Enabled = false;
//        }

//        private void BtnStop_Click(object sender, EventArgs e)
//        {
//            if (null != _mqttServer )
//            {
//               // GetClientSessionsStatusAsync
//                foreach (var clientSessionStatuse in _mqttServer.GetClientStatusAsync().Result)
//                {
//                    clientSessionStatuse.DisconnectAsync();
//                }
//                _mqttServer.StopAsync();
//                _mqttServer = null;
//            }
//            BtnStart.Enabled = true;
//            BtnStop.Enabled = false;
//            TxbServer.Enabled = true;
//            TxbPort.Enabled = true;
//        }

//        private async void MqttServiceV3()
//        {
//            try
//            {
//                var options = new MqttServerOptions
//                {
//                    //连接验证
//                    ConnectionValidator = new MqttServerConnectionValidatorDelegate(p =>
//                    {
//                        if (p.ClientId == "SpecialClient")
//                        {
//                            if (p.Username != "USER" || p.Password != "PASS")
//                            {
//                                p.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
//                            }
//                        }
//                    }),

//                    //   Storage = new RetainedMessageHandler(),

//                    //消息拦截发送验证
//                    ApplicationMessageInterceptor = new MqttServerApplicationMessageInterceptorDelegate(context =>
//                    {
//                        if (MqttTopicFilterComparer.IsMatch(context.ApplicationMessage.Topic, "/myTopic/WithTimestamp/#"))
//                        {
//                            // Replace the payload with the timestamp. But also extending a JSON 
//                            // based payload with the timestamp is a suitable use case.
//                            context.ApplicationMessage.Payload = Encoding.UTF8.GetBytes(DateTime.Now.ToString("O"));
//                        }

//                        if (context.ApplicationMessage.Topic == "not_allowed_topic")
//                        {
//                            context.AcceptPublish = false;
//                            context.CloseConnection = true;
//                        }
//                    }),
//                    ///订阅拦截验证
//                    SubscriptionInterceptor = new MqttServerSubscriptionInterceptorDelegate(context =>
//                    {
//                        if (context.TopicFilter.Topic.StartsWith("admin/foo/bar") && context.ClientId != "theAdmin")
//                        {
//                            context.AcceptSubscription = false;
//                        }

//                        if (context.TopicFilter.Topic.StartsWith("the/secret/stuff") && context.ClientId != "Imperator")
//                        {
//                            context.AcceptSubscription = false;
//                            context.CloseConnection = true;
//                        }
//                    })
//                };

//                // Extend the timestamp for all messages from clients.
//                // Protect several topics from being subscribed from every client.

//                //var certificate = new X509Certificate(@"C:\certs\test\test.cer", "");
//                //options.TlsEndpointOptions.Certificate = certificate.Export(X509ContentType.Cert);
//                //options.ConnectionBacklog = 5;
//                //options.DefaultEndpointOptions.IsEnabled = true;
//                //options.TlsEndpointOptions.IsEnabled = false;

//                var mqttServer = new MqttFactory().CreateMqttServer();

//                mqttServer.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
//                {
//                    Console.WriteLine(
//                        $"'{e.ClientId}' reported '{e.ApplicationMessage.Topic}' > '{Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? new byte[0])}'",
//                        ConsoleColor.Magenta);
//                });

//                //options.ApplicationMessageInterceptor = c =>
//                //{
//                //    if (c.ApplicationMessage.Payload == null || c.ApplicationMessage.Payload.Length == 0)
//                //    {
//                //        return;
//                //    }

//                //    try
//                //    {
//                //        var content = JObject.Parse(Encoding.UTF8.GetString(c.ApplicationMessage.Payload));
//                //        var timestampProperty = content.Property("timestamp");
//                //        if (timestampProperty != null && timestampProperty.Value.Type == JTokenType.Null)
//                //        {
//                //            timestampProperty.Value = DateTime.Now.ToString("O");
//                //            c.ApplicationMessage.Payload = Encoding.UTF8.GetBytes(content.ToString());
//                //        }
//                //    }
//                //    catch (Exception)
//                //    {
//                //    }
//                //};


//                //开启订阅以及取消订阅
//                mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(e=> 
//                {
//                });
//                mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(e=>
//                {
//                });

//                //客户端连接事件
//                mqttServer.UseClientConnectedHandler(MqttServer_ClientConnected);

//                //客户端断开事件
//                mqttServer.UseClientDisconnectedHandler(MqttServer_ClientDisConnected);
//                await mqttServer.StartAsync(options);

//                Console.WriteLine("服务启动成功,输入任意内容并回车停止服务");
//                Console.ReadLine();

//                await mqttServer.StopAsync();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }

//            Console.ReadLine();

//        }



//        private Task MqttServer_ClientDisConnected(MqttServerClientDisconnectedEventArgs arg)
//        {
//            throw new NotImplementedException();
//        }

//        private Task MqttServer_ClientConnected(MqttServerClientConnectedEventArgs arg)
//        {
//            throw new NotImplementedException();
//        }



//        //private IMqttServerOptions CreateMqttServerOptions()
//        //{
//        //    var options = new MqttServerOptionsBuilder()
//        //        .WithMaxPendingMessagesPerClient(10)
//        //        .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(500))
//        //        .WithConnectionValidator(x=>x.Password="",)
//        //        .WithApplicationMessageInterceptor(_mqttApplicationMessageInterceptor)
//        //        .WithSubscriptionInterceptor(_mqttSubscriptionInterceptor)
//        //        .WithStorage(_mqttServerStorage);

//        //    // Configure unencrypted connections
//        //    if (_settings.TcpEndPoint.Enabled)
//        //    {
//        //        options.WithDefaultEndpoint();

//        //        if (_settings.TcpEndPoint.TryReadIPv4(out var address4))
//        //        {
//        //            options.WithDefaultEndpointBoundIPAddress(address4);
//        //        }

//        //        if (_settings.TcpEndPoint.TryReadIPv6(out var address6))
//        //        {
//        //            options.WithDefaultEndpointBoundIPV6Address(address6);
//        //        }

//        //        if (_settings.TcpEndPoint.Port > 0)
//        //        {
//        //            options.WithDefaultEndpointPort(_settings.TcpEndPoint.Port);
//        //        }
//        //    }
//        //    else
//        //    {
//        //        options.WithoutDefaultEndpoint();
//        //    }

//        //    // Configure encrypted connections
//        //    if (_settings.EncryptedTcpEndPoint.Enabled)
//        //    {
//        //        options
//        //            .WithEncryptedEndpoint()
//        //            .WithEncryptionSslProtocol(SslProtocols.Tls12);

//        //        if (!string.IsNullOrEmpty(_settings.EncryptedTcpEndPoint?.Certificate?.Path))
//        //        {
//        //            IMqttServerCertificateCredentials certificateCredentials = null;

//        //            if (!string.IsNullOrEmpty(_settings.EncryptedTcpEndPoint?.Certificate?.Password))
//        //            {
//        //                certificateCredentials = new MqttServerCertificateCredentials
//        //                {
//        //                    Password = _settings.EncryptedTcpEndPoint.Certificate.Password
//        //                };
//        //            }

//        //            options.WithEncryptionCertificate(_settings.EncryptedTcpEndPoint.Certificate.ReadCertificate(), certificateCredentials);
//        //        }

//        //        if (_settings.EncryptedTcpEndPoint.TryReadIPv4(out var address4))
//        //        {
//        //            options.WithEncryptedEndpointBoundIPAddress(address4);
//        //        }

//        //        if (_settings.EncryptedTcpEndPoint.TryReadIPv6(out var address6))
//        //        {
//        //            options.WithEncryptedEndpointBoundIPV6Address(address6);
//        //        }

//        //        if (_settings.EncryptedTcpEndPoint.Port > 0)
//        //        {
//        //            options.WithEncryptedEndpointPort(_settings.EncryptedTcpEndPoint.Port);
//        //        }
//        //    }
//        //    else
//        //    {
//        //        options.WithoutEncryptedEndpoint();
//        //    }

//        //    if (_settings.ConnectionBacklog > 0)
//        //    {
//        //        options.WithConnectionBacklog(_settings.ConnectionBacklog);
//        //    }

//        //    if (_settings.EnablePersistentSessions)
//        //    {
//        //        options.WithPersistentSessions();
//        //    }

//        //    return options.Build();
//        //}

//        //private static Action<MqttConnectionValidatorContext> ValidatingMqttClients()
//        //{
//        //    // Setup client validator.    
//        //    var options = new MqttServerOptions();
//        //    options.ConnectionValidator = new MqttConnectionValidatorContext(p =>
//        //        {
//        //            if (p.ClientId == "SpecialClient")
//        //            {
//        //                if (p.Username != "USER" || p.Password != "PASS")
//        //                {
//        //                    p.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
//        //                }
//        //            }
//        //        });

//        //    //options.ConnectionValidator = c =>
//        //    //{
//        //    //    Dictionary<string, string> c_u = new Dictionary<string, string>();
//        //    //    c_u.Add("client001", "username001");
//        //    //    c_u.Add("client002", "username002");
//        //    //    Dictionary<string, string> u_psw = new Dictionary<string, string>();
//        //    //    u_psw.Add("username001", "psw001");
//        //    //    u_psw.Add("username002", "psw002");

//        //    //    if (c_u.ContainsKey(c.ClientId) && c_u[c.ClientId] == c.Username)
//        //    //    {
//        //    //        if (u_psw.ContainsKey(c.Username) && u_psw[c.Username] == c.Password)
//        //    //        {
//        //    //            c.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
//        //    //        }
//        //    //        else
//        //    //        {
//        //    //            c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
//        //    //        }
//        //    //    }
//        //    //    else
//        //    //    {
//        //    //        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
//        //    //    }
//        //    //};
//        //    return options.ConnectionValidator;
//        //}

//        private async void MqttServer()
//        {
//            if (null != _mqttServer)
//            {
//                return;
//            }

//            var optionBuilder =
//                new MqttServerOptionsBuilder().WithConnectionBacklog(1000).WithDefaultEndpointPort(Convert.ToInt32(TxbPort.Text));

//            if (!TxbServer.Text.IsNullOrEmpty())
//            {
//                optionBuilder.WithDefaultEndpointBoundIPAddress(IPAddress.Parse(TxbServer.Text));
//            }
//           // optionBuilder.WithConnectionValidator(ValidatingMqttClients());

//               var options = optionBuilder.Build();


//            //var options = new MqttServerOptions
//            //{
//            //    //连接验证
//            //    ConnectionValidator = new MqttServerConnectionValidatorDelegate(p =>
//            //    {
//            //        if (p.ClientId == "SpecialClient")
//            //        {
//            //            if (p.Username != "USER" || p.Password != "PASS")
//            //            {
//            //                p.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
//            //            }
//            //        }
//            //    }),
//            //};



//            _mqttServer = new MqttFactory().CreateMqttServer();

//            //连接ClientConnected
//            //_mqttServer.ClientConnected += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction, $">Client Connected:ClientId:{args.ClientId},ProtocalVersion:");

//            //    var s = _mqttServer.GetClientSessionsStatusAsync();
//            //    label3.BeginInvoke(new Action(() => { label3.Text = $"连接总数：{s.Result.Count}"; }));
//            //};
//            _mqttServer.UseClientConnectedHandler(e =>
//            {
//              string dss= e.ClientId;
//                return;


//            });


//            //连接关闭
//            //_mqttServer.ClientDisconnected += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction, $"<Client DisConnected:ClientId:{args.ClientId}");
//            //    var s = _mqttServer.GetClientSessionsStatusAsync();
//            //    label3.BeginInvoke(new Action(() => { label3.Text = $"连接总数：{s.Result.Count}"; }));
//            //};
//            _mqttServer.UseClientDisconnectedHandler(e =>
//            {
//                string saads = e.ClientId;
//            });


//            //转发消息
//            //_mqttServer.ApplicationMessageReceived += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction,
//            //        $"ClientId:{args.ClientId} Topic:{args.ApplicationMessage.Topic} Payload:{Encoding.UTF8.GetString(args.ApplicationMessage.Payload)} QualityOfServiceLevel:{args.ApplicationMessage.QualityOfServiceLevel}");

//            //};
//            _mqttServer.UseApplicationMessageReceivedHandler(e =>
//            {
//                string dsad = e.ClientId;


//            });


//            //订阅
//            //_mqttServer.ClientSubscribedTopic += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction, $"@ClientSubscribedTopic ClientId:{args.ClientId} Topic:{args.TopicFilter.Topic} QualityOfServiceLevel:{args.TopicFilter.QualityOfServiceLevel}");
//            //};

//            _mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(e =>
//            {
//            });

//            //取消订阅
//            //_mqttServer.ClientUnsubscribedTopic += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction, $"%ClientUnsubscribedTopic ClientId:{args.ClientId} Topic:{args.TopicFilter.Length}");
//            //};

//            _mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(e =>
//            {
//            });

//            var dd=  _mqttServer.StartedHandler;

//            var dad = _mqttServer.StoppedHandler;
//            //_mqttServer.Started += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction, "Mqtt Server Start...");
//            //};

//            //_mqttServer.Stopped += (sender, args) =>
//            //{
//            //    listBox1.BeginInvoke(_updateListBoxAction, "Mqtt Server Stop...");

//            //};

//            await _mqttServer.StartAsync(options);
//        }









//    }
//}



using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;
using ServiceStack;
using ServiceStack.Text.Common;

namespace MqttNetServer
{
    public partial class FrmMqttServer : Form
    {

        private IMqttServer _mqttServer = null;

        private Action<string> _updateListBoxAction;
        public FrmMqttServer()
        {
            InitializeComponent();
        }

        private void FrmMqttServer_Load(object sender, EventArgs e)
        {
            var ips = Dns.GetHostAddressesAsync(Dns.GetHostName());

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

            _updateListBoxAction = new Action<string>((s) =>
            {
                listBox1.Items.Add(s);
                if (listBox1.Items.Count > 1000)
                {
                    listBox1.Items.RemoveAt(0);
                }
                var visibleItems = listBox1.ClientRectangle.Height / listBox1.ItemHeight;

                listBox1.TopIndex = listBox1.Items.Count - visibleItems + 1;
            });


            listBox1.KeyPress += (o, args) =>
            {
                if (args.KeyChar == 'c' || args.KeyChar == 'C')
                {
                    listBox1.Items.Clear();
                }
            };

            BtnStart.Enabled = true;
            BtnStop.Enabled = false;
            TxbServer.Enabled = true;
            TxbPort.Enabled = true;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            MqttServer();
            BtnStart.Enabled = false;
            BtnStop.Enabled = true;
            TxbServer.Enabled = false;
            TxbPort.Enabled = false;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (null != _mqttServer)
            {
                foreach (var clientSessionStatuse in _mqttServer.GetClientSessionsStatusAsync().Result)
                {
                    clientSessionStatuse.DisconnectAsync();
                }
                _mqttServer.StopAsync();
                _mqttServer = null;
            }
            BtnStart.Enabled = true;
            BtnStop.Enabled = false;
            TxbServer.Enabled = true;
            TxbPort.Enabled = true;
        }

        /// <summary>
        /// 启动服务使用的mqtt2.8.5.0
        /// </summary>
        private async void MqttServer()
        {
            try
            {
                if (null != _mqttServer)
                {
                    return;
                }

                var optionBuilder =
                    new MqttServerOptionsBuilder().WithConnectionBacklog(1000).WithDefaultEndpointPort(Convert.ToInt32(TxbPort.Text));

                if (!TxbServer.Text.IsNullOrEmpty())
                {
                    optionBuilder.WithDefaultEndpointBoundIPAddress(IPAddress.Parse(TxbServer.Text));
                }

                //连接配置
                var options = optionBuilder.Build();

                (options as MqttServerOptions).ConnectionValidator += context =>
                {
                    if (context.ClientId.Length < 10)
                    {
                        context.ReturnCode = MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
                        return;
                    }


                    //if (!context.Username.Equals("root@server"))//账号
                    //{
                    //    context.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    //    return;
                    //}
                    //if (!context.Password.Equals("root2020"))//密码
                    //{
                    //    context.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    //    return;
                    //}

                    var AllConnect = _mqttServer.GetClientSessionsStatus();
                    foreach(var i in AllConnect)
                    {
                        if(i.ClientId==context.ClientId)
                        {
                            context.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                            return;
                        }
                    }
                    context.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;

                };

                _mqttServer = new MqttFactory().CreateMqttServer();
                _mqttServer.ClientConnected += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction, $">客户端连接:客户端ID:{args.ClientId},ProtocalVersion:");

                    var s = _mqttServer.GetClientSessionsStatus().Count;

                    label3.BeginInvoke(new Action(() => { label3.Text = $"连接总数：{s}"; }));
                };

                _mqttServer.ClientDisconnected += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction, $"<客户端关闭:客户端ID:{args.ClientId}");
                    //var sf = _mqttServer.GetClientSessionsStatusAsync();
                    var s = _mqttServer.GetClientSessionsStatus().Count;
                    label3.BeginInvoke(new Action(() => { label3.Text = $"连接总数：{s - 1}"; }));
                };

                _mqttServer.ApplicationMessageReceived += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction,
                        $"客户端ID:{args.ClientId} 主题:{args.ApplicationMessage.Topic} 内容:{Encoding.UTF8.GetString(args.ApplicationMessage.Payload)} 服务质量等级:{args.ApplicationMessage.QualityOfServiceLevel}");

                };

                _mqttServer.ClientSubscribedTopic += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction, $"@客户端订阅主题 客户端ID:{args.ClientId} 主题:{args.TopicFilter.Topic} 服务质量等级:{args.TopicFilter.QualityOfServiceLevel}");
                };
                _mqttServer.ClientUnsubscribedTopic += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction, $"%客户端取消订阅主题 客户端ID:{args.ClientId} 主题:{args.TopicFilter.Length}");
                };

                _mqttServer.Started += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction, "Mqtt服务器启动...");
                };

                _mqttServer.Stopped += (sender, args) =>
                {
                    listBox1.BeginInvoke(_updateListBoxAction, "Mqtt服务器暂停...");

                };

                await _mqttServer.StartAsync(options);
            }
            catch(Exception e)
            {
               // await _mqttServer.StartAsync(options);
            }
           
        }
    }
}

