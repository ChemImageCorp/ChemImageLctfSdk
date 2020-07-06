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
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace LCTFCommander
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private LCTFDeviceViewModel selectedLCTF = null;
		private CancellationTokenSource sequenceTokenSource;

		public MainWindow()
		{
			this.InitializeComponent();
		}

		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;

		public LCTFDeviceViewModel SelectedLCTF
		{
			get => this.selectedLCTF;
			set
			{
				if (this.selectedLCTF != null)
				{
					this.selectedLCTF.PropertyChanged -= this.SelectedLCTF_PropertyChanged;
				}

				this.selectedLCTF = value;

				if (this.selectedLCTF != null)
				{
					this.selectedLCTF.PropertyChanged += this.SelectedLCTF_PropertyChanged;
				}

				ArbitrarySequenceItem.MinWavelength = Convert.ToInt32(this.SelectedLCTF?.MinWavelength ?? 0);
				ArbitrarySequenceItem.MaxWavelength = Convert.ToInt32(this.SelectedLCTF?.MaxWavelength ?? 0);
				ArbitrarySequenceItem.DefaultWavelength = this.SelectedLCTF?.CurrentWavelength ?? ArbitrarySequenceItem.MinWavelength;
			}
		}

		[DependsOn(nameof(SelectedLCTF))]
		public int CurrentWavelength
		{
			get
			{
				return this.SelectedLCTF == null ? 0 : this.SelectedLCTF.CurrentWavelength;
			}

			set
			{
				ArbitrarySequenceItem.DefaultWavelength = value;

				if (this.SelectedLCTF != null)
				{
					this.SelectedLCTF.CurrentWavelength = value;
				}
			}
		}

		[DependsOn(nameof(SelectedLCTF))]
		public int WavelengthMin
		{
			get
			{
				return this.SelectedLCTF == null ? 0 : this.SelectedLCTF.LCTFDevice.WavelengthMin;
			}
		}

		[DependsOn(nameof(SelectedLCTF))]
		public int WavelengthMax
		{
			get
			{
				return this.SelectedLCTF == null ? 0 : this.SelectedLCTF.LCTFDevice.WavelengthMax;
			}
		}

		[DependsOn(nameof(WavelengthMax), nameof(WavelengthMin))]
		public int WavelengthRange
		{
			get => Math.Abs(this.WavelengthMax - this.WavelengthMin);
		}

		[DependsOn(nameof(SelectedLCTF))]
		public int WavelengthStep
		{
			get
			{
				return this.SelectedLCTF == null ? 0 : this.SelectedLCTF.LCTFDevice.WavelengthStep;
			}
		}

		[DependsOn(nameof(SelectedLCTF))]
		public bool CanOperate
		{
			get
			{
				return this.SelectedLCTF == null ?
					false :
					this.SelectedLCTF.CurrentState == LCTFState.Ready || this.SelectedLCTF.CurrentState == LCTFState.Tuning;
			}
		}

		[DependsOn(nameof(SelectedLCTF))]
		public bool IsCalibrating
		{
			get
			{
				return this.SelectedLCTF == null ? false : this.SelectedLCTF.CurrentState == LCTFState.Calibrating;
			}
		}

		public bool SequenceOrdered { get; set; } = true;

		public bool SequenceArbitrary { get; set; } = false;

		public bool SequenceContinuous { get; set; } = false;

		public bool IsSequencing { get; set; } = false;

		[DependsOn(nameof(IsSequencing))]
		public bool IsNotSequencing => !this.IsSequencing;

		public ObservableCollection<ArbitrarySequenceItem> ArbitrarySequenceItems { get; } = new ObservableCollection<ArbitrarySequenceItem>();

		public int OrderedSequenceStart { get; set; } = 0;

		public int OrderedSequenceStop { get; set; } = 0;

		public int OrderedSequenceStep { get; set; } = 1;

		public int OrderedSequenceDwell { get; set; } = 30;

		private static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int colIndex)
		{
			if (row != null)
			{
				DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);

				if (presenter != null)
				{
					return presenter.ItemContainerGenerator.ContainerFromIndex(colIndex) as DataGridCell;
				}
			}

			return null;
		}

		private static T FindVisualChild<T>(DependencyObject obj)
		where T : DependencyObject
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
					{
						return childOfChild;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Have to listen for property changes on the LCTF to get updates on State.
		/// </summary>
		private void SelectedLCTF_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LCTFDeviceViewModel.CurrentState))
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanOperate)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsCalibrating)));
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.DataContext = this;

			LCTFController.OnLctfAttached += this.Controller_OnLctfAttachedOrDetached;
			LCTFController.OnLctfDetached += this.Controller_OnLctfAttachedOrDetached;

			this.Controller_OnLctfAttachedOrDetached();
		}

		private void Controller_OnLctfAttachedOrDetached()
		{
			if (this.SelectedLCTF != null && !LCTFController.AttachedLCTFs.Select(x => x.InstanceId).Contains(this.SelectedLCTF.LCTFDevice.InstanceId))
			{
				this.SelectedLCTF.Dispose();
				this.SelectedLCTF = null;
			}

			foreach (LCTFDevice lctf in LCTFController.AttachedLCTFs)
			{
				if (this.SelectedLCTF == null)
				{
					this.SelectedLCTF = new LCTFDeviceViewModel(lctf);

					this.OrderedSequenceStart = this.WavelengthMin;
					this.OrderedSequenceStop = this.WavelengthMax;
					this.OrderedSequenceStep = this.WavelengthStep;
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
			this.SelectedLCTF?.Dispose();
		}

		private async void SequenceBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!this.IsSequencing)
			{
				this.IsSequencing = true;
				this.sequenceTokenSource = new CancellationTokenSource();
			}
			else
			{
				this.sequenceTokenSource?.Cancel();
				return;
			}

			var cancellationToken = this.sequenceTokenSource.Token;
			Task sequenceTask = null;

			if (this.SequenceArbitrary)
			{
				var sequenceItems = this.ArbitrarySequenceItems.ToList();

				if (sequenceItems.Count == 0)
				{
					MessageBox.Show("Please enter values for sequencing before starting.", "LCTF Commander");
					this.sequenceTokenSource.Cancel();
					this.IsSequencing = false;
					return;
				}

				sequenceTask = Task.Run(
				async () =>
				{
					do
					{
						foreach (var item in sequenceItems)
						{
							cancellationToken.ThrowIfCancellationRequested();

							if (item.Wavelength < this.SelectedLCTF.LCTFDevice.WavelengthMin || item.Wavelength > this.SelectedLCTF.LCTFDevice.WavelengthMax)
							{
								MessageBox.Show($"Wavelength {item.Wavelength} is outside the min and max of the LCTF.", "LCTF Commander");
								this.IsSequencing = false;
								return;
							}

							await this.SelectedLCTF.LCTFDevice.SetWavelengthAsync(item.Wavelength);
							this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentWavelength)));

							await Task.Delay(Convert.ToInt32(item.DwellTime), cancellationToken);
						}
					}
					while (this.SequenceContinuous && !cancellationToken.IsCancellationRequested);
				}, cancellationToken);
			}
			else if (this.SequenceOrdered)
			{
				var wlStart = this.OrderedSequenceStart;
				var wlStop = this.OrderedSequenceStop;
				var wlStep = this.OrderedSequenceStep;
				var wlDwell = this.OrderedSequenceDwell;

				if (wlStep == 0)
				{
					MessageBox.Show("Cannot sequence with zero step size, sequence would never end.", "LCTF Commander");
					this.IsSequencing = false;
					return;
				}

				if ((wlStart < wlStop && wlStep < 0) || (wlStart > wlStop && wlStep > 0))
				{
					MessageBox.Show("The start, stop, and/or step size must be defined such that the sequence will end.", "LCTF Commander");
					this.IsSequencing = false;
					return;
				}

				sequenceTask = Task.Run(
				async () =>
				{
					do
					{
						for (var wlCurrent = wlStart; wlStep > 0 ? wlCurrent <= wlStop : wlCurrent >= wlStop; wlCurrent += wlStep)
						{
							cancellationToken.ThrowIfCancellationRequested();

							await this.SelectedLCTF.LCTFDevice.SetWavelengthAsync(wlCurrent);
							this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentWavelength)));

							await Task.Delay(wlDwell, cancellationToken);
						}
					}
					while (this.SequenceContinuous && !cancellationToken.IsCancellationRequested);
				}, cancellationToken);
			}

			try
			{
				await sequenceTask;
			}
			catch (OperationCanceledException)
			{
			}
			finally
			{
				this.sequenceTokenSource.Dispose();
			}

			this.IsSequencing = false;
		}

		private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter && e.Key != Key.Tab)
			{
				return;
			}

			DataGrid dataGrid = sender as DataGrid;

			var dataGridRowCount = dataGrid.Items.Count;
			var currentRowIndex = dataGrid.SelectedIndex;

			DataGridRow nextRow;
			int nextCellColumnIndex = 0;

			// If we're not on the last row
			if (currentRowIndex != (dataGridRowCount - 2))
			{
				// We only want to override tab for the last row
				if (e.Key == Key.Tab)
				{
					return;
				}

				nextRow = dataGrid.ItemContainerGenerator.ContainerFromIndex(currentRowIndex + 1) as DataGridRow;
				nextCellColumnIndex = dataGrid.CurrentCell.Column.DisplayIndex;
			}
			else
			{
				// If we're on the last row, but not the last column, we only need to change the column
				if (e.Key == Key.Tab)
				{
					nextCellColumnIndex = dataGrid.CurrentCell.Column.DisplayIndex + 1;

					// Does the next column exist?
					if (nextCellColumnIndex < dataGrid.Columns.Count)
					{
						// select the next cell
						var currentRow = dataGrid.ItemContainerGenerator.ContainerFromIndex(currentRowIndex) as DataGridRow;
						dataGrid.SelectedItem = currentRow.DataContext;
						DataGridCell cell = GetCell(dataGrid, currentRow, nextCellColumnIndex);

						// set the next cell
						if (cell != null)
						{
							dataGrid.CurrentCell = new DataGridCellInfo(cell);
						}

						e.Handled = true;
						return;
					}
					else
					{
						// loop back to the first column
						nextCellColumnIndex = 0;
					}
				}

				// generate a new row
				nextRow = this.ArbitraryDataGrid.ItemContainerGenerator.ContainerFromItem(CollectionView.NewItemPlaceholder) as DataGridRow;
			}

			// Set the next row and cell
			if (nextRow != null)
			{
				dataGrid.SelectedItem = nextRow.DataContext;
				DataGridCell cell = GetCell(dataGrid, nextRow, nextCellColumnIndex);

				if (cell != null)
				{
					dataGrid.CurrentCell = new DataGridCellInfo(cell);
				}
			}

			e.Handled = true;
		}

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

		private void Context_Delete(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem)sender;

			var contextMenu = (ContextMenu)menuItem.Parent;

			var item = (DataGrid)contextMenu.PlacementTarget;

			if (item.SelectedCells[0].Item is ArbitrarySequenceItem selectedSequenceItem)
			{
				this.ArbitrarySequenceItems.Remove(selectedSequenceItem);
			}
		}

		private void Context_InsertAbove(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem)sender;

			var contextMenu = (ContextMenu)menuItem.Parent;

			var item = (DataGrid)contextMenu.PlacementTarget;

			if (item.SelectedCells[0].Item is ArbitrarySequenceItem selectedSequenceItem)
			{
				this.ArbitrarySequenceItems.Insert(this.ArbitrarySequenceItems.IndexOf(selectedSequenceItem), new ArbitrarySequenceItem());
			}
			else
			{
				this.ArbitrarySequenceItems.Add(new ArbitrarySequenceItem());
			}
		}

		private void Context_InsertBelow(object sender, RoutedEventArgs e)
		{
			// Get the clicked MenuItem
			var menuItem = (MenuItem)sender;

			// Get the ContextMenu to which the menuItem belongs
			var contextMenu = (ContextMenu)menuItem.Parent;

			// Find the placementTarget
			var item = (DataGrid)contextMenu.PlacementTarget;

			if (item.SelectedCells[0].Item is ArbitrarySequenceItem selectedSequenceItem)
			{
				// Remove the toDeleteFromBindedList object from your ObservableCollection
				this.ArbitrarySequenceItems.Insert(this.ArbitrarySequenceItems.IndexOf(selectedSequenceItem) + 1, new ArbitrarySequenceItem());
			}
			else
			{
				this.ArbitrarySequenceItems.Add(new ArbitrarySequenceItem());
			}
		}

		private void IntegerUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!(sender is IntegerUpDown integerUpDown))
			{
				return;
			}

			int change = integerUpDown.Increment ?? 0;
			switch (e.Key)
			{
				case Key.PageUp:
					change *= 10;
					break;
				case Key.PageDown:
					change *= -10;
					break;
				default:
					return;
			}

			var newValue = (integerUpDown.Value ?? 0) + change;

			if (integerUpDown.Minimum.HasValue)
			{
				newValue = Math.Max(integerUpDown.Minimum.Value, newValue);
			}

			if (integerUpDown.Maximum.HasValue)
			{
				newValue = Math.Min(integerUpDown.Maximum.Value, newValue);
			}

			integerUpDown.Value = newValue;
		}
	}
}
