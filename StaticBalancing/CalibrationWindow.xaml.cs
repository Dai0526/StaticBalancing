using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using StaticBalancing;
using StaticBalancing.ViewModel;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        CalibrationViewModel m_caliViewModel;
        MainWindowViewModel m_mainVM;
        SystemInfo m_system;

        public CalibrationWindow()
        {
            InitializeComponent();
        }

        public CalibrationWindow(ref SystemInfo selected, ref MainWindowViewModel mwvm)
        {

            InitializeComponent();

            m_system = selected;
            m_mainVM = mwvm;

            InitDisplayInfo();
        }

        private void InitDisplayInfo()
        {
            m_caliViewModel = new CalibrationViewModel();
            this.DataContext = m_caliViewModel;

            // Copy Counter to CaliCounter
            List<Counter> ctrs = (m_system.m_counters.Values).ToList();
            // set calibration balance position
            List<BalancePosition> bps = m_system.m_balancePos;

            //// add base
            List<CaliCounter> listCtr = new List<CaliCounter>();
            for (int i = 0; i < ctrs.Count(); ++i)
            {
                Counter src = ctrs[i];
                CaliCounter cc = new CaliCounter(src.PartNumber, src.Mass, src.Thickness);
                listCtr.Add(cc);
            }

            CalibrationData run0 = new CalibrationData("BaseRun");
            run0.Counters = new List<CaliCounter>(listCtr);
            m_caliViewModel.CalibrationDataSet.Add(run0);

            foreach (BalancePosition src in bps)
            {
                List<CaliCounter> counters = new List<CaliCounter>();
                for (int i = 0; i < ctrs.Count(); ++i)
                {
                    Counter temp = ctrs[i];
                    CaliCounter cc = new CaliCounter(temp.PartNumber, temp.Mass, temp.Thickness);
                    counters.Add(cc);
                }

                CalibrationData cd = new CalibrationData(src.ID);
                cd.Counters = new List<CaliCounter>(counters);
                m_caliViewModel.CalibrationDataSet.Add(cd);
            }
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            // validation data
            bool isValid = true;
            foreach(CalibrationData data in m_caliViewModel.CalibrationDataSet)
            {
                if (string.IsNullOrEmpty(data.InputDataPath))
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                m_mainVM.CalibrationStatus = false;
                MessageBox.Show("Please set Input Data Path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // calculation
                DataHandler dh = new DataHandler();
                Arithmetic math = new Arithmetic();

                CalibrationData baseData = m_caliViewModel.CalibrationDataSet[0];
                InputRaw baseIn = dh.LoadDataFromCSV(baseData.InputDataPath);
                SineRegCoef baseRun = math.GetRegressionCoef(baseIn);

                // it shoule be sequential
                for (int i = 1; i < m_caliViewModel.CalibrationDataSet.Count(); ++i)
                {
                    CalibrationData curr = m_caliViewModel.CalibrationDataSet[i];
                    InputRaw raw = dh.LoadDataFromCSV(curr.InputDataPath);
                    SineRegCoef coef = math.GetRegressionCoef(raw);

                    BalancePosition bp = m_system.m_balancePos[i - 1];
                    bp.LastRunCoef = coef;
                    bp.Counters = new Dictionary<string, int>();

                    foreach (CaliCounter cc in curr.Counters)
                    {
                        bp.Counters[cc.PartName] = cc.Count;
                    }

                    // set back to selected system
                    m_system.m_balancePos[i - 1] = bp;
                }

                // Compute dataset
                CalibrationResult result = math.GetCalibrationMatrix(baseRun, m_system.m_balancePos, baseRun, m_system.m_counters, Convert.ToSingle(m_system.m_maxSpeed));
                m_mainVM.SetCalibrationResult(result);

                // set data to table
                HistoryData temp = new HistoryData();
                temp.Imbalance = result.ResidualImblance;
                temp.Speed = result.Speed;
                temp.SpeedVariation = result.SpeedVariation;
                temp.Timestamp = GetCurrentTimestamp();
                temp.SerialNumber = m_mainVM.SelectedSerialNumber;
                temp.Angle = result.Phase;
                temp.ForceAtMaxSpeed = result.ForceAtMaxSpeed;
                temp.DeWeightMap = new Dictionary<string, double>(result.WeightChange);
                temp.Model = m_mainVM.SelectedModel;
                temp.StatusCoef = baseRun;
                temp.RawData = baseIn;
                temp.BalanceStats = temp.Imbalance > m_mainVM.SelectedModelMaxImba ? BALANCE_STATUS.FAILED : BALANCE_STATUS.SUCCESS;

                m_mainVM.HistoryRecord.Add(temp);
                m_mainVM.CurrentChoseData = temp;

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                m_mainVM.CalibrationStatus = false;
                MessageBox.Show("Fail to Calibrate: " + ex.Data, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Close();
        }

        private string GetCurrentTimestamp()
        {
            DateTime now = DateTime.Now;
            return now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            m_mainVM.CalibrationStatus = false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fbd = new System.Windows.Forms.OpenFileDialog();
            fbd.Title = "Please select Input Data File";
            fbd.InitialDirectory = @"C:\";
            fbd.CheckFileExists = true;
            fbd.CheckPathExists = true;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string myTag = ((Button)sender).Tag.ToString();
                foreach (CalibrationData data in m_caliViewModel.CalibrationDataSet)
                {
                    if (data.BalancePosition == myTag)
                    {
                        data.InputDataPath = fbd.FileName;
                    }
                }
            }
        }
    }
}
