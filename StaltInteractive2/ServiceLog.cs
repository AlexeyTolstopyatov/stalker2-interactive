using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace StaltInteractive2
{
    /*
     *      (C) Толстопятов Алексей А.
     * Исходный код формы-логгера 4.09.2022.
     * NET Framework 4.5
     * Visual Studio 2019 
     */
    public partial class ServiceLog : Form
    {
        private static Userlist form = new Userlist();
        public ServiceLog()
        {
            InitializeComponent();
            fileSystemWatcher1.EnableRaisingEvents = false;
            button1.BackColor = Color.LightGreen;
            button5.BackColor = Color.LightPink;

            OutBox.WordWrap = true;
            OutBox.TabStop = false;
            OutBox.Font  = new Font("Consolas", 10);
            OutBox.Text = "Stalker Interactive 2.1.0.0 Console Interface\r\n";
            OutBox.Text += "Для запуска службы укажите путь исследования в адресной строке\r\n"; 

            button5.Font = new Font("Consolas", 9, FontStyle.Bold);
            button1.Font = new Font("Consolas", 9, FontStyle.Bold);
            button6.BackColor = Color.Aquamarine;
            button7.BackColor = Color.Cyan;
        }
        private Point mouseOffset;
        private bool isMouseDown = false;
        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(1170, 32); //размер панели.

            LinearGradientBrush lgb =
                new LinearGradientBrush(startPoint, endPoint, Color.Blue, Color.White);
            Graphics g = e.Graphics;
            g.FillRectangle(lgb, 0, 0, 1170, 32);
            // g.DrawLine(new Pen(Color.Yellow, 1.5f), startPoint, endPoint);
        }

        /*
         * Движение Панели
         * --------------------
         * MouseDown MouseUp
         * MouseMove
         */
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


        private void button2_Click(object sender, EventArgs e) => Application.Exit();
        private void button3_Click(object sender, EventArgs e)
        {

        } // FullScreen потом допишу :D
        private void button4_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized; // Min
        private void OutBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e) => OutBox.AppendText($"{DateTime.Now} [ИЗМЕНЕНО]: {e.FullPath} \r\n");
        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e) => OutBox.AppendText($"{DateTime.Now} [ДОБАВЛЕНО]: {e.FullPath} \r\n");
        private void fileSystemWatcher1_Deleted(object sender, System.IO.FileSystemEventArgs e) => OutBox.AppendText($"{DateTime.Now} [УДАЛЕНО]: {e.FullPath} \r\n");

        private void button1_Click(object sender, EventArgs e)
        {
            fileSystemWatcher1.Path = AddressBox.Text;
            fileSystemWatcher1.IncludeSubdirectories = true;
            OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: ***Запуск Stalt Interactive ***\r\n");
            OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: Исследуемый путь: {fileSystemWatcher1.Path} \r\n");
            fileSystemWatcher1.EnableRaisingEvents = true;
        } // Запуск
        private void button5_Click(object sender, EventArgs e)
        {
            OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: ***Остановка службы***\r\n");
            fileSystemWatcher1.EnableRaisingEvents = false;
            if (fileSystemWatcher1.EnableRaisingEvents == false)
                OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: Stalt Interactive остановлена\r\n");
        } // Остановка
        private void button6_Click(object sender, EventArgs e)
        {
            fileSystemWatcher1.EnableRaisingEvents = false;
            OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: ***Остановка службы***");
            saveFileDialog1.FileName = $"file";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                File.WriteAllText(saveFileDialog1.FileName, OutBox.Text);
        } // Лог

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            form.ShowDialog();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: ***Пользователи online***\r\n");
            for (int i = 0; i < Userlist.IPArray.Length; i++)
            {
                if (Userlist.IPArray[i] == null 
                    || Userlist.IPArray[i] == "")
                    break;
                else
                    OutBox.AppendText($"{DateTime.Now} [ПОЛЬЗОВАТЕЛЬ]: {Userlist.IPArray[i]} \r\n");
            }
        }

        public void GetLocalAdress()
        {
            // if Network enabled
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                OutBox.AppendText($"{DateTime.Now} [СТАЛТ]: Нет сетевых адаптеров или окружения.");
            else 
            { 
                
            // searching ip adapters
            var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                OutBox.AppendText($"{DateTime.Now} [СОЕДИНЕНИЯ]: {host.HostName}\\{ni.Name} ({ip.Address.ToString()}) \r\n");
                            }
                        }
                    }
                }

            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //OutBox.AppendText($"{DateTime.Now} [СВЯЗЬ]: {GetDefaultGateway().ToString()}");
            GetLocalAdress();
        }
    }
}
