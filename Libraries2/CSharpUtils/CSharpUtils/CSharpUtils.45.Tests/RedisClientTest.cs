using System.Threading.Tasks;
using CSharpUtils._45.Redis;
using NUnit.Framework;

namespace CSharpUtils._45.Tests
{
	[TestFixture]
	public class RedisClientTest
	{
		public async Task _CommandTestAsync()
		{
			var RedisClient = new RedisClientAsync();
			/*
			await RedisClient.Connect("localhost", 6379);
			Console.WriteLine((await RedisClient.Command("set", "hello-csharp", "1")).ToJson());
			Console.WriteLine((await RedisClient.Command("get", "hello-csharp")).ToJson());
			*/
		}

		[Test]
		public void CommandTest()
		{
			var Task = _CommandTestAsync();
			Task.WaitAll(Task);
		}
	}
}
