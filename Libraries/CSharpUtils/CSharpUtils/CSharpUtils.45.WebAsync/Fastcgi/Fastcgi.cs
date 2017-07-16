namespace CSharpUtils.Web._45.Fastcgi
{
	public class Fastcgi
	{
		public enum ProtocolStatus
		{
			FCGI_REQUEST_COMPLETE = 0, // normal end of request.
			FCGI_CANT_MPX_CONN = 1, // rejecting a new request. This happens when a Web server sends concurrent requests over one connection to an application that is designed to process one request at a time per connection.
			FCGI_OVERLOADED = 2, // rejecting a new request. This happens when the application runs out of some resource, e.g. database connections.
			FCGI_UNKNOWN_ROLE = 3, // rejecting a new request. This happens when the Web server has specified a role that is unknown to the application.
		}

		public enum Flags
		{
			FCGI_KEEP_CONN = 1,
		}

		public enum Role
		{
			FCGI_RESPONDER = 1,
			FCGI_AUTHORIZER = 2,
			FCGI_FILTER = 3,
		}

		public enum PacketType
		{
			FCGI_BEGIN_REQUEST = 1,
			FCGI_ABORT_REQUEST = 2,
			FCGI_END_REQUEST = 3,
			FCGI_PARAMS = 4,
			FCGI_STDIN = 5,
			FCGI_STDOUT = 6,
			FCGI_STDERR = 7,
			FCGI_DATA = 8,
			FCGI_GET_VALUES = 9,
			FCGI_GET_VALUES_RESULT = 10,
			FCGI_UNKNOWN_TYPE = 11,
		}
	}
}
