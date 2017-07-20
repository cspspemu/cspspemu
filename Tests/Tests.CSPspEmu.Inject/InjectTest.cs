using NUnit.Framework;

namespace Tests.CSPspEmu.Inject
{
    [TestFixture]
    public class InjectTest
    {
        public class Test : IInjectInitialize
        {
            [Inject]
            protected InjectContext _InjectContext1 { get; private set; }

            protected InjectContext _InjectContext2;
            protected InjectContext _InjectContext3;

            public InjectContext InjectContext1
            {
                get { return _InjectContext1; }
            }

            public InjectContext InjectContext2
            {
                get { return _InjectContext2; }
            }

            public InjectContext InjectContext3
            {
                get { return _InjectContext3; }
            }

            private Test()
            {
            }

            void IInjectInitialize.Initialize()
            {
                _InjectContext2 = _InjectContext1;
            }
        }

        public class Test2
        {
            [Inject] protected InjectContext _InjectContext1;
        }

        public class Test3 : Test2
        {
            [Inject] InjectContext _InjectContext2;

            public InjectContext InjectContext1
            {
                get { return _InjectContext1; }
            }

            public InjectContext InjectContext2
            {
                get { return _InjectContext2; }
            }
        }

        [Test]
        public void TestInjection()
        {
            var Context = new InjectContext();
            var Test = Context.GetInstance<Test>();
            Assert.AreEqual(Context, Test.InjectContext1);
            Assert.AreEqual(Context, Test.InjectContext2);
            Assert.AreEqual(null, Test.InjectContext3);
        }

        [Test]
        public void TestInjectionExtended()
        {
            var Context = new InjectContext();
            var Test = Context.GetInstance<Test3>();
            Assert.AreEqual(Context, Test.InjectContext1);
            Assert.AreEqual(Context, Test.InjectContext2);
        }
    }
}