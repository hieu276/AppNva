using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.WinForms.SmithChart;
using System.Collections.ObjectModel;

// Giao tiếp qua Serial
using System.IO;
using System.IO.Ports;
using System.Xml;

// Thêm ZedGraph
using ZedGraph;

namespace AppVna
{
    public partial class ComName : Form
    {
        // biến string để lấy dữ liệu từ Serial
        string Srl_db = String.Empty; 
        string Sphi_deg = String.Empty;
        string Srs = String.Empty;
        string Sxs = String.Empty;
        string Sswr = String.Empty;
        string Sz = String.Empty;
        string Sre = String.Empty;
        string Sim = String.Empty;


        int status = 0; // Khai báo biến để xử lý sự kiện vẽ đồ thị
        double rl_db = 0; //convert string thành số để vẽ đồ thị
        double phi_deg = 0;
        double rs = 0;
        double xs = 0;
        double swr = 0;
        double z = 0;
        double re = 0;// dùng để vẽ smith chart
        double im = 0;// dùng để vẽ smith chart


        ViewModel vm = new ViewModel(); //Khai báo ...

        public ComName()
        {
            InitializeComponent();
            
            //real impedance part 
            LineSeries series = new LineSeries();
            series.MarkerVisible = true;
            series.LegendText = "real";
            series.DataSource = vm.Trace1;
            series.ResistanceMember = "Resistance";
            series.ReactanceMember = "Reactance";
            series.TooltipVisible = true;
            sfSmithChart1.Series.Add(series);

            // complex impedance part
            LineSeries series1 = new LineSeries();
            series1.MarkerVisible = true;
            series1.LegendText = "complex";
            series1.DataSource = vm.Trace2;
            series1.ResistanceMember = "Resistance";
            series1.ReactanceMember = "Reactance";
            series1.TooltipVisible = true;
            sfSmithChart1.Series.Add(series1);

            sfSmithChart1.RadialAxis.MinorGridlinesVisible = true;
            sfSmithChart1.HorizontalAxis.MinorGridlinesVisible = true;

            sfSmithChart1.ThemeName = "Office2016White";
            sfSmithChart1.Legend.Visible = true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames(); // Lấy nguồn cho comboBox là tên của cổng COM
            comboBox1.Text = Properties.Settings.Default.ComName; // Lấy ComName đã làm ở bước 5 cho comboBox
            
            // Khởi tạo ZedGraph
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị dữ liệu theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Dữ liệu";

            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Dữ liệu", list, Color.Red, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;

            myPane.AxisChange();
        }
        // Hàm Tick bắt sự kiện cổng Serial mở hay không
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                progressBar1.Value = 0;
            }
            else if (serialPort1.IsOpen)
            {
                progressBar1.Value = 100;
                Draw();
                Data_Listview();
                status = 0;
            }
        }

        // Hàm này lưu lại cổng COM đã chọn cho lần kết nối
        private void SaveSetting()
        {
            Properties.Settings.Default.ComName = comboBox1.Text;
            Properties.Settings.Default.Save();
        }
        // Nhận và xử lý string gửi từ Serial
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string[] measure_result = serialPort1.ReadLine().Split('|'); // Đọc một dòng của Serial, cắt chuỗi khi gặp ký tự gạch đứng
                Srl_db = measure_result[2]; 
                Sphi_deg = measure_result[3];
                Srs = measure_result[4];
                Sxs = measure_result[5];
                Sre = measure_result[6];
                Sim = measure_result[7];
                Sswr = measure_result[8];
                Sz = measure_result[9];


                double.TryParse(Srl_db, out rl_db); // Chuyển đổi sang kiểu double
                double.TryParse(Sphi_deg, out phi_deg);
                double.TryParse(Srs, out rs);
                double.TryParse(Sxs, out xs);
                double.TryParse(Sre, out re);
                double.TryParse(Sim, out im);
                double.TryParse(Sswr, out swr);
                double.TryParse(Sz, out z);
                status = 1; // Bắt sự kiện xử lý xong chuỗi, đổi starus về 1 để hiển thị dữ liệu trong ListView và vẽ đồ thị
            }
            catch
            {
                return;
            }
        }



        // Hiển thị dữ liệu trong ListView
        private void Data_Listview()
        {
            if (status == 0)
                return;
            else
            {
                ListViewItem item = new ListViewItem(realtime.ToString()); // Gán biến realtime vào cột đầu tiên của ListView
                item.SubItems.Add(datas.ToString());
                listView1.Items.Add(item); // Gán biến datas vào cột tiếp theo của ListView
                                           // Không nên gán string SDatas vì khi xuất dữ liệu sang Excel sẽ là dạng string, không thực hiện các phép toán được

                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiện thị dòng được gán gần nhất ở ListView, tức là mình cuộn ListView theo dữ liệu gần nhất đó
            }
        }



        // Vẽ đồ thị
        private void Draw()
        {

            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;

            if (curve == null)
                return;

            IPointListEdit list = curve.Points as IPointListEdit;

            if (list == null)
                return;

            list.Add(realtime, datas); // Thêm điểm trên đồ thị

            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (realtime > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = realtime + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }

            // Tự động Scale theo trục y
            if (datas > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = datas + yScale.MajorStep;
            }
            else if (datas < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = datas - yScale.MajorStep;
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }


        // Xóa đồ thị
        private void ClearZedGraph()
        {
            zedGraphControl1.GraphPane.CurveList.Clear(); // Xóa đường
            zedGraphControl1.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị dữ liệu theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Dữ liệu";

            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Dữ liệu", list, Color.Red, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;

            zedGraphControl1.AxisChange();
        }


         
        // Hàm xóa dữ liệu
        private void ResetValue()
        {
            realtime = 0;
            datas = 0;
            SDatas = String.Empty;
            SRealTime = String.Empty;
            status = 0; // Chuyển status về 0
        }





        // Hàm lưu ListView sang Excel
        private void SaveToExcel()
        {
            Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();
            xla.Visible = true;
            Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;

            // Đặt tên cho hai ô A1. B1 lần lượt là "Thời gian (s)" và "Dữ liệu", sau đó tự động dãn độ rộng
            Microsoft.Office.Interop.Excel.Range rg = (Microsoft.Office.Interop.Excel.Range)ws.get_Range("A1", "B1");
            ws.Cells[1, 1] = "Thời gian (s)";
            ws.Cells[1, 2] = "Dữ liệu";
            rg.Columns.AutoFit();

            // Lưu từ ô đầu tiên của dòng thứ 2, tức ô A2
            int i = 2;
            int j = 1;

            foreach (ListViewItem comp in listView1.Items)
            {
                ws.Cells[i, j] = comp.Text.ToString();
                foreach (ListViewItem.ListViewSubItem drv in comp.SubItems)
                {
                    ws.Cells[i, j] = drv.Text.ToString();
                    j++;
                }
                j = 1;
                i++;
            }
        }




        // Sự kiện nhấn nút btConnect
        private void btConnect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("2"); //Gửi ký tự "2" qua Serial, tương ứng với state = 2
                serialPort1.Close();
                btConnect.Text = "Kết nối";
                btExit.Enabled = true;
                SaveSetting(); // Lưu cổng COM vào ComName
            }
            else
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text; // Lấy cổng COM
                    serialPort1.BaudRate = 9600; // Baudrate là 9600, trùng với baudrate của Arduino
                    serialPort1.Open();
                    btConnect.Text = "Ngắt kết nối";
                    btExit.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("Không thể mở cổng " + serialPort1.PortName, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        // Sự kiện nhấn nút btExit
        private void btExit_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có chắc muốn thoát?", "Thoát", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                Application.Exit(); // Đóng ứng dụng
            }
        }


        // Sự kiện nhấn nút btSave
        private void btSave_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có muốn lưu số liệu?", "Lưu", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                SaveToExcel(); // Thực thi hàm lưu ListView sang Excel
            }
        }


        // Sự kiện nhấn nút btRun
        private void btRun_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("1"); //Gửi ký tự "1" qua Serial, chạy hàm tạo Random ở Arduino
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        // Sự kiện nhấn nút btPause
        private void btPause_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("0"); //Gửi ký tự "0" qua Serial, Dừng Arduino
            }
            else
                MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        // Sự kiện nhấn nút Clear
        private void btClear_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                DialogResult traloi;
                traloi = MessageBox.Show("Bạn có chắc muốn xóa?", "Xóa dữ liệu", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (traloi == DialogResult.OK)
                {
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.Write("2"); //Gửi ký tự "2" qua Serial
                        listView1.Items.Clear(); // Xóa listview

                        //Xóa đường trong đồ thị
                        ClearZedGraph();

                        //Xóa dữ liệu trong Form
                        ResetValue();
                    }
                    else
                        MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Bạn không thể xóa khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        

        // Đổi màu line Smith Chart
        private void PaletteChanged(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedIndex == 0)
                sfSmithChart1.ColorModel.Palette = Syncfusion.WinForms.SmithChart.ChartColorPalette.Metro;
            else if (combo.SelectedIndex == 1)
                sfSmithChart1.ColorModel.Palette = Syncfusion.WinForms.SmithChart.ChartColorPalette.WarmCold;
            else if (combo.SelectedIndex == 2)
                sfSmithChart1.ColorModel.Palette = Syncfusion.WinForms.SmithChart.ChartColorPalette.Triad;
            else if (combo.SelectedIndex == 3)
                sfSmithChart1.ColorModel.Palette = Syncfusion.WinForms.SmithChart.ChartColorPalette.Colorful;
            else if (combo.SelectedIndex == 4)
                sfSmithChart1.ColorModel.Palette = Syncfusion.WinForms.SmithChart.ChartColorPalette.Nature;
        }


        // Chú thích cho Smith Chart
        private void LegendPosition(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedIndex == 0)
                sfSmithChart1.Legend.DockPosition = Syncfusion.WinForms.SmithChart.ChartDockPosition.Top;
            else if (combo.SelectedIndex == 1)
                sfSmithChart1.Legend.DockPosition = Syncfusion.WinForms.SmithChart.ChartDockPosition.Left;
            else if (combo.SelectedIndex == 2)
                sfSmithChart1.Legend.DockPosition = Syncfusion.WinForms.SmithChart.ChartDockPosition.Right;
            else if (combo.SelectedIndex == 3)
                sfSmithChart1.Legend.DockPosition = Syncfusion.WinForms.SmithChart.ChartDockPosition.Bottom;
            else
                sfSmithChart1.Legend.DockPosition = Syncfusion.WinForms.SmithChart.ChartDockPosition.Floating;
        }


        private void RadialMinor(object sender, EventArgs e)
        {
            var check = sender as CheckBox;
            if (check.Checked)
                sfSmithChart1.RadialAxis.MinorGridlinesVisible = true;
            else
                sfSmithChart1.RadialAxis.MinorGridlinesVisible = false;
        }


        private void HorizontalMinor(object sender, EventArgs e)
        {
            var check = sender as CheckBox;
            if (check.Checked)
                sfSmithChart1.HorizontalAxis.MinorGridlinesVisible = true;
            else
                sfSmithChart1.HorizontalAxis.MinorGridlinesVisible = false;
        }


        private void Radius(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedIndex == 0)
                sfSmithChart1.Radius = 0.1f;
            else if (combo.SelectedIndex == 1)
                sfSmithChart1.Radius = 0.2f;
            else if (combo.SelectedIndex == 2)
                sfSmithChart1.Radius = 0.3f;
            else if (combo.SelectedIndex == 3)
                sfSmithChart1.Radius = 0.4f;
            else if (combo.SelectedIndex == 4)
                sfSmithChart1.Radius = 0.5f;
            else if (combo.SelectedIndex == 5)
                sfSmithChart1.Radius = 0.6f;
            else if (combo.SelectedIndex == 6)
                sfSmithChart1.Radius = 0.7f;
            else if (combo.SelectedIndex == 7)
                sfSmithChart1.Radius = 0.8f;
            else if (combo.SelectedIndex == 8)
                sfSmithChart1.Radius = 0.9f;
            else if (combo.SelectedIndex == 9)
                sfSmithChart1.Radius = 1f;
        }


        private void VisualStyleChanged(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedIndex == 0)
                sfSmithChart1.ThemeName = "Office2016White";
            else if (combo.SelectedIndex == 1)
                sfSmithChart1.ThemeName = "Office2016DarkGray";
            else if (combo.SelectedIndex == 2)
                sfSmithChart1.ThemeName = "Office2016Colorful";
            else
                sfSmithChart1.ThemeName = "Office2016Black";
        }
    }


    public class Model
    {
        public double Resistance { get; set; }

        public double Reactance { get; set; }
    }


    public class ViewModel
    {
        public ViewModel()
        {
            Trace1 = new ObservableCollection<Model>();

            Trace1.Add(new Model() { Resistance = 0, Reactance = 0.05 });
            Trace1.Add(new Model() { Resistance = 0.3, Reactance = 0.1 });
            Trace1.Add(new Model() { Resistance = 0.5, Reactance = 0.2 });
            Trace1.Add(new Model() { Resistance = 1.0, Reactance = 0.4 });
            Trace1.Add(new Model() { Resistance = 1.5, Reactance = 0.5 });
            Trace1.Add(new Model() { Resistance = 2.0, Reactance = 0.5 });
            Trace1.Add(new Model() { Resistance = 2.5, Reactance = 0.4 });
            Trace1.Add(new Model() { Resistance = 3.5, Reactance = 0.0 });
            Trace1.Add(new Model() { Resistance = 4.5, Reactance = -0.5 });
            Trace1.Add(new Model() { Resistance = 5, Reactance = -1.0 });
            Trace1.Add(new Model() { Resistance = 6, Reactance = -1.5 });
            Trace1.Add(new Model() { Resistance = 7, Reactance = -2.5 });
            Trace1.Add(new Model() { Resistance = 8, Reactance = -3.5 });
            Trace1.Add(new Model() { Resistance = 9, Reactance = -4.5 });
            Trace1.Add(new Model() { Resistance = 10, Reactance = -10 });
            Trace1.Add(new Model() { Resistance = 20, Reactance = -50 });

            Trace2 = new ObservableCollection<Model>();

            Trace2.Add(new Model() { Resistance = 0, Reactance = 0.15 });
            Trace2.Add(new Model() { Resistance = 0.3, Reactance = 0.2 });
            Trace2.Add(new Model() { Resistance = 0.5, Reactance = 0.4 });
            Trace2.Add(new Model() { Resistance = 1.0, Reactance = 0.8 });
            Trace2.Add(new Model() { Resistance = 1.5, Reactance = 1.0 });
            Trace2.Add(new Model() { Resistance = 2.0, Reactance = 1.2 });
            Trace2.Add(new Model() { Resistance = 2.5, Reactance = 1.3 });
            Trace2.Add(new Model() { Resistance = 3.5, Reactance = 1.6 });
            Trace2.Add(new Model() { Resistance = 4.5, Reactance = 2.0 });
            Trace2.Add(new Model() { Resistance = 6, Reactance = 4.5 });
            Trace2.Add(new Model() { Resistance = 8, Reactance = 6 });
            Trace2.Add(new Model() { Resistance = 10, Reactance = 25 });
        }

        public ObservableCollection<Model> Trace1 { get; set; }
        public ObservableCollection<Model> Trace2 { get; set; }
    }
}
