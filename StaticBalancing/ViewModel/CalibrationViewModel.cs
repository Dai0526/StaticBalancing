using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticBalancing.ViewModel
{
    class CalibrationViewModel : ViewModelBase
    {
        class DataAtBalancePosition
        {
            public string BalancePosition { get; set; }
            public string InputDataPath { get; set; }
            public Dictionary<string, Coutner> Counters { get; set; }
        };

        class Coutner
        {
            public string PartName { get; set; }
            public double Mass { get; set; }
            public double Thickness { get; set; }
            public int Count { get; set; }
        };


        public CalibrationViewModel() { }



    }
}
