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
using ZedGraph;
using System.Xml;

namespace ComPort
{
    public partial class Form1 : Form
    {
        string temp;
        string force;

        GraphPane myPane1 = new GraphPane();
        GraphPane myPane2 = new GraphPane();

        public Form1()
        {
            InitializeComponent();
        }
 
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cBoxCOMPORT.Items.AddRange(ports);

            btnOpen.Enabled = true;
            btnClose.Enabled = false;

            serialPort1.DtrEnable = false;
            serialPort1.RtsEnable = false;


            // 
            myPane1 = zedGraphControl1.GraphPane;

            myPane1.Title.Text = "Đồ thị";
            myPane1.XAxis.Title.Text = "Thời gian";
            myPane1.YAxis.Title.Text = "Nhiệt độ";

            RollingPointPairList listPoint1 = new RollingPointPairList(60000);
            LineItem curve1 = myPane1.AddCurve("Nhiệt độ", listPoint1, Color.Red, SymbolType.None);

            myPane1.XAxis.Scale.Min = 0;
            myPane1.XAxis.Scale.Max = 30;
            myPane1.XAxis.Scale.MinorStep = 1;
            myPane1.XAxis.Scale.MajorStep = 5;
            myPane1.YAxis.Scale.Min = 0;
            myPane1.YAxis.Scale.Max = 100;
            myPane1.AxisChange();

            myPane2 = zedGraphControl2.GraphPane;

            myPane2.Title.Text = "Đồ thị";
            myPane2.XAxis.Title.Text = "Thời gian";
            myPane2.YAxis.Title.Text = "Lực";


            RollingPointPairList listPoint2 = new RollingPointPairList(60000);
            LineItem curve2 = myPane2.AddCurve("Lực", listPoint2, Color.Blue, SymbolType.None);

            myPane2.XAxis.Scale.Min = 0;
            myPane2.XAxis.Scale.Max = 30;
            myPane2.XAxis.Scale.MinorStep = 1;
            myPane2.XAxis.Scale.MajorStep = 5;
            myPane2.YAxis.Scale.Min = 0;
            myPane2.YAxis.Scale.Max = 100;
            myPane2.AxisChange();

            TickStart1 = Environment.TickCount;
        }
        int TickStart1;

        private void Draw()
        {
            double _force;
            double _temp;
            double time1 = (Environment.TickCount - TickStart1) / 1000.0;

            double.TryParse(temp, out _temp);
            double.TryParse(force, out _force);

            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve1 = zedGraphControl1.GraphPane.CurveList[0] as LineItem;


            if (curve1 == null)
                return;


            IPointListEdit listPoint1 = curve1.Points as IPointListEdit;


            if (listPoint1 == null)
                return;

            listPoint1.Add(time1, _temp); // Thêm điểm trên đồ thị


            Scale xScale1 = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale1 = zedGraphControl1.GraphPane.YAxis.Scale;



            // Tự động Scale theo trục x
            if (time1 > xScale1.Max - xScale1.MajorStep)
            {
                xScale1.Max = time1 + xScale1.MajorStep;
                xScale1.Min = xScale1.Max - 30;
            }

            // Tự động Scale theo trục y
            if (_temp > yScale1.Max - yScale1.MajorStep )
            {
                yScale1.Max = _temp + yScale1.MajorStep;
            }

            if (_temp < yScale1.Min + yScale1.MajorStep)
            {
                yScale1.Min = _temp - yScale1.MajorStep;
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();

            if (zedGraphControl2.GraphPane.CurveList.Count <= 0)
                                       return;
            LineItem curve2 = zedGraphControl2.GraphPane.CurveList[0] as LineItem;
                                         if (curve2 == null)
                                       return;
            IPointListEdit listPoint2 = curve2.Points as IPointListEdit;
                    if (listPoint1 == null)
                           return;
            listPoint2.Add(time1, _force);

            Scale xScale2 = zedGraphControl2.GraphPane.XAxis.Scale;
            Scale yScale2 = zedGraphControl2.GraphPane.YAxis.Scale;

            if (time1 > xScale2.Max - xScale2.MajorStep)
            {
                xScale2.Max = time1 + xScale2.MajorStep;
                xScale2.Min = xScale2.Max - 30;
            }

            if (_force > yScale2.Max - yScale2.MajorStep)
                 {
                       yScale2.Max = _force + yScale2.MajorStep;
                 }    

            else if (_force < yScale2.Min + yScale2.MajorStep)                   
                 {
                    yScale2.Max = _force - yScale2.MajorStep;
                 }
            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();
            zedGraphControl2.Refresh();
                       

        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cBoxCOMPORT.Text;
                serialPort1.BaudRate = Convert.ToInt32(CBoxBaudRate.Text);
                serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);

                serialPort1.Open();
                progressBar1.Value = 100;
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                btnClearGraph.Enabled = false;
                lblStatusCom.Text = "ON";
                timer1.Enabled = true;
            }

