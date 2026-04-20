using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FluentRoslyn
{
    /// <summary>
    /// Extension methods on <see cref="AttributeData"/> for convenient argument extraction.
    /// </summary>
    public static class AttributeDataExtensions
    {
        /// <summary>
        /// Gets the value of a named argument (property or field initializer) by name.
        /// Returns <c>default</c> if the argument is not present.
        /// </summary>
        public static T? GetNamedArgument<T>(this AttributeData attribute, string name)
        {
            foreach (var kvp in attribute.NamedArguments)
            {
                if (kvp.Key == name)
                    return (T?)kvp.Value.Value;
            }
            return default;
        }

        /// <summary>
        /// Gets the value of a positional constructor argument by zero-based index.
        /// Returns <c>default</c> if the index is out of range.
        /// </summary>
        public static T? GetConstructorArgument<T>(this AttributeData attribute, int index)
        {
            if (index < 0 || index >= attribute.ConstructorArguments.Length)
                return default;
            return (T?)attribute.ConstructorArguments[index].Value;
        }

        /// <summary>
        /// Determines whether this attribute is of type <typeparamref name="T"/>.
        /// </summary>
        public static bool IsOfType<T>(this AttributeData attribute)
            where T : Attribute
        {
            return attribute.IsOfType(typeof(T).FullName!.Replace('+', '.'));
        }

        /// <summary>
        /// Determines whether this attribute's class has the given full type name.
        /// </summary>
        public static bool IsOfType(this AttributeData attribute, string fullName)
        {
            return attribute.AttributeClass?.ToDisplayString() == fullName;
        }
    }
}
