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
	public class LCTFDeviceModel : INotifyPropertyChanged, IDisposable
	{
		private DispatcherTimer temperatureTimer;

		public LCTFDevice LCTFDevice { get; private set; }
		public string SerialNumber { get; set; }
		public string FirmwareVersion { get; set; }
		public double Temperature { get; set; } = 0;
		public int CurrentLambda { get; set; } = 0;

		public LCTFState CurrentState { get; set; } = LCTFState.None;

		public LCTFDeviceModel(LCTFDevice mcfDevice)
		{
			LCTFDevice = mcfDevice;
			LCTFDevice.OnStateChanged += LCTFDevice_OnStateChanged;
			LCTFDevice.OnTuningDone += LCTFDevice_OnTuningDone;
			SerialNumber = LCTFDevice.DeviceInfo.SerialNumber;
			FirmwareVersion = $@"{(float)(LCTFDevice.DeviceInfo.FirmwareVersion) / 100f:0.00}";
			CurrentState = LCTFDevice.GetState();
			CurrentLambda = LCTFDevice.GetCurrentWavelength();

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

		private bool mDisposed = false;

		/// <summary>
		/// Finalizes an instance of the <see cref="LCTFDevice"/> class.
		/// </summary>
		~LCTFDeviceModel()
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
		/// Disposes the <see cref="LCTFDeviceModel"/>.
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
			CurrentLambda = lambda;
		}

		private void LCTFDevice_OnStateChanged(LCTFState status, int tunedWavelength)
		{
			CurrentState = status;
		}

#pragma warning disable CS0067 // Used by generated code
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
