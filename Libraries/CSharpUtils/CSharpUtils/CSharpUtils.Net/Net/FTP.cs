/* Copyright (c) 2006, J.P. Trosclair
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 *
 *  * Redistributions of source code must retain the above copyright notice, this list of conditions and 
 *		the following disclaimer.
 *  * Redistributions in binary form must reproduce the above copyright notice, this list of conditions 
 *		and the following disclaimer in the documentation and/or other materials provided with the 
 *		distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED 
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR 
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF 
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * Modifications of original ftplib.cs
 * Based on FTPFactory.cs code, pretty much a complete re-write with FTPFactory.cs
 * as a reference.
 * 
 ***********************
 * Authors of this code:
 ***********************
 * J.P. Trosclair     (jptrosclair@judelawfirm.com)
 * Filipe Madureira   (filipe_madureira@hotmail.com) 
 * Carlo M. Andreoli  (cmandreoli@numericaprogetti.it)
 * Sloan Holliday     (sloan@ipass.net)
 * Carlos Ballesteros (soywiz@gmail.com)
 * 
 *********************** 
 * FTPFactory.cs was written by Jaimon Mathew (jaimonmathew@rediffmail.com)
 * and modified by Dan Rolander (Dan.Rolander@marriott.com).
 *	http://www.csharphelp.com/archives/archive9.html
 * and modified again by Carlos Ballesteros (soywiz@gmail.com)
 *  http://code.google.com/p/csharputils/
 ***********************
 * 
 * ** DO NOT ** contact the authors of FTPFactory.cs about problems with this code. It
 * is not their responsibility. Only contact people listed as authors of THIS CODE.
 * 
 *  Any bug fixes or additions to the code will be properly credited to the author.
 * 
 *  BUGS: There probably are plenty. If you fix one, please email me with info
 *   about the bug and the fix, code is welcome.
 * 
 * All calls to the ftplib functions should be:
 * 
 * try 
 * { 
 *		// ftplib function call
 * } 
 * catch(Exception ex) 
 * {
 *		// error handeler
 * }
 * 
 * If you add to the code please make use of OpenDataSocket(), CloseDataSocket(), and
 * ReadResponse() appropriately. See the comments above each for info about using them.
 * 
 * The Fail() function terminates the entire connection. Only call it on critical errors.
 * Non critical errors should NOT close the connection.
 * All errors should throw an exception of type Exception with the response string from
 * the server as the message.
 * 
 * See the simple ftp client for examples on using this class
 */

