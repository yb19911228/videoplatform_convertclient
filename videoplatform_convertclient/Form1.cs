using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using MySQLDriverCS;
using System.Diagnostics;
namespace videoplatform_convertclient
{
    public partial class Form1 : Form
    {
        public string DB_Address, DB_User, DB_Pass, DB_Database, File_Input, File_Output, Host_ID, Host_IP, Host_Name;
        public string Host_Status="0";
        public Form1()
        {
            InitializeComponent();
        }

        //数据库连接参数初始化
        public void DB_set()
        {
            DB_Address = textBox1.Text;
            DB_User = textBox2.Text;
            DB_Pass = textBox3.Text;
            DB_Database = textBox4.Text;
            File_Input = textBox5.Text;
            File_Output = textBox6.Text;
            Host_ID = ReadTxt();
        }
        //注册按钮
        private void button1_Click(object sender, EventArgs e)
        {
            //读取输入的数据库地址、用户、密码、数据库和转码文件输入输出目录
            DB_Address = textBox1.Text;
            DB_User = textBox2.Text;
            DB_Pass = textBox3.Text;
            DB_Database = textBox4.Text;
            File_Input = textBox5.Text;
            File_Output = textBox6.Text;
            Host_Name = GetHostName();//获取本机计算名
            Host_IP = GetHostIP();//获取本机IP地址
            if (MysqlTest() == false)//测试数据库是否连接成功 
            {
                MessageBox.Show("数据库连接错误，请检查参数");
            }
            //检查是否已经注册，若还未注册则注册并上线，若已经注册则扫描任务
            else if (ReadTxt() == null)
            {
                //向数据库插入主机信息，并更新ID信息
                MessageBox.Show("正在注册");
                //向数据库写入信息
                string InsertQuery = "INSERT INTO chost(chostname,chostip,choststatus)VALUES"+"('"+Host_Name+"','"+Host_IP+"','0')";//注册语句，字段名不需要引号
                MysqlWrite(InsertQuery);
                //返回ID
                string SelectQuery = "SELECT chostid FROM chost WHERE chostname='" + Host_Name + "' AND chostip='" + Host_IP + "'";
                //MessageBox.Show(SelectQuery);
                Host_ID = MysqlRead(SelectQuery);
                WriteTxt(Host_ID);//将ID信息写入文件
            }
            else
            {
                string SelectQuery = "SELECT choststatus FROM chost WHERE chostname='" + Host_Name + "' AND chostip='" + Host_IP + "'";
                Host_Status = MysqlRead(SelectQuery);
                if (Host_Status == "0")
                    MessageBox.Show("本机已经注册，但未上线，请点击上线按钮！");
                else
                    MessageBox.Show("本机已运行！");
            }

        }
        //点击重置按钮，清空输入信息
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
        }

        //数据库连接测试
        public Boolean MysqlTest()
        {
            Boolean ReturnValue;
            MySQLConnection conn = null;
            conn = new MySQLConnection(new MySQLConnectionString(DB_Address, DB_Database, DB_User, DB_Pass).AsString);
            /* try
             {
                 //创建一个TcpClient对象，设置连接服务器的IP和端口号
                 TcpClient myclient = new TcpClient(DB_Address, 3306);
                    
             }
             catch
             {
                 ReturnValue = false;
             }*/
            try
            {
                conn.Open();
                HostInfo();
                ReturnValue = true;
            }
            catch (Exception e)
            {
                //MessageBox.Show("数据库连接失败，请检查参数");
                ReturnValue = false;
            }
            conn.Close();
            ReturnValue = true;

            return ReturnValue;
        }

        //数据库读取
        public string MysqlRead(string Query)
        {
            string ReturnValue = null;
            MySQLConnection conn = null;
            //MySQLCommand commn = new MySQLCommand("set names utf-8", conn);
            conn = new MySQLConnection(new MySQLConnectionString(DB_Address, DB_Database, DB_User, DB_Pass).AsString);
            //string query = "select * from video";
            conn.Open();
            MySQLCommand cmd = new MySQLCommand(Query, conn);
           
            MySQLDataReader reader = cmd.ExecuteReaderEx();
            while (reader.Read())
            {
                ReturnValue = reader.GetString(0);
            }
            conn.Close();
            return ReturnValue;

        }

        //数据库写入
        public void MysqlWrite(string Query)
        {
            MySQLConnection conn = null;
            //MySQLCommand commn = new MySQLCommand("set names utf-8", conn);
            conn = new MySQLConnection(new MySQLConnectionString(DB_Address, DB_Database, DB_User, DB_Pass).AsString);
            //string query = "select * from video";
            MySQLCommand cmd = new MySQLCommand(Query, conn);
            conn.Open();
            cmd.ExecuteNonQuery();//执行数据库代码
            conn.Close();
        }

        //更改状态信息
        public void HostInfo()
        {
            if (Host_Status == "0")
                Host_Status = "已下线";
            else if (Host_Status == "9")
                Host_Status = "下线中";
            else
                Host_Status = "已上线";
            //显示本机信息
            label10.Text = "本机ID：" + Host_ID.ToString() + "\n" +
                           "数据库IP:" + DB_Address.ToString() + "\n" +
                           "数据库用户：" + DB_User.ToString() + "\n" +
                           "数据库名：" + DB_Database.ToString() + "\n" +
                           "输入目录：" + File_Input.ToString() + "\n" +
                           "输出目录：" + File_Output.ToString() + "\n" +
                           "运行状态：" + Host_Status.ToString();
        }

