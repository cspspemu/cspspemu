using System;

namespace Tamir.SharpSsh.jsch
{
	public class SftpException : Exception
	{
		public int Id;

		public SftpException (int Id, String Message, Exception innerException = null) : base(Message, innerException) 
		{
			this.Id = Id;
		}

		public override String ToString()
		{
			return this.Id + " : " + this.Message + " : " + this.InnerException;
		}
	}
}
