using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CSharpUtils.Templates;
using CSharpUtils.Templates.TemplateProvider;
using CSharpUtils.Web._45.Fastcgi;
using CSharpUtils.Threading;
using CSharpUtils._45.Redis;

namespace CSharpUtils._45.Sandbox
{
	class Program
	{
		class MyFastcgiServerAsync : FastcgiServerAsync
		{
			TemplateProviderMemory TemplateProvider;
			TemplateFactory TemplateFactory;

			public MyFastcgiServerAsync()
			{
				TemplateProvider = new TemplateProviderMemory();
				TemplateFactory = new TemplateFactory(TemplateProvider);
				TemplateProvider.Add("Base.html", "Test{% block Body %}Base{% endblock %}Test");
				TemplateProvider.Add("Test.html", "{% extends 'Base.html' %}{% block Body %}Ex{% endblock %}");
				//TemplateProvider.Add("Test.html", "{% block Body %}Ex{% endblock %}");
			}

            public override async Task HandleRequestAsync(FastcgiRequestAsync Request, FastcgiResponseAsync Response)
			{
                var StreamWriter = new StreamWriter(Response.StdoutStream);
				StreamWriter.AutoFlush = true;
				//StreamWriter.Write("Hello World!");

				var Stopwatch = new Stopwatch();
				Stopwatch.Start();
				var Html = TemplateFactory.GetTemplateCodeByFile("Test.html").RenderToString();
				Stopwatch.Stop();

				StreamWriter.Write("Content-type: text/html\r\n");
				StreamWriter.Write("X-Time: {0}\r\n", Stopwatch.Elapsed);
				StreamWriter.Write("\r\n");

				StreamWriter.Write(Html);
				StreamWriter.Write(Request.Params.ToJson());
				//throw (new Exception("Error!"));
			}
		}

		static void Test1(string[] args)
		{
			//new MyFastcgiServerAsync().Listen(8000);

			var Values = new List<int>();

			var Thread1 = new GreenThread();
			var Thread2 = new GreenThread();
			Thread1.InitAndStartStopped(() =>
			{
				Values.Add(1);
				Console.WriteLine(1);
				GreenThread.Yield();
				Console.WriteLine(3);
			});
			Thread2.InitAndStartStopped(() =>
			{
				Values.Add(2);
				Console.WriteLine(2);
				GreenThread.Yield();
				Console.WriteLine(4);
			});

			Console.WriteLine("a");
			Thread1.SwitchTo();
			Thread2.SwitchTo();
			Thread1.SwitchTo();
			Thread2.SwitchTo();
			Console.WriteLine("b");
			Console.ReadKey();
		}

		async static Task Test2Async()
		{
			var RedisClient = new RedisClientAsync();
			await RedisClient.Connect("localhost", 6379);
			Console.WriteLine((await RedisClient.Command("set", "hello-csharp", "999")).ToJson());
			Console.WriteLine((await RedisClient.Command("get", "hello-csharp")).ToJson());
		}

		static void Main(string[] args)
		{
			//Test2Async().
			Test2Async().Wait();
			Console.ReadKey();
		}
	}
}
