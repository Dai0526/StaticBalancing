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
using System.Windows.Shapes;
using StaticBalancing.ViewModel;

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for MeasureWindow.xaml
    /// </summary>
    public partial class MeasureWindow : Window
    {

        CalibrationViewModel m_caliViewModel;
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

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
