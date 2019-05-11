

using Xunit;

namespace Tests.CSPspEmu.Inject
{
    
    public class InjectTest
    {
        public class Test : IInjectInitialize
        {
            [Inject]
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            protected InjectContext _InjectContext1 { get; private set; }

            // ReSharper disable once InconsistentNaming
            protected InjectContext _InjectContext2;
            // ReSharper disable once InconsistentNaming
            protected InjectContext _InjectContext3;

            public InjectContext InjectContext1 => _InjectContext1;
            public InjectContext InjectContext2 => _InjectContext2;
            public InjectContext InjectContext3 => _InjectContext3;

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
            // ReSharper disable once InconsistentNaming
            [Inject] protected InjectContext _InjectContext1;
        }

        public class Test3 : Test2
        {
            // ReSharper disable once InconsistentNaming
            [Inject] protected InjectContext _injectContext2;

            public InjectContext InjectContext1 => _InjectContext1;

            public InjectContext InjectContext2 => _injectContext2;
        }

        [Fact]
        public void TestInjection()
        {
            var context = new InjectContext();
            var test = context.GetInstance<Test>();
            Assert.Equal(context, test.InjectContext1);
            Assert.Equal(context, test.InjectContext2);
            Assert.Equal(null, test.InjectContext3);
        }

        [Fact]
        public void TestInjectionExtended()
        {
            var context = new InjectContext();
            var test = context.GetInstance<Test3>();
            Assert.Equal(context, test.InjectContext1);
            Assert.Equal(context, test.InjectContext2);
        }
    }
}