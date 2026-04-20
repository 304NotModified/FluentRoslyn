using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace FluentRoslyn.Tests
{
    public class MethodSymbolExtensionsTests
    {
        private const string SampleCode = """
            using System;
            using System.ComponentModel;

            public class MyClass
            {
                [Description("Does something")]
                public void DoSomething() {}

                public int GetValue() { return 42; }

                private void InternalMethod() {}

                public static void StaticMethod() {}

                [Obsolete("Old method")]
                public virtual void VirtualMethod() {}
            }
            """;

        private static IMethodSymbol GetMethod(string typeName, string methodName)
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, typeName);
            return type.GetMembers(methodName).OfType<IMethodSymbol>().First();
        }

        [Fact]
        public void IsDefined_WhenAttributePresent_ReturnsTrue()
        {
            var method = GetMethod("MyClass", "DoSomething");

            Assert.True(method.IsDefined<System.ComponentModel.DescriptionAttribute>());
        }

        [Fact]
        public void IsDefined_WhenAttributeAbsent_ReturnsFalse()
        {
            var method = GetMethod("MyClass", "GetValue");

            Assert.False(method.IsDefined<System.ComponentModel.DescriptionAttribute>());
        }

        [Fact]
        public void GetAttribute_WhenAttributePresent_ReturnsAttributeData()
        {
            var method = GetMethod("MyClass", "DoSomething");

            var attr = method.GetAttribute<System.ComponentModel.DescriptionAttribute>();

            Assert.NotNull(attr);
        }

        [Fact]
        public void GetAttribute_WhenAttributeAbsent_ReturnsNull()
        {
            var method = GetMethod("MyClass", "GetValue");

            var attr = method.GetAttribute<System.ComponentModel.DescriptionAttribute>();

            Assert.Null(attr);
        }

        [Fact]
        public void HasReturnType_Void_ReturnsTrue()
        {
            var method = GetMethod("MyClass", "DoSomething");

            Assert.True(method.HasReturnType(SpecialType.System_Void));
        }

        [Fact]
        public void HasReturnType_Int_ReturnsTrue()
        {
            var method = GetMethod("MyClass", "GetValue");

            Assert.True(method.HasReturnType(SpecialType.System_Int32));
        }

        [Fact]
        public void HasReturnType_WrongType_ReturnsFalse()
        {
            var method = GetMethod("MyClass", "GetValue");

            Assert.False(method.HasReturnType(SpecialType.System_String));
        }

        [Fact]
        public void IsPublicInstance_ForPublicInstanceMethod_ReturnsTrue()
        {
            var method = GetMethod("MyClass", "DoSomething");

            Assert.True(method.IsPublicInstance());
        }

        [Fact]
        public void IsPublicInstance_ForPrivateMethod_ReturnsFalse()
        {
            var method = GetMethod("MyClass", "InternalMethod");

            Assert.False(method.IsPublicInstance());
        }

        [Fact]
        public void IsPublicInstance_ForStaticMethod_ReturnsFalse()
        {
            var method = GetMethod("MyClass", "StaticMethod");

            Assert.False(method.IsPublicInstance());
        }
    }
}
