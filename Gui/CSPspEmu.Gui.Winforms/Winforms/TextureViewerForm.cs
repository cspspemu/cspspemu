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
                return $"{this.TextureOpengl.TextureHash:X16}";
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
            InfoLines.Add($"Hash: 0x{TextureOpengl.TextureHash:X16}");
            InfoLines.Add($"Size: {TextureOpengl.Width}x{TextureOpengl.Height}");
            InfoLines.Add($"Swizzled: {TextureOpengl.TextureCacheKey.Swizzled}");
            InfoLines.Add(string.Format("--"));
            InfoLines.Add($"ColorTestEnabled: {TextureOpengl.TextureCacheKey.ColorTestEnabled}");
            InfoLines.Add($"ColorTestMask: {TextureOpengl.TextureCacheKey.ColorTestMask}");
            InfoLines.Add($"ColorTestFunction: {TextureOpengl.TextureCacheKey.ColorTestFunction}");
            InfoLines.Add($"ColorTestRef: {TextureOpengl.TextureCacheKey.ColorTestRef}");
            InfoLines.Add(string.Format("--"));
            InfoLines.Add($"ClutHash: 0x{TextureOpengl.TextureCacheKey.ClutHash:X16}");
            InfoLines.Add($"ClutAddress: 0x{TextureOpengl.TextureCacheKey.ClutAddress:X8}");
            InfoLines.Add($"ClutFormat: {TextureOpengl.TextureCacheKey.ClutFormat}");
            InfoLines.Add($"ClutMask: {TextureOpengl.TextureCacheKey.ClutMask}");
            InfoLines.Add($"ClutShift: {TextureOpengl.TextureCacheKey.ClutShift}");
            InfoLines.Add($"ClutStart: {TextureOpengl.TextureCacheKey.ClutStart}");
            InfoLines.Add(string.Format("--"));
            InfoLines.Add($"TextureHash: 0x{TextureOpengl.TextureCacheKey.TextureHash:X16}");
            InfoLines.Add($"TextureAddress: 0x{TextureOpengl.TextureCacheKey.TextureAddress:X8}");
            InfoLines.Add($"TextureFormat: {TextureOpengl.TextureCacheKey.TextureFormat}");

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