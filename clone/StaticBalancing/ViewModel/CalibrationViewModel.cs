using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticBalancing.ViewModel
{
    public class CalibrationData : ViewModelBase
    {
        private string _balancePosition = string.Empty;
        public string BalancePosition
        {
            get { return _balancePosition; }
            set
            {
                _balancePosition = value;
                OnPropertyChanged("BalancePosition");
            }
        }

        private string _dataPath = string.Empty;
        public string InputDataPath
        {
            get { return _dataPath; }
            set
            {
                _dataPath = value;
                OnPropertyChanged("InputDataPath");
            }
        }

        private List<CaliCounter> _counters = new List<CaliCounter>();
        public List<CaliCounter> Counters
        {
            get { return _counters; }
            set
            {
                _counters = value;
                OnPropertyChanged("Counters");
            }
        }

        public CalibrationData(string bp)
        {
            BalancePosition = bp;
        }
    };

    public class CaliCounter : ViewModelBase
    {
        private string _partname = string.Empty;
        public string PartName
        {
            get { return _partname; }
            set
            {
                _partname = value;
                OnPropertyChanged("PartName");
            }
        }

        private double _mass = 0.0;
        public double Mass
        {
            get { return _mass; }
            set
            {
                _mass = value;
                OnPropertyChanged("Mass");
            }
        }

        private double _thickness = 0.0;
        public double Thickness
        {
            get { return _thickness; }
            set
            {
                _thickness = value;
                OnPropertyChanged("Thickness");
            }
        }

        private int _count = 0;
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        public CaliCounter()
        {

        }

        public CaliCounter(string pn, double w, double t, int count = 0)
        {
            PartName = pn;
            Mass = w;
            Thickness = t;
            Count = count;
        }
    };


    class CalibrationViewModel : ViewModelBase
    {

        public CalibrationViewModel()
        {
            _caliData = new List<CalibrationData>();
        }

        private List<CalibrationData> _caliData = new List<CalibrationData>();
        public List<CalibrationData> CalibrationDataSet
        {
            get { return _caliData; }
            set
            {
                _caliData = value;
                OnPropertyChanged(nameof(CalibrationDataSet));
            }
        }

    }
}