//#define FTP_DEBUG   

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace CSharpUtils.Net
{
	public class FTPEntry
	{
		public enum FileType
		{
			Unknown,
			File,
			Directory,
			Link,
		}

		public FileType Type = FileType.Unknown;
		public String Name;
		public String RawInfo;
		public int Unknown;
		public String UserName, GroupName;
		public int UserId, GroupId;
		public long Size;
		public DateTimeRange ModifiedTime;

		public override string ToString()
		{
			return "FTPEntry(Name='" + Name + "', RawInfo=" + RawInfo + ", Size=" + Size + ", ModifiedTime=" + ModifiedTime + ")";
		}
	}

	public class FTP
	{
		#region Public Variables

		/// <summary>
		/// IP address or hostname to connect to
		/// </summary>
		public string server;
		/// <summary>
		/// Username to login as
		/// </summary>
		public string user;
		/// <summary>
		/// Password for account
		/// </summary>
		public string pass;
		/// <summary>
		/// Port number the FTP server is listening on
		/// </summary>
		public int port;

		protected int _timeout;

		protected void SetSocketTimeout(Socket Socket, int Timeout)
		{
			if (Socket != null)
			{
				Socket.SendTimeout = Timeout;
				Socket.ReceiveTimeout = Timeout;
			}
		}

		protected void SetSocketTimeout(Socket Socket)
		{
			SetSocketTimeout(Socket, timeout);
		}

		/// <summary>
		/// The timeout (miliseconds) for waiting on data to arrive
		/// </summary>
		public int timeout
		{
			set
			{
				_timeout = value;
				SetSocketTimeout(main_sock);
				SetSocketTimeout(data_sock);
				SetSocketTimeout(listening_sock);
			}
			get
			{
				return _timeout;
			}
		}

		#endregion

		#region Private Variables

		private string messages; // server messages
		private string responseStr; // server response if the user wants it.
		private bool passive_mode;		// #######################################
		private long bytes_total; // upload/download info if the user wants it.
		private long file_size; // gets set when an upload or download takes place
		private Socket main_sock;
		//private StreamReader main_sock_LineReader;
		private Stream main_sock_LineReader;
		private Socket listening_sock;
		private Socket data_sock;
		private Stream file;
		private int response;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public FTP()
			: this(null, 21, null, null)
		{
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="server">Server to connect to</param>
		/// <param name="user">Account to login as</param>
		/// <param name="pass">Account password</param>
		public FTP(string server, string user, string pass)
			: this(server, 21, user, pass)
		{
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="server">Server to connect to</param>
		/// <param name="port">Port server is listening on</param>
		/// <param name="user">Account to login as</param>
		/// <param name="pass">Account password</param>
		public FTP(string server, int port, string user, string pass)
		{
			this.server = server;
			this.user = user;
			this.pass = pass;
			this.port = port;
			passive_mode = true;		// #######################################
			main_sock = null;
			listening_sock = null;
			data_sock = null;
			file = null;
			bytes_total = 0;
			timeout = 10000;	// 10 seconds
			messages = "";
		}

		#endregion

		public enum ResponseCode
		{
			COMMAND_OK = 200,
			COMMAND_NOT_IMPLEMENTED = 202,
			SYSTEM_STATUS = 211,
			DIRECTORY_STATUS = 212,
			FILE_STATUS = 213,
			/*
			100	Series: The requested action is being initiated, expect another reply before proceeding with a new command.
			110	Restart marker replay . In this case, the text is exact and not left to the particular implementation; it must read: MARK yyyy = mmmm where yyyy is User-process data stream marker, and mmmm server's equivalent marker (note the spaces between markers and "=").
			120	Service ready in nnn minutes.
			125	Data connection already open; transfer starting.
			150	File status okay; about to open data connection.
			214	Help message.On how to use the server or the meaning of a particular non-standard command. This reply is useful only to the human user.
			215	NAME system type. Where NAME is an official system name from the registry kept by IANA.
			220	Service ready for new user.
			221	Service closing control connection.
			225	Data connection open; no transfer in progress.
			226	Closing data connection. Requested file action successful (for example, file transfer or file abort).
			227	Entering Passive Mode (h1,h2,h3,h4,p1,p2).
			228	Entering Long Passive Mode (long address, port).
			229	Entering Extended Passive Mode (|||port|).
			230	User logged in, proceed. Logged out if appropriate.
			231	User logged out; service terminated.
			232	Logout command noted, will complete when transfer done.
			250	Requested file action okay, completed.
			257	"PATHNAME" created.
			331	User name okay, need password.
			332	Need account for login.
			350	Requested file action pending further information
			421	Service not available, closing control connection. This may be a reply to any command if the service knows it must shut down.
			425	Can't open data connection.
			426	Connection closed; transfer aborted.
			430	Invalid username or password
			434	Requested host unavailable.
			450	Requested file action not taken.
			451	Requested action aborted. Local error in processing.
			452	Requested action not taken. Insufficient storage space in system.File unavailable (e.g., file busy).
			500	Syntax error, command unrecognized. This may include errors such as command line too long.
			501	Syntax error in parameters or arguments.
			502	Command not implemented.
			503	Bad sequence of commands.
			504	Command not implemented for that parameter.
			530	Not logged in.
			532	Need account for storing files.
			550	Requested action not taken. File unavailable (e.g., file not found, no access).
			551	Requested action aborted. Page type unknown.
			552	Requested file action aborted. Exceeded storage allocation (for current directory or dataset).
			553	Requested action not taken. File name not allowed.
			631	Integrity protected reply.
			632	Confidentiality and integrity protected reply.
			633	Confidentiality protected reply.
			*/
		}

		static Dictionary<String, int> MonthsMap = new Dictionary<String, int>()
		{
			{ "jan", 1 },
			{ "feb", 2 },
			{ "mar", 3 },
			{ "apr", 4 },
			{ "may", 5 },
			{ "jun", 6 },
			{ "jul", 7 },
			{ "aug", 8 },
			{ "sep", 9 },
			{ "oct", 10 },
			{ "nov", 11 },
			{ "dec", 12 },
		};

		// Linux standard
		/*
			-rw-r--r--
			1
			1000
			1000
			12
			Mar
			30
			13:14
			this is a test.txt
		*/
		static Regex regex = new Regex(
			@"^" +
			@"(?<Perms>\S+)\s+" +
			@"(?<Unknown>\S+)\s+" +
			@"(?<Uid>\S+)\s+" +
			@"(?<Gid>\S+)\s+" +
			@"(?<Size>\S+)\s+" +
			@"(?<Month>\S+)\s+" +
			@"(?<Day>\S+)\s+" +
			@"(?<YearOrHour>\S+)\s+" +
			@"(?<FileName>.*)" +
			@"$"
		, RegexOptions.Compiled);

		/// <summary>
		/// List FTPEntry items with the current path on the FTP connection.
		/// </summary>
		/// <returns>List of FTPEntry items</returns>
		public LinkedList<FTPEntry> ListEntries()
		{
			var entries = new LinkedList<FTPEntry>();
			int defalt_year = DateTime.Now.Year;

			foreach (var row in List())
			{
				//Console.WriteLine(row.ToString());
				var matches = regex.Match(row.ToString());
				int year = defalt_year, month = 1, day = 1, hour = 0, minute = 0, second = 0;
				String year_or_hour = matches.Groups["YearOrHour"].Value;
				month = MonthsMap[matches.Groups["Month"].Value.ToLower()];
				day = Convert.ToInt32(matches.Groups["Day"].Value);

				DateTimeRange.PrecisionType Precision;

				// Hour
				if (year_or_hour.IndexOf(":") >= 0)
				{
					var c = year_or_hour.Split(':');
					hour = Convert.ToInt32(c[0]);
					minute = Convert.ToInt32(c[1]);
					Precision = DateTimeRange.PrecisionType.Minutes;
				}
				// Year
				else
				{
					year = Convert.ToInt32(year_or_hour);
					Precision = DateTimeRange.PrecisionType.Days;
				}

				var entry = new FTPEntry();
				entry.Name = matches.Groups["FileName"].Value;
				entry.RawInfo = matches.Groups["Perms"].Value;
				entry.Unknown = Convert.ToInt32(matches.Groups["Unknown"].Value);
				entry.UserName = matches.Groups["Uid"].Value;
				entry.GroupName = matches.Groups["Gid"].Value;
				if (!int.TryParse(entry.UserName, out entry.UserId))
				{
					entry.UserId = -1;
				}
				if (!int.TryParse(entry.GroupName, out entry.GroupId))
				{
					entry.GroupId = -1;
				}
				entry.Size = Convert.ToInt64(matches.Groups["Size"].Value);
				entry.ModifiedTime = new DateTimeRange(new DateTime(year, month, day, hour, minute, second), Precision);

				switch (entry.RawInfo[0])
				{
					case 'd': entry.Type = FTPEntry.FileType.Directory; break;
					case '-': entry.Type = FTPEntry.FileType.File; break;
					case '1': entry.Type = FTPEntry.FileType.Link; break;
				}

				entries.AddLast(entry);
			}

			return entries;
		}

		/// <summary>
		/// Connection status to the server
		/// </summary>
		public bool IsConnected
		{
			get
			{
				if (main_sock != null)
				{
					return main_sock.Connected;
				}
				return false;
			}
		}
		/// <summary>
		/// Returns true if the message buffer has data in it
		/// </summary>
		public bool MessagesAvailable
		{
			get
			{
				if (messages.Length > 0)
					return true;
				return false;
			}
		}
		/// <summary>
		/// Server messages if any, buffer is cleared after you access this property
		/// </summary>
		public string Messages
		{
			get
			{
				string tmp = messages;
				messages = "";
				return tmp;
			}
		}
		/// <summary>
		/// The response string from the last issued command
		/// </summary>
		public string ResponseString
		{
			get
			{
				return responseStr;
			}
		}
		/// <summary>
		/// The total number of bytes sent/recieved in a transfer
		/// </summary>
		public long BytesTotal		// #######################################
		{
			get
			{
				return bytes_total;
			}
		}
		/// <summary>
		/// The size of the file being downloaded/uploaded (Can possibly be 0 if no size is available)
		/// </summary>
		public long FileSize		// #######################################
		{
			get
			{
				return file_size;
			}
		}
		/// <summary>
		/// True:  Passive mode [default]
		/// False: Active Mode
		/// </summary>
		public bool PassiveMode		// #######################################
		{
			get
			{
				return passive_mode;
			}
			set
			{
				passive_mode = value;
			}
		}


		private void Fail()
		{
			Disconnect();
			throw new Exception(responseStr);
		}


		private void SetBinaryMode(bool mode)
		{
			if (mode)
				SendCommand("TYPE I");
			else
				SendCommand("TYPE A");

			ReadResponse();
			if (response != 200)
				Fail();
		}


		private void SendCommand(string command)
		{
			Byte[] cmd = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());

#if (FTP_DEBUG)
			if (command.Length > 3 && command.Substring(0, 4) == "PASS")
				Console.WriteLine("\rPASS xxx");
			else
				Console.WriteLine("\r" + command);
#endif

			main_sock.Send(cmd, cmd.Length, 0);
		}

		// Any time a command is sent, use ReadResponse() to get the response
		// from the server. The variable responseStr holds the entire string and
		// the variable response holds the response number.
		private void ReadResponse()
		{
			string buf;
			messages = "";

			while (true)
			{
				//buf = main_sock_LineReader.ReadLine();
				buf = main_sock_LineReader.ReadUntilString((byte)'\n', Encoding.ASCII, IncludeExpectedByte:false);

#if (FTP_DEBUG)
				Console.WriteLine(buf);
#endif
				// the server will respond with "000-Foo bar" on multi line responses
				// "000 Foo bar" would be the last line it sent for that response.
				// Better example:
				// "000-This is a multiline response"
				// "000-Foo bar"
				// "000 This is the end of the response"
				if (Regex.Match(buf, "^[0-9]+ ").Success)
				{
					responseStr = buf;
					response = int.Parse(buf.Substring(0, 3));
					break;
				}
				else
					messages += Regex.Replace(buf, "^[0-9]+-", "") + "\n";
			}
		}


		// if you add code that needs a data socket, i.e. a PASV or PORT command required,
		// call this function to do the dirty work. It sends the PASV or PORT command,
		// parses out the port and ip info and opens the appropriate data socket
		// for you. The socket variable is private Socket data_socket. Once you
		// are done with it, be sure to call CloseDataSocket()
		private void OpenDataSocket()
		{
			if (passive_mode)		// #######################################
			{
				string[] pasv;
				string server;
				int port;

				Connect();
				SendCommand("PASV");
				ReadResponse();
				if (response != 227)
					Fail();

				try
				{
					int i1, i2;

					i1 = responseStr.IndexOf('(') + 1;
					i2 = responseStr.IndexOf(')') - i1;
					pasv = responseStr.Substring(i1, i2).Split(',');
				}
				catch (Exception)
				{
					Disconnect();
					throw new Exception("Malformed PASV response: " + responseStr);
				}

				if (pasv.Length < 6)
				{
					Disconnect();
					throw new Exception("Malformed PASV response: " + responseStr);
				}

				server = String.Format("{0}.{1}.{2}.{3}", pasv[0], pasv[1], pasv[2], pasv[3]);
				port = (int.Parse(pasv[4]) << 8) + int.Parse(pasv[5]);

				try
				{
#if (FTP_DEBUG)
					Console.WriteLine("Data socket: {0}:{1}", server, port);
#endif
					CloseDataSocket();

#if (FTP_DEBUG)
					Console.WriteLine("Creating socket...");
#endif
					data_sock = new Socket(
						AddressFamily.InterNetwork,
						SocketType.Stream,
						ProtocolType.Tcp
					);
					SetSocketTimeout(data_sock);

#if (FTP_DEBUG)
					Console.WriteLine("Resolving host");
#endif


#if (FTP_DEBUG)
					Console.WriteLine("Connecting..");
#endif
					data_sock.Connect(server, port);

#if (FTP_DEBUG)
					Console.WriteLine("Connected.");
#endif
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to connect for data transfer: " + ex.Message);
				}
			}
			else		// #######################################
			{
				Connect();

				try
				{
#if (FTP_DEBUG)
					Console.WriteLine("Data socket (active mode)");
#endif
					CloseDataSocket();

#if (FTP_DEBUG)
					Console.WriteLine("Creating listening socket...");
#endif
					listening_sock = new Socket(
						AddressFamily.InterNetwork,
						SocketType.Stream,
						ProtocolType.Tcp
					);
					SetSocketTimeout(listening_sock);

#if (FTP_DEBUG)
					Console.WriteLine("Binding it to local address/port");
#endif
					// for the PORT command we need to send our IP address; let's extract it
					// from the LocalEndPoint of the main socket, that's already connected
					string sLocAddr = main_sock.LocalEndPoint.ToString();
					int ix = sLocAddr.IndexOf(':');
					if (ix < 0)
					{
						throw new Exception("Failed to parse the local address: " + sLocAddr);
					}
					string sIPAddr = sLocAddr.Substring(0, ix);
					// let the system automatically assign a port number (setting port = 0)
					System.Net.IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(sIPAddr), 0);

					listening_sock.Bind(localEP);
					sLocAddr = listening_sock.LocalEndPoint.ToString();
					ix = sLocAddr.IndexOf(':');
					if (ix < 0)
					{
						throw new Exception("Failed to parse the local address: " + sLocAddr);
					}
					int nPort = int.Parse(sLocAddr.Substring(ix + 1));
#if (FTP_DEBUG)
					Console.WriteLine("Listening on {0}:{1}", sIPAddr, nPort);
#endif
					// start to listen for a connection request from the host (note that
					// Listen is not blocking) and send the PORT command
					listening_sock.Listen(1);
					string sPortCmd = string.Format("PORT {0},{1},{2}",
													sIPAddr.Replace('.', ','),
													nPort / 256, nPort % 256);
					SendCommand(sPortCmd);
					ReadResponse();
					if (response != 200)
						Fail();
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to connect for data transfer: " + ex.Message);
				}
			}
		}


		private void ConnectDataSocket()		// #######################################
		{
			if (data_sock != null)		// already connected (always so if passive mode)
				return;

			try
			{
#if (FTP_DEBUG)
				Console.WriteLine("Accepting the data connection.");
#endif
				data_sock = listening_sock.Accept();	// Accept is blocking
				listening_sock.Close();
				listening_sock = null;

				if (data_sock == null)
				{
					throw new Exception("Winsock error: " +
						Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
				}
#if (FTP_DEBUG)
				Console.WriteLine("Connected.");
#endif
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to connect for data transfer: " + ex.Message);
			}
		}


		private void CloseDataSocket()
		{
#if (FTP_DEBUG)
			Console.WriteLine("Attempting to close data channel socket...");
#endif
			if (data_sock != null)
			{
				if (data_sock.Connected)
				{
#if (FTP_DEBUG)
						Console.WriteLine("Closing data channel socket!");
#endif
					data_sock.Close();
#if (FTP_DEBUG)
						Console.WriteLine("Data channel socket closed!");
#endif
				}
				data_sock = null;
			}
		}
		/// <summary>
		/// Closes all connections to the ftp server
		/// </summary>
		public void Disconnect()
		{
			CloseDataSocket();

			if (main_sock != null)
			{
				if (main_sock.Connected)
				{
					SendCommand("QUIT");
					main_sock.Close();
				}
				main_sock = null;
			}

			if (file != null)
				file.Close();

			file = null;
		}
		/// <summary>
		/// Connect to a ftp server
		/// </summary>
		/// <param name="server">IP or hostname of the server to connect to</param>
		/// <param name="port">Port number the server is listening on</param>
		/// <param name="user">Account name to login as</param>
		/// <param name="pass">Password for the account specified</param>
		public void Connect(string server, int port, string user, string pass)
		{
			this.port = port;

			Connect(server, user, pass);
		}
		/// <summary>
		/// Connect to a ftp server
		/// </summary>
		/// <param name="server">IP or hostname of the server to connect to</param>
		/// <param name="user">Account name to login as</param>
		/// <param name="pass">Password for the account specified</param>
		public void Connect(string server, string user, string pass)
		{
			this.server = server;
			this.user = user;
			this.pass = pass;

			Connect();
		}
		/// <summary>
		/// Connect to an ftp server
		/// </summary>
		public void Connect()
		{
			if (server == null)
				throw new Exception("No server has been set.");
			if (user == null)
				throw new Exception("No username has been set.");

			if (main_sock != null)
				if (main_sock.Connected)
					return;

			main_sock = new Socket(
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp
			);
			SetSocketTimeout(main_sock);

			try
			{
				main_sock.Connect(server, port);
				//main_sock_LineReader = new StreamReader(new NetworkStream(main_sock));
				main_sock_LineReader = new NetworkStream(main_sock);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			ReadResponse();
			if (response != 220)
				Fail();

			SendCommand("USER " + user);
			ReadResponse();

			switch (response)
			{
				case 331:
					if (pass == null)
					{
						Disconnect();
						throw new Exception("No password has been set.");
					}
					SendCommand("PASS " + pass);
					ReadResponse();
					if (response != 230)
						Fail();
					break;
				case 230:
					break;
			}

			return;
		}
		/// <summary>
		/// Retrieves a list of files from the ftp server
		/// </summary>
		/// <returns>An ArrayList of files</returns>
		public ArrayList List()
		{
			Byte[] bytes = new Byte[512];
			string file_list = "";
			long bytesgot = 0;
			//int msecs_passed = 0;
			ArrayList list = new ArrayList();

			Connect();
			OpenDataSocket();
			SendCommand("LIST");
			ReadResponse();

			//FILIPE MADUREIRA.
			//Added response 125
			switch (response)
			{
				case 125:
				case 150:
					break;
				default:
					CloseDataSocket();
					throw new Exception(responseStr);
			}

			// http://www.codethinked.com/net-40-and-systemthreadingtasks
			// http://msmvps.com/blogs/peterritchie/archive/2007/04/26/thread-sleep-is-a-sign-of-a-poorly-designed-program.aspx
			//data_sock.BeginReceive(
			ConnectDataSocket();
			{
				do
				{
					bytesgot = data_sock.Receive(bytes, bytes.Length, 0);
					file_list += Encoding.ASCII.GetString(bytes, 0, (int)bytesgot);
				} while (data_sock.Available > 0);
			}
			CloseDataSocket();

			ReadResponse();
			if (response != 226)
				throw new Exception(responseStr);

			foreach (string f in file_list.Split('\n'))
			{
				if (f.Length > 0 && !Regex.Match(f, "^total").Success)
					list.Add(f.Substring(0, f.Length - 1));
			}

			return list;
		}
		/// <summary>
		/// Gets a file list only
		/// </summary>
		/// <returns>ArrayList of files only</returns>
		public ArrayList ListFiles()
		{
			ArrayList list = new ArrayList();

			foreach (string f in List())
			{
				//FILIPE MADUREIRA
				//In Windows servers it is identified by <DIR>
				if ((f.Length > 0))
				{
					if ((f[0] != 'd') && (f.ToUpper().IndexOf("<DIR>") < 0))
						list.Add(f);
				}
			}

			return list;
		}
		/// <summary>
		/// Gets a directory list only
		/// </summary>
		/// <returns>ArrayList of directories only</returns>
		public ArrayList ListDirectories()
		{
			ArrayList list = new ArrayList();

			foreach (string f in List())
			{
				//FILIPE MADUREIRA
				//In Windows servers it is identified by <DIR>
				if (f.Length > 0)
				{
					if ((f[0] == 'd') || (f.ToUpper().IndexOf("<DIR>") >= 0))
						list.Add(f);
				}
			}

			return list;
		}
		/// <summary>
		/// Returns the 'Raw' DateInformation in ftp format. (YYYYMMDDhhmmss). Use GetFileDate to return a DateTime object as a better option.
		/// </summary>
		/// <param name="fileName">Remote FileName to Query</param>
		/// <returns>Returns the 'Raw' DateInformation in ftp format</returns>
		public string GetFileDateRaw(string fileName)
		{
			Connect();

			SendCommand("MDTM " + fileName);
			ReadResponse();
			if (response != 213)
			{
#if (FTP_DEBUG)
				Console.Write("\r" + responseStr);
#endif
				throw new Exception(responseStr);
			}

			return (this.responseStr.Substring(4));
		}
		/// <summary>
		/// GetFileDate will query the ftp server for the date of the remote file.
		/// </summary>
		/// <param name="fileName">Remote FileName to Query</param>
		/// <returns>DateTime of the Input FileName</returns>
		public DateTime GetFileDate(string fileName)
		{
			return ConvertFTPDateToDateTime(GetFileDateRaw(fileName));
		}

		private DateTime ConvertFTPDateToDateTime(string input)
		{
			if (input.Length < 14)
				throw new ArgumentException("Input Value for ConvertFTPDateToDateTime method was too short.");

			//YYYYMMDDhhmmss": 
			int year = Convert.ToInt16(input.Substring(0, 4));
			int month = Convert.ToInt16(input.Substring(4, 2));
			int day = Convert.ToInt16(input.Substring(6, 2));
			int hour = Convert.ToInt16(input.Substring(8, 2));
			int min = Convert.ToInt16(input.Substring(10, 2));
			int sec = Convert.ToInt16(input.Substring(12, 2));

			return new DateTime(year, month, day, hour, min, sec);
		}
		/// <summary>
		/// Get the working directory on the ftp server
		/// </summary>
		/// <returns>The working directory</returns>
		public string GetWorkingDirectory()
		{
			//PWD - print working directory
			Connect();
			SendCommand("PWD");
			ReadResponse();

			if (response != 257)
				throw new Exception(responseStr);

			string pwd;
			try
			{
				pwd = responseStr.Substring(responseStr.IndexOf("\"", 0) + 1);//5);
				pwd = pwd.Substring(0, pwd.LastIndexOf("\""));
				pwd = pwd.Replace("\"\"", "\""); // directories with quotes in the name come out as "" from the server
			}
			catch (Exception ex)
			{
				throw new Exception("Uhandled PWD response: " + ex.Message);
			}

			return pwd;
		}
		/// <summary>
		/// Change to another directory on the ftp server
		/// </summary>
		/// <param name="path">Directory to change to</param>
		public void ChangeDir(string path)
		{
			Connect();
			SendCommand("CWD " + path);
			ReadResponse();
			if (response != 250)
			{
#if (FTP_DEBUG)
				Console.Write("\r" + responseStr);
#endif
				throw new Exception(responseStr + " : " + path);
			}
		}
		/// <summary>
		/// Create a directory on the ftp server
		/// </summary>
		/// <param name="dir">Directory to create</param>
		public void MakeDir(string dir)
		{
			Connect();
			SendCommand("MKD " + dir);
			ReadResponse();

			switch (response)
			{
				case 257:
				case 250:
					break;
				default:
#if (FTP_DEBUG)
                    Console.Write("\r" + responseStr);
#endif
					throw new Exception(responseStr + " : " + dir);
			}
		}
		/// <summary>
		/// Remove a directory from the ftp server
		/// </summary>
		/// <param name="dir">Name of directory to remove</param>
		public void RemoveDir(string dir)
		{
			Connect();
			SendCommand("RMD " + dir);
			ReadResponse();
			if (response != 250)
			{
#if (FTP_DEBUG)
				Console.Write("\r" + responseStr);
#endif
				throw new Exception(responseStr + " : " + dir);
			}
		}
		/// <summary>
		/// Remove a file from the ftp server
		/// </summary>
		/// <param name="filename">Name of the file to delete</param>
		public void RemoveFile(string filename)
		{
			Connect();
			SendCommand("DELE " + filename);
			ReadResponse();
			if (response != 250)
			{
#if (FTP_DEBUG)
				Console.Write("\r" + responseStr);
#endif
				throw new Exception(responseStr + " : " + filename);
			}
		}
		/// <summary>
		/// Rename a file on the ftp server
		/// </summary>
		/// <param name="oldfilename">Old file name</param>
		/// <param name="newfilename">New file name</param>
		public void RenameFile(string oldfilename, string newfilename)		// #######################################
		{
			Connect();
			SendCommand("RNFR " + oldfilename);
			ReadResponse();
			if (response != 350)
			{
#if (FTP_DEBUG)
				Console.Write("\r" + responseStr);
#endif
				throw new Exception(responseStr);
			}
			else
			{
				SendCommand("RNTO " + newfilename);
				ReadResponse();
				if (response != 250)
				{
#if (FTP_DEBUG)
					Console.Write("\r" + responseStr);
#endif
					throw new Exception(responseStr);
				}
			}
		}
		/// <summary>
		/// Get the size of a file (Provided the ftp server supports it)
		/// </summary>
		/// <param name="filename">Name of file</param>
		/// <returns>The size of the file specified by filename</returns>
		public long GetFileSize(string filename)
		{
			Connect();
			SendCommand("SIZE " + filename);
			ReadResponse();
			if (response != 213)
			{
#if (FTP_DEBUG)
				Console.Write("\r" + responseStr);
#endif
				throw new Exception(responseStr);
			}

			return Int64.Parse(responseStr.Substring(4));
		}
		public void OpenUpload(string filename, string remote_filename, bool resume = false)
		{
			try
			{
				file = new FileStream(filename, FileMode.Open);
			}
			catch (Exception ex)
			{
				file = null;
				throw new Exception(ex.Message);
			}
			OpenUpload(file, remote_filename, resume);
		}

		/// <summary>
		/// Open an upload with resume support
		/// </summary>
		/// <param name="_file">Local file to upload (Can include path to file)</param>
		/// <param name="remote_filename">Filename to store file as on ftp server</param>
		/// <param name="resume">Attempt resume if exists</param>
		public void OpenUpload(Stream _file, string remote_filename, bool resume = false)
		{
			Connect();
			SetBinaryMode(true);
			OpenDataSocket();

			bytes_total = 0;

			file = _file;
			file_size = _file.Length;

			if (resume)
			{
				long size = GetFileSize(remote_filename);
				SendCommand("REST " + size);
				ReadResponse();
				if (response == 350)
					file.Seek(size, SeekOrigin.Begin);
			}

			SendCommand("STOR " + remote_filename);
			ReadResponse();

			switch (response)
			{
				case 125:
				case 150:
					break;
				default:
					file.Close();
					file = null;
					throw new Exception(responseStr);
			}
			ConnectDataSocket();		// #######################################	

			return;
		}

		/// <summary>
		/// Open a file for download
		/// </summary>
		/// <param name="remote_filename">The name of the file on the FTP server</param>
		/// <param name="local_filename">The name of the file to save as (Can include path to file)</param>
		/// <param name="resume">Attempt resume if file exists</param>
		public void OpenDownload(string remote_filename, string local_filename, bool resume = false)
		{
			Connect();
			SetBinaryMode(true);

			bytes_total = 0;

			try
			{
				file_size = GetFileSize(remote_filename);
			}
			catch
			{
				file_size = 0;
			}

			if (resume && File.Exists(local_filename))
			{
				try
				{
					file = new FileStream(local_filename, FileMode.Open);
				}
				catch (Exception ex)
				{
					file = null;
					throw new Exception(ex.Message);
				}

				SendCommand("REST " + file.Length);
				ReadResponse();
				if (response != 350)
					throw new Exception(responseStr);
				file.Seek(file.Length, SeekOrigin.Begin);
				bytes_total = file.Length;
			}
			else
			{
				try
				{
					file = new FileStream(local_filename, FileMode.Create);
				}
				catch (Exception ex)
				{
					file = null;
					throw new Exception(ex.Message);
				}
			}

			OpenDataSocket();
			SendCommand("RETR " + remote_filename);
			ReadResponse();

			switch (response)
			{
				case 125:
				case 150:
					break;
				default:
					file.Close();
					file = null;
					throw new Exception(responseStr);
			}
			ConnectDataSocket();		// #######################################	

			return;
		}
		/// <summary>
		/// Upload the file, to be used in a loop until file is completely uploaded
		/// </summary>
		/// <returns>Bytes sent</returns>
		public long DoUpload()
		{
			Byte[] bytes = new Byte[512];
			long bytes_got;

			try
			{
				bytes_got = file.Read(bytes, 0, bytes.Length);
				bytes_total += bytes_got;
				data_sock.Send(bytes, (int)bytes_got, 0);

				if (bytes_got <= 0)
				{
					// the upload is complete or an error occured
					file.Close();
					file = null;

					CloseDataSocket();
					ReadResponse();
					switch (response)
					{
						case 226:
						case 250:
							break;
						default:
							throw new Exception(responseStr);
					}

					SetBinaryMode(false);
				}
			}
			catch (Exception ex)
			{
				file.Close();
				file = null;
				CloseDataSocket();
				ReadResponse();
				SetBinaryMode(false);
				throw ex;
			}

			return bytes_got;
		}
		/// <summary>
		/// Download a file, to be used in a loop until the file is completely downloaded
		/// </summary>
		/// <returns>Number of bytes recieved</returns>
		public long DoDownload()
		{
			Byte[] bytes = new Byte[512];
			long bytes_got;

			try
			{
				bytes_got = data_sock.Receive(bytes, bytes.Length, 0);

				if (bytes_got <= 0)
				{
					// the download is done or an error occured
					CloseDataSocket();
					file.Close();
					file = null;

					ReadResponse();
					switch (response)
					{
						case 226:
						case 250:
							break;
						default:
							throw new Exception(responseStr);
					}

					SetBinaryMode(false);

					return bytes_got;
				}

				file.Write(bytes, 0, (int)bytes_got);
				bytes_total += bytes_got;
			}
			catch (Exception ex)
			{
				CloseDataSocket();
				file.Close();
				file = null;
				ReadResponse();
				SetBinaryMode(false);
				throw ex;
			}

			return bytes_got;
		}
	}
}