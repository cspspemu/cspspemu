using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CSPspEmu.Core;
using System.Reflection;
using System.Collections.Generic;

namespace CSPspEmu.Gui.Winforms
{
    public partial class ButtonMappingForm : Form, IInjectInitialize
    {
        [Inject] PspStoredConfig StoredConfig;

        ControllerConfig CurrentControllerConfig;

        private ButtonMappingForm()
        {
        }

        private static readonly Dictionary<string, FieldInfo> _CacheControllerConfigField =
            new Dictionary<string, FieldInfo>();

        static FieldInfo GetCachedControllerConfigField(string Name)
        {
            if (!_CacheControllerConfigField.ContainsKey(Name))
            {
                _CacheControllerConfigField[Name] = typeof(ControllerConfig).GetField(Name);
            }
            return _CacheControllerConfigField[Name];
        }

        void IInjectInitialize.Initialize()
        {
            InitializeComponent();

            LoadConfig();

            foreach (var Field in typeof(ButtonMappingForm).GetFields()
                .Where(Item => Item.FieldType == typeof(TextBox)))
            {
                var TextBox = (Field.GetValue(this) as TextBox);
                TextBox.KeyDown += this.HandleKeyDown;
                TextBox.GotFocus += HandleGotFocus;
                TextBox.LostFocus += HandleLostFocus;
                var ConfigField = GetCachedControllerConfigField(TextBox.Name);
                if (ConfigField != null) TextBox.Text = (string) ConfigField.GetValue(CurrentControllerConfig);
            }

            this.AcceptButton = button1;
            this.CancelButton = button2;

            this.Load += HandleLoad;
        }

        private void HandleLoad(object sender, EventArgs e)
        {
            (this.AcceptButton as Button).Focus();
        }

        private static void HandleLostFocus(object sender, EventArgs e)
        {
            var TextBox = (sender as TextBox);
            TextBox.BackColor = Color.White;
        }

        private static void HandleGotFocus(object sender, EventArgs e)
        {
            var TextBox = (sender as TextBox);
            TextBox.BackColor = Color.Yellow;
        }

        public void LoadConfig()
        {
            this.CurrentControllerConfig = StoredConfig.ControllerConfig;
        }

        public void StoreConfig()
        {
            StoredConfig.ControllerConfig = this.CurrentControllerConfig;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            var TextBox = (sender as TextBox);
            var Key = e.KeyCode;
            if ((Key & Keys.KeyCode) != 0)
            {
                if (Key == Keys.ShiftKey) return;
                if (Key == Keys.ControlKey) return;
                if (Key == Keys.Alt) return;

                TextBox.Text = Key.ToString();
                var ConfigField = GetCachedControllerConfigField(TextBox.Name);
                if (ConfigField != null) ConfigField.SetValue(CurrentControllerConfig, TextBox.Text);
                e.SuppressKeyPress = true;
                (this.AcceptButton as Button).Focus();
            }
            //Focus();
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