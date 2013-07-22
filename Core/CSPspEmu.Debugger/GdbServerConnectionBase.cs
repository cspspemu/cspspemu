using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Debugger
{
	public class GdbServerConnectionBase
	{
		protected bool debugFlag = true;
		protected Sigval lastSigval = Sigval.Ok;

		protected bool running = false;
		protected int[] threads = new int[] { 100, 200, 311 };
		protected int threadCursor;
		protected GdbProcessorRegisters registers;

		protected bool debugData;
		protected bool debugMaster = false;

		public GdbServerConnectionBase()
		{
			init();
		}

		protected void init()
		{
			//foreach (k, ref register; registers.ALL) register = k;
		}

		protected byte checksum(string text)
		{
			byte checksum = 0;
			foreach (var c in text) checksum += (byte)c;
			return checksum;
		}
	
		// http://www.embecosm.com/appnotes/ean4/html/ch04s05s03.html
		protected string generatePacketWithChecksum(string packet)
		{
			return "$%s#%02x".Sprintf(packet, checksum(packet));
		}

		protected void sendPacket(string packet)
		{
		
		}
	
		/*
		void sendPacketf(T...)(T args) {
			sendPacket(std.string.format(args));
		}
		*/

		protected void handlePacket(string packet)
		{
			string responsePacket = "E00";
			try
			{
				try
				{
					responsePacket = handlePacket2(packet);
				}
				catch (Exception e)
				{
					responsePacket = "E01";
					throw(e);			
				}
			}
			finally
			{
				sendPacket(responsePacket);
			}
		}

		protected int hexDigitDecode(char hexDigit)
		{
			if (hexDigit >= '0' && hexDigit <= '9') return hexDigit - '0';
			if (hexDigit >= 'a' && hexDigit <= 'f') return hexDigit - 'a' + 10;
			if (hexDigit >= 'A' && hexDigit <= 'F') return hexDigit - 'A' + 10;
			return 0;
		}

		protected string hexEncode(byte[] data)
		{
			string r = "";
			foreach (var c in data) r += "%02x".Sprintf(c);
			return r;
		}

		protected byte[] hexDecode(string hexString)
		{
			var r = new List<byte>();
			for (int n = 0; n < hexString.Length; n += 2) {
				r.Add((byte)(
					(hexDigitDecode(hexString[n + 0]) << 4) |
					(hexDigitDecode(hexString[n + 1]) << 0)
				));
			}
			return r.ToArray();
		}

		protected string getSigvalPacket()
		{
			if (lastSigval == Sigval.Ok) return "T00";
			return "S%02x".Sprintf(lastSigval);
		}

		protected string handlePacket2(string packet)
		{
			//string[] parts = std.array.split(";", packet);
			char type = packet[0];
		
			if (type != 'm') {
				if (debugMaster) {
					Console.WriteLine("recvData: %s", packet);
				}
				debugData = true;
			} else {
				debugData = false;
			}
		
			switch (type) {
				// A simple reply of "OK" indicates the target will support extended remote debugging.
				case '!':
					return "OK";
				break;
				// The detach is acknowledged with a reply packet of "OK" before the client
				// connection is closed and rsp.client_fd set to -1. The semantics of detach
				// require the target to resume execution, so the processor is unstalled
				// using set_stall_state (0).
				case 'D':
					return "OK";
				break;
				// This sets the thread number of subsequent operations.
				// Since thread numbers are of no relevance to this target,
				// a response of "OK" is always acceptable.
				case 'H':
					return "OK";
				break;
				// The kill request is used in extended mode before a restart or
				// request to run a new program (vRun packet). Since the CPU is
				// already stalled, it seems to have no additional semantic meaning.
				// Since it requires no reply it can be silently ignored.
				case 'k':
					return "OK";
				break;
				// Since this is a bare level target, there is no concept of separate threads.
				// The one thread is always active, so a reply of "OK" is always acceptable.
				case 'T':
					return "OK";
				break;
				// The response to the ? packet is provided by rsp_report_exception ().
				// This is always a S packet. The signal value (as a GDB target signal)
				// is held in rsp.sigval, and is presented as two hexadecimal digits.
				case '?':
					if (running) {
						return getSigvalPacket();
					} else {
						return "E01";
					}
				break;
				case 'd':
					debugFlag = !debugFlag;
					return "OK";
				break;
			
				// Read all registers
				case 'g': {
					string o = "";
					foreach (var register in registers.ALL)
					{
						o += hexEncode(StructUtils.StructToBytes(register));
					}
					return o;
				} break;
				// Write all registers
				case 'G':
				break;
			
				// Read a register
				case 'p': {
					var regNum = Convert.ToUInt32(packet.Substring(1), 16);
					return hexEncode(StructUtils.StructToBytes(registers.ALL[regNum]));
				} break;
				// Write a register
				case 'P': {
					string[] parts = packet.Substring(1).Split('=');
					var regNum = Convert.ToUInt32(parts[0], 16);
					var value = BitConverter.ToUInt32(hexDecode(parts[1]), 0);
					registers.ALL[regNum] = value; 
					return "OK";
				} break;
			
				// http://www.embecosm.com/appnotes/ean4/html/ch04s07s07.html
			
				// Read from memory
				case 'm': {
					var parts = packet.Substring(1).Split(',');
					var addr = Convert.ToUInt32(parts[0], 16);
					var size = Convert.ToUInt32(parts[1], 16);
					var data = new byte[size];
					for (int n = 0; n < data.Length; n++) data[n] = 0xFF;
					return hexEncode(data);
				} break;
				// Write to memory
				case 'M':
				break;

				// The functionality for the R packet is provided in rsp_restart ().
				// The start address of the current target is held in rsp.start_addr.
				// The program counter is set to this address using set_npc () (see Section 4.6.5).
				case 'R':
				break;

				// Continue
				case 'c': {
					//scope (exit) lastSigval = Sigval.Ok;
					lastSigval = Sigval.DebugException;
					return getSigvalPacket();
				
					/*
					(new Thread({
						Thread.sleep(dur!"msecs"(400));
						lastSigval = Sigval.DebugException; 
						sendPacket(getSigvalPacket());
					})).start();

					lastSigval = Sigval.DebugException; 
					sendPacket(getSigvalPacket());
				
					return "";
					*/
					//return "";
				}

				// Step
				case 's':
					return "T00";
				break;

				// Extended packets.
				case 'v': {
					string[] packetParts = packet.Split(';');
					switch (packetParts[0]) {
						case "vRun": {
							var args = new List<string>();
							foreach (var argHex in packetParts.Slice(1))
							{
								args.Add(Encoding.ASCII.GetString(hexDecode(argHex)));
							}
							Console.WriteLine("%s", args);
							try {
								return "S00";
							} finally {
								lastSigval = Sigval.InvalidOpcode;
							}
							//return getSigvalPacket();
							//return "";
						} break;
						case "vAttach":
						break;
						case "vCont":
						break;
						case "vCont?":
							return "OK";
						break;
						case "vFile":
						break;
						case "vFlashErase": case "vFlashWrite": case "vFlashDone":
						break;
						default:
							throw(new Exception(String.Format("Unknown packet '{0}'", packet)));
					}
				} break;
			
				// Query.
				// http://www.manpagez.com/info/gdb/gdb_282.php
				case 'q':
				{
					Func<int, string> getNextThread = (cursor) =>
					{
						if (threadCursor >= threads.Length) {
							return "l";
						} else {
							return "m%x".Sprintf(threads[threadCursor]);
						}
					};
			
					switch (packet) {
						case "qfThreadInfo": threadCursor = 0; return getNextThread(threadCursor++);
						case "qsThreadInfo": return getNextThread(threadCursor++);
						case "qC": return "QC%x".Sprintf(threads[0]);
						default: throw(new Exception(String.Format("Unknown packet '{0}'", packet)));
					}
				}
				default:
					throw(new Exception(String.Format("Unknown packet '{0}'", packet[0])));
			}
		
			return "E01";
		}
	}
}
