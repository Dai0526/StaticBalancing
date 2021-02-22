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
using StaticBalancing.ViewModel;
using System.IO;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BalancingCore m_balancer;
        public SystemInfo m_selectedSystem;
        public Arithmetic m_balanceCalculator;

        public Dictionary<string, Grid> m_BalancePositions;
        public Dictionary<string, Grid> m_CalibrationsDict;

        private string m_executablePath;

        static MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

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

            
        }

        // TODO: Windows File Explore browser to choose system configuration
        private void ConfigFilePathTextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                MessageBox.Show("Error", "System Configuraton file doesn't exist: " + mainWindowViewModel.SystemConfigFile + ". Please verify the file location.", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                m_balancer = new BalancingCore();
                m_balancer.ReadSystemInfoFiles(mainWindowViewModel.SystemConfigFile);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Error", "Failed to parse system configuration: " + ex.Data + ". Please check file format. ", MessageBoxButton.OK, MessageBoxImage.Error);
            }




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
