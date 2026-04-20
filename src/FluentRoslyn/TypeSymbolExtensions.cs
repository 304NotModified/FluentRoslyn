using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace FluentRoslyn
{
    /// <summary>
    /// Extension methods on <see cref="INamedTypeSymbol"/> that mirror the <see cref="System.Type"/> reflection API.
    /// </summary>
    public static class TypeSymbolExtensions
    {
        /// <summary>
        /// Returns the properties of this type, similar to <see cref="Type.GetProperties(BindingFlags)"/>.
        /// </summary>
        public static IEnumerable<IPropertySymbol> GetProperties(
            this INamedTypeSymbol type,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => MatchesBindingFlags(p.DeclaredAccessibility, p.IsStatic, flags));
        }

        /// <summary>
        /// Returns the ordinary methods of this type, similar to <see cref="Type.GetMethods(BindingFlags)"/>.
        /// Excludes property accessors, event accessors, and constructors.
        /// </summary>
        public static IEnumerable<IMethodSymbol> GetMethods(
            this INamedTypeSymbol type,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary)
                .Where(m => MatchesBindingFlags(m.DeclaredAccessibility, m.IsStatic, flags));
        }

        /// <summary>
        /// Returns the fields of this type, similar to <see cref="Type.GetFields(BindingFlags)"/>.
        /// </summary>
        public static IEnumerable<IFieldSymbol> GetFields(
            this INamedTypeSymbol type,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => MatchesBindingFlags(f.DeclaredAccessibility, f.IsStatic, flags));
        }

        /// <summary>
        /// Returns the interfaces implemented by this type, similar to <see cref="Type.GetInterfaces()"/>.
        /// </summary>
        public static IEnumerable<INamedTypeSymbol> GetInterfaces(this INamedTypeSymbol type)
        {
            return type.Interfaces;
        }

        /// <summary>
        /// Determines whether this type inherits from the type with the given full name,
        /// similar to <see cref="Type.IsSubclassOf(Type)"/>.
        /// </summary>
        public static bool IsSubclassOf(this INamedTypeSymbol type, string fullName)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.ToDisplayString() == fullName)
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Determines whether an attribute of type <typeparamref name="T"/> is applied to this type,
        /// similar to <see cref="Attribute.IsDefined(System.Reflection.MemberInfo, Type)"/>.
        /// </summary>
        public static bool IsDefined<T>(this INamedTypeSymbol type)
            where T : Attribute
        {
            return type.IsDefined(typeof(T).FullName!.Replace('+', '.'));
        }

        /// <summary>
        /// Determines whether an attribute with the given full type name is applied to this type.
        /// </summary>
        public static bool IsDefined(this INamedTypeSymbol type, string fullAttributeName)
        {
            return type.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == fullAttributeName);
        }

        /// <summary>
        /// Returns the first attribute of type <typeparamref name="T"/> applied to this type,
        /// similar to <see cref="Attribute.GetCustomAttribute(System.Reflection.MemberInfo, Type)"/>.
        /// Returns <c>null</c> if no such attribute exists.
        /// </summary>
        public static AttributeData? GetAttribute<T>(this INamedTypeSymbol type)
            where T : Attribute
        {
            var fullName = typeof(T).FullName!.Replace('+', '.');
            return type.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == fullName);
        }

        /// <summary>
        /// Returns all attributes of type <typeparamref name="T"/> applied to this type,
        /// similar to <see cref="Attribute.GetCustomAttributes(System.Reflection.MemberInfo, Type)"/>.
        /// </summary>
        public static IEnumerable<AttributeData> GetAttributes<T>(this INamedTypeSymbol type)
            where T : Attribute
        {
            var fullName = typeof(T).FullName!.Replace('+', '.');
            return type.GetAttributes()
                .Where(a => a.AttributeClass?.ToDisplayString() == fullName);
        }

        /// <summary>
        /// Determines whether this type is a concrete class:
        /// not abstract, not an interface, not static.
        /// </summary>
        public static bool IsConcreteClass(this INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Class
                && !type.IsAbstract
                && !type.IsStatic;
        }

        private static bool MatchesBindingFlags(Accessibility accessibility, bool isStatic, BindingFlags flags)
        {
            bool isPublic = accessibility == Accessibility.Public;
            bool wantsPublic = (flags & BindingFlags.Public) != 0;
            bool wantsNonPublic = (flags & BindingFlags.NonPublic) != 0;

            if (!wantsPublic && !wantsNonPublic)
                return false;
            if (wantsPublic && !wantsNonPublic && !isPublic)
                return false;
            if (wantsNonPublic && !wantsPublic && isPublic)
                return false;

            bool wantsStatic = (flags & BindingFlags.Static) != 0;
            bool wantsInstance = (flags & BindingFlags.Instance) != 0;

            if (!wantsStatic && !wantsInstance)
                return false;
            if (wantsStatic && !wantsInstance && !isStatic)
                return false;
            if (wantsInstance && !wantsStatic && isStatic)
                return false;

            return true;
        }
    }
}
