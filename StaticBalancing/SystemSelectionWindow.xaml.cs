using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace StaticBalancing
{
    /// <summary>
    /// Interaction logic for SystemSelectionWindow.xaml
    /// </summary>
    public partial class SystemSelectionWindow : Window
    {
        BalancingCore m_refSystemCore;

        public SystemSelectionWindow()
        {
            InitializeComponent();
        }

        public SystemSelectionWindow(ref BalancingCore core)
        {
            InitializeComponent();

            m_refSystemCore = core;
            InitCombobox();
        }

        public void InitCombobox()
        {
            foreach(KeyValuePair<string, SystemInfo> pair in m_refSystemCore.m_systemArchives)
            {
                string id = pair.Key;
                SystemInfo info = pair.Value;

                ComboBoxItem cbItem = new ComboBoxItem();
                cbItem.Name = id;
                cbItem.Content = id;

                SystemSelectionCbx.Items.Add(cbItem);
            }

            if(SystemSelectionCbx.Items.Count > 0)
            {
                SystemSelectionCbx.SelectedIndex = 0;
            }
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {

            if(m_refSystemCore.m_systemSelected != null)
            {
                // new selection found, ask if erase
                MessageBoxResult res = MessageBox.Show("It will erase the previous selection and record. Do you want to Continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // if there is no selection, or there is a selection but erase confirmed, then set selection
            string selectedItem = ((ComboBoxItem)SystemSelectionCbx.SelectedItem).Content.ToString();
            m_refSystemCore.SetCurrentSystem(selectedItem);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void SystemSelectionCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = ((ComboBoxItem)SystemSelectionCbx.SelectedItem).Content.ToString();
            SystemInfo sys = m_refSystemCore.GetSystem(selectedItem);

            CTModelValueLabel.Content = sys.m_model;
            MaxSpeValueLabel.Content = sys.m_maxSpeed;
            MaxImbaValueLabel.Content = sys.m_maxImbalance;
            OffsetValueLabel.Content = sys.m_homeTickOffset;

            List<BalancePosition> bps = sys.m_balancePos;
            List<Counter> ctrs = (sys.m_counters.Values).ToList();
            BalancePositionDataGrid.ItemsSource = bps;
            CounterDataGrid.ItemsSource = ctrs;
        }
    }
}
