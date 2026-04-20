using System.ComponentModel;
using Xunit;

namespace FluentRoslyn.Tests
{
    public class AttributeDataExtensionsTests
    {
        private const string SampleCode = """
            using System;
            using System.ComponentModel;

            [Description("My description")]
            public class WithDescription { }

            [Obsolete("Use NewMethod instead", true)]
            public class WithObsolete { }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
            public class MyCustomAttribute : System.Attribute
            {
                public MyCustomAttribute(string name) { }
                public int Priority { get; set; }
            }

            [MyCustom("First", Priority = 10)]
            [MyCustom("Second", Priority = 20)]
            public class WithCustom { }
            """;

        [Fact]
        public void GetConstructorArgument_FirstArg_ReturnsValue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            var value = attr!.GetConstructorArgument<string>(0);

            Assert.Equal("My description", value);
        }

        [Fact]
        public void GetConstructorArgument_OutOfRange_ReturnsDefault()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            var value = attr!.GetConstructorArgument<string>(99);

            Assert.Null(value);
        }

        [Fact]
        public void GetConstructorArgument_Bool_ReturnsCorrectValue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithObsolete");
            var attr = type.GetAttribute<System.ObsoleteAttribute>();

            var isError = attr!.GetConstructorArgument<bool>(1);

            Assert.True(isError);
        }

        [Fact]
        public void GetNamedArgument_WhenPresent_ReturnsValue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithCustom");

            // Use Roslyn's built-in GetAttributes() to retrieve the attribute data,
            // then assert on GetNamedArgument which is what we're testing.
            var attrs = type.GetAttributes();
            Assert.Equal(2, attrs.Length);

            var first = attrs[0];
            var priority = first.GetNamedArgument<int>("Priority");

            Assert.Equal(10, priority);
        }

        [Fact]
        public void GetNamedArgument_WhenAbsent_ReturnsDefault()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            var missing = attr!.GetNamedArgument<string>("NonExistent");

            Assert.Null(missing);
        }

        [Fact]
        public void IsOfType_Generic_WhenMatch_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            Assert.True(attr!.IsOfType<DescriptionAttribute>());
        }

        [Fact]
        public void IsOfType_Generic_WhenNoMatch_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            Assert.False(attr!.IsOfType<System.ObsoleteAttribute>());
        }

        [Fact]
        public void IsOfType_ByName_WhenMatch_ReturnsTrue()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            Assert.True(attr!.IsOfType("System.ComponentModel.DescriptionAttribute"));
        }

        [Fact]
        public void IsOfType_ByName_WhenNoMatch_ReturnsFalse()
        {
            var compilation = TestHelper.CreateCompilation(SampleCode);
            var type = TestHelper.GetTypeSymbol(compilation, "WithDescription");
            var attr = type.GetAttribute<DescriptionAttribute>();

            Assert.False(attr!.IsOfType("System.ObsoleteAttribute"));
        }
    }

    // Helper attribute used in tests — visible to the test compilation at runtime
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class MyCustomAttribute : System.Attribute
    {
        public MyCustomAttribute(string name) { }
        public int Priority { get; set; }
    }
}
