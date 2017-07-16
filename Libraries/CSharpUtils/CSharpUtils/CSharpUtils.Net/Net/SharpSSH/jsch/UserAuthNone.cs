using System;

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

	class UserAuthNone : UserAuth
	{
		private String methods=null;
		private UserInfo userinfo;
		internal UserAuthNone(UserInfo userinfo)
		{
			this.userinfo=userinfo;
		}

		public override bool start(Session session)
		{
			base.start(session);
			//System.out.println("UserAuthNone: start");
			Packet packet=session.packet;
			Buffer buf=session.buf;
			String username=session.username;

			byte[] _username=null;
			try{ _username=Util.getBytesUTF8(username); }
			catch
			{//(java.io.UnsupportedEncodingException e){
				_username=Util.getBytes(username);
			}

			// send
			// byte      SSH_MSG_USERAUTH_REQUEST(50)
			// string    user name
			// string    service name ("ssh-connection")
			// string    "none"
			packet.reset();
			buf.WriteByte((byte)Session.SSH_MSG_USERAUTH_REQUEST);
			buf.WriteString(_username);
			buf.WriteString(Util.getBytes("ssh-connection"));
			buf.WriteString(Util.getBytes("none"));
			session.write(packet);

			loop:
				while(true)
				{
					// receive
					// byte      SSH_MSG_USERAUTH_SUCCESS(52)
					// string    service name
					buf=session.read(buf);
					//System.out.println("UserAuthNone: read: 52 ? "+    buf.buffer[5]);
					if(buf.buffer[5]==Session.SSH_MSG_USERAUTH_SUCCESS)
					{
						return true;
					}
					if(buf.buffer[5]==Session.SSH_MSG_USERAUTH_BANNER)
					{
						buf.ReadInt(); buf.ReadByte(); buf.ReadByte();
						byte[] _message=buf.ReadString();
						byte[] lang=buf.ReadString();
						String message=null;
						try{ message=Util.getStringUTF8(_message); }
						catch
						{//(java.io.UnsupportedEncodingException e){
							message=Util.getString(_message);
						}
						if(userinfo!=null)
						{
							userinfo.showMessage(message);
						}
						goto loop;
					}
					if(buf.buffer[5]==Session.SSH_MSG_USERAUTH_FAILURE)
					{
						buf.ReadInt(); buf.ReadByte(); buf.ReadByte(); 
						byte[] foo=buf.ReadString();
						int partial_success=buf.ReadByte();
						methods=Util.getString(foo);
						//System.out.println("UserAuthNONE: "+methods+
						//		   " partial_success:"+(partial_success!=0));
						//	if(partial_success!=0){
						//	  throw new JSchPartialAuthException(Encoding.UTF8.GetString(foo));
						//	}
						break;
					}
					else
					{
						//      System.out.println("USERAUTH fail ("+buf.buffer[5]+")");
						throw new JSchException("USERAUTH fail ("+buf.buffer[5]+")");
					}
				}
			//throw new JSchException("USERAUTH fail");
			return false;
		}
		internal String getMethods()
		{
			return methods;
		}
	}

}
