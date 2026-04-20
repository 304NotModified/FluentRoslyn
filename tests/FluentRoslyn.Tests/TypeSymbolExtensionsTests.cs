using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit;

namespace FluentRoslyn.Tests
{
    public class TypeSymbolExtensionsTests
    {
        private const string SampleCode = """
            using System;
            using System.ComponentModel;

            [Description("My class")]
            public class MyClass
            {
                public string Name { get; set; }
                public int Age { get; set; }
                private int _age;
                public static int Counter;
                public void DoSomething() {}
                private void InternalMethod() {}
                public static void StaticMethod() {}
            }

            public abstract class AbstractBase { }
            public interface IMyInterface { }
            public class Derived : AbstractBase, IMyInterface { }
            public sealed class SealedClass { }
            public static class StaticClass { }
            """;

        [Fact]
        public void GetProperties_PublicInstance_ReturnsPublicInstanceProperties()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var properties = type.GetProperties().ToList();

            Assert.Equal(2, properties.Count);
            Assert.Contains(properties, p => p.Name == "Name");
            Assert.Contains(properties, p => p.Name == "Age");
        }

        [Fact]
        public void GetProperties_PublicAndNonPublic_ReturnsAll()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();

            Assert.Equal(2, properties.Count);
        }

        [Fact]
        public void GetMethods_PublicInstance_ReturnsOnlyPublicInstanceOrdinaryMethods()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var methods = type.GetMethods().ToList();

            Assert.Single(methods);
            Assert.Equal("DoSomething", methods[0].Name);
        }

        [Fact]
        public void GetMethods_Static_ReturnsOnlyStaticMethods()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

            Assert.Single(methods);
            Assert.Equal("StaticMethod", methods[0].Name);
        }

        [Fact]
        public void GetMethods_AllPublic_ReturnsBothInstanceAndStatic()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).ToList();

            Assert.Equal(2, methods.Count);
        }

        [Fact]
        public void GetFields_PublicInstance_ReturnsPublicInstanceFields()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var fields = type.GetFields().ToList();

            Assert.Empty(fields);
        }

        [Fact]
        public void GetFields_PublicStatic_ReturnsPublicStaticFields()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static).ToList();

            Assert.Single(fields);
            Assert.Equal("Counter", fields[0].Name);
        }

        [Fact]
        public void GetFields_NonPublicInstance_ReturnsPrivateInstanceFields()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList();

            // Includes _age plus the compiler-generated backing fields for auto-properties
            // (mirrors System.Reflection behaviour which also returns backing fields)
            Assert.Contains(fields, f => f.Name == "_age");
            Assert.True(fields.Count >= 1);
        }

        [Fact]
        public void GetInterfaces_ReturnsImplementedInterfaces()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "Derived");

            var interfaces = type.GetInterfaces().ToList();

            Assert.Single(interfaces);
            Assert.Equal("IMyInterface", interfaces[0].Name);
        }

        [Fact]
        public void IsSubclassOf_WithDirectBaseClass_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "Derived");

            Assert.True(type.IsSubclassOf("AbstractBase"));
        }

        [Fact]
        public void IsSubclassOf_WithUnrelatedType_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            Assert.False(type.IsSubclassOf("AbstractBase"));
        }

        [Fact]
        public void IsDefined_Generic_WhenAttributePresent_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            Assert.True(type.IsDefined<DescriptionAttribute>());
        }

        [Fact]
        public void IsDefined_Generic_WhenAttributeAbsent_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "Derived");

            Assert.False(type.IsDefined<DescriptionAttribute>());
        }

        [Fact]
        public void IsDefined_ByName_WhenAttributePresent_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            Assert.True(type.IsDefined("System.ComponentModel.DescriptionAttribute"));
        }

        [Fact]
        public void GetAttribute_Generic_WhenAttributePresent_ReturnsAttributeData()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            var attr = type.GetAttribute<DescriptionAttribute>();

            Assert.NotNull(attr);
        }

        [Fact]
        public void GetAttribute_Generic_WhenAttributeAbsent_ReturnsNull()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "Derived");

            var attr = type.GetAttribute<DescriptionAttribute>();

            Assert.Null(attr);
        }

        [Fact]
        public void GetAttributes_Generic_ReturnsAllMatchingAttributes()
        {
            var code = """
                using System.ComponentModel;
                [Description("first")]
                [Description("second")]
                public class Multi { }
                """;
            var compilation = TestHelper.CreateCompilation(code);
            var type = TestHelper.GetTypeSymbol(compilation, "Multi");

            var attrs = type.GetAttributes<DescriptionAttribute>().ToList();

            Assert.Equal(2, attrs.Count);
        }

        [Fact]
        public void IsConcreteClass_ForRegularClass_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "MyClass");

            Assert.True(type.IsConcreteClass());
        }

        [Fact]
        public void IsConcreteClass_ForAbstractClass_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "AbstractBase");

            Assert.False(type.IsConcreteClass());
        }

        [Fact]
        public void IsConcreteClass_ForInterface_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "IMyInterface");

            Assert.False(type.IsConcreteClass());
        }

        [Fact]
        public void IsConcreteClass_ForStaticClass_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "StaticClass");

            Assert.False(type.IsConcreteClass());
        }

        [Fact]
        public void IsConcreteClass_ForSealedClass_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "SealedClass");

            Assert.True(type.IsConcreteClass());
        }
    }
}
