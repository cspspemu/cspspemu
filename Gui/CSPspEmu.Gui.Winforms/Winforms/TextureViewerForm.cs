using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Types;
using CSPspEmu.Gui.texture;
using HQ2x;
using Imager;
using Imager.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CSharpUtils.Drawing;
using CSharpUtils.Drawing.Extensions;

namespace CSPspEmu.Gui.Winforms.Winforms
{
    public partial class TextureViewerForm : Form
    {
        public TextureViewerForm()
        {
            InitializeComponent();
        }

        public class TextureElement
        {
            public TextureOpengl TextureOpengl;

            public TextureElement(TextureOpengl TextureOpengl)
            {
                this.TextureOpengl = TextureOpengl;
            }

            public override string ToString()
            {
                return string.Format("{0:X16}", this.TextureOpengl.TextureHash);
            }
        }

        private void UpdateTextureList()
        {
            var OpenglGpuImpl =
                (OpenglGpuImpl) PspDisplayForm.Singleton.IGuiExternalInterface.InjectContext.GetInstance<GpuImpl>();
            TextureList.SuspendLayout();
            try
            {
                TextureList.Items.Clear();
                foreach (var Element in OpenglGpuImpl.TextureCache.Cache.Values)
                {
                    TextureList.Items.Add(new TextureElement(Element));
                }
            }
            finally
            {
                TextureList.ResumeLayout();
            }
        }

        private void TextureViewerForm_Load(object sender, EventArgs e)
        {
            UpdateTextureList();
            if (TextureList.Items.Count > 0)
            {
                TextureList.SelectedIndex = 0;
            }
        }

        private void TextureView_Paint(object sender, PaintEventArgs e)
        {
        }

        private void UpdateTexture()
        {
            var Item = (TextureElement) TextureList.SelectedItem;
            var TextureOpengl = Item.TextureOpengl;
            var Texture = Item.TextureOpengl.Texture;
            TextureView.Image = new Bitmap(Texture.Width, Texture.Height).SetChannelsDataInterleaved(
                Item.TextureOpengl.Texture.GetDataFromCached(),
                BitmapChannelList.Rgba
            );
            TextureView.Size = new System.Drawing.Size(Texture.Width, Texture.Height);

            var InfoLines = new List<string>();
            InfoLines.Add(string.Format("Hash: 0x{0:X16}", TextureOpengl.TextureHash));
            InfoLines.Add(string.Format("Size: {0}x{1}", TextureOpengl.Width, TextureOpengl.Height));
            InfoLines.Add(string.Format("Swizzled: {0}", TextureOpengl.TextureCacheKey.Swizzled));
            InfoLines.Add(string.Format("--"));
            InfoLines.Add(string.Format("ColorTestEnabled: {0}", TextureOpengl.TextureCacheKey.ColorTestEnabled));
            InfoLines.Add(string.Format("ColorTestMask: {0}", TextureOpengl.TextureCacheKey.ColorTestMask));
            InfoLines.Add(string.Format("ColorTestFunction: {0}", TextureOpengl.TextureCacheKey.ColorTestFunction));
            InfoLines.Add(string.Format("ColorTestRef: {0}", TextureOpengl.TextureCacheKey.ColorTestRef));
            InfoLines.Add(string.Format("--"));
            InfoLines.Add(string.Format("ClutHash: 0x{0:X16}", TextureOpengl.TextureCacheKey.ClutHash));
            InfoLines.Add(string.Format("ClutAddress: 0x{0:X8}", TextureOpengl.TextureCacheKey.ClutAddress));
            InfoLines.Add(string.Format("ClutFormat: {0}", TextureOpengl.TextureCacheKey.ClutFormat));
            InfoLines.Add(string.Format("ClutMask: {0}", TextureOpengl.TextureCacheKey.ClutMask));
            InfoLines.Add(string.Format("ClutShift: {0}", TextureOpengl.TextureCacheKey.ClutShift));
            InfoLines.Add(string.Format("ClutStart: {0}", TextureOpengl.TextureCacheKey.ClutStart));
            InfoLines.Add(string.Format("--"));
            InfoLines.Add(string.Format("TextureHash: 0x{0:X16}", TextureOpengl.TextureCacheKey.TextureHash));
            InfoLines.Add(string.Format("TextureAddress: 0x{0:X8}", TextureOpengl.TextureCacheKey.TextureAddress));
            InfoLines.Add(string.Format("TextureFormat: {0}", TextureOpengl.TextureCacheKey.TextureFormat));

            TextureInfo.Text = string.Join("\r\n", InfoLines);
        }

        private void TextureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTexture();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var Item = (TextureElement) TextureList.SelectedItem;
            var SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.DefaultExt = "png";
            SaveFileDialog.AddExtension = true;
            SaveFileDialog.FileName = Item.ToString();
            SaveFileDialog.Filter = "Png Image (.png)|*.png";
            if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextureView.Image.Save(SaveFileDialog.FileName);
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            var Item = (TextureElement) TextureList.SelectedItem;

            var TextureHookPlugin = PspDisplayForm.Singleton.IGuiExternalInterface.InjectContext
                .GetInstance<TextureHookPlugin>();

            var OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.FileName = Item.ToString();
            OpenFileDialog.Filter = "Png Image (.png)|*.png";
            if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var Bitmap = new Bitmap(Image.FromFile(OpenFileDialog.FileName));
                Item.TextureOpengl.SetData(
                    Bitmap.GetChannelsDataInterleaved(BitmapChannelList.Rgba).CastToStructArray<OutputPixel>(),
                    Bitmap.Width, Bitmap.Height);
                TextureHookPlugin.AddMapping(Item.TextureOpengl.TextureHash, OpenFileDialog.FileName);
                UpdateTexture();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var Item = (TextureElement) TextureList.SelectedItem;
            var InBitmap = (Bitmap) TextureView.Image;
            Bitmap OutBitmap;

            if (false)
            {
                var OutImage = new cImage(InBitmap.Width * 2, InBitmap.Height * 2);
                libXBR.Xbr2X(cImage.FromBitmap(InBitmap), 0, 0, OutImage, 0, 0, true);
                OutBitmap = OutImage.ToBitmap();
            }
            else
            {
                OutBitmap =
                    (new Engine(new ColorAlphaLerp(), new ColorAlphaThreshold(32, 32, 32, 32))).Process(InBitmap);
            }
            Item.TextureOpengl.SetData(
                OutBitmap.GetChannelsDataInterleaved(BitmapChannelList.Rgba).CastToStructArray<OutputPixel>(),
                OutBitmap.Width, OutBitmap.Height);
            UpdateTexture();
            TextureList.Focus();
        }
    }
}