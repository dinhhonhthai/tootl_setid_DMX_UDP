using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindDevice
{
    public partial class FormMain : Form
    {       
        private string BaseIP = "172.16.0.";
        private int StartIP = 2;
        private int StopIP = 255;
        private string ip;
        private string list_ip;
        private int timeout = 3000;
        private int nFound = 0;
       
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static object lockObj = new object();
        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts;
        public FormMain()
        {
            InitializeComponent();
            string[] BaudRate = { "101", "102", "103", "104", "105", "106", "107", "108", "109", "110","200" };
            comboBox2.Items.AddRange(BaudRate);//  
        }

        private void buttonSweepAsync_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            RunPingSweep_Async(txt_ip.Text);          
        }
        public async void RunPingSweep_Async(string txt_ip)
        {
            nFound = 0;
            list_ip = "";
            BaseIP = txt_ip + ".";
            var tasks = new List<Task>();

            stopWatch.Start();

            for (int i = StartIP; i <= StopIP; i++)
            {
                ip = BaseIP + i.ToString();

                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                var task = PingAndUpdateAsync(p, ip);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ContinueWith(t =>
            {
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
             
                richTextBox1.BeginInvoke(new Action(() =>
                {

                    label3.Visible = true;
                    label4.Visible = true;
                    comboBox1.Visible = true;
                    comboBox2.Visible = true;
                    //richTextBox1.Text = list_ip;
                    //comboBox1.Items.Add(list_ip);
                    //list_ip = "";
                    lblresult.Text = nFound.ToString() + " thiết bị được tìm thấy! Elapsed time: " + ts.ToString();
                }));
                
            });
        }
        

        private async Task PingAndUpdateAsync(System.Net.NetworkInformation.Ping ping, string ip)
        {
            var reply = await ping.SendPingAsync(ip, timeout);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                lock (lockObj)
                {
                    list_ip += ip + Environment.NewLine;
                    comboBox1.Items.Add(ip);
                    nFound++;
                }
            }
        }
        String ip_select;
     
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ip_select = comboBox1.SelectedItem.ToString();

        }
        String ip_cuoi;
        private void button1_Click(object sender, EventArgs e)
        {
            //label2.Text = ip_select;
            IPAddress broadcast = IPAddress.Parse(ip_select);
            byte[] sendbuf = Encoding.ASCII.GetBytes("?Setaddress^"+ (Int32.Parse(ip_cuoi)-100) + "*");
            IPEndPoint ep = new IPEndPoint(broadcast, 96);
            s.SendTo(sendbuf, ep);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            lblresult.Visible = true;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ip_cuoi = comboBox2.SelectedItem.ToString();
            label2.Text = txt_ip.Text+"." + ip_cuoi;
            foreach (String khachHang in comboBox1.Items)
            {
                // Thực thi phần in hóa đơn của từng khách hàng
               if(khachHang == label2.Text)
                {
                    
                    label2.Text = "Bị trùng IP";
                    label2.BackColor = System.Drawing.Color.Red;
                }
            }
        }
    }

    
}
