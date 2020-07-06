using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LCTFCommander
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml.
	/// </summary>
	public partial class AboutWindow : Window, INotifyPropertyChanged
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AboutWindow"/> class.
		/// </summary>
		/// <param name="parent">The parent window.</param>
		/// <param name="current">The assembly to use for populating the window.</param>
		public AboutWindow(Window parent, Assembly current)
		{
			this.InitializeComponent();

			this.LoadValuesFromAssembly(current);

			this.DataContext = this;
		}

#pragma warning disable CS0067 // Used by generated code
		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

		public string ProductName { get; set; }

		public string ProductVersion { get; set; }

		public string Copyright { get; set; }

		public string CompanyName { get; set; }

		public string ProductDescription { get; set; }

		private void LoadValuesFromAssembly(Assembly assembly)
		{
			object[] attributes;

			attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

			if (attributes.Length == 0)
			{
				this.ProductName = string.Empty;
			}
			else
			{
				this.ProductName = ((AssemblyProductAttribute)attributes[0]).Product;
			}

			this.ProductVersion = assembly.GetName().Version.ToString();

			attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

			if (attributes.Length == 0)
			{
				this.Copyright = string.Empty;
			}
			else
			{
				this.Copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}

			attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

			if (attributes.Length == 0)
			{
				this.CompanyName = string.Empty;
			}
			else
			{
				this.CompanyName = ((AssemblyCompanyAttribute)attributes[0]).Company;
			}

			attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

			if (attributes.Length == 0)
			{
				this.ProductDescription = string.Empty;
			}
			else
			{
				this.ProductDescription = ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}
	}
}
