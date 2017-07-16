using Tamir.Streams;
using Tamir.SharpSsh.jsch;
using System.IO;
using Exception = System.Exception;
using System.Net.Sockets;
using System;
using System.Text;

namespace Tamir.SharpSsh.jsch
{
	public class ProxyHTTP : Proxy
	{
		private static int DEFAULTPORT=80;
		private String proxy_host;
		private int proxy_port;
		private JStream ins;
		private JStream outs;
		private Socket socket;

		private String user;
		private String passwd;

		public ProxyHTTP(String proxy_host)
		{
			int port=DEFAULTPORT;
			String host=proxy_host;
			if(proxy_host.IndexOf(':')!=-1)
			{
				try
				{
					host=proxy_host.Substring(0, proxy_host.IndexOf(':'));

					port = System.Convert.ToInt32(proxy_host.Substring(proxy_host.IndexOf(':') + 1));
				}
				catch(Exception)
				{
				}
			}
			this.proxy_host=host;
			this.proxy_port=port;
		}
		public ProxyHTTP(String proxy_host, int proxy_port)
		{
			this.proxy_host=proxy_host;
			this.proxy_port=proxy_port;
		}
		public void setUserPasswd(String user, String passwd)
		{
			this.user=user;
			this.passwd=passwd;
		}
		public void connect(SocketFactory socket_factory, String host, int port, int timeout)
		{
			try
			{
				if(socket_factory==null)
				{
					socket=Util.createSocket(proxy_host, proxy_port, timeout);    
					ins= new JStream(new NetworkStream(socket));
					outs=new JStream(new NetworkStream(socket));
				}
				else
				{
					socket=socket_factory.createSocket(proxy_host, proxy_port);
					ins=new JStream(socket_factory.getInputStream(socket));
					outs=new JStream(socket_factory.getOutputStream(socket));
				}
				if(timeout>0)
				{
					socket.ReceiveTimeout = timeout;
					socket.SendTimeout = timeout;
				}
				socket.NoDelay = true;

				outs.write("CONNECT "+host+":"+port+" HTTP/1.0\r\n");

				if(user!=null && passwd!=null)
				{
					byte[] _code=Encoding.UTF8.GetBytes(user + ":" + passwd);
					_code=Util.toBase64(_code, 0, _code.Length);
					outs.write("Proxy-Authorization: Basic ");
					outs.write(_code);
					outs.write("\r\n");
				}

				outs.write("\r\n");
				outs.flush();

				int foo=0;

				string sb = "";
				while(foo>=0)
				{
					foo=ins.read(); if(foo!=13){sb += ((char)foo);  continue;}
					foo=ins.read(); if(foo!=10){continue;}
					break;
				}
				if(foo<0)
				{
					throw new System.IO.IOException(); 
				}

				String response = sb; 
				String reason="Unknow reason";
				int code=-1;
				try
				{
					foo=response.IndexOf(' ');
					int bar=response.IndexOf(' ', foo+1);
					code = System.Convert.ToInt32(response.Substring(foo + 1, bar));
					reason=response.Substring(bar+1);
				}
				catch(Exception)
				{
				}
				if(code!=200)
				{
					throw new System.IO.IOException("proxy error: "+reason);
				}

				/*
				while(foo>=0){
				  foo=in.read(); if(foo!=13) continue;
				  foo=in.read(); if(foo!=10) continue;
				  foo=in.read(); if(foo!=13) continue;      
				  foo=in.read(); if(foo!=10) continue;
				  break;
				}
				*/

				int count=0;
				while(true)
				{
					count=0;
					while(foo>=0)
					{
						foo=ins.read(); if(foo!=13){count++;  continue;}
						foo=ins.read(); if(foo!=10){continue;}
						break;
					}
					if(foo<0)
					{
						throw new System.IO.IOException();
					}
					if(count==0)break;
				}
			}
			catch(Exception e)
			{
				try{ if(socket!=null)socket.Close(); }
				catch(Exception)
				{
				}
				String message="ProxyHTTP: "+e.ToString();
				throw e;
			}
		}
		public Stream getInputStream(){ return ins.s; }
		public Stream getOutputStream(){ return outs.s; }
		public Socket getSocket(){ return socket; }
		public void close()
		{
			try
			{
				if(ins!=null)ins.close();
				if(outs!=null)outs.close();
				if(socket!=null)socket.Close();
			}
			catch(Exception)
			{
			}
			ins=null;
			outs=null;
			socket=null;
		}
		public static int getDefaultPort()
		{
			return DEFAULTPORT;
		}
	}
}
