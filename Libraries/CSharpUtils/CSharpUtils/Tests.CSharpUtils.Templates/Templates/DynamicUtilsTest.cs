using System;
using System.Collections;
using System.Collections.Generic;
using CSharpUtils.Templates.Runtime;
using NUnit.Framework;

namespace CSharpUtilsTests.Templates
{
	[TestFixture]
	public class DynamicUtilsTest
	{
		[Test]
		public void TestConvertToBool()
		{
			Assert.AreEqual(true, DynamicUtils.ConvertToBool((float)7.1));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool((double)7.1));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool(7));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool(true));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool(new Object()));

			Assert.AreEqual(false, DynamicUtils.ConvertToBool(0));
			Assert.AreEqual(false, DynamicUtils.ConvertToBool((double)0.0));
			Assert.AreEqual(false, DynamicUtils.ConvertToBool(false));
			Assert.AreEqual(false, DynamicUtils.ConvertToBool(null));
		}

		[Test]
		public void TestAdd()
		{
			Assert.AreEqual("10a", DynamicUtils.BinaryOperation_Add(10, "a"));
			Assert.AreEqual("a", DynamicUtils.BinaryOperation_Add(null, "a"));
			Assert.AreEqual("a", DynamicUtils.BinaryOperation_Add("a", null));
			Assert.AreEqual(null, DynamicUtils.BinaryOperation_Add(null, null));
			Assert.AreEqual(21.5, DynamicUtils.BinaryOperation_Add(10, 11.5));
		}

		[Test]
		public void TestAccess()
		{
			Assert.AreEqual("Value", DynamicUtils.Access(new Hashtable() { { "MyKey", "Value" } }, "MyKey"));
			Assert.AreEqual(10, DynamicUtils.Access(new ClassTestAccess(), "SampleField"));
			Assert.AreEqual(20, DynamicUtils.Access(new ClassTestAccess(), "SampleProperty"));
			Assert.AreEqual(30, DynamicUtils.Access(new ClassTestAccess(), "SampleMethod"));
			Assert.AreEqual(null, DynamicUtils.Access(null, "Test"));
			Assert.AreEqual(null, DynamicUtils.Access(null, null));
			Assert.AreEqual(null, DynamicUtils.Access(10, "Test"));
			Assert.AreEqual(null, DynamicUtils.Access("Test", "Test"));
			Assert.AreEqual(2, DynamicUtils.Access(new int[] { 0, 1, 2, 3, 4 }, 2));
			Assert.AreEqual(2, DynamicUtils.Access(new List<int>(new int[] { 0, 1, 2, 3, 4 }), 2));
		}

		[Test]
		public void TestCountArray()
		{
			Assert.AreEqual(0, DynamicUtils.CountArray(null));
			Assert.AreEqual(0, DynamicUtils.CountArray(10));
			Assert.AreEqual(0, DynamicUtils.CountArray("String"));
			Assert.AreEqual(3, DynamicUtils.CountArray(new int[] { 1, 2, 3 }));
			Assert.AreEqual(3, DynamicUtils.CountArray(new List<int>(new int[] { 1, 2, 3 })));
		}

		[Test]
		public void TestCountArrayList()
		{
			Assert.AreEqual(3, DynamicUtils.CountArray(new List<Post> {
				new Post() { Title = "Test1" },
				new Post() { Title = "Test2" },
				new Post() { Title = "Test3" },
			}));
		}

		sealed class Post
		{
			public string Title;
		}

		[Test]
		public void TestConvertToIEnumerable()
		{
			CollectionAssert.AreEquivalent(
				new object[] { },
				DynamicUtils.ConvertToIEnumerable(null)
			);
			CollectionAssert.AreEquivalent(
				new object[] { "1", 2, 3 },
				DynamicUtils.ConvertToIEnumerable(new Dictionary<string, object> { { "Test1", "1" }, { "Test2", 2 }, { "Test3", 3 } })
			);
			CollectionAssert.AreEquivalent(
				new object[] { "1", 2, 3 },
				DynamicUtils.ConvertToIEnumerable(new List<object> { "1", 2, 3 })
			);
		}

		[Test]
		public void TestCall()
		{
			Assert.AreEqual("Hello World", DynamicUtils.Call(typeof(String), "Format", "Hello {0}", "World"));
		}

		sealed class ClassTestAccess
		{
			public int SampleField = 10;

			public int SampleProperty {
				get
				{
					return 20;
				}
			}

			public int SampleMethod()
			{
				return 30;
			}
		}
	}
}
