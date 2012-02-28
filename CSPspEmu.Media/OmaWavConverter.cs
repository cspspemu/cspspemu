using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow.Internals;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace CSPspEmu.Media
{
	public class OmaWavConverter
	{
		static public void convertOmaToWav(string Source, string Destination)
		{
			IGraphBuilder graphBuilder;
			IMediaControl mediaControl;
			IMediaEventEx mediaEvent;
			IBaseFilter sourceFilter;
			IBaseFilter omgTransform;
			IBaseFilter waveDest;
			IBaseFilter fileWriter;

			if (!File.Exists(Source)) throw (new Exception(String.Format("Can't find file '{0}'", Source)));

			graphBuilder = (IGraphBuilder)new FilgraphManager();
			mediaControl = (IMediaControl)graphBuilder;
			mediaEvent = (IMediaEventEx)graphBuilder;

			/*
			1. Open Registry Editor
			2. HKEY_LOCAL_MACHINE\SOFTWARE\Sony Corporation\OpenMG\
			3. InstallerVersion 5.4.00.04020
			*/
			try
			{
				sourceFilter = (IBaseFilter)new OpenMGOmgSourceFilter();
				omgTransform = (IBaseFilter)new OMG_TRANSFORM();
			}
			catch (COMException)
			{
				if (MessageBox.Show("Must download and install 'OpenMG Setup Update Program' in order to play Atrac3+ files\n\nGo to download page?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
				{
					Process.Start("http://www.sony-mea.com/support/download/375063");
				}
				return;
			}

			Console.WriteLine("[1]");

			try
			{
				waveDest = (IBaseFilter)new WavDest();
				//waveDest = (IBaseFilter)new WavDest2();
				//waveDest = new MyWavDest();
				//waveDest = new WavDestProxy((IBaseFilter)new WavDest());
			}
			catch (COMException)
			{
				MessageBox.Show("Missing WavDest", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Debug.WriteLine("[2]");

			fileWriter = (IBaseFilter)new File_writer();

			Debug.WriteLine("[3]");

			var fileSourceFilter = (IFileSourceFilter)sourceFilter;
			fileSourceFilter.Load(Source, null);

			Debug.WriteLine("[4]");

			var fileSinkFilter = (IFileSinkFilter)fileWriter;
			fileSinkFilter.SetFileName(Destination, null);

			Debug.WriteLine("[5]");

			graphBuilder.AddFilter(sourceFilter, "CLSID_OpenMGOmgSourceFilter");
			graphBuilder.AddFilter(omgTransform, "CLSID_OMG_TRANSFORM");
			graphBuilder.AddFilter(waveDest, "CLSID_WavDest");
			graphBuilder.AddFilter(fileWriter, "CLSID_File_writer");
			IPin SourceOutPin;
			IPin FileWriterInPin;

			Debug.WriteLine("[6]");

			sourceFilter.FindPin("Output", out SourceOutPin);
			fileWriter.FindPin("in", out FileWriterInPin);

			Debug.WriteLine("[7]");

			graphBuilder.Connect(SourceOutPin, FileWriterInPin);

			graphBuilder.Render(SourceOutPin);

			mediaControl.Run();

			Debug.WriteLine("[8]");

			int evCode;
			mediaEvent.WaitForCompletion(int.MaxValue, out evCode);

			Action<object> Release = (o) =>
			{
				try
				{
					while (Marshal.ReleaseComObject(o) > 0) { }
				}
				catch
				{
				}
			};

			Release(sourceFilter);
			Release(omgTransform);
			Release(waveDest);
			Release(SourceOutPin);
			Release(FileWriterInPin);
			Release(mediaControl);
			Release(mediaEvent);
			Release(fileWriter);
			Release(fileSinkFilter);
			Release(graphBuilder);

			Debug.WriteLine("[9]");
		}
	}
}
