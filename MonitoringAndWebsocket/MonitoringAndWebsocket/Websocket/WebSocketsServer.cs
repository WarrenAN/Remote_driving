using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;


namespace WebSocketServer
{
    public class Logger
    {
        public bool LogEvents { get; set; }

        public Logger()
        {
            LogEvents = true;
        }

        public void Log(string Text)
        {
            if (LogEvents) Console.WriteLine(Text);
        }
    }

    public enum ServerStatusLevel { Off, WaitingConnection, ConnectionEstablished };
    /// <summary>
    /// 声明新连接处理事件
    /// </summary>
    /// <param name="loginName"></param>
    /// <param name="e"></param>
    public delegate void NewConnectionEventHandler(string loginName,EventArgs e);
    /// <summary>
    /// 声明接收数据处理事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="e"></param>
    public delegate void DataReceivedEventHandler(Object sender, string message, EventArgs e);
    /// <summary>
    /// 声明断开连接处理事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DisconnectedEventHandler(Object sender,EventArgs e);
    /// <summary>
    /// 声明广播处理事件
    /// </summary>
    /// <param name="message"></param>
    /// <param name="e"></param>
    public delegate void BroadcastEventHandler(string message, EventArgs e);

    public class WebSocketServer : IDisposable
    {
        private bool AlreadyDisposed;
        private Socket Listener;
        private int ConnectionsQueueLength;
        private int MaxBufferSize;
        private Logger logger;
        static private byte[] FirstByte;
        static private byte[] LastByte;
       

       static List<SocketConnection> connectionSocketList = new List<SocketConnection>();

        public ServerStatusLevel Status { get; private set; }
        public int ServerPort { get; set; }
        public string ServerLocation { get; set; }
        public string ConnectionOrigin { get; set; }


        public event NewConnectionEventHandler NewConnection;
        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;

        private void Initialize()
        {
            AlreadyDisposed = false;
            logger = new Logger();

            Status = ServerStatusLevel.Off;
            ConnectionsQueueLength = 500;
            MaxBufferSize = 1024 * 100;
            FirstByte = new byte[MaxBufferSize];
            LastByte = new byte[MaxBufferSize];
            FirstByte[0] = 0x00;
            LastByte[0] = 0xFF;
            logger.LogEvents = true;
        }

        public WebSocketServer() 
        {
            ServerPort = 4399;
            ServerLocation = string.Format("ws://{0}:4141/start", getLocalmachineIPAddress());
            Initialize();
        }


        public void Dispose()
        {
            Close();
        }

