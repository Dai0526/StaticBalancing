using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace StaticBalancing.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() {}

        //Bind System Configuration file path
        private string systemConfigurationFile = string.Empty;
        public string SystemConfigFile
        {
            get { return systemConfigurationFile; }
            set
            {
                systemConfigurationFile = value;
                OnPropertyChanged(nameof(SystemConfigFile));
            }
        }

        // Bind System info displayed
        private string selectedSystemModel = string.Empty;
        public string SelectedModel
        {
            get { return selectedSystemModel; }
            set
            {
                selectedSystemModel = value;
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        private string selectedSerialNumber = string.Empty;
        public string SelectedSerialNumber
        {
            get { return selectedSerialNumber; }
            set
            {
                selectedSerialNumber = value;
                OnPropertyChanged(nameof(SelectedSerialNumber));
            }
        }

        private double selectedMaxImba = 0.0;
        public double SelectedModelMaxImba
        {
            get { return selectedMaxImba; }
            set
            {
                selectedMaxImba = value;
                OnPropertyChanged(nameof(SelectedModelMaxImba));
            }
        }

        private double selectedMaxSpe = 0.0;
        public double SelectedModelMaxSpeed
        {
            get { return selectedMaxSpe; }
            set
            {
                selectedMaxSpe = value;
                OnPropertyChanged(nameof(SelectedModelMaxSpeed));
            }
        }

        private double selectedOffset = 0.0;
        public double SelectedModelOffset
        {
            get { return selectedOffset; }
            set
            {
                selectedOffset = value;
                OnPropertyChanged(nameof(SelectedModelOffset));
            }
        }

        private List<BalancePosition> selectedBPList;
        public List<BalancePosition> SelectedBalancePositions
        {
            get { return selectedBPList; }
            set
            {
                selectedBPList = value;
                OnPropertyChanged(nameof(SelectedBalancePositions));
            }
        }

        private List<Counter> selectedCounterList;
        public List<Counter> SelectedCounters
        {
            get { return selectedCounterList; }
            set
            {
                selectedCounterList = value;
                OnPropertyChanged(nameof(SelectedCounters));
            }
        }

        public void SetDisplayedInfo(string model, string serial, double maxspe, double maximba, double offset)
        {
            SelectedModel = model;
            SelectedSerialNumber = serial;
            SelectedModelMaxImba = maximba;
            SelectedModelMaxSpeed = maxspe;
            SelectedModelOffset = offset;
        }

        public void SetDisplayedInfo(SystemInfo info)
        {
            SelectedModel = info.m_model;
            SelectedModelMaxImba = info.m_maxImbalance;
            SelectedModelMaxSpeed = info.m_maxSpeed;
            SelectedModelOffset = info.m_homeTickOffset;

            SelectedBalancePositions = info.m_balancePos;
            SelectedCounters = (info.m_counters.Values).ToList();
        }


        // Bind Calibration Result
        private CalibrationResult calibrationResult;
        public CalibrationResult CalibrationResult
        {
            get { return calibrationResult; }
            set
            {
                calibrationResult = value;
                OnPropertyChanged(nameof(CalibrationResult));
            }
        }

        private List<ForceVector> caliForceVectors = new List<ForceVector>();
        public List<ForceVector> ForceVectors
        {
            get { return caliForceVectors; }
            set
            {
                caliForceVectors = value;
                OnPropertyChanged(nameof(ForceVectors));
            }
        }

        public void SetCalibrationResult(CalibrationResult res)
        {
            CalibrationResult = res;
            ForceVectors = res.ForceVectors.Values.ToList();
        }

        // Bind Status Label attribute
        private SolidColorBrush statusLabelColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        public SolidColorBrush StatusLabelColor
        {
            get { return statusLabelColor; }
            set
            {
                statusLabelColor = value;
                OnPropertyChanged(nameof(StatusLabelColor));
            }
        }

        //Bind reconMainStatusLabel
        private string statusLabelContent = "";
        public string StatusLabelContent
        {
            get { return statusLabelContent; }
            set
            {
                statusLabelContent = value;
                OnPropertyChanged(nameof(StatusLabelContent));
            }
        }

        //bind history result
        private List<MeasurementData> historyRecord = new List<MeasurementData>();
        public List<MeasurementData> HistoryRecord
        {
            get { return historyRecord; }
            set
            {
                historyRecord = value;
                OnPropertyChanged(nameof(HistoryRecord));
            }
        }

        private MeasurementData currentChoseData = new MeasurementData();
        public MeasurementData CurrentChoseData
        {
            get { return currentChoseData; }
            set
            {
                currentChoseData = value;
                OnPropertyChanged(nameof(CurrentChoseData));
            }
        }


        // Calibrate status
        private bool calibrateStatus = true;
        public bool CalibrationStatus
        {
            get { return calibrateStatus; }
            set
            {
                calibrateStatus = value;
                OnPropertyChanged(nameof(CalibrationStatus));
            }
        }


        // direcoty path
        private string lastVisistedDirecoty = @"C:\";
        public string LastVisitedDirectory
        {
            get { return lastVisistedDirecoty; }
            set
            {
                lastVisistedDirecoty = value;
                OnPropertyChanged(nameof(LastVisitedDirectory));
            }
        }

        public void UpdateWorkingDirectory(string path)
        {
            LastVisitedDirectory = System.IO.Path.GetDirectoryName(path);  
        }
    }
}
