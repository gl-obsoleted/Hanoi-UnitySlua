using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace SLua.Test
{
    [TestFixture]
    public class TestSLua
    {
        private LuaSvr _luaSvr;

        [SetUp]
        public void Init()
        {
            _luaSvr = new LuaSvr();
            _luaSvr.init(x => { }, () => { });

        }
        [Test]
        public void LuaState()
        {
            var luaState = new LuaState();
            var obj = luaState.doString("print 'abc';return 123");
            Assert.AreEqual(obj, 123);
        }

        static readonly LuaSvr CacheLuaSvr = new LuaSvr();

        [Test]
        public void LuaSvr()
        {
            var luaSvr = CacheLuaSvr;
            luaSvr.init((x) => { }, () => { });

            GC.Collect();

            var code = @"
local testVar = 12345;
return testVar;
";
            object ret;
            luaSvr.luaState.doBuffer(Encoding.UTF8.GetBytes(code), "TestLua", out ret);
            Assert.AreEqual(ret, 12345);
        }

        [Test]
        public void SLuaLib()
        {
            var code = @"
local TestSLua = Slua.GetClass('SLua.Test.TestSLua')
return TestSLua
";
            var ret = _luaSvr.luaState.doString(code);
            Assert.AreEqual("SLua.LuaClassObject", ret.GetType().ToString());

            var clsField = ret.GetType().GetField("cls", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(clsField);
            var cls = clsField.GetValue(ret);
            Assert.AreEqual(typeof(TestSLua), cls);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        static float DoMethod(int a1)
        {
            Assert.AreEqual(123, a1);
            return (float) a1;
        }

        [Test]
        public void DoMethod()
        {
            var code = @"
    local TestSLua = Slua.GetClass('SLua.Test.TestSLua')
    return TestSLua.DoMethod(123)
";
            var ret = _luaSvr.luaState.doString(code);
            Assert.AreEqual(123, ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static ulong DoMethodLong(long a1, ulong a2)
        {
            Assert.AreEqual(123, a1);
            return a2;
        }

        [Test]
        public void DoMethodLong()
        {
            var code = @"
    local TestSLua = Slua.GetClass('SLua.Test.TestSLua')
    return TestSLua.DoMethodLong(123, 321)
";
            var ret = _luaSvr.luaState.doString(code);
            Assert.AreEqual(321, ret);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        static decimal DoMethodDecimal(decimal a1, decimal a2)
        {
            Assert.AreEqual(123, a1);
            return a2;
        }

        [Test]
        public void DoMethodDecimal()
        {
            var code = @"
    local TestSLua = Slua.GetClass('SLua.Test.TestSLua')
    return TestSLua.DoMethodDecimal(123, 321)
";
            var ret = _luaSvr.luaState.doString(code);
            Assert.AreEqual(321, ret);
        }


        [Test]
        public void UTF8Bom()
        {
            var bytes = Encoding.UTF8.GetBytes("return 1");

            var bomBytes = new byte[bytes.Length + 3];
            Array.Copy(bytes, 0, bomBytes, 3, bytes.Length);

            bomBytes[0] = 0xEF;
            bomBytes[1] = 0xBB;
            bomBytes[2] = 0xBF;



            object ret;
            Assert.IsTrue(_luaSvr.luaState.doBuffer(bomBytes, "TestUtf8Bom", out ret));
            Assert.AreEqual(1, ret);

            // Test BOM filter
            var noBomBytes = SLua.LuaState.CleanUTF8Bom(bomBytes);
            Assert.AreNotEqual(noBomBytes[0], 0xEF);
            Assert.AreNotEqual(noBomBytes[0], 0xBB);
            Assert.AreNotEqual(noBomBytes[0], 0xBF);
        }
    }
}