using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSharpUtils.Fastcgi.Http
{
	public interface IHttpRequestHandler
	{
		void DispatchRequest(HttpRequest HttpRequest);
	}

	public class UrlsHandler : IHttpRequestHandler
	{
		protected List<Tuple<Regex, IHttpRequestHandler>> Criterias = new List<Tuple<Regex, IHttpRequestHandler>>();

		public UrlsHandler AddCriteria(Regex Regex, IHttpRequestHandler Handler)
		{
			Criterias.Add(new Tuple<Regex, IHttpRequestHandler>(Regex, Handler));
			return this;
		}

		public void DispatchRequest(HttpRequest HttpRequest)
		{
			string RequestUri;

			if (!HttpRequest.Enviroment.TryGetValue("REQUEST_URI", out RequestUri))
			{
				RequestUri = "";
			}

			foreach (var Pair in Criterias)
			{
				if (Pair.Item1.IsMatch(RequestUri))
				{
					Pair.Item2.DispatchRequest(HttpRequest);
					return;
				}
			}

			throw (new Exception(String.Format("No criteria for '{0}'", RequestUri)));
		}
	}
}
