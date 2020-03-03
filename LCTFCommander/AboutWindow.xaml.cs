﻿using System;
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
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window, INotifyPropertyChanged
	{
		public string ProductName { get; set; }
		public string ProductVersion { get; set; }
		public string Copyright { get; set; }
		public string CompanyName { get; set; }
		public string ProductDescription { get; set; }

		public AboutWindow(Window parent, Assembly current)
		{
			InitializeComponent();

			LoadValuesFromAssembly(current);

			DataContext = this;
		}

		private void LoadValuesFromAssembly(Assembly assembly)
		{
			object[] attributes;
			
			attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

			if (attributes.Length == 0)
			{
				ProductName = string.Empty;
			}
			else
			{
				ProductName = ((AssemblyProductAttribute)attributes[0]).Product;
			}

			ProductVersion = assembly.GetName().Version.ToString();

			attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

			if (attributes.Length == 0)
			{
				Copyright = string.Empty;
			}
			else
			{
				Copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}

			attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

			if (attributes.Length == 0)
			{
				CompanyName = string.Empty;
			}
			else
			{
				CompanyName = ((AssemblyCompanyAttribute)attributes[0]).Company;
			}

			attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

			if (attributes.Length == 0)
			{
				ProductDescription = string.Empty;
			}
			else
			{
				ProductDescription = ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

#pragma warning disable CS0067 // Used by generated code
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
