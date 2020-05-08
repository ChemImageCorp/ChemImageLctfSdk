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

        [DependsOn(nameof(SelectedLCTF))]
        public bool CanOperate
        {
            get
            {
                return SelectedLCTF == null ? false : SelectedLCTF.CurrentState == LCTFState.Ready;
            }
        }

        public bool SequenceOrdered { get; set; } = true;
        public bool SequenceArbitrary { get; set; } = false;

        public bool IsSequencing { get; set; } = false;
        
        [DependsOn(nameof(IsSequencing))]
        public bool IsNotSequencing => !IsSequencing;

        public ObservableCollection<ArbitrarySequenceItem> ArbitrarySequenceItems {get;} = new ObservableCollection<ArbitrarySequenceItem>();

        public int OrderedSequenceStart { get; set; } = 0;
        public int OrderedSequenceStop { get; set; } = 0;
        public int OrderedSequenceStep { get; set; } = 1;
        public int OrderedSequenceDwell { get; set; } = 30;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ArbitrarySequenceItems.Add(new ArbitrarySequenceItem() { Wavelength=800, DwellTime=50 });
            DataContext = this;

            LCTFController.OnMcfAttached += Controller_OnMcfAttachedOrDetached;
            LCTFController.OnMcfDetached += Controller_OnMcfAttachedOrDetached;

            this.Controller_OnMcfAttachedOrDetached();
        }

        private void Controller_OnMcfAttachedOrDetached()
        {
            if (SelectedLCTF != null && !LCTFController.AttachedLCTFs.Select(x => x.InstanceId).Contains(SelectedLCTF.LCTFDevice.InstanceId))
            {
                SelectedLCTF = null;
            }

            foreach (LCTFDevice lctf in LCTFController.AttachedLCTFs)
            {
                if (SelectedLCTF == null)
                {
                    SelectedLCTF = new LCTFDeviceModel(lctf);

                    OrderedSequenceStart = WavelengthMin;
                    OrderedSequenceStop = WavelengthMax;
                    OrderedSequenceStep = WavelengthStep;
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

        private async void SequenceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSequencing)
            {
                IsSequencing = true;
            }
            else
            {
                return;
            }

            if (SequenceArbitrary)
            {
                var sequenceItems = ArbitrarySequenceItems.ToList();

                Task sequenceTask = Task.Factory.StartNew(new Action(async () =>
                {
                    foreach (var item in sequenceItems)
                    {
                        if (item.Wavelength < SelectedLCTF.LCTFDevice.WavelengthMin || item.Wavelength > SelectedLCTF.LCTFDevice.WavelengthMax)
                        {
                            MessageBox.Show("Wavelength is outside the min and max of the LCTF.");
                            IsSequencing = false;
                            return;
                        }

                        await SelectedLCTF.LCTFDevice.SetWavelengthAsync(item.Wavelength);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWavelength)));

                        await Task.Delay(item.DwellTime);
                    }
                }));

                await sequenceTask;
            }
            else if (SequenceOrdered)
            {
                var wlStart = OrderedSequenceStart;
                var wlStop = OrderedSequenceStop;
                var wlStep = OrderedSequenceStep;
                var wlDwell = OrderedSequenceDwell;

                if (wlStep == 0)
                {
                    MessageBox.Show("Cannot sequence with zero step size, sequence would never terminate.");
                    IsSequencing = false;
                    return;
                }
                
                if ((wlStart < wlStop && wlStep < 0) || (wlStart > wlStop && wlStep > 0))
                {
                    MessageBox.Show("Start, stop, and step must be selected so that the sequence will terminate.");
                    IsSequencing = false;
                    return;
                }

                Task sequenceTask = Task.Run(async () =>
                {
                    for (var wlCurrent = wlStart; wlStep > 0 ? wlCurrent <= wlStop : wlCurrent >= wlStop; wlCurrent += wlStep)
                    {
                        await SelectedLCTF.LCTFDevice.SetWavelengthAsync(wlCurrent);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWavelength)));

                        await Task.Delay(wlDwell);
                    }
                });

                await sequenceTask;
            }
            else
            {
                MessageBox.Show("Arbitrary or ordered sequencing must be selected.");
                IsSequencing = false;
                return;
            }

            IsSequencing = false;
        }

        private void MaskNumericInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextIsNumeric(e.Text);
        }

        private void MaskNumericPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string input = (string)e.DataObject.GetData(typeof(string));
                if (!TextIsNumeric(input)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool TextIsNumeric(string input)
        {
            return input.All(c => Char.IsDigit(c) || Char.IsControl(c));
        }

#pragma warning disable CS0067 // Used by generated code
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }
}
