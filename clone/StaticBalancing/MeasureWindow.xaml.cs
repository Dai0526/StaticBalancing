using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using StaticBalancing.ViewModel;

using MathNet.Numerics.LinearAlgebra;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for MeasureWindow.xaml
    /// </summary>
    public partial class MeasureWindow : Window
    {

        MeasureWindowViewModel m_measureViewModel;
        MainWindowViewModel m_mainVM;
        SystemInfo m_system;

        public MeasureWindow()
        {
            InitializeComponent();
        }

        public MeasureWindow(ref SystemInfo selected, ref MainWindowViewModel mwvm)
        {
            InitializeComponent();

            m_system = selected;
            m_mainVM = mwvm;

            InitNewMeasure();
        }

        public void InitNewMeasure()
        {
            m_measureViewModel = new MeasureWindowViewModel();
            this.DataContext = m_measureViewModel;

            // Copy Counter to CaliCounter
            List<Counter> ctrs = (m_system.m_counters.Values).ToList();
            // set calibration balance position
            List<BalancePosition> bps = m_system.m_balancePos;

            foreach (BalancePosition src in bps)
            {
                List<MeasureCounter> counters = new List<MeasureCounter>();
                for (int i = 0; i < ctrs.Count(); ++i)
                {
                    Counter temp = ctrs[i];
                    MeasureCounter cc = new MeasureCounter(temp.PartNumber, temp.Mass, temp.Thickness);
                    counters.Add(cc);
                }

                MeasureData md = new MeasureData(src.ID);
                md.Counters = new List<MeasureCounter>(counters);
                m_measureViewModel.MeasureDataSet.Add(md);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fbd = new System.Windows.Forms.OpenFileDialog();
            fbd.Title = "Please select Input Data File";
            fbd.InitialDirectory = @m_mainVM.LastVisitedDirectory;
            fbd.CheckFileExists = true;
            fbd.CheckPathExists = true;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataPathTextbox.Text = fbd.FileName;
                m_mainVM.UpdateWorkingDirectory(fbd.FileName);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DataPathTextbox.Text))
            {
                MessageBox.Show("Please set Input Data Path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // calculation
                DataHandler dh = new DataHandler();
                Arithmetic math = new Arithmetic();

                InputRaw measureIn = dh.LoadData(DataPathTextbox.Text);
                SineRegCoef coefIn = math.GetRegressionCoef(measureIn);

                // Need a Balance Position List
                List<BalancePosition> bpList = new List<BalancePosition>();
                for (int i = 0; i < m_measureViewModel.MeasureDataSet.Count(); ++i)
                {
                    MeasureData curr = m_measureViewModel.MeasureDataSet[i];
                    BalancePosition bp = m_system.m_balancePos[i];

                    bp.Counters = new Dictionary<string, int>();

                    foreach (MeasureCounter cc in curr.Counters)
                    {
                        bp.Counters[cc.PartName] = cc.Count;
                    }

                    bpList.Add(bp);
                }

                // Compute dataset
                SineRegCoef baseRun = m_mainVM.CalibrationResult.BaseCoef;
                Matrix<double> caliMatrix = m_mainVM.CalibrationResult.CalibrationMatrix;
                MeasurementData result = math.Measure(baseRun,
                                                        caliMatrix,
                                                        coefIn,
                                                        bpList,
                                                        m_system.m_counters, 
                                                        Convert.ToSingle(m_system.m_maxSpeed));

                // update result info
                result.SerialNumber = m_mainVM.SelectedSerialNumber;
                result.Model = m_mainVM.SelectedModel;
                result.StatusCoef = coefIn;
                result.RawData = measureIn;
                result.BalanceStats = result.Imbalance > m_mainVM.SelectedModelMaxImba ? BALANCE_STATUS.FAILED : BALANCE_STATUS.SUCCESS;

                result.WeightMap = new Dictionary<string, double>();
                foreach (BalancePosition bp in bpList)
                {
                    result.WeightMap.Add(bp.ID, bp.GetWeight(m_system.m_counters));
                }

                m_mainVM.HistoryRecord.Add(result);
                m_mainVM.CurrentChoseData = result;

                this.DialogResult = true;
            }
            catch (Exception)
            {
                m_mainVM.CalibrationStatus = false;
                //MessageBox.Show("Fail to Add Measurement: " + ex.Tostring(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show("Fail to Add Measurement. Invalid History data file format.","Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private string GetCurrentTimestamp()
        {
            DateTime now = DateTime.Now;
            return now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
