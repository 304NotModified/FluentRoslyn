using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace FluentRoslyn
{
    /// <summary>
    /// Extension methods on <see cref="IMethodSymbol"/> that mirror the <see cref="System.Reflection.MethodInfo"/> API.
    /// </summary>
    public static class MethodSymbolExtensions
    {
        /// <summary>
        /// Determines whether an attribute of type <typeparamref name="T"/> is applied to this method,
        /// similar to <see cref="Attribute.IsDefined(System.Reflection.MemberInfo, Type)"/>.
        /// </summary>
        public static bool IsDefined<T>(this IMethodSymbol method)
            where T : Attribute
        {
            var fullName = typeof(T).FullName!;
            return method.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == fullName);
        }

        /// <summary>
        /// Returns the first attribute of type <typeparamref name="T"/> applied to this method,
        /// similar to <see cref="Attribute.GetCustomAttribute(System.Reflection.MemberInfo, Type)"/>.
        /// Returns <c>null</c> if no such attribute exists.
        /// </summary>
        public static AttributeData? GetAttribute<T>(this IMethodSymbol method)
            where T : Attribute
        {
            var fullName = typeof(T).FullName!;
            return method.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == fullName);
        }

        /// <summary>
        /// Determines whether this method's return type matches the given <see cref="SpecialType"/>.
        /// </summary>
        public static bool HasReturnType(this IMethodSymbol method, SpecialType specialType)
        {
            return method.ReturnType.SpecialType == specialType;
        }

        /// <summary>
        /// Determines whether this method is a public, non-static (instance) method.
        /// </summary>
        public static bool IsPublicInstance(this IMethodSymbol method)
        {
            return method.DeclaredAccessibility == Accessibility.Public
                && !method.IsStatic;
        }
    }
}
