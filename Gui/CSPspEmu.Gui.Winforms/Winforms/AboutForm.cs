using System;
using System.Windows.Forms;
using System.Diagnostics;
using CSPspEmu.Core;
using CSPspEmu.Resources;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using CSharpUtils.Extensions;

namespace CSPspEmu.Gui.Winforms
{
    public partial class AboutForm : Form
    {
        static readonly Programmer[] Programmers =
        {
            new Programmer() {Name = "soywiz", Url = "http://cballesterosvelasco.es/"},
            new Programmer() {Name = "archanox", Url = "https://github.com/archanox"},
            new Programmer() {Name = "Lioncash", Url = "https://github.com/lioncash"},
        };

        static readonly Library[] UsedLibraries =
        {
            new Library()
            {
                Name = "MaiAT3PlusDecoder",
                Author = "ryuukazenomai",
                Url = "http://sourceforge.net/projects/maiat3plusdec/"
            },
            new Library() {Name = "HQ2X", Author = "Ivan Neeson", Url = "https://code.google.com/p/hq2x-csharp/"},
            new Library()
            {
                Name = "xBR",
                Author = "Hyllian/Hawkynt",
                Url = "http://board.byuu.org/viewtopic.php?f=10&t=2248"
            },
            new Library() {Name = "SMAA", Author = "Several authors", Url = "http://www.iryoku.com/smaa/"},
        };

        public AboutForm(Form ParentForm, IGuiExternalInterface IGuiExternalInterface)
        {
            this.Icon = ParentForm.Icon;
            InitializeComponent();
            GpuPluginInfoLabel.Text = "GPU " + IGuiExternalInterface.GetGpuPluginInfo().ToString();
            AudioPluginInfoLabel.Text = "Audio " + IGuiExternalInterface.GetAudioPluginInfo().ToString();
            versionLabel.Text = "Version: " + PspGlobalConfiguration.CurrentVersion + " : r" +
                                PspGlobalConfiguration.CurrentVersionNumeric;
            GitRevisionValueLinkLabel.Text = PspGlobalConfiguration.GitRevision;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=J9DXYUSNPH5SC");
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            AddTitle(CreditsListPanel, "Programmers");
            AddLinkList(CreditsListPanel, 1, Programmers);
            AddTitle(CreditsListPanel, "Translators");
            AddLinkList(CreditsListPanel, 2, Translations.AvailableLanguages.Select((Language) => new Translator()
            {
                Language = Language,
                Name = Translations.GetString("info", "Credits", Language),
                Url = Translations.GetString("info", "CreditsUrls", Language),
            }).Where(Translator => Translator.Name != "soywiz"));

            AddTitle(CreditsListPanel, "Libraries");
            AddLinkList(CreditsListPanel, 1, UsedLibraries);
            AddTitle(CreditsListPanel, "Special Thanks");
            AddLinkList(CreditsListPanel, 2, new[] {new Person() {Name = "Noxa"}, new Person() {Name = "MaXiMu"}});
        }

        private void AddTitle(FlowLayoutPanel Panel, string Title)
        {
            var Label = new Label();
            Label.Font = new System.Drawing.Font("Arial", 10, FontStyle.Bold | FontStyle.Underline);
            Label.Padding = new Padding(0);
            Label.Margin = new Padding(0, 6, 0, 6);
            Label.Width = Panel.Width;
            Label.Text = Title;
            Panel.Controls.Add(Label);
        }

        private void AddLinkList(FlowLayoutPanel Panel, int Columns, IEnumerable<BaseLinkable> List)
        {
            Panel.SuspendLayout();
            //Panel.Controls.Clear();
            Panel.Margin = new Padding(0);
            Panel.Padding = new Padding(0);
            List.ForEach((Index, Item) =>
            {
                var Label = new LinkLabel();
                Label.Padding = new Padding(0);
                Label.Margin = new Padding(0);
                Label.Width = Panel.Width / Columns;
                Label.Text = Item.ToString();
                Label.Click += (_sender, _e) => { Process.Start(Item.Url); };
                Panel.Controls.Add(Label);
            });
            Panel.ResumeLayout();
        }

        public class BaseLinkable
        {
            public string Url;
        }

        public class Person : BaseLinkable
        {
            public string Name;

            public override string ToString()
            {
                return string.Format("{0}", Name);
            }
        }

        public class Translator : Person
        {
            public string Language;

            public override string ToString()
            {
                return string.Format("{0} : {1}", Language, Name);
            }
        }

        public class Programmer : Person
        {
        }

        public class Library : BaseLinkable
        {
            public string Name;
            public string Author;

            public override string ToString()
            {
                if (string.IsNullOrEmpty(Author))
                {
                    return string.Format("{0}", Name);
                }
                else
                {
                    return string.Format("{0}: {1}", Name, Author);
                }
            }
        }

        private void TwitterPictureBox_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://twitter.com/dpspemu");
        }

        private void FacebookPictureBox_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://www.facebook.com/pspemu");
        }

        private void GitRevisionValueLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/soywiz/cspspemu/commit/" + PspGlobalConfiguration.GitRevision);
        }
    }
}