using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using ChemImage.LCTF;

namespace LCTFCommander
{
	/// <summary>
	/// A viewmodel to wrap LCTFDevice for display. Handles disposing the LCTFDevice as well.
	/// </summary>
	public class LCTFDeviceViewModel : INotifyPropertyChanged, IDisposable
	{
		private bool mDisposed = false;

		private bool isSyncing = false;

		private int currentWavelength = 0;

		private DispatcherTimer temperatureTimer;

		public LCTFDeviceViewModel(LCTFDevice lctfDevice)
		{
			this.LCTFDevice = lctfDevice;
			this.LCTFDevice.OnStateChanged += this.LCTFDevice_OnStateChanged;
			this.LCTFDevice.OnTuningDone += this.LCTFDevice_OnTuningDone;
			this.SerialNumber = this.LCTFDevice.DeviceInfo.SerialNumber;
			this.FirmwareVersion = $@"{(float)this.LCTFDevice.DeviceInfo.FirmwareVersion / 100f:0.00}";
			this.CurrentState = this.LCTFDevice.GetState();
			this.MinWavelength = this.LCTFDevice.WavelengthMin;
			this.MaxWavelength = this.LCTFDevice.WavelengthMax;
			this.StepWavelength = this.LCTFDevice.WavelengthStep;

			// Don't set the property because we don't want it to try to re-sync unnecessarily.
			this.currentWavelength = this.LCTFDevice.GetCurrentWavelength();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentWavelength)));

			this.temperatureTimer = new DispatcherTimer();
			this.temperatureTimer.Interval = new TimeSpan(0, 0, 2);
			this.temperatureTimer.Tick += new EventHandler((sender, args) =>
			{
				try
				{
					this.Temperature = this.LCTFDevice.GetTemperature();
				}
				catch
				{
				}
			});

			this.temperatureTimer.Start();
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="LCTFDeviceViewModel"/> class.
		/// </summary>
		~LCTFDeviceViewModel()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;

		public LCTFDevice LCTFDevice { get; private set; }

		public string SerialNumber { get; set; }

		public string FirmwareVersion { get; set; }

		public double Temperature { get; set; } = 0;

		public double MinWavelength { get; set; } = 0;

		public double MaxWavelength { get; set; } = 0;

		public double StepWavelength { get; set; } = 0;

		/// <summary>
		/// Gets or sets the wavelength the LCTF is currently tuned to.
		/// </summary>
		public int CurrentWavelength
		{
			get
			{
				return this.currentWavelength;
			}

			set
			{
				lock (this)
				{
					this.currentWavelength = value;

					// Start syncing the wavelength if not already doing so
					if (!this.isSyncing)
					{
						this.isSyncing = true;

						Task.Factory.StartNew(() =>
						{
							this.SyncWavelength();
						});
					}
				}
			}
		}

		public LCTFState CurrentState { get; set; } = LCTFState.None;

		/// <summary>
		/// Cleans up USB connections and disposes the object.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes the <see cref="LCTFDeviceViewModel"/>.
		/// </summary>
		/// <param name="disposeManagedResources">Indicates if managed resources should be disposed in addition to unmanaged.</param>
		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!this.mDisposed)
			{
				if (disposeManagedResources)
				{
					// Release managed resources
					this.temperatureTimer.Stop();
					this.LCTFDevice.Dispose();
				}

				// Release unmanaged resources
				this.mDisposed = true;
			}
		}

		/// <summary>
		/// Used to tune the LCTF to a desired wavelength. The desired wavelength can be changed while tuning, which then will cause another tune to the new desired wavelength.
		/// </summary>
		private async void SyncWavelength()
		{
			// The last wavelength that the LCTF tuned to
			int lastWavelength = 0;

			while (true)
			{
				// Start tuning to the current desired wavelength and await
				lastWavelength = await this.LCTFDevice.SetWavelengthAsync(this.currentWavelength);

				// Lock to prevent CurrentWavelength setter from setting currentWavelength after we think we're done here
				lock (this)
				{
					// If the desired (currentWavelength) is the same as what we just tuned to, we're done syncing.
					if (this.currentWavelength == lastWavelength)
					{
						// done syncing, and return to end the task
						this.isSyncing = false;
						return;
					}
				}
			}
		}

		/// <summary>
		/// Handler for the TuningDone event.
		/// </summary>
		/// <param name="lambda">The wavelength that was just tuned to.</param>
		private void LCTFDevice_OnTuningDone(int lambda)
		{
			lock (this)
			{
				// Sync also handles currentWavelength, so we don't want to update here too.
				if (!this.isSyncing)
				{
					this.currentWavelength = lambda;
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentWavelength)));
				}
			}
		}

		private void LCTFDevice_OnStateChanged(LCTFState status, int tunedWavelength)
		{
			this.CurrentState = status;
		}
	}
}
