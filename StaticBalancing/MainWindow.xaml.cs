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
            string configFilePath = m_executablePath + "\\system.xml"; ;

            if (File.Exists(configFilePath))
            {
                mainWindowViewModel.SystemConfigFile = configFilePath;
            }

            // init status bar
            InitStatusTimer();
            SetStatus("Applicatio Started", COLOR_SUCCESS);
        }

        private void ConfigFilePathTextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BrowseConfigurationFile();
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            BrowseConfigurationFile();
        }

        private void BrowseConfigurationFile()
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
            }
        }

        // TODO: pop up a windows, display info from the configuraiton file, and let user to select target system and enter serial numbers
        private void SystemSelectButton_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Failed to parse system configuration: " + ex.Data + ". Please check file format. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MeasurementData curr = mainWindowViewModel.CurrentChoseData;

            DataTable table = new DataTable();
            table.Columns.Add("Timestamp", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Imbalance", typeof(double));
            table.Columns.Add("Angle", typeof(double));
            table.Columns.Add("Speed", typeof(double));
            table.Columns.Add("Speed Variation(%)", typeof(double));
            table.Columns.Add("ForceAtMaxSpeed", typeof(double));

            string WEIGHT = "Weight";
            string WEIGHT_CHANGE = "dWeight";

            foreach (KeyValuePair<string, double> kv in curr.DeWeightMap)
            {
                table.Columns.Add(WEIGHT + " " + kv.Key, typeof(double));
            }

            foreach (KeyValuePair<string, double> kv in curr.DeWeightMap)
            {
                table.Columns.Add(WEIGHT_CHANGE + " " + kv.Key, typeof(double));
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
                    thisRow[WEIGHT_CHANGE + " " + kv.Key] = Math.Round(kv.Value, 6);
                }

                foreach (KeyValuePair<string, double> kv in hd.WeightMap)
                {
                    thisRow[WEIGHT + " " + kv.Key] = Math.Round(kv.Value, 6);
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

            AngleAxis axis = new AngleAxis();
            axis.Minimum = 0;
            axis.Maximum = 360;
            axis.StartAngle = 0;
            axis.EndAngle = 360;
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

        private void SerialNumValueLabel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            mainWindowViewModel.SelectedSerialNumber = SerialNumValueLabel.Text;
        }

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

        private void ClearSpeedVsAnglePlot()
        {
            PlotModel empty = new PlotModel();
            empty.Title = "None";
            DataPlotView.Model = empty;
        }

        private void MeasureButton_Click(object sender, RoutedEventArgs e)
        {
            MeasureWindow mw = new MeasureWindow(ref m_balancer.m_systemSelected, ref mainWindowViewModel);
            mw.ShowDialog();

            if(mw.DialogResult == true)
            {
                UpdateHistoryDataGrid();
                DrawSpeedVerseAngle(mainWindowViewModel.CurrentChoseData);
                HistoryResultDataGrid.SelectedIndex = -1;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DumpButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
