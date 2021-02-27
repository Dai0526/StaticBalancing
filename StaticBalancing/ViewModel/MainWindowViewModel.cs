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
            SelectedSerialNumber = info.m_serialNumber;
            SelectedModelMaxImba = info.m_maxImbalance;
            SelectedModelMaxSpeed = info.m_maxSpeed;
            SelectedModelOffset = info.m_homeTickOffset;

            SelectedBalancePositions = info.m_balancePos;
            SelectedCounters = (info.m_counters.Values).ToList();
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

    }
}
