using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SystemInfo m_selectedSystem;
        public Arithmetic m_balanceCalculator;

        public Dictionary<string, Grid> m_BalancePositions;
        public Dictionary<string, Grid> m_CalibrationsDict;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConfigFilePathTextbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // TODO: Windows File Explore browser to choose system configuration
        }

        private void SystemSelectButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: pop up a windows, display info from the configuraiton file, and let user to select target system and enter serial numbers
        }



        // create GUI item dynamically
        private Grid CreateBalancePositionLabel(string label, double radius, double degree)
        {
            return new Grid();
        }


        private Grid CreateCalibrationField()
        {
            return new Grid();
        }
    }
}
