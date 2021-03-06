﻿using System.Collections.Generic;
using CSharpUtils.Extensions;
using CSharpUtils.Getopt;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class GetoptTest
    {
        [Fact]
        public void AddRuleTest()
        {
            bool BooleanValue = false;
            int IntegerValue = 0;
            var Getopt = new Getopt(new string[] {"-b", "-i", "50"});
            Getopt.AddRule("-b", ref BooleanValue);
            Getopt.AddRule("-i", ref IntegerValue);
            Getopt.Process();

            Assert.Equal(true, BooleanValue);
            Assert.Equal(50, IntegerValue);
        }

        [Fact]
        public void AddRule2Test()
        {
            bool BooleanValue = false;
            int IntegerValue = 0;
            string StringValue = "";
            var Getopt = new Getopt(new string[] {"-b", "-i", "50", "-s", "hello_world"});
            Getopt.AddRule("-b", (bool _Value) => { BooleanValue = _Value; });
            Getopt.AddRule("-i", (int _Value) => { IntegerValue = _Value; });
            Getopt.AddRule("-s", (string _Value) => { StringValue = _Value; });
            Getopt.Process();

            Assert.Equal(true, BooleanValue);
            Assert.Equal(50, IntegerValue);
            Assert.Equal("hello_world", StringValue);
        }

        [Fact]
        public void AddRule3Test()
        {
            var Values = new List<int>();
            var Getopt = new Getopt(new string[] {"-i=50", "-i=25"});
            Getopt.AddRule("-i", (int Value) => { Values.Add(Value); });
            Getopt.Process();
            Assert.Equal("50,25", Values.ToStringArray());
        }

        [Fact]
        public void AddRule4Test()
        {
            int ExecutedCount = 0;
            var Getopt = new Getopt(new string[] {"-a", "-a"});
            Getopt.AddRule("-a", () => { ExecutedCount++; });
            Getopt.Process();
            Assert.Equal(2, ExecutedCount);
        }
    }
}