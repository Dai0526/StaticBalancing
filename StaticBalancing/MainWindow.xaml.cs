﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StaticBalancing.ViewModel;
using System.IO;
using System.Windows.Threading;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // constant
        private readonly SolidColorBrush COLOR_ERROR = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush COLOR_DEFAULT = new SolidColorBrush(Colors.Blue);
        private readonly SolidColorBrush COLOR_SUCCESS = new SolidColorBrush(Colors.Green);

        // member
        public BalancingCore m_balancer;
        public SystemInfo m_selectedSystem;
        public Arithmetic m_balanceCalculator;

        // path
        private string m_executablePath;

        // view model
        static MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

        // for time elapsed status bar
        private DispatcherTimer m_dispatcherTimer;

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

            // init status bar
            InitStatusTimer();
            SetStatus("Applicatio Started", COLOR_SUCCESS);
        }

        private void ConfigFilePathTextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BrowseConfigurationFile();
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            BrowseConfigurationFile();
        }

        private void BrowseConfigurationFile()
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
                MessageBox.Show("System Configuraton file doesn't exist: " + mainWindowViewModel.SystemConfigFile + ". Please verify the file location.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                m_balancer = new BalancingCore();
                m_balancer.ReadSystemInfoFiles(mainWindowViewModel.SystemConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse system configuration: " + ex.Data + ". Please check file format. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SystemSelectionWindow ssw = new SystemSelectionWindow(ref m_balancer);
            ssw.ShowDialog();

            DisplaySelectSystem();
        }

        private void DisplaySelectSystem()
        {
            if(m_balancer.m_systemSelected == null)
            {
                MessageBox.Show("Please select a system to Calibrate", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SystemInfo selected = m_balancer.m_systemSelected;

            mainWindowViewModel.SetDisplayedInfo(selected);

        }

        // 
        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_balancer.m_systemSelected == null)
            {
                MessageBox.Show("Please select a system before Calibration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CalibrationWindow cw = new CalibrationWindow(ref m_balancer.m_systemSelected, ref mainWindowViewModel);
            cw.ShowDialog();

            DisplayCalibrationResult();
        }

        private void DisplayCalibrationResult()
        {

        }

        #region Status and Status Bar Control
        // Status Bar Funcs
        private void SetStatus(string status, SolidColorBrush c, int durationSec = 3)
        {
            mainWindowViewModel.StatusLabelContent = status;
            mainWindowViewModel.StatusLabelColor = c;
            m_dispatcherTimer.Interval = new TimeSpan(0, 0, durationSec);
            m_dispatcherTimer.Start();
        }

        private void InitStatusTimer()
        {
            //Create a timer with interval of 2 secs for status display
            m_dispatcherTimer = new DispatcherTimer();
            m_dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            m_dispatcherTimer.Interval = new TimeSpan(0, 0, 3); // 3 seconds as default
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            mainWindowViewModel.StatusLabelContent = "";
            mainWindowViewModel.StatusLabelColor = COLOR_DEFAULT;
            //Disable the timer
            m_dispatcherTimer.IsEnabled = false;
        }

        #endregion



    }
}
