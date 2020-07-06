using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LCTFCommander.Converters
{
	public class IntToDoubleMultiplyingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double parameterValue = 1.0;

			if (value != null && targetType == typeof(double) && double.TryParse((string)parameter, out parameterValue))
			{
				return (int)value * parameterValue;
			}
			else
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double && parameter is string)
			{
				return (int)((double)value / System.Convert.ToDouble(parameter as string));
			}
			else
			{
				return null;
			}
		}
	}
}
