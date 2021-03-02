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

namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        SystemInfo m_system;

        public CalibrationWindow()
        {
            InitializeComponent();
        }

        public CalibrationWindow(ref SystemInfo selected)
        {
            m_system = selected;



            InitializeComponent();
        }

        private void CreateGrid()
        {
            DataGrid dg = new DataGrid();
            dg.Name = "CalibrationInputDataGrid";

            foreach(BalancePosition bp in m_system.m_balancePos)
            {
                DataGridColumn dgc = new DataGridColumn();
                dgc.Header = "Balance Position";
            }



        }

        private void SetDisplayInfo()
        {

        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        // need n + 1 path, and n's counter info
    }
}
