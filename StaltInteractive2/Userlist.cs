using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace StaltInteractive2
{
    /*
     *      (C) Толстопятов Алексей А. 2022
     * Форма просморта сетевых адресов для службы FSW
     * Исходный код не менялся с 4.09.2022
     * 
     */
    public partial class Userlist : Form
    {
        private ServiceLog sLog = new ServiceLog();
        public Userlist()
        {
            InitializeComponent();
        }
        private Point mouseOffset;
        private bool isMouseDown = false;
        public static string[] IPArray = new string[30];
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(347, 32);

            LinearGradientBrush lgb =
                new LinearGradientBrush(startPoint, endPoint, Color.Blue, Color.White);
            Graphics g = e.Graphics;
            g.FillRectangle(lgb, 0, 0, 347, 32);
            // g.DrawLine(new Pen(Color.Yellow, 1.5f), startPoint, endPoint);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;

            if (e.Button == MouseButtons.Left)
            {
                xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                yOffset = -e.Y - SystemInformation.CaptionHeight -
                    SystemInformation.FrameBorderSize.Height;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }
        private void Count()
        {
            for (int i = 10; i < 21; i++) // решение из StackOverFlow
            {
                AutoResetEvent waiter = new AutoResetEvent(false);
                Ping pingSender = new Ping();
                pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 1000;
                PingOptions options = new PingOptions(64, true);
                pingSender.SendAsync("192.168.0." + i, timeout, buffer, options, waiter);
                waiter.WaitOne();
            }
        }
        private void GetAdapters() 
        {
            //listBox1.ForeColor = Color.Green;
            // if Network enabled
            int Count = 0;
            if (!NetworkInterface.GetIsNetworkAvailable())
                listBox1.Items.Add("Нет сетевого окружения");
            else
            {
                listBox1.Items.Clear();
                
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                listBox1.Items.Add($"{ip.Address} ({host.HostName}\\{ni.Name})");
                                IPArray[Count] = ip.Address.ToString();
                                Count++;
                            }
                        }
            }
            //listBox1.Items.CopyTo(IPArray, 0);
            //listBox1.ResetForeColor();
            
        }
        public void GetLAN()
        {
            // решение из Stack Overflow
            Stopwatch time = new Stopwatch();
            time.Start();
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa |in Russians|";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            for (int i = 161; i < 191; i++) 
            {
                AutoResetEvent waiter = new AutoResetEvent(false);
                Ping pingSender = new Ping();
                pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                PingOptions options = new PingOptions(64, true);
                pingSender.SendAsync("192.168.0." + i, timeout, buffer, options, waiter); // тут вы должны сами вставить свой вариант IP-адреса (не считая последней цифры после точки)
                waiter.WaitOne();
            }
            time.Stop();
            time.Restart();
            
            Thread th = new Thread(new ThreadStart(Count));
            th.Start();
            th.Name = "th";
            
            sLog.OutBox.AppendText($"Домен приложения {Thread.GetDomain().FriendlyName}\r\n");
            for (int i = 0; i < 10; i++)
            {
                AutoResetEvent waiter = new AutoResetEvent(false);
                Ping pingSender = new Ping();
                pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                PingOptions options = new PingOptions(64, true);
                pingSender.SendAsync("192.168.0." + i, timeout, buffer, options, waiter);
                waiter.WaitOne();
            }

            sLog.OutBox.AppendText($"Статус потока: {th.ThreadState}\r\n");
            time.Stop();
            
        }
        public void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            // решение из StackOverFlow
            if (e.Cancelled)
            {
                sLog.OutBox.AppendText("Ping canceled.\r\n");
                ((AutoResetEvent)e.UserState).Set();
            }
            if (e.Error != null)
            {
                sLog.OutBox.AppendText("Ping failed: ");
                sLog.OutBox.AppendText(e.Error.ToString());
                ((AutoResetEvent)e.UserState).Set();
            }
            PingReply reply = e.Reply;
            DisplayReply(reply);
            ((AutoResetEvent)e.UserState).Set();
        }
        public void DisplayReply(PingReply reply)
        {
            if (reply == null)
                return;
            if (reply.Status != IPStatus.DestinationHostUnreachable && reply.Status != IPStatus.TimedOut)
            {
                sLog.OutBox.AppendText($"ping status: {reply.Status}");
                if (reply.Status == IPStatus.Success)
                {
                    sLog.OutBox.AppendText($"Address: {reply.Address.ToString()}\r\n");
                    sLog.OutBox.AppendText($"RoundTrip time: {reply.RoundtripTime}\r\n");
                    sLog.OutBox.AppendText($"Time to live: {reply.Options.Ttl} \r\n");
                    sLog.OutBox.AppendText($"$Buffer size: {reply.Buffer.Length}\r\n");
                    
                }
            }
        }
        private void Userlist_Load(object sender, EventArgs e)
        {
            GetAdapters();
            GetLAN();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Process.Start(@"explorer.exe", $@"\\{IPArray[listBox1.SelectedIndex]}\");
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}