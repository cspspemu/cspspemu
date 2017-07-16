using System.Drawing;
using CSharpUtils;
using System.Runtime.InteropServices;
using System;

public static class GraphicsExtensions
{
	[DllImport("gdi32.dll", CharSet=CharSet.Auto)] 
	private static extern int SetTextCharacterExtra( 
		IntPtr hdc,    // DC handle
		int nCharExtra // extra-space value 
	);

	public static void SetTextCharacterExtra(this Graphics Graphics, int CharacterSpacing)
	{
		IntPtr hdc = Graphics.GetHdc();
		SetTextCharacterExtra(hdc, CharacterSpacing); //set spacing between characters 
		Graphics.ReleaseHdc(hdc); 
	}
}
