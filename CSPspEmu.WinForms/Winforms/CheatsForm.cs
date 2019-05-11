using CSPspEmu.cheats;
using System;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms.Winforms
{
    public partial class CheatsForm : Form, IInjectInitialize
    {
        public CheatsForm()
        {
            InitializeComponent();
        }

        [Inject] CWCheatPlugin CWCheatPlugin;

        void IInjectInitialize.Initialize()
        {
            CheatsTextBox.Text = CWCheatPlugin.Cheats;
        }

        //private CWCheatPlugin CWCheatPlugin
        //{
        //	get
        //	{
        //		return PspDisplayForm.Singleton.IGuiExternalInterface.InjectContext.GetInstance<CWCheatPlugin>();
        //	}
        //}

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            CWCheatPlugin.Cheats = CheatsTextBox.Text;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CheatsTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void CheatsForm_Load(object sender, EventArgs e)
        {
        }
    }
}