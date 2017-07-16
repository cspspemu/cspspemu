using System;
using System.Collections.Generic;

namespace CSharpUtils.Web._45.Fastcgi
{
	public class FastcgiHeaders
	{
		//protected List<KeyValuePair<String, String>> Headers = new List<KeyValuePair<String, String>>();
		public List<KeyValuePair<String, String>> Headers = new List<KeyValuePair<string, string>>();

		public void Replace(String Key, String Value)
		{
			for (int n = 0; n < Headers.Count; n++)
			{
				if (Headers[n].Key == Key)
				{
					Headers[n] = new KeyValuePair<String, String>(Key, Value);
					return;
				}
			}
			Add(Key, Value);
		}

		public void Add(String Key, String Value)
		{
			Headers.Add(new KeyValuePair<String, String>(Key, Value));
		}
	}
}
