using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ChemImage.LCTF;

namespace LCTFCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<LCTFDeviceModel> AttachedLCTFs { get; } = new ObservableCollection<LCTFDeviceModel>();
        public LCTFDeviceModel SelectedLCTF { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;

            LCTFController.Instance.OnMcfAttached += Controller_OnMcfAttachedOrDetached;
            LCTFController.Instance.OnMcfDetached += Controller_OnMcfAttachedOrDetached;

            this.Controller_OnMcfAttachedOrDetached();
        }

        private void Controller_OnMcfAttachedOrDetached()
        {
            foreach (LCTFDevice lctf in LCTFController.Instance.AttachedLCTFs)
            {
                if (!AttachedLCTFs.Select(x => x.LCTFDevice.InstanceId).Contains(lctf.InstanceId))
                {
                    AttachedLCTFs.Add(new LCTFDeviceModel(lctf));
                }
            }

            List<LCTFDeviceModel> toRemove = new List<LCTFDeviceModel>();

            foreach (LCTFDeviceModel lctf in AttachedLCTFs)
            {
                if (!LCTFController.Instance.AttachedLCTFs.Select(x => x.InstanceId).Contains(lctf.LCTFDevice.InstanceId))
                {
                    toRemove.Add(lctf);
                }
            }

            foreach (var lctf in toRemove)
            {
                AttachedLCTFs.Remove(lctf);
            }

            if ((SelectedLCTF == null || !AttachedLCTFs.Contains(SelectedLCTF)) && AttachedLCTFs.Any())
            {
                SelectedLCTF = AttachedLCTFs.First();
            }
        }

        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var lctf in AttachedLCTFs)
            {
                lctf.Dispose();
            }
        }
    }
}
