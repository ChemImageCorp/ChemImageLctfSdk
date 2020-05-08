using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCTFCommander
{
	public class ArbitrarySequenceItem: INotifyPropertyChanged
	{
		public int Wavelength { get; set; } = 0;

		public int DwellTime { get; set; } = 0;

#pragma warning disable CS0067 // Used by generated code
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
