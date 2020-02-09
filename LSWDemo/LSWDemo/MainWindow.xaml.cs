using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace LSWDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //创建 1个客户端套接字 和1个负责监听服务端请求的线程  
        Thread threadclient = null;
        Socket socketclient = null;

        struct lgData  //自定义的数据类型。用来描述罗技驾驶信息。 
        {
           public int index;// 罗技G29设备号（可以接两个设备） 0为第一个设备 1为第二个设备
            //以下按照107协议封装
           public string AcceleratePedal; // 油门 |32767 ~ -32768|
           public string BreakPedal; // 刹车 32767 ~ -32768
           public int direction_flag;// 正为右1  负为左0 
           public int gear; //档位 1前进  2后退  0空挡 
           public string SteeringAngle; // 方向盘角度 -32768~32767 
           
         //离合目前不需要（2019.1.16）
         //  public string ClutchPedal;// 离合 32767 ~ -32768
        }

        //timer_Tick实时获取的信息
        lgData realTimeData = new lgData();
        //timer发送的信息
        lgData sendData = new lgData();

        public MainWindow()
        {
            InitializeComponent();
        }

        
        /// <summary>
        /// 启动定时器，轮询获取罗技G29模拟器数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            int DeviceIndex = 0;
            
            if (LogitechGSDK.LogiUpdate())
            {
                if (LogitechGSDK.LogiIsConnected(DeviceIndex))
                {
                    var Properties = new LogitechGSDK.LogiControllerPropertiesData();
                    LogitechGSDK.LogiGetCurrentControllerProperties(DeviceIndex, ref Properties);

                    var State = LogitechGSDK.LogiGetStateCSharp(DeviceIndex);

                    realTimeData.index = DeviceIndex;
                    realTimeData.AcceleratePedal = accleratePedal(State.lY).ToString();
                    realTimeData.BreakPedal = breakPedal(State.lRz).ToString();
                    realTimeData.direction_flag = directionFlag(State.lX);
                    realTimeData.gear = 1;
                    realTimeData.SteeringAngle = steeringAngle(State.lX).ToString();
                    
                    
                    string OutputTail = "Steering Angle: " + steeringAngle(State.lX).ToString() + "\r\n";
                    OutputTail += "Direction flag: " + directionFlag(State.lX).ToString() + "\r\n";
                    OutputTail += "Accelerate Pedal: " + accleratePedal(State.lY).ToString() + "\r\n";
                    OutputTail += "Break Pedal: " + breakPedal(State.lRz).ToString() + "\r\n";
                    OutputTail += "Clutch Pedal: " + State.rglSlider[0].ToString() + "\r\n";

                    string Output107 = realTimeData.AcceleratePedal + ',' 
                     + realTimeData.BreakPedal + ','
                     + realTimeData.direction_flag + ','
                     + realTimeData.gear + ','
                     + realTimeData.SteeringAngle;

                    this.Output.Text = OutputTail + "发送给云服务器的107协议： "+ Output107;
                }
            }
        }

        /// <summary>
        /// 启动定时器，将结构体数据交换，保证数据完整性，然后发送给服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_send(object sender, EventArgs e)
        {
            sendData = realTimeData;
            string OutputSend = sendData.AcceleratePedal + ','
                     + sendData.BreakPedal + ','
                     + sendData.direction_flag + ','
                     + sendData.gear + ','
                     + sendData.SteeringAngle;
            ClientSendMsg(OutputSend);
        }
        // 
        /// <summary>
        /// 方向盘的方向     +/1表示右 -/0表示左
        /// </summary>
        /// <param name="steeringAngle"></param>
        /// <returns></returns>
        private int directionFlag(int steeringAngle)
        {
            return steeringAngle > 0 ? 1 : 0;
        }
        // 
        /// <summary>
        /// 方向盘旋转角度 （800-1500）有方向+表示右 -表示左
        /// </summary>
        /// <param name="steeringAngle"></param>
        /// <returns></returns>
        private int steeringAngle(int steeringAngle)
        {
            return Math.Abs(700 * steeringAngle / 32768) + 800;
        }

        //
        /// <summary>
        /// 油门 0 ~ 50
        /// </summary>
        /// <param name="a=cceleratePedal"></param>
        /// <returns></returns>
        private int accleratePedal(int acceleratePedal)
        {
            return 0-(50 * acceleratePedal / 32767 /2) + 25;
        }
        
        /// <summary>
        /// 刹车 0 ~ 10 
        /// </summary>
        /// <param name="breakPedal"></param>
        /// <returns></returns>
        private int breakPedal(int breakPedal)
        {
            return 0 - (10 * breakPedal / 32767 / 2) + 5;
        }

        /// <summary>
        /// 建立TCP连接，然后Timer 发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            socketclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //获取服务器ip地址
            //IPAddress address = IPAddress.Parse("120.78.162.85");
            IPAddress address = IPAddress.Parse(this.ip.Text);


            //获取服务器端口号
            int port = int.Parse(this.port.Text);
            
            //将获取的IP地址和端口号绑定在网络节点上  
            //IPEndPoint point = new IPEndPoint(address, 6666);
            IPEndPoint point = new IPEndPoint(address, port);

            try
            {
                //客户端套接字连接到网络节点上，用的是Connect  
                socketclient.Connect(point);
                this.resultText.Items.Add("连接成功！");
                ClientSendMsg("测试");

                var Timer = new System.Windows.Threading.DispatcherTimer();
                Timer.Tick += new EventHandler(timer_send);
                Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                Timer.Start();
                
            }
            catch (Exception)
            {
                
                this.resultText.Items.Add("连接失败\r\n");
                return;
            }
            //接受服务器发来的数据
            threadclient = new Thread(recv);
            threadclient.IsBackground = true;
            threadclient.Start();
        }

        /// <summary>
        /// 接受服务器发来的数据
        /// </summary>
        private void recv()
        {
            int x = 0;
            //持续监听服务端发来的消息 
            while (true)
            {
                try
                {
                    //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
                    byte[] arrRecvmsg = new byte[1024 * 1024];

                    //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                    int length = socketclient.Receive(arrRecvmsg);

                    //将套接字获取到的字符数组转换为人可以看懂的字符串  
                    string strRevMsg = Encoding.UTF8.GetString(arrRecvmsg, 0, length);
                    if (x == 1)
                    {
                        this.resultText.Items.Add("服务器:" + GetCurrentTime() + "\r\n" + strRevMsg + "\r\n\n");
                    }
                    else
                    {
                        this.resultText.Items.Add(strRevMsg + "\r\n\n");
                        x = 1;
                    }
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

         
        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns></returns>
        DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        /// <summary>
        /// 客户端发送信息给服务器
        /// </summary>
        /// <param name="sendMsg"></param>
        void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组     
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组     
            socketclient.Send(arrClientSendMsg); 
            // 测试用
            this.resultText.Items.Add("hello...." + ": " + GetCurrentTime() + "\r\n" + sendMsg + "\r\n");
        }


        /// <summary>
        /// windows加载 罗技加载以及启动timer轮询获取信号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogitechGSDK.LogiSteeringInitialize(false);

            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            dispatcherTimer.Start();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            LogitechGSDK.LogiSteeringShutdown();
        }
    }
}