        private void Close()
        {
            if (!AlreadyDisposed)
            {
                AlreadyDisposed = true;
                if (Listener != null) Listener.Close();
                foreach (SocketConnection item in connectionSocketList)
                {
                    item.ConnectionSocket.Close();//关闭所有连接
                }
                connectionSocketList.Clear();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// 获取本机ip地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress getLocalmachineIPAddress()
        {
                string strHostName = Dns.GetHostName();   //得到主机名
                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);

                foreach (IPAddress ip in ipEntry.AddressList)
                {
                    //↓从IP地址列表中筛选出IPv4类型的IP地址
                    //↓AddressFamily.InterNetwork表示此IP为IPv4,
                    //↓AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip;
                }
                return ipEntry.AddressList[0];
        }

        /// <summary>
        /// constr用于连接数据库
        /// </summary>
        public static string conStr = ConfigurationManager.ConnectionStrings["CCCloudDataConnectionString"].ToString();//定义数据库连接字符串
        
        /// <summary>
        /// data变量用于存储CAN数据
        /// </summary>
        private static string data = "";//用于接收数据库里实时更新的CAN数据
        
        /// <summary>
        /// 建立websocket开始
        /// </summary>
        public void StartServer()
        {
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            //↑第一个参数是指定socket对象使用的寻址方案，即IPV4或IPV6；
            //↑第二个参数socket对象的套接字的类型，此处stream是表示流式套接字
            //↑第三个参数socket对象支持的协议，TCP协议或UDP协议。
            Listener.Bind(new IPEndPoint(getLocalmachineIPAddress(), ServerPort));  //IPEndPoint (IP地址和端口的组合)
            
            Listener.Listen(ConnectionsQueueLength); //Listen方法的整型参数表示的是：排队等待连接的最大数量，注意这个数量不包含已经连接的数量

            logger.Log(string.Format("服务器启动。监听地址：{0}, 端口：{1}",getLocalmachineIPAddress(),ServerPort));
            logger.Log(string.Format("WebSocket服务器地址: ws://{0}:{1}/start", getLocalmachineIPAddress(), ServerPort));

            while (true)
            {
                Socket sc = Listener.Accept();//Socket监听

                if (sc != null)
                {
                    System.Threading.Thread.Sleep(100);
                    SocketConnection socketConn = new SocketConnection();
                    socketConn.ConnectionSocket = sc;
                    //相关事件绑定
                    socketConn.NewConnection += new NewConnectionEventHandler(socketConn_NewConnection);

                    try
                    {
                        SqlDependency.Start(conStr);
                    }
                    catch
                    {
                        Console.WriteLine("数据库服务已关闭，请检查！");
                    }

                    socketConn.DataReceived += new DataReceivedEventHandler(socketConn_BroadcastMessage);
                    
                    socketConn.Disconnected += new DisconnectedEventHandler(socketConn_Disconnected);
                    //异步接收数据
                    socketConn.ConnectionSocket.BeginReceive(socketConn.receivedDataBuffer,
                                                             0, socketConn.receivedDataBuffer.Length, 
                                                             0, new AsyncCallback(socketConn.ManageHandshake), 
                                                             socketConn.ConnectionSocket.Available);
                    //存入集合,以便在Socket发送消息时发送给所有连接的Socket套接字
                    connectionSocketList.Add(socketConn);
                }
            }
        }

        /// <summary>
        /// 数据库数据更新
        /// </summary>
        /// <returns>data</returns>
        public static string UpdateData()
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT [Cid],[speed],[stop],[flag] ,[angle],[exp_speed],[traffic_light_status],[long] ,[lat] FROM [dbo].[t_data_test]", connection))
                {
                    command.CommandType = CommandType.Text;
                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);
                    using (SqlDataReader sdr = command.ExecuteReader())
                    {
                        Console.WriteLine();
                        while (sdr.Read())
                        {
                            //data = "id:" + sdr["uid"].ToString() + "  user:" + sdr["userName"].ToString();//获取最后一行数据
                            data = sdr["speed"].ToString() + "," + sdr["stop"].ToString() + "," + sdr["flag"].ToString() + "," + sdr["angle"].ToString() + "," + sdr["exp_speed"].ToString() + ","+ sdr["traffic_light_status"].ToString() + "," + sdr["long"].ToString() + "," + sdr["lat"].ToString();
                            Console.WriteLine(data);
                            //Console.WriteLine("id:{0}\tuserName:{1}\tuserPassword:{2}", sdr["uid"].ToString(), sdr["userName"].ToString(), sdr["userPassword"].ToString());//打印数据库某个表的所有数据，后期可能影响实时性，要删
                        }
                        sdr.Close();
                        return data;
                    }
                }
            }
        }
        
        /// <summary>
        /// 断开连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void socketConn_Disconnected(Object sender, EventArgs e)
        {
            SocketConnection sConn = sender as SocketConnection;
            if (sConn != null)
            {
                Send(string.Format("【{0}】断开连接！", sConn.Name));//xx人离开
                sConn.ConnectionSocket.Close();
                connectionSocketList.Remove(sConn);
            }
        }

        /// <summary>
        /// 数据库监控事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            Console.WriteLine("ok");
            if (e.Type == SqlNotificationType.Change) //只有数据发生变化时,才重新获取并数据
            {
                Send( UpdateData());
            }
        }

        /// <summary>
        /// 消息广播
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <param name="e"></param>
        void socketConn_BroadcastMessage(Object sender, string message, EventArgs e)
        {
            
            SocketConnection sConn = sender as SocketConnection;
            //sConn.Name = message.Substring(message.IndexOf("login:") + "login:".Length);
            //message = string.Format("欢迎【{0}】建立连接！",sConn.Name);
            message = UpdateData();
            Send(message);
        }
        /// <summary>
        /// 新连接事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="e"></param>
        void socketConn_NewConnection(string name, EventArgs e)
        {
            if (NewConnection != null)
                NewConnection(name,EventArgs.Empty);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message"></param>
        public static void Send(string message)
        {
            foreach (SocketConnection item in connectionSocketList)//遍历connectionsocketlist
            {
                if (!item.ConnectionSocket.Connected) return;
                try
                {
                    if (item.IsDataMasked)
                    {
                        DataFrame dr = new DataFrame(message);
                        item.ConnectionSocket.Send(dr.GetBytes());
                    }
                    else
                    {
                        item.ConnectionSocket.Send(FirstByte);
                        item.ConnectionSocket.Send(Encoding.UTF8.GetBytes(message));
                        item.ConnectionSocket.Send(LastByte);
                    }
                }
                catch(Exception ex)
                {
                   // logger.Log(ex.Message);
                }
            }
        }
    }
}