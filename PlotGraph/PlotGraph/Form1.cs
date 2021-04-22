using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace PlotGraph
{
    public partial class Form1 : Form
    {
        string temp;
//        string force;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cbPortName.Items.AddRange(ports);

            btConnect.Enabled = true;
            btDisconnect.Enabled = false;

            serialPort1.DtrEnable = false;
            serialPort1.RtsEnable = false;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            try 
            {    
            serialPort1.PortName = cbPortName.Text;
            serialPort1.BaudRate = Convert.ToInt32(txtBaudrate.Text);
            serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
            serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
            serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);
            
            serialPort1.Open();
            btConnect.Enabled = false;
            btDisconnect.Enabled = true;
            lbStatus.Text = "ON";
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btConnect.Enabled = true;
                btDisconnect.Enabled = false;
                lbStatus.Text = "OFF";
            }
        }

        private void btDisconnect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == false) return;
            serialPort1.Close();
            btConnect.Enabled = true;
            btDisconnect.Enabled = false;
            lbStatus.Text = "OFF";
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            temp = serialPort1.ReadExisting();
            this.Invoke(new EventHandler(ShowData));
        }
        private void ShowData(object sender, EventArgs e)
        {
            txtData.Text = temp;
            txtData.Text += temp;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtData.Text = temp;
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            if (txtData.Text != "")
            {
                txtData.Text = "";
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
