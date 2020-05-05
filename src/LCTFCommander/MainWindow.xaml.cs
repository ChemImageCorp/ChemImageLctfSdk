using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using PropertyChanged;

namespace LCTFCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public LCTFDeviceModel SelectedLCTF { get; set; } = null;

        [DependsOn(nameof(SelectedLCTF))]
        public int CurrentWavelength
        {
            get
            {
                return SelectedLCTF == null ? 0 : SelectedLCTF.CurrentWavelength;
            }
            set
            {
                if (SelectedLCTF != null)
                {
                    SelectedLCTF.CurrentWavelength = value;
                }
            }
        }

        [DependsOn(nameof(SelectedLCTF))]
        public int WavelengthMin
        {
            get
            {
                return SelectedLCTF == null ? 0 : SelectedLCTF.LCTFDevice.WavelengthMin;
            }
        }

        [DependsOn(nameof(SelectedLCTF))]
        public int WavelengthMax
        {
            get
            {
                return SelectedLCTF == null ? 0 : SelectedLCTF.LCTFDevice.WavelengthMax;
            }
        }

        [DependsOn(nameof(SelectedLCTF))]
        public int WavelengthStep
        {
            get
            {
                return SelectedLCTF == null ? 0 : SelectedLCTF.LCTFDevice.WavelengthStep;
            }
        }

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
            if (SelectedLCTF != null && !LCTFController.Instance.AttachedLCTFs.Select(x => x.InstanceId).Contains(SelectedLCTF.LCTFDevice.InstanceId))
            {
                SelectedLCTF = null;
            }

            foreach (LCTFDevice lctf in LCTFController.Instance.AttachedLCTFs)
            {
                if (SelectedLCTF == null)
                {
                    SelectedLCTF = new LCTFDeviceModel(lctf);
                }
            }
        }

        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var aboutWindow = new AboutWindow(this, assembly);
            aboutWindow.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SelectedLCTF?.Dispose();
        }

#pragma warning disable CS0067 // Used by generated code
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }
}
