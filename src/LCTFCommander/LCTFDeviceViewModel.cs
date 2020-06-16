using ChemImage.LCTF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LCTFCommander
{
	/// <summary>
	/// A viewmodel to wrap LCTFDevice for display
	/// </summary>
	public class LCTFDeviceViewModel : INotifyPropertyChanged, IDisposable
	{
		private DispatcherTimer temperatureTimer;

		public LCTFDevice LCTFDevice { get; private set; }
		public string SerialNumber { get; set; }
		public string FirmwareVersion { get; set; }
		public double Temperature { get; set; } = 0;
		public double MinWavelength { get; set; } = 0;
		public double MaxWavelength { get; set; } = 0;
		public double StepWavelength { get; set; } = 0;

		private bool isSyncing = false;
		private int currentWavelength = 0;
		public int CurrentWavelength
		{
			get
			{
				return currentWavelength;
			} 
			set
			{
				lock (this)
				{
					currentWavelength = value;

					if (!isSyncing)
					{
						isSyncing = true;

						Task.Factory.StartNew(() =>
						{
							SyncWavelength();
						});
					}
					else
					{

					}
				}
				
			}
		}

		public LCTFState CurrentState { get; set; } = LCTFState.None;

		public LCTFDeviceViewModel(LCTFDevice mcfDevice)
		{
			LCTFDevice = mcfDevice;
			LCTFDevice.OnStateChanged += LCTFDevice_OnStateChanged;
			LCTFDevice.OnTuningDone += LCTFDevice_OnTuningDone;
			SerialNumber = LCTFDevice.DeviceInfo.SerialNumber;
			FirmwareVersion = $@"{(float)(LCTFDevice.DeviceInfo.FirmwareVersion) / 100f:0.00}";
			CurrentState = LCTFDevice.GetState();
			MinWavelength = LCTFDevice.WavelengthMin;
			MaxWavelength = LCTFDevice.WavelengthMax;
			StepWavelength = LCTFDevice.WavelengthStep;
			// Don't set the property because we don't want it to try to re-sync unnecessarily.
			currentWavelength = LCTFDevice.GetCurrentWavelength();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWavelength)));

			this.temperatureTimer = new DispatcherTimer();
			this.temperatureTimer.Interval = new TimeSpan(0, 0, 2);
			this.temperatureTimer.Tick += new EventHandler((sender, args) =>
			{
				try
				{
					this.Temperature = this.LCTFDevice.GetTemperature();
				}
				catch
				{ }
			});

			this.temperatureTimer.Start();
		}

		private async void SyncWavelength()
		{
			int lastWavelength = 0;

			while (true)
			{ 
				var setWavelengthTask = this.LCTFDevice.SetWavelengthAsync(currentWavelength);

				lastWavelength = await setWavelengthTask;

				lock(this)
				{
					if (currentWavelength == lastWavelength)
					{
						isSyncing = false;
						return;
					}
					else
					{

					}
				}
			}
		}

		private bool mDisposed = false;

		/// <summary>
		/// Finalizes an instance of the <see cref="LCTFDevice"/> class.
		/// </summary>
		~LCTFDeviceViewModel()
		{
			this.Dispose(false);
		}

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

		private void LCTFDevice_OnTuningDone(int lambda)
		{
			currentWavelength = lambda;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWavelength)));
		}

		private void LCTFDevice_OnStateChanged(LCTFState status, int tunedWavelength)
		{
			CurrentState = status;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
