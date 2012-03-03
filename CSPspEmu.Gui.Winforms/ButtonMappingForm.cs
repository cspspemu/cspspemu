using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSPspEmu.Core;
using CSharpUtils.Extensions;
using System.Windows.Input;
using System.Globalization;

namespace CSPspEmu.Gui.Winforms
{
	public partial class ButtonMappingForm : Form
	{
		PspConfig PspConfig;
		ControllerConfig CurrentControllerConfig;

		public ButtonMappingForm(PspConfig PspConfig)
		{
			InitializeComponent();

			this.PspConfig = PspConfig;

			LoadConfig();

			foreach (var Field in typeof(ButtonMappingForm).GetFields().Where(Item => Item.FieldType == typeof(TextBox)))
			{
				var TextBox = (Field.GetValue(this) as TextBox);
				TextBox.KeyDown += this.HandleKeyDown;
				var ConfigField = typeof(ControllerConfig).GetField(TextBox.Name);
				if (ConfigField != null) TextBox.Text = (String)ConfigField.GetValue(CurrentControllerConfig);
			}
		}

		public void LoadConfig()
		{
			this.CurrentControllerConfig = PspConfig.StoredConfig.ControllerConfig;
		}

		public void StoreConfig()
		{
			PspConfig.StoredConfig.ControllerConfig = this.CurrentControllerConfig;
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			var TextBox = (sender as TextBox);
			var Key = e.KeyCode;
			TextBox.Text = Key.ToString();
			var ConfigField = typeof(ControllerConfig).GetField(TextBox.Name);
			if (ConfigField != null) ConfigField.SetValue(CurrentControllerConfig, TextBox.Text);
			e.SuppressKeyPress = true;
			//KeyInterop.KeyFromVirtualKey
		}

		private void ButtonMappingForm_Load(object sender, EventArgs e)
		{

		}

		private void button2_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			StoreConfig();
			this.Close();
		}
	}
}
