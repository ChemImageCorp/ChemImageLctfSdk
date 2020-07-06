using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCTFCommander
{
	/// <summary>
	/// Defines wavelength and dwell time for a single item in a sequence.
	/// </summary>
	public class ArbitrarySequenceItem : INotifyPropertyChanged
	{
		private int wavelength = DefaultWavelength;
		private uint dwellTime = DefaultDwellTime;

#pragma warning disable CS0067 // Used by generated code
		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

		public static int DefaultWavelength { get; set; } = MinWavelength;

		public static uint DefaultDwellTime { get; set; } = 0;

		public static int MinWavelength { get; set; }

		public static int MaxWavelength { get; set; }

		public static uint MinDwellTime { get; set; } = 0;

		public static uint MaxDwellTime { get; set; } = 10000;

		public int Wavelength
		{
			get => this.wavelength;
			set
			{
				if (value > MaxWavelength)
				{
					this.wavelength = MaxWavelength;
				}
				else if (value < MinWavelength)
				{
					this.wavelength = MinWavelength;
				}
				else
				{
					this.wavelength = value;
				}
			}
		}

		public string WavelengthString
		{
			get => this.Wavelength.ToString();
			set
			{
				decimal newValue = this.wavelength;
				try
				{
					newValue = Convert.ToDecimal(value);

					if (newValue >= int.MaxValue)
					{
						this.Wavelength = int.MaxValue;
					}
					else if (newValue <= int.MinValue)
					{
						this.Wavelength = int.MinValue;
					}
					else
					{
						this.Wavelength = (int)newValue;
					}
				}
				catch
				{
				}
			}
		}

		public uint DwellTime
		{
			get => this.dwellTime;
			set
			{
				if (value > MaxDwellTime)
				{
					this.dwellTime = MaxDwellTime;
				}
				else if (value < MinDwellTime)
				{
					this.dwellTime = MinDwellTime;
				}
				else
				{
					this.dwellTime = value;
				}

				// Sets the default to the last used
				DefaultDwellTime = this.DwellTime;
			}
		}

		public string DwellTimeString
		{
			get => this.DwellTime.ToString();
			set
			{
				decimal newValue = this.wavelength;
				try
				{
					newValue = Convert.ToDecimal(value);

					if (newValue >= uint.MaxValue)
					{
						this.DwellTime = uint.MaxValue;
					}
					else if (newValue <= uint.MinValue)
					{
						this.DwellTime = uint.MinValue;
					}
					else
					{
						this.DwellTime = (uint)newValue;
					}
				}
				catch
				{
				}
			}
		}
	}
}
