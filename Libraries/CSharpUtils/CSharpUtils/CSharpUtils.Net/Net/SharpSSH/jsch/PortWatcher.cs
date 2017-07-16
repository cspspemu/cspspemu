using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace Tamir.SharpSsh.jsch
{
	/* -*-mode:java; c-basic-offset:2; -*- */
	/*
	Copyright (c) 2002,2003,2004 ymnk, JCraft,Inc. All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:

	  1. Redistributions of source code must retain the above copyright notice,
		 this list of conditions and the following disclaimer.

	  2. Redistributions in binary form must reproduce the above copyright 
		 notice, this list of conditions and the following disclaimer in 
		 the documentation and/or other materials provided with the distribution.

	  3. The names of the authors may not be used to endorse or promote products
		 derived from this software without specific prior written permission.

	THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
	INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
	FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
	INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
	INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
	LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
	OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
	LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
	NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
	EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	*/

	class PortWatcher : Runnable
	{
		private static ArrayList pool = new ArrayList();


		internal Session session;
		internal int lport;
		internal int rport;
		internal String host;
		internal IPAddress boundaddress;
		internal Runnable thread;
		internal TcpListener ss;

		internal static String[] getPortForwarding(Session session)
		{
			ArrayList foo = new ArrayList();
			lock(pool)
			{
				for(int i=0; i<pool.Count; i++)
				{
					PortWatcher p=(PortWatcher)(pool[i]);
					if(p.session==session)
					{
						foo.Add(p.lport + ":" + p.host + ":" + p.rport);
					}
				}
			}
			String[] bar=new String[foo.Count];
			for(int i = 0; i < foo.Count; i++)
			{
				bar[i] = (String)(foo[i]);
			}
			return bar;
		}
		internal static PortWatcher getPort(Session session, String address, int lport)
		{
			IPAddress addr;
			try
			{
				addr = Dns.GetHostEntry(address).AddressList[0];
			}
			catch(Exception)
			{
				throw new JSchException("PortForwardingL: invalid address "+address+" specified.");
			}
			lock(pool)
			{
				for(int i=0; i<pool.Count; i++)
				{
					PortWatcher p=(PortWatcher)(pool[i]);
					if(p.session==session && p.lport==lport)
					{

						if (
							IPAddress.IsLoopback(p.boundaddress) ||
							p.boundaddress == addr
						)
						{
							return p;
						}
					}
				}
				return null;
			}
		}
		internal static PortWatcher addPort(Session session, String address, int lport, String host, int rport, ServerSocketFactory ssf) 
		{
			if(getPort(session, address, lport)!=null)
			{
				throw new JSchException("PortForwardingL: local port "+ address+":"+lport+" is already registered.");
			}
			PortWatcher pw=new PortWatcher(session, address, lport, host, rport, ssf);
			pool.Add(pw);
			return pw;
		}
		internal static void delPort(Session session, String address, int lport) 
		{
			PortWatcher pw=getPort(session, address, lport);
			if(pw==null)
			{
				throw new JSchException("PortForwardingL: local port "+address+":"+lport+" is not registered.");
			}
			pw.delete();
			pool.Remove(pw);
		}
		internal static void delPort(Session session)
		{
			lock(pool)
			{
				PortWatcher[] foo=new PortWatcher[pool.Count];
				int count=0;
				for (int i = 0; i < pool.Count; i++)
				{
					PortWatcher p=(PortWatcher)(pool[i]);
					if(p.session==session) 
					{
						p.delete();
						foo[count++]=p;
					}
				}
				for(int i=0; i<count; i++)
				{
					PortWatcher p=foo[i];
					pool.Remove(p);
				}
			}
		}
		internal PortWatcher(Session session, 
			String address, int lport, 
			String host, int rport,
			ServerSocketFactory factory) 
		{
			this.session=session;
			this.lport=lport;
			this.host=host;
			this.rport=rport;
			try
			{
				boundaddress = Dns.GetHostEntry(address).AddressList[0];
				ss=(factory==null) ?
					new TcpListener(boundaddress, lport) :
					factory.createServerSocket(lport, 0, boundaddress);
			}
			catch(Exception e)
			{ 
				Console.WriteLine(e);
				throw new JSchException("PortForwardingL: local port "+address+":"+lport+" cannot be bound.");
			}
		}

		public void run()
		{
			Buffer buf=new Buffer(300); // ??
			Packet packet=new Packet(buf);
			thread=this;
			try
			{
				while(thread!=null)
				{
					Socket socket = ss.AcceptSocket();
					socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
					Stream In=new NetworkStream(socket);
					Stream Out=new NetworkStream(socket);
					ChannelDirectTCPIP channel=new ChannelDirectTCPIP();
					channel.init();
					channel.setInputStream(In);
					channel.setOutputStream(Out);
					session.addChannel(channel);
					((ChannelDirectTCPIP)channel).setHost(host);
					((ChannelDirectTCPIP)channel).setPort(rport);
					((ChannelDirectTCPIP)channel).setOrgIPAddress(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
					((ChannelDirectTCPIP)channel).setOrgPort(((IPEndPoint)socket.RemoteEndPoint).Port);
					channel.connect();
					if(channel.exitstatus!=-1)
					{
					}
				}
			}
			catch(Exception)
			{
				//System.out.println("! "+e);
			}

			delete();
		}

		internal void delete()
		{
			thread=null;
			try
			{
				if (ss != null) ss.Stop();
				ss = null;
			}
			catch(Exception)
			{
			}
		}
	}
}
