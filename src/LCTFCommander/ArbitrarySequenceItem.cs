using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LCTFCommander
{
	public class ArbitrarySequenceItem : INotifyPropertyChanged
	{
		private int wavelength = MinWavelength;
		private uint dwellTime = MinDwellTime;

		public static int MinWavelength { get; set; }
		public static int MaxWavelength { get; set; }

		public static uint MinDwellTime { get; set; } = 0;
		public static uint MaxDwellTime { get; set; } = 10000;

		public int Wavelength {
			get => wavelength;
			set
			{
				if (value > MaxWavelength)
				{
					wavelength = MaxWavelength;
				}
				else if (value < MinWavelength)
				{
					wavelength = MinWavelength;
				}
				else
				{
					wavelength = value;
				}
			}
		}
		public uint DwellTime
		{
			get => dwellTime;
			set
			{
				if (value > MaxDwellTime)
				{
					dwellTime = MaxDwellTime;
				}
				else if (value < MinDwellTime)
				{
					dwellTime = MinDwellTime;
				}
				else
				{
					dwellTime = value;
				}
			}
		}

#pragma warning disable CS0067 // Used by generated code
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
