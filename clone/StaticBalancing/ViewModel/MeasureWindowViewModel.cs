using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticBalancing.ViewModel
{
    public class MeasureData : ViewModelBase
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

        private List<MeasureCounter> _counters = new List<MeasureCounter>();
        public List<MeasureCounter> Counters
        {
            get { return _counters; }
            set
            {
                _counters = value;
                OnPropertyChanged("Counters");
            }
        }

        public MeasureData(string bp)
        {
            BalancePosition = bp;
        }
    };

    public class MeasureCounter : ViewModelBase
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

        public MeasureCounter()
        {

        }

        public MeasureCounter(string pn, double w, double t, int count = 0)
        {
            PartName = pn;
            Mass = w;
            Thickness = t;
            Count = count;
        }
    };

    class MeasureWindowViewModel : ViewModelBase
    {
        public MeasureWindowViewModel()
        {
            _measureData = new List<MeasureData>();
        }

        private List<MeasureData> _measureData = new List<MeasureData>();
        public List<MeasureData> MeasureDataSet
        {
            get { return _measureData; }
            set
            {
                _measureData = value;
                OnPropertyChanged(nameof(MeasureDataSet));
            }
        }
    }
}
