using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
 

namespace ntptime
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public  DateTime GetNetworkTime(ulong gmttime, string ntps)
        {
            //default Windows time server
            string ntpServer = ntps;

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);

            socket.Send(ntpData);
            socket.Receive(ntpData);    
            socket.Close();

  

            //Offset to get to the "Transmit Timestamp" field (time at which the reply
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            
            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
            
            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);



            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var time = gmttime * 3600000;

            
            if (button2.Text == "-")
            {
                milliseconds = milliseconds - unchecked((ulong)time);
            }
            else
            {
                milliseconds = milliseconds + unchecked((ulong)time);
            }

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                string abc = textBox1.Text;
                string ntpserver = textBox2.Text;

                ulong gmt = Convert.ToUInt64(abc);


                label1.Text = (GetNetworkTime(gmt, ntpserver).ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "+")
            {
                button2.Text = "-";
            }
            else
            {
                button2.Text = "+";
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
