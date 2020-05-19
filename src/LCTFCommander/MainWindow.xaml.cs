using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
		private LCTFDeviceModel selectedLCTF = null;
		public LCTFDeviceModel SelectedLCTF
		{
			get => selectedLCTF;
			set
			{
				if(selectedLCTF != null)
					selectedLCTF.PropertyChanged -= SelectedLCTF_PropertyChanged;

				selectedLCTF = value;

				if (selectedLCTF != null)
					selectedLCTF.PropertyChanged += SelectedLCTF_PropertyChanged;

				ArbitrarySequenceItem.MinWavelength = Convert.ToInt32(SelectedLCTF?.MinWavelength ?? 0);
				ArbitrarySequenceItem.MaxWavelength = Convert.ToInt32(SelectedLCTF?.MaxWavelength ?? 0);
			}
		}

		private void SelectedLCTF_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LCTFDeviceModel.CurrentState))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanOperate)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCalibrating)));
			}
		}

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

		[DependsOn(nameof(WavelengthMax), nameof(WavelengthMin))]
		public int WavelengthRange
		{
			get => Math.Abs(WavelengthMax - WavelengthMin);
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
				return SelectedLCTF == null ? false : SelectedLCTF.CurrentState == LCTFState.Ready || SelectedLCTF.CurrentState == LCTFState.Tuning;
			}
		}

		[DependsOn(nameof(SelectedLCTF))]
		public bool IsCalibrating
		{
			get
			{
				return SelectedLCTF == null ? false : SelectedLCTF.CurrentState == LCTFState.Calibrating;
			}
		}

		public bool SequenceOrdered { get; set; } = true;
		public bool SequenceArbitrary { get; set; } = false;

		public bool SequenceContinuous { get; set; } = false;

		public bool IsSequencing { get; set; } = false;

		[DependsOn(nameof(IsSequencing))]
		public bool IsNotSequencing => !IsSequencing;

		private CancellationTokenSource sequenceTokenSource;
		
		public ObservableCollection<ArbitrarySequenceItem> ArbitrarySequenceItems { get; } = new ObservableCollection<ArbitrarySequenceItem>();

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
			DataContext = this;

			LCTFController.OnMcfAttached += Controller_OnMcfAttachedOrDetached;
			LCTFController.OnMcfDetached += Controller_OnMcfAttachedOrDetached;

			this.Controller_OnMcfAttachedOrDetached();
		}

		private void Controller_OnMcfAttachedOrDetached()
		{
			if (SelectedLCTF != null && !LCTFController.AttachedLCTFs.Select(x => x.InstanceId).Contains(SelectedLCTF.LCTFDevice.InstanceId))
			{
				SelectedLCTF.Dispose();
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
				sequenceTokenSource = new CancellationTokenSource();
			}
			else
			{
				sequenceTokenSource?.Cancel();
				return;
			}

			var cancellationToken = sequenceTokenSource.Token;
			Task sequenceTask = null;

			if (SequenceArbitrary)
			{
				var sequenceItems = ArbitrarySequenceItems.ToList();

				sequenceTask = Task.Run(async () =>
				{
					do
					{
						foreach (var item in sequenceItems)
						{
							cancellationToken.ThrowIfCancellationRequested();

							if (item.Wavelength < SelectedLCTF.LCTFDevice.WavelengthMin || item.Wavelength > SelectedLCTF.LCTFDevice.WavelengthMax)
							{
								MessageBox.Show($"Wavelength {item.Wavelength} is outside the min and max of the LCTF.");
								IsSequencing = false;
								return;
							}

							await SelectedLCTF.LCTFDevice.SetWavelengthAsync(item.Wavelength);
							PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWavelength)));

							await Task.Delay(Convert.ToInt32(item.DwellTime), cancellationToken);
						}
					}
					while (SequenceContinuous && !cancellationToken.IsCancellationRequested);
				}, cancellationToken);
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

				sequenceTask = Task.Run(async () =>
				{
					do
					{
						for (var wlCurrent = wlStart; wlStep > 0 ? wlCurrent <= wlStop : wlCurrent >= wlStop; wlCurrent += wlStep)
						{
							cancellationToken.ThrowIfCancellationRequested();

							await SelectedLCTF.LCTFDevice.SetWavelengthAsync(wlCurrent);
							PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWavelength)));

							await Task.Delay(wlDwell, cancellationToken);
						}
					} 
					while (SequenceContinuous && !cancellationToken.IsCancellationRequested);
				}, cancellationToken);
			}

			try
			{
				await sequenceTask;
			}
			catch (OperationCanceledException)
			{
				;
			}
			finally
			{
				sequenceTokenSource.Dispose();
			}

			IsSequencing = false;
		}

#pragma warning disable CS0067 // Used by generated code
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
