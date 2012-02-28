using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow.Internals;
using System.Runtime.InteropServices;

namespace CSPspEmu.Media
{
	[ComImport,
	Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IFileSinkFilter
	{
		[PreserveSig]
		int SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string fileName, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

		//[PreserveSig]
		//int GetCurFile( [In, MarshalAs( UnmanagedType.LPWStr )] string fileName, [In, MarshalAs( UnmanagedType.LPStruct )] AMMediaType mediaType );

		//HRESULT SetFileName(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt);
		//HRESULT GetCurFile (LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt);
	};

	[ComImport, Guid("E436EBB3-524F-11CE-9F53-0020AF0BA770")]
	class FilgraphManager
	{
	}

	[ComImport, Guid("855FBD04-8AD5-40B2-AA34-A6581E59831C")]
	class OpenMGOmgSourceFilter
	{
	}

	[ComImport, Guid("98660581-C9A8-4C92-B480-F27DE3C3AAB4")]
	class OMG_TRANSFORM
	{
	}

	[ComImport, Guid("E882F102-F626-49E9-BD68-CE2BE7E59EA0")]
	class WavDest2
	{
	}

	[ComImport, Guid("3c78b8e2-6c4d-11d1-ade2-0000f8754b99")]
	class WavDest
	{
	}

	[ComImport, Guid("8596E5F0-0DA5-11D0-BD21-00A0C911CE86")]
	class File_writer
	{
	}

	class MyWavPin : IPin
	{
		MyWavDest MyWavDest;

		public MyWavPin(MyWavDest MyWavDest)
		{
			this.MyWavDest = MyWavDest;
		}

		public int Connect(IPin receivePin, AMMediaType mediaType)
		{
			throw new NotImplementedException();
		}

		public int ReceiveConnection(IPin receivePin, AMMediaType mediaType)
		{
			throw new NotImplementedException();
		}

		public int Disconnect()
		{
			throw new NotImplementedException();
		}

		public int ConnectedTo(out IPin pin)
		{
			// temporal
			pin = null;
			//throw new NotImplementedException();
			return 0;
		}

		public int ConnectionMediaType(AMMediaType mediaType)
		{
			throw new NotImplementedException();
		}

		public int QueryPinInfo(out PinInfo pinInfo)
		{
			throw new NotImplementedException();
		}

		public int QueryDirection(out PinDirection pinDirection)
		{
			pinDirection = PinDirection.Output;
			//throw new NotImplementedException();
			return 0;
		}

		public int QueryId(out string id)
		{
			throw new NotImplementedException();
		}

		public int QueryAccept(AMMediaType mediaType)
		{
			throw new NotImplementedException();
		}

		public int EnumMediaTypes(IntPtr enumerator)
		{
			throw new NotImplementedException();
		}

		public int QueryInternalConnections(IntPtr apPin, ref int nPin)
		{
			throw new NotImplementedException();
		}

		public int EndOfStream()
		{
			throw new NotImplementedException();
		}

		public int BeginFlush()
		{
			throw new NotImplementedException();
		}

		public int EndFlush()
		{
			throw new NotImplementedException();
		}

		public int NewSegment(long start, long stop, double rate)
		{
			throw new NotImplementedException();
		}
	}

	class MyWavEnumPins : IEnumPins
	{
		int Offset = 0;
		MyWavDest MyWavDest;
		MyWavPin[] Pins;

		public MyWavEnumPins(MyWavDest MyWavDest)
		{
			this.MyWavDest = MyWavDest;
			Pins = new[] { new MyWavPin(MyWavDest) };
		}

		public int Next(int cPins, IPin[] pins, out int pinsFetched)
		{
			pinsFetched = 0;
			for (int n = 0; n < cPins; n++)
			{
				if (Offset < Pins.Length)
				{
					pins[n] = Pins[Offset];
					pinsFetched++;
					Offset++;
				}
			}
			return 0;
		}

		public int Skip(int cPins)
		{
			Offset += cPins;
			return 0;
		}

		public int Reset()
		{
			Offset = 0;
			return 0;
		}

		public int Clone(out IEnumPins enumPins)
		{
			enumPins = new MyWavEnumPins(MyWavDest);
			return 0;
		}
	}

	[ComVisible(true)]
	[Guid("11111111-2222-3333-4444-555555555555")]
	[ClassInterface(ClassInterfaceType.None)]
	class WavDestProxy : IBaseFilter
	{
		IBaseFilter BaseFilter;

		public WavDestProxy(IBaseFilter BaseFilter)
		{
			this.BaseFilter = BaseFilter;
		}

		public int GetClassID(out Guid ClassID)
		{
			var Result = BaseFilter.GetClassID(out ClassID);
			Console.WriteLine("WavDestProxy.GetClassID: {0}, {1}", Result, ClassID);
			return Result;
		}

		public int Stop()
		{
			var Result = BaseFilter.Stop();
			Console.WriteLine("WavDestProxy.Stop: {0}", Result);
			return Result;
		}

		public int Pause()
		{
			var Result = BaseFilter.Pause();
			Console.WriteLine("WavDestProxy.Stop: {0}", Result);
			return Result;
		}

		public int Run(long start)
		{
			var Result = BaseFilter.Run(start);
			Console.WriteLine("WavDestProxy.Run: {0}, {1}", Result, start);
			return Result;
		}

		public int GetState(int milliSecsTimeout, out int filterState)
		{
			var Result = BaseFilter.GetState(milliSecsTimeout, out filterState);
			Console.WriteLine("WavDestProxy.GetState: {0}, {1}, {2}", Result, milliSecsTimeout, filterState);
			return Result;
		}

		public int SetSyncSource(IntPtr clock)
		{
			var Result = BaseFilter.SetSyncSource(clock);
			Console.WriteLine("WavDestProxy.SetSyncSource: {0}, {1}", Result, clock);
			return Result;
		}

		public int GetSyncSource(out IntPtr clock)
		{
			var Result = BaseFilter.GetSyncSource(out clock);
			Console.WriteLine("WavDestProxy.GetSyncSource: {0}, {1}", Result, clock);
			return Result;
		}

		public int EnumPins(out IEnumPins enumPins)
		{
			var Result = BaseFilter.EnumPins(out enumPins);
			Console.WriteLine("WavDestProxy.EnumPins: {0}, {1}", Result, enumPins);
			return Result;
		}

		public int FindPin(string id, out IPin pin)
		{
			var Result = BaseFilter.FindPin(id, out pin);
			Console.WriteLine("WavDestProxy.FindPin: {0}, {1}, {2}", Result, id, pin);
			return Result;
		}

		public int QueryFilterInfo(out FilterInfo filterInfo)
		{
			var Result = BaseFilter.QueryFilterInfo(out filterInfo);
			Console.WriteLine("WavDestProxy.QueryFilterInfo: {0}, {1}", Result, filterInfo);
			return Result;
		}

		public int JoinFilterGraph(IFilterGraph graph, string name)
		{
			var Result = BaseFilter.JoinFilterGraph(graph, name);
			Console.WriteLine("WavDestProxy.JoinFilterGraph: {0}, {1}, {2}", Result, graph, name);
			return Result;
		}

		public int QueryVendorInfo(out string vendorInfo)
		{
			var Result = BaseFilter.QueryVendorInfo(out vendorInfo);
			Console.WriteLine("WavDestProxy.QueryVendorInfo: {0}, {1}", Result, vendorInfo);
			return Result;
		}
	}

	//[ComImport]
	[ComVisible(true)]
	[Guid("11111111-2222-3333-4444-555555555555")]
	[ClassInterface(ClassInterfaceType.None)]
	class MyWavDest : IBaseFilter
	{
		public int GetClassID(out Guid ClassID)
		{
			ClassID = Guid.Parse("56A86895-0AD4-11CE-B03A-0020AF0BA770");
			//Console.WriteLine("aaaaaaaaaaa");
			//throw new NotImplementedException();
			return 0;
		}

		public int Stop()
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int Pause()
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int Run(long start)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int GetState(int milliSecsTimeout, out int filterState)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int SetSyncSource(IntPtr clock)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int GetSyncSource(out IntPtr clock)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int EnumPins(out IEnumPins enumPins)
		{
			enumPins = new MyWavEnumPins(this);
			Console.WriteLine("EnumPins");
			return 0;
			//throw new NotImplementedException();
		}

		public int FindPin(string id, out IPin pin)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int QueryFilterInfo(out FilterInfo filterInfo)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}

		public int JoinFilterGraph(IFilterGraph graph, string name)
		{
			Console.WriteLine("JoinFilterGraph({0}, {1})", graph, name);
			//throw new NotImplementedException();
			return 0;
		}

		public int QueryVendorInfo(out string vendorInfo)
		{
			Console.WriteLine("aaaaaaaaaaa");
			throw new NotImplementedException();
		}
	}
}
