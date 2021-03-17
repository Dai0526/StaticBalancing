using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;

using OxyPlot;
using OxyPlot.Wpf;

using StaticBalancing.ViewModel;
using System.Windows.Media.Imaging;
using System.Data;
using System.Windows.Controls;
using System.Text;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // constant
        private readonly SolidColorBrush COLOR_ERROR = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush COLOR_DEFAULT = new SolidColorBrush(Colors.Blue);
        private readonly SolidColorBrush COLOR_SUCCESS = new SolidColorBrush(Colors.Green);

        public string WEIGHT = "Weight";
        public string WEIGHT_CHANGE = "DeWeight";

        // member
        public BalancingCore m_balancer;
        public SystemInfo m_selectedSystem;
        public Arithmetic m_balanceCalculator;

        // path
        private string m_executablePath;

        // view model
        static MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

        // for time elapsed status bar
        private DispatcherTimer m_dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitBalancing();
        }

        private void InitBalancing()
        {
            this.DataContext = mainWindowViewModel;

            // init configuration path
            m_executablePath = Directory.GetCurrentDirectory();
            string configFilePath = m_executablePath + "\\Systems.xml";

            if (File.Exists(configFilePath))
            {
                mainWindowViewModel.SystemConfigFile = configFilePath;
                LoadBalacingCore();
            }

            // init status bar
            InitStatusTimer();
            SetStatus("Applicatio Started", COLOR_SUCCESS);
        }

        private void ConfigFilePathTextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.DialogResult browsResult = BrowseConfigurationFile();

            if (browsResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            LoadBalacingCore();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult browsResult = BrowseConfigurationFile();

            if(browsResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            LoadBalacingCore();
        }

        private void LoadBalacingCore()
        {
            if (!File.Exists(mainWindowViewModel.SystemConfigFile))
            {
                MessageBox.Show("System Configuraton file doesn't exist: " + mainWindowViewModel.SystemConfigFile + ". Please verify the file location.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                m_balancer = new BalancingCore();
                m_balancer.ReadSystemInfoFiles(mainWindowViewModel.SystemConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse system configuration: " + ex.ToString() + ". Please check file format. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private System.Windows.Forms.DialogResult BrowseConfigurationFile()
        {
            System.Windows.Forms.OpenFileDialog fbd = new System.Windows.Forms.OpenFileDialog();
            fbd.Title = "Please select the configuration file";
            fbd.DefaultExt = "xml";
            fbd.Filter = "xml files (*.xml)|*.xml";
            fbd.InitialDirectory = @"C:\";
            fbd.CheckFileExists = true;
            fbd.CheckPathExists = true;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mainWindowViewModel.SystemConfigFile = fbd.FileName;
                return System.Windows.Forms.DialogResult.OK;
            }

            return System.Windows.Forms.DialogResult.Cancel;
        }

        // TODO: pop up a window, display info from the configuraiton file, and let user to select target system and enter serial numbers
        private void SystemSelectButton_Click(object sender, RoutedEventArgs e)
        {
            if(m_balancer == null || string.IsNullOrEmpty(mainWindowViewModel.SystemConfigFile))
            {
                MessageBox.Show("Please use Browse Button to set the system cofiguration file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SystemSelectionWindow ssw = new SystemSelectionWindow(ref m_balancer);
            ssw.ShowDialog();

            DisplaySelectSystem();
        }

        private void DisplaySelectSystem()
        {
            if(m_balancer.m_systemSelected == null)
            {
                MessageBox.Show("Please select a system to Calibrate", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SystemInfo selected = m_balancer.m_systemSelected;

            mainWindowViewModel.SetDisplayedInfo(selected);
        }

        private void UpdateHistoryDataGrid()
        {
            // set up table header
            if(mainWindowViewModel.HistoryRecord.Count <=0)
            {
                return;
            }

            MeasurementData curr = mainWindowViewModel.HistoryRecord[0];

            DataTable table = new DataTable();
            table.Columns.Add("Timestamp", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Imbalance", typeof(double));
            table.Columns.Add("Angle", typeof(double));
            table.Columns.Add("Speed", typeof(double));
            table.Columns.Add("Speed Variation(%)", typeof(double));
            table.Columns.Add("ForceAtMaxSpeed", typeof(double));

            foreach (KeyValuePair<string, double> kv in curr.DeWeightMap)
            {
                table.Columns.Add(WEIGHT + "_" + kv.Key, typeof(double));
            }

            foreach (KeyValuePair<string, double> kv in curr.DeWeightMap)
            {
                table.Columns.Add(WEIGHT_CHANGE + "_" + kv.Key, typeof(double));
            }


            // Fill with value
            foreach (MeasurementData hd in mainWindowViewModel.HistoryRecord)
            {
                DataRow thisRow = table.NewRow();
                thisRow["Timestamp"] = hd.Timestamp;
                thisRow["Imbalance"] = Math.Round(hd.Imbalance, 6);
                thisRow["Status"] = hd.BalanceStats.ToString();
                thisRow["Angle"] = Math.Round(hd.Angle, 6);
                thisRow["Speed"] = Math.Round(hd.Speed, 6);
                thisRow["Speed Variation(%)"] = Math.Round(hd.SpeedVariation * 100, 6);
                thisRow["ForceAtMaxSpeed"] = Math.Round(hd.ForceAtMaxSpeed, 6);


                foreach (KeyValuePair<string, double> kv in hd.DeWeightMap)
                {
                    thisRow[WEIGHT_CHANGE + "_" + kv.Key] = Math.Round(kv.Value, 6);
                }

                foreach (KeyValuePair<string, double> kv in hd.WeightMap)
                {
                    thisRow[WEIGHT + "_" + kv.Key] = Math.Round(kv.Value, 6);
                }

                table.Rows.Add(thisRow);
            }

            // update dataview
            HistoryResultDataGrid.ItemsSource = table.DefaultView;
        }


        #region Draw Plot Functions
        private void DrawForceVector()
        {
            List<ForceVector> fs = mainWindowViewModel.ForceVectors;
            double maxMag = double.MinValue;
            foreach(ForceVector fv in fs)
            {
                maxMag = Math.Max(Math.Abs(fv.Imbalance), maxMag);
            }

            // Set OxyPlot Model attributes
            OxyPlot.Wpf.Plot ForceDiagramPlot = new Plot();
            ForceDiagramPlot.PlotType = PlotType.Polar;
            ForceDiagramPlot.PlotAreaBorderThickness = new Thickness(0);
            ForceDiagramPlot.PlotMargins = new Thickness(25, 25, 25, 25);
            ForceDiagramPlot.Title = "Force Diagram";

            AngleAxis axis = new AngleAxis();
            axis.Minimum = 0;
            axis.Maximum = 360;
            axis.StartAngle = 90;
            axis.EndAngle = -270;
            axis.MajorStep = 30;
            axis.MinorStep = 15;
            axis.Title = "Angle";

            MagnitudeAxis magAxis = new MagnitudeAxis();
            magAxis.Minimum = 0;
            magAxis.Maximum = maxMag * 1.2;
            magAxis.MajorStep = (int) maxMag / 5;
            magAxis.MinorStep = (int) maxMag / 5;
            magAxis.Angle = 0;
            magAxis.Title = "Magnitude";

            ForceDiagramPlot.Axes.Add(axis);
            ForceDiagramPlot.Axes.Add(magAxis);

            // Add Data Point
            foreach(ForceVector fv in fs)
            {
                DataPoint start = new DataPoint(0, 0);
                DataPoint end = new DataPoint(fv.Imbalance, fv.CoefDiffVector.Phase / Math.PI * 180);

                OxyPlot.Wpf.Series series = new OxyPlot.Wpf.LineSeries();
                series.Title = fv.ID;

                // set force vector
                List<DataPoint> dps = new List<DataPoint>();
                dps.Add(start);
                dps.Add(end);

                series.ItemsSource = dps;

                ForceDiagramPlot.Series.Add(series);
            }

            ImageSource src = new BitmapImage(new Uri("pack://application:,,,/StaticBalancing;component/Resource/Images/CTRotorWithAngle.png"));
            ImageBrush backgroud = new ImageBrush(src);
            backgroud.TileMode = TileMode.FlipX;
            backgroud.Stretch = Stretch.UniformToFill;
            backgroud.Opacity = 0.1;
            ImbalanceVectorPlotGrid.Background = backgroud;
            ForceDiagramPlot.Background = backgroud;

            ImbalanceVectorPlotGrid.Children.Add(ForceDiagramPlot);
        }

        private void DrawSpeedVerseAngle(MeasurementData data)
        {
            PlotModel model = new PlotModel();
            model.Title = data.Timestamp;
            model.LegendPosition = LegendPosition.RightBottom;
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendOrientation = LegendOrientation.Horizontal;

            double A = data.StatusCoef.A, B = data.StatusCoef.B, C = data.StatusCoef.C;
            OxyPlot.Series.FunctionSeries serie = new OxyPlot.Series.FunctionSeries();

            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int angle = 0; angle < 360; angle += 1)
            {
                double x = (double)angle;
                double y = A * Math.Cos(x * Math.PI / 180) + B * Math.Sin(x * Math.PI / 180) + C;

                minY = Math.Min(y, minY);
                maxY = Math.Max(y, maxY);

                DataPoint dp = new DataPoint(x, y);
                serie.Points.Add(dp);
            }

            model.Series.Add(serie);

            OxyPlot.Axes.LinearAxis Yaxis = new OxyPlot.Axes.LinearAxis();
            Yaxis.Minimum = minY - 1.5;
            Yaxis.Maximum = maxY + 1.5;
            Yaxis.MajorStep = 1;
            Yaxis.MinorStep = 0.2;
            Yaxis.MajorGridlineStyle = LineStyle.Solid;
            Yaxis.MinorGridlineStyle = LineStyle.Dot;
            Yaxis.Title = "Speed (rpm)";
            Yaxis.IsZoomEnabled = false;

            OxyPlot.Axes.LinearAxis Xaxis = new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 360,
                MinorStep = 5,
                MajorStep = 90,
                IsZoomEnabled = false
            };

            Xaxis.Title = "Angle";

            model.Axes.Add(Xaxis);
            model.Axes.Add(Yaxis);

            DataPlotView.Model = model;

        }
        #endregion

        #region Status and Status Bar Control
        // Status Bar Funcs
        private void SetStatus(string status, SolidColorBrush c, int durationSec = 3)
        {
            mainWindowViewModel.StatusLabelContent = status;
            mainWindowViewModel.StatusLabelColor = c;
            m_dispatcherTimer.Interval = new TimeSpan(0, 0, durationSec);
            m_dispatcherTimer.Start();
        }

        private void InitStatusTimer()
        {
            //Create a timer with interval of 2 secs for status display
            m_dispatcherTimer = new DispatcherTimer();
            m_dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            m_dispatcherTimer.Interval = new TimeSpan(0, 0, 3); // 3 seconds as default
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            mainWindowViewModel.StatusLabelContent = "";
            mainWindowViewModel.StatusLabelColor = COLOR_DEFAULT;
            //Disable the timer
            m_dispatcherTimer.IsEnabled = false;
        }



        #endregion



        private void HistoryResultDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(HistoryResultDataGrid.SelectedItem == null)
            {
                return;
            }

            DataRowView dtr = (HistoryResultDataGrid.SelectedItem as DataRowView);
            string selectedTimestamp = (string)dtr.Row[0];

            foreach(MeasurementData md in mainWindowViewModel.HistoryRecord)
            {
                if(string.Compare(md.Timestamp, selectedTimestamp,true) == 0)
                {
                    mainWindowViewModel.CurrentChoseData = md;
                    DrawSpeedVerseAngle(mainWindowViewModel.CurrentChoseData);
                }
            }
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_balancer.m_systemSelected == null)
            {
                MessageBox.Show("Please select a system before Calibration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CalibrationWindow cw = new CalibrationWindow(ref m_balancer.m_systemSelected, ref mainWindowViewModel);
            cw.ShowDialog();

            if (mainWindowViewModel.CalibrationStatus == true)
            {
                DrawForceVector();
                UpdateHistoryDataGrid();
                DrawSpeedVerseAngle(mainWindowViewModel.CurrentChoseData);

                DeleteButton.IsEnabled = true;
                MeasureButton.IsEnabled = true;
            }

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int idx = HistoryResultDataGrid.SelectedIndex;

            if (idx == -1)
            {
                MessageBox.Show("Please select an Data entry to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataRowView dtr = (HistoryResultDataGrid.SelectedItem as DataRowView);
            string selectedTimestamp = (string)dtr.Row[0];

            MessageBoxResult res = MessageBox.Show("Do you want to delete [" + selectedTimestamp + "]", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Error);

            if (res == MessageBoxResult.Cancel)
            {
                return;
            }


            foreach (MeasurementData md in mainWindowViewModel.HistoryRecord)
            {
                if (string.Compare(md.Timestamp, selectedTimestamp, true) == 0)
                {
                    // remove founded item
                    mainWindowViewModel.HistoryRecord.Remove(md);
                    UpdateHistoryDataGrid();
                    // Clear view
                    ClearSpeedVsAnglePlot();
                    return;
                }
            }


        }

        private void MeasureButton_Click(object sender, RoutedEventArgs e)
        {
            if(mainWindowViewModel.CalibrationResult.CalibrationMatrix == null)
            {
                MessageBox.Show("Add Measurement Failed. Please finish calibration", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MeasureWindow mw = new MeasureWindow(ref m_balancer.m_systemSelected, ref mainWindowViewModel);
            mw.ShowDialog();

            if (mw.DialogResult == true)
            {
                UpdateHistoryDataGrid();
                DrawSpeedVerseAngle(mainWindowViewModel.CurrentChoseData);
                HistoryResultDataGrid.SelectedIndex = -1;
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mainWindowViewModel.SelectedModel))
            {
                MessageBox.Show("Failed. Please select system before load data", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.Forms.OpenFileDialog fbd = new System.Windows.Forms.OpenFileDialog();
            fbd.Title = "Please select a data file to Load";
            fbd.DefaultExt = "csv";
            fbd.Filter = "csv files (*.csv)|*.csv";
            fbd.InitialDirectory = @"C:\";
            fbd.CheckFileExists = true;
            fbd.CheckPathExists = true;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    bool loadSuccess = LoadBalancingDataFromFile(fbd.FileName);
                    if (loadSuccess)
                    {
                        UpdateHistoryDataGrid();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load Data Failed: " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {

            if (mainWindowViewModel.HistoryRecord.Count <= 0)
            {
                MessageBox.Show("No measurment data found. Nothing to Write.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    DumpBalancingData(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dump Data Failed: " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

        }


        private void ClearSpeedVsAnglePlot()
        {
            PlotModel empty = new PlotModel();
            empty.Title = "None";
            DataPlotView.Model = empty;
        }


        #region dump/load Measurment data

        public string GetMetaHeader(MeasurementData data)
        {
            // set hdr
            StringBuilder hdr = new StringBuilder();
            hdr.AppendFormat("{0},", "Timestamp");
            hdr.AppendFormat("{0},", "Imbalance");
            hdr.AppendFormat("{0},", "Angle");
            hdr.AppendFormat("{0},", "Speed");
            hdr.AppendFormat("{0},", "ForceAtMaxSpeed");
            hdr.AppendFormat("{0},", "SpeedVariation");
            hdr.AppendFormat("{0}", "Coef");

            int count = 0;
            foreach (KeyValuePair<string, double> p in data.DeWeightMap)
            {
                hdr.AppendFormat(",{0}_{1}", WEIGHT_CHANGE, p.Key);
                ++count;
            }

            foreach (KeyValuePair<string, double> p in data.WeightMap)
            {
                hdr.AppendFormat(",{0}_{1}", WEIGHT, p.Key);
                ++count;
            }

            // set meta
            StringBuilder meta = new StringBuilder();
            meta.AppendFormat("{0},{1}\r\n", "Model", mainWindowViewModel.SelectedModel);
            meta.AppendFormat("{0},{1}\r\n", "Serial", mainWindowViewModel.SelectedSerialNumber);
            meta.AppendFormat("{0},{1}\r\n", "MaxImbalance", mainWindowViewModel.SelectedModelMaxImba);
            meta.AppendFormat("{0},{1}\r\n", "HeaderSize", count + 7);

            string header = hdr.ToString();
            meta.Append(header);

            return meta.ToString();
        }

        public string GetHistoryRecordString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (MeasurementData data in mainWindowViewModel.HistoryRecord)
            {
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},",
                    data.Timestamp,
                    data.Imbalance,
                    data.Angle,
                    data.Speed,
                    data.ForceAtMaxSpeed,
                    data.SpeedVariation);
                sb.AppendFormat("{0};{1};{2}", data.StatusCoef.A, data.StatusCoef.B, data.StatusCoef.C);

                foreach (KeyValuePair<string, double> p in data.DeWeightMap)
                {
                    sb.AppendFormat(",{0}", p.Value);
                }

                foreach (KeyValuePair<string, double> p in data.WeightMap)
                {
                    sb.AppendFormat(",{0}", p.Value);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private bool LoadBalancingDataFromFile(string path)
        {
            string ext = Path.GetExtension(path);
            if (string.Compare(ext, ".csv", true) != 0)
            {
                MessageBox.Show("Please select a csv file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader file = new StreamReader(fs);

            string line = null;

            // check model
            line = file.ReadLine();
            string[] item = line.Split(',');
            if (item.Length != 2) { throw new Exception("Invalid data format."); }

            if (string.Compare(item[0], "Model", true) != 0 || string.Compare(item[1], mainWindowViewModel.SelectedModel, true) != 0)
            {
                throw new Exception("Failed to load data. Model not match: " + item[1]);
            }

            //check serial
            line = file.ReadLine();
            item = line.Split(',');
            if (item.Length != 2) { throw new Exception("Invalid data format."); }

            if (string.Compare(item[0], "Serial", true) != 0 || string.Compare(item[1], mainWindowViewModel.SelectedSerialNumber, true) != 0)
            {
                throw new Exception("Failed to load data. Serial Number not match: " + item[1]);
            }

            // check MaxImbalance
            line = file.ReadLine();
            item = line.Split(',');
            if (item.Length != 2)
            {
                throw new Exception("Invalid data format.");
            }
            double maxImba = Convert.ToDouble(item[1]);
            if (string.Compare(item[0], "MaxImbalance", true) != 0 || maxImba != mainWindowViewModel.SelectedModelMaxImba)
            {
                throw new Exception("Failed to load data. Max Imbalance not match: " + item[1]);
            }


            // check head line
            line = file.ReadLine();
            item = line.Split(',');

            if (string.Compare(item[0], "HeaderSize", true) != 0)
            {
                throw new Exception("Failed to load data. The 4th line is not HeaderLength: " + item[1]);
            }

            int headerCount = Int32.Parse(item[1]);

            // read header
            if (Convert.ToInt32(item[1]) != headerCount)
            {
                throw new Exception("Invalid Data format: Header size not match. Expected = " + headerCount + ", actual get = " + item.Length);
            }

            // get bp name
            int bpCount = (headerCount - 7) / 2;
            List<string> bpPosition = new List<string>();
            line = file.ReadLine();
            item = line.Split(',');

            for (int i = 7; i < 7 + bpCount; ++i)
            {
                string tag = item[i];
                string[] bp = tag.Split('_');

                if (string.Compare(bp[0], WEIGHT_CHANGE) != 0)
                {
                    throw new Exception("Invalid Header format: The 7th header name should be: " + WEIGHT_CHANGE + ". But Actually value is " + bp[0]);
                }
                else
                {
                    bpPosition.Add(bp[1]);
                }
            }


            List<MeasurementData> datas = new List<MeasurementData>();

            while ((line = file.ReadLine()) != null)
            {
                item = line.Split(',');

                if (item.Length == 1)
                {
                    continue;
                }

                if (item.Length != headerCount)
                {
                    throw new Exception("Invalid Data format: Header size not match. Expected = " + headerCount + ", actual get = " + item.Length);
                }

                MeasurementData temp = new MeasurementData();
                temp.Timestamp = item[0];
                temp.Imbalance = Convert.ToDouble(item[1]);
                temp.Angle = Convert.ToDouble(item[2]);
                temp.Speed = Convert.ToDouble(item[3]);
                temp.ForceAtMaxSpeed = Convert.ToDouble(item[4]);
                temp.SpeedVariation = Convert.ToDouble(item[5]);

                // set coef
                SineRegCoef coef = new SineRegCoef();
                string[] coefArry = item[6].Split(';');

                coef.A = Convert.ToDouble(coefArry[0]);
                coef.B = Convert.ToDouble(coefArry[1]);
                coef.C = Convert.ToDouble(coefArry[2]);

                temp.StatusCoef = coef;

                Dictionary<string, double> dwMap = new Dictionary<string, double>();
                Dictionary<string, double> wMap = new Dictionary<string, double>();

                for (int i = 0; i < bpPosition.Count; ++i)
                {
                    dwMap[bpPosition[i]] = 7 + i;
                    wMap[bpPosition[i]] = 7 + i + bpCount;
                }

                temp.DeWeightMap = dwMap;
                temp.WeightMap = wMap;

                temp.BalanceStats = temp.Imbalance <= maxImba ? BALANCE_STATUS.SUCCESS : BALANCE_STATUS.FAILED;

                datas.Add(temp);
            }

            foreach (var mdata in datas)
            {
                AddHisotryData(mdata);
            }

            return true;
        }

        private void AddHisotryData(MeasurementData data)
        {
            bool exist = false;
            foreach (MeasurementData d in mainWindowViewModel.HistoryRecord)
            {
                if (string.Compare(d.Timestamp, data.Timestamp) == 0)
                {
                    exist = true;
                    break;
                }
            }

            if (exist)
            {
                // Already Existed , do not add
                return;
            }

            mainWindowViewModel.HistoryRecord.Add(data);
        }

        private bool DumpBalancingData(string path)
        {
            StreamWriter sw = new StreamWriter(path);

            // Write meta
            string metahdr = GetMetaHeader(mainWindowViewModel.HistoryRecord[0]);
            sw.WriteLine(metahdr);

            // Create data
            string data = GetHistoryRecordString();
            sw.WriteLine(data);

            sw.Close();

            return true;
        }

        #endregion

        private void SerialNumValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
