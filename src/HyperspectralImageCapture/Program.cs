using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemImage.LCTF;

namespace HyperspectralImageCapture
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Connecting to LCTF");
			var lctf = LCTFController.GetFirstLCTF();

			if (lctf == null)
			{
				Console.WriteLine("No LCTF available. Press any key to exit.");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("Starting hyperspectral capture");
			var min = lctf.WavelengthMin;
			var max = lctf.WavelengthMax;
			var step = lctf.WavelengthStep;
			for (var currentWavelength = min; currentWavelength <= max; currentWavelength += step)
			{
				// We're in a non-async function, so we'll use Wait() instead of await
				lctf.SetWavelengthAsync(currentWavelength).Wait();
				Console.WriteLine($"Tuned to {currentWavelength} nm");

				// Insert your image capture code here
			}

			// The lctf object needs to be disposed
			lctf.Dispose();

			Console.WriteLine("Hyperspectral capture completed. Press any key to exit.");
			Console.ReadKey();
			return;
		}
	}
}
