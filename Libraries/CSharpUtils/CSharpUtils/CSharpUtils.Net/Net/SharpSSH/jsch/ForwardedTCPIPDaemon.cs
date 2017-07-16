using System;

namespace Tamir.SharpSsh.jsch
{
	public interface ForwardedTCPIPDaemon : Runnable
	{
		void setChannel(ChannelForwardedTCPIP channel);
		void setArg(Object[] arg);
	}
}
