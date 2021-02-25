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
    /// Interaction logic for SystemSelectionWindow.xaml
    /// </summary>
    public partial class SystemSelectionWindow : Window
    {
        BalancingCore m_systemCore;

        public SystemSelectionWindow()
        {
            InitializeComponent();
        }

        public SystemSelectionWindow(ref BalancingCore core)
        {
            InitializeComponent();
            m_systemCore = core;
            InitCombobox();
        }

        public void InitCombobox()
        {
            foreach(KeyValuePair<string, SystemInfo> pair in m_systemCore.m_systemArchives)
            {
                string id = pair.Key;
                SystemInfo info = pair.Value;

                ComboBoxItem cbItem = new ComboBoxItem();
                cbItem.Name = id;
                cbItem.Content = id;

                SystemSelectionCbx.Items.Add(cbItem);
            }

            ComboBoxItem newItem = new ComboBoxItem();
            newItem.Name = "NewSystem";
            newItem.Content = "New System";

            SystemSelectionCbx.SelectedIndex = 0;
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = ((ComboBoxItem)SystemSelectionCbx.SelectedItem).Content.ToString();
            m_systemCore.SetCurrentSystem(selectedItem);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
