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
        private string systemConfigurationFile = "<- please set system configuration file ->";
        public string SystemConfigFile
        {
            get { return systemConfigurationFile; }
            set
            {
                systemConfigurationFile = value;
                OnPropertyChanged(nameof(SystemConfigFile));
            }
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