        //读取配置文件
        public string ReadTxt()
        {
            string ReturnValue;
            try
            {
                //创建新的StreamReader对象
                StreamReader SR = new StreamReader(@"config.txt");
                //读取数据放入ConfigData中
                string ConfigData = SR.ReadToEnd();
                if (ConfigData != "")
                    //如果配置文件不为空，则表示已经注册
                    //MessageBox.Show("已注册"+ConfigData);
                    ReturnValue = ConfigData;
                else
                    //如果配置文件为空，则连接数据库申请ID后写入
                    //MessageBox.Show("配置文件为空");
                    ReturnValue = null;
                SR.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("配置文件读取错误！");
                return "error";
            }
            return ReturnValue;
        }

        //写入配置文件
        public void WriteTxt(string OriginalHostId)
        {
            try
            {
                //保留文件现有数据，追加方式打开 confif.txt文件
                StreamWriter SW = new StreamWriter(@"config.txt", true);
                //向文件写入新的字符串，并关闭StreamWriter
                SW.WriteLine(OriginalHostId);
                SW.Close();
            }
            catch (IOException)
            {
                MessageBox.Show("配置文件写入异常");
                return;
            }
            MessageBox.Show("已注册，ID号为：" + Host_ID);
            return;

        }

        //用户获取主机名和IP地址
        public string GetHostName()
        {
            string HostName;
            HostName = Dns.GetHostName();
            return HostName;
        }

        public string GetHostIP()
        {
            string HostIP = null, HostName;
            HostName = Dns.GetHostName();
            System.Net.IPAddress[] addressList = Dns.GetHostByName(HostName).AddressList;
            foreach (IPAddress ip in addressList)
            {
                HostIP = ip.ToString();
            }
            return HostIP;
        }
        
        //上线按钮
        private void button3_Click(object sender, EventArgs e)
        {
            DB_set();
            string SelectQuery = "SELECT choststatus FROM chost WHERE chostid='" + Host_ID + "'";
            string Online = "UPDATE chost SET choststatus='1'WHERE chostid='" + Host_ID + "'";
            Host_Status = MysqlRead(SelectQuery);
            if (Host_Status == "0")
            //修改信息上线
            {
                MysqlWrite(Online);
                MessageBox.Show("本机已上线运行");
                Host_Status = "1";
                //开启转码线程
                Thread t = new Thread(new ThreadStart(ConvertVideo)); //创建Thread对象
                t.Start();//开始线程

                //ConvertVideo();
            }
            else
            {
                MessageBox.Show("本机已上线运行！");
                ConvertVideo();
            }
            HostInfo();//更新状态信息
        }

        //下线按钮
        private void button4_Click(object sender, EventArgs e)
        {
            DB_set();
            string SelectQuery = "SELECT choststatus FROM chost WHERE chostid='" + Host_ID + "'";
            string Offline = "UPDATE chost SET choststatus='0'WHERE chostid='" + Host_ID + "'";
            Host_Status = MysqlRead(SelectQuery);
            if (Host_Status == "0")
                MessageBox.Show("本机已下线！");
            else
            //修改信息下线
            {
                MysqlWrite(Offline);
                //MessageBox.Show("本机已下线！");
                Host_Status = "9";
                //label12.Text = "无";
            }
            HostInfo();//更新状态信息
        }


        //运行转码程序
        public void ConvertVideo()
        {
            DB_set();
            //搜索状态表
            string SelectQuery = "SELECT choststatus FROM chost WHERE chostid='" + Host_ID + "'";
           Host_Status = MysqlRead(SelectQuery);
            //MessageBox.Show(Host_Status);
            if (Host_Status == "0")
            {
                //label12.Text = "已下线";
                MessageBox.Show("已下线");
                //Thread.Sleep(0);//阻止当前线程
            }
            else if (Host_Status == "1")
            {
                //等待一段时间继续运行转码函数
                //label12.Text = "正在检索转码任务...";
                Thread.Sleep(2000);
                ConvertVideo();
            }
            else
            {
                MessageBox.Show("本机已运行！");
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
           
            string Cmd = "ffmpeg.exe -i x:/test.flv  x:/1muh1.avi";
            //Cmd = "ipconfig";
              RunCmd(Cmd);
        }

        private void RunCmd (string Command)
        {
            //实例一个Process类，启动独立进程
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";//设定程序名
            p.StartInfo.Arguments = "/c" + Command;//设定启动参数
            p.StartInfo.UseShellExecute = false;//关闭shell的使用
            p.StartInfo.RedirectStandardInput = true;//重定向标准输入
            p.StartInfo.RedirectStandardOutput = true;//重定向标准输出
            p.StartInfo.RedirectStandardError = true;//重定向错误输出
            //p.StartInfo.CreateNoWindow = true;//设置不显示窗口
             p.Start();//启动
             //p.StandardInput.WriteLine(Command);//也可以用这种方式输入要执行的命令
            //p.StandardInput.WriteLine("exit");//防止死机
            //return p.StandardOutput.ReadLine();//从输出流取得命令执行结果
        }
        
        
    }
}