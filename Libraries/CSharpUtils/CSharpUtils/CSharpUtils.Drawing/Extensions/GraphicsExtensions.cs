using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CSharpUtils.Drawing.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class GraphicsExtensions
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern int SetTextCharacterExtra(
            IntPtr hdc, // DC handle
            int nCharExtra // extra-space value 
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="characterSpacing"></param>
        public static void SetTextCharacterExtra(this Graphics graphics, int characterSpacing)
        {
            IntPtr hdc = graphics.GetHdc();
            SetTextCharacterExtra(hdc, characterSpacing); //set spacing between characters 
            graphics.ReleaseHdc(hdc);
        }
    }
}