            catch (Exception err)
            {
                MessageBox.Show(err.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                serialPort1.Close();
                progressBar1.Value = 0;
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                btnClearGraph.Enabled = true;
                lblStatusCom.Text = "OFF";
                timer1.Enabled = false;
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string[] arrList = serialPort1.ReadLine().Split('|');
            temp = arrList[0];
            force = arrList[1];
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(btnOpen.Enabled == false )
            Draw();
            Data_Listview();
        }
        private void Data_Listview()
        {
            if (serialPort1.IsOpen == true)
            {
                double _force;
                double _temp;
                double.TryParse(temp, out _temp);
                double.TryParse(force, out _force);
                ListViewItem item = new ListViewItem(_temp.ToString()); // Gán biến temp vào cột đầu tiên của ListView
                item.SubItems.Add(_force.ToString());
                listView1.Items.Add(item); // Gán biến force vào cột tiếp theo của ListView
                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiện thị dòng được gán gần nhất ở ListView, tức là mình cuộn ListView theo dữ liệu gần nhất đó

            }
        }
        private void ClearZedGraph()
        {
            zedGraphControl1.GraphPane.CurveList.Clear(); // Xóa đường
            zedGraphControl1.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            
            GraphPane myPane1 = zedGraphControl1.GraphPane;
            myPane1.Title.Text = "Đồ thị";
            myPane1.XAxis.Title.Text = "Thời gian";
            myPane1.YAxis.Title.Text = "Nhiệt độ";


            RollingPointPairList listPoint1 = new RollingPointPairList(60000);
            LineItem curve1 = myPane1.AddCurve("Nhiệt độ", listPoint1, Color.Red, SymbolType.None);


           
            myPane1.XAxis.Scale.Min = 0;
            myPane1.XAxis.Scale.Max = 30;
            myPane1.XAxis.Scale.MinorStep = 1;
            myPane1.XAxis.Scale.MajorStep = 5;
            myPane1.YAxis.Scale.Min = 0;
            myPane1.YAxis.Scale.Max = 100;

            zedGraphControl1.AxisChange();

            zedGraphControl2.GraphPane.CurveList.Clear(); // Xóa đường
            zedGraphControl2.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();

            GraphPane myPane2 = zedGraphControl2.GraphPane;
            myPane2.Title.Text = "Đồ thị";
            myPane2.XAxis.Title.Text = "Thời gian";
            myPane2.YAxis.Title.Text = "Lực";

            RollingPointPairList listPoint2 = new RollingPointPairList(60000);
            LineItem curve2 = myPane2.AddCurve("Lực", listPoint2, Color.Blue, SymbolType.None);

            myPane2.XAxis.Scale.Min = 0;
            myPane2.XAxis.Scale.Max = 30;
            myPane2.XAxis.Scale.MinorStep = 1;
            myPane2.XAxis.Scale.MajorStep = 5;
            myPane2.YAxis.Scale.Min = 0;
            myPane2.YAxis.Scale.Max = 100;

            zedGraphControl2.AxisChange();
        }

        private void ResetValue()
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ClearZedGraph();
            btnClose.Enabled = false;
            btnOpen.Enabled = true;
            serialPort1.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void cBoxDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void zedGraphControl2_Load(object sender, EventArgs e)
        {

        }

        private void btClearData_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }
    }
}
