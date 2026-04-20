# FluentRoslyn

A library that provides a familiar reflection-like API for writing Roslyn analyzers and source generators. If you know the `System.Reflection` API, you already know FluentRoslyn.

## What is FluentRoslyn?

FluentRoslyn is a thin extension-method library over Microsoft's Roslyn API that lets you query `INamedTypeSymbol`, `IMethodSymbol`, and `AttributeData` using the same vocabulary you already know from `System.Type`, `System.Reflection.MethodInfo`, and `System.Attribute`. Instead of learning the verbose Roslyn equivalents from scratch, you get a concise, discoverable API right in IntelliSense.

## Why?

Writing a Roslyn analyzer forces you to learn a completely different API for questions you already know how to ask with reflection:

**Before (raw Roslyn API):**
```csharp
// Is this type decorated with [Obsolete]?
var hasObsolete = namedType.GetAttributes()
    .Any(a => a.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");

// Get all public instance methods (excluding property accessors etc.)
var methods = namedType.GetMembers()
    .OfType<IMethodSymbol>()
    .Where(m => m.MethodKind == MethodKind.Ordinary
             && m.DeclaredAccessibility == Accessibility.Public
             && !m.IsStatic);

// Read a named argument from an attribute
var message = attr.NamedArguments
    .FirstOrDefault(a => a.Key == "Message").Value.Value as string;
```

**After (FluentRoslyn):**
```csharp
// Is this type decorated with [Obsolete]?
var hasObsolete = namedType.IsDefined<ObsoleteAttribute>();

// Get all public instance methods
var methods = namedType.GetMethods(); // default: Public | Instance

// Read a named argument from an attribute
var message = attr.GetNamedArgument<string>("Message");
```

## Installation

```
dotnet add package FluentRoslyn
```

## Examples

### 1. Check whether a type has an attribute

```csharp
using FluentRoslyn;

// By generic type (requires the attribute assembly to be loaded)
bool isObsolete = namedType.IsDefined<ObsoleteAttribute>();

// By full type name (safe from any analyzer context)
bool isObsolete = namedType.IsDefined("System.ObsoleteAttribute");
```

### 2. Retrieve properties, methods, and fields

```csharp
using System.Reflection;
using FluentRoslyn;

// Public instance properties (mirrors Type.GetProperties())
var props = namedType.GetProperties();

// All properties including static and non-public
var allProps = namedType.GetProperties(
    BindingFlags.Public | BindingFlags.NonPublic |
    BindingFlags.Instance | BindingFlags.Static);

// Public instance methods (excludes property/event accessors)
var methods = namedType.GetMethods();

// Private instance fields
var fields = namedType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
```

### 3. Walk the inheritance hierarchy

```csharp
using FluentRoslyn;

// Mirrors Type.IsSubclassOf()
bool isException = namedType.IsSubclassOf("System.Exception");

// Get directly implemented interfaces (mirrors Type.GetInterfaces())
var interfaces = namedType.GetInterfaces();
```

### 4. Read attribute arguments

```csharp
using FluentRoslyn;

var attr = namedType.GetAttribute<DescriptionAttribute>();
if (attr != null)
{
    // Positional (constructor) argument at index 0
    string? description = attr.GetConstructorArgument<string>(0);

    // Named argument (property/field initializer)
    int? order = attr.GetNamedArgument<int>("Order");

    // Type check
    bool isDescription = attr.IsOfType<DescriptionAttribute>();
}
```

### 5. Use helpers on methods

```csharp
using FluentRoslyn;

foreach (var method in namedType.GetMethods())
{
    if (method.IsPublicInstance() && method.HasReturnType(SpecialType.System_Void))
    {
        // public void Foo() { ... }
    }

    if (method.IsDefined<ObsoleteAttribute>())
    {
        var attr = method.GetAttribute<ObsoleteAttribute>();
        string? message = attr?.GetConstructorArgument<string>(0);
    }
}
```

### 6. Check for concrete classes

```csharp
using FluentRoslyn;

// True only if the type is a class that is NOT abstract, NOT static, NOT an interface
if (namedType.IsConcreteClass())
{
    // safe to create an instance
}
```

## Supported APIs

| `System.Reflection` | FluentRoslyn |
|---|---|
| `Type.GetProperties(BindingFlags)` | `INamedTypeSymbol.GetProperties(BindingFlags)` |
| `Type.GetMethods(BindingFlags)` | `INamedTypeSymbol.GetMethods(BindingFlags)` |
| `Type.GetFields(BindingFlags)` | `INamedTypeSymbol.GetFields(BindingFlags)` |
| `Type.GetInterfaces()` | `INamedTypeSymbol.GetInterfaces()` |
| `Type.IsSubclassOf(Type)` | `INamedTypeSymbol.IsSubclassOf(string)` |
| `Attribute.IsDefined(MemberInfo, Type)` | `INamedTypeSymbol.IsDefined<T>()` |
| `Attribute.IsDefined(MemberInfo, Type)` | `INamedTypeSymbol.IsDefined(string)` |
| `Attribute.GetCustomAttribute<T>(MemberInfo)` | `INamedTypeSymbol.GetAttribute<T>()` |
| `Attribute.GetCustomAttributes<T>(MemberInfo)` | `INamedTypeSymbol.GetAttributes<T>()` |
| `MethodInfo.GetCustomAttribute<T>()` | `IMethodSymbol.GetAttribute<T>()` |
| `MethodInfo.IsDefined(Type, bool)` | `IMethodSymbol.IsDefined<T>()` |
| `MethodInfo.ReturnType == typeof(void)` | `IMethodSymbol.HasReturnType(SpecialType)` |
| `!method.IsStatic && method.IsPublic` | `IMethodSymbol.IsPublicInstance()` |
| — (no direct equivalent) | `INamedTypeSymbol.IsConcreteClass()` |
| `attr.GetType().FullName` | `AttributeData.IsOfType<T>()` / `IsOfType(string)` |
| Named argument via `CustomAttributeNamedArgument` | `AttributeData.GetNamedArgument<T>(string)` |
| Constructor argument via `CustomAttributeTypedArgument` | `AttributeData.GetConstructorArgument<T>(int)` |

## License

MIT — see [LICENSE](LICENSE).
