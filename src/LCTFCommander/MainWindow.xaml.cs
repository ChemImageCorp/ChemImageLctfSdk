﻿using System;
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
using System.Windows.Controls.Primitives;
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
		private LCTFDeviceViewModel selectedLCTF = null;
		public LCTFDeviceViewModel SelectedLCTF
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
			if (e.PropertyName == nameof(LCTFDeviceViewModel.CurrentState))
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
					SelectedLCTF = new LCTFDeviceViewModel(lctf);

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

				if (sequenceItems.Count == 0)
				{
					MessageBox.Show("Please enter values for sequencing before trying to start.");
					sequenceTokenSource.Cancel();
					IsSequencing = false;
					return;
				}
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
					MessageBox.Show("Cannot sequence with zero step size, sequence would never end.", "Sequence Definition Error");
					IsSequencing = false;
					return;
				}

				if ((wlStart < wlStop && wlStep < 0) || (wlStart > wlStop && wlStep > 0))
				{
					MessageBox.Show("The sstart, stop, and/or step must be defined such that the sequence will end.", "Sequence Definition Error");
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

		private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			DataGrid dataGrid = sender as DataGrid;

			var dataGridRowCount = dataGrid.Items.Count;
			var currentRowIndex = dataGrid.SelectedIndex;

			DataGridRow nextRow;
			int nextCellColumnIndex = 0;

			// If we're not on the last row
			if (currentRowIndex != (dataGridRowCount - 2))
			{
				nextRow = dataGrid.ItemContainerGenerator.ContainerFromIndex(currentRowIndex + 1) as DataGridRow;
				nextCellColumnIndex = dataGrid.CurrentCell.Column.DisplayIndex;
			}
			else
			{
				nextRow = ArbitraryDataGrid.ItemContainerGenerator.ContainerFromItem(CollectionView.NewItemPlaceholder) as DataGridRow;
			}

			if (nextRow != null)
			{
				dataGrid.SelectedItem = nextRow.DataContext;
				DataGridCell cell = GetCell(dataGrid, nextRow, nextCellColumnIndex);

				if (cell != null)
					dataGrid.CurrentCell = new DataGridCellInfo(cell);
			}

			e.Handled = true;
		}

		private static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int colIndex)
		{
			if (row != null)
			{
				DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);

				if (presenter != null)
					return presenter.ItemContainerGenerator.ContainerFromIndex(colIndex) as DataGridCell;
			}
			return null;
		}

		private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is T t)
				{
					return t;
				}
				else
				{
					T childOfChild = FindVisualChild<T>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

#pragma warning disable CS0067 // Used by generated code
		public event PropertyChangedEventHandler PropertyChanged;

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (!e.Key.Equals(Key.Enter))
			{
				return;
			}

			UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

			if (elementWithFocus != null)
			{
				elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			}
		}

		private void Context_InsertAbove(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem)sender;

			var contextMenu = (ContextMenu)menuItem.Parent;

			var item = (DataGrid)contextMenu.PlacementTarget;

			if (item.SelectedCells[0].Item is ArbitrarySequenceItem selectedSequenceItem)
			{
				//Remove the toDeleteFromBindedList object from your ObservableCollection
				ArbitrarySequenceItems.Insert(ArbitrarySequenceItems.IndexOf(selectedSequenceItem), new ArbitrarySequenceItem());
			}
			else
			{
				ArbitrarySequenceItems.Add(new ArbitrarySequenceItem());
			}
		}

		private void Context_InsertBelow(object sender, RoutedEventArgs e)
		{
			//Get the clicked MenuItem
			var menuItem = (MenuItem)sender;

			//Get the ContextMenu to which the menuItem belongs
			var contextMenu = (ContextMenu)menuItem.Parent;

			//Find the placementTarget
			var item = (DataGrid)contextMenu.PlacementTarget;

			if (item.SelectedCells[0].Item is ArbitrarySequenceItem selectedSequenceItem)
			{
				//Remove the toDeleteFromBindedList object from your ObservableCollection
				ArbitrarySequenceItems.Insert(ArbitrarySequenceItems.IndexOf(selectedSequenceItem) + 1, new ArbitrarySequenceItem());
			}
			else
			{
				ArbitrarySequenceItems.Add(new ArbitrarySequenceItem());
			}
		}
#pragma warning restore CS0067
	}
}
