namespace PropertyChangedAnalyzers
{
    using System;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Enables compilation against older versions of Roslyn while accessing features from newer versions.
    /// </summary>
    internal static class RoslynVersionSupportExtensions
    {
        private static readonly Lazy<Func<SemanticModel, int, int>?> GetNullableContext = new Lazy<Func<SemanticModel, int, int>?>(
            () => typeof(SemanticModel).GetInstanceMethodDelegate<Func<SemanticModel, int, int>>("GetNullableContext"));

        /// <summary>
        /// Evaluates <c>semanticModel.GetNullableContext(position).AnnotationsEnabled()</c> if the loaded version of
        /// Roslyn contains these APIs. Otherwise, returns <see langword="false"/> since the loaded version of Roslyn
        /// does not support C# 8.
        /// </summary>
        internal static bool NullableAnnotationsEnabled(this SemanticModel semanticModel, int position)
        {
            if (GetNullableContext.Value is { } getNullableContext)
            {
                var nullableContext = getNullableContext.Invoke(semanticModel, position);

                // Roslyn's public flags enum value definition
                const int AnnotationsEnabled = 1 << 1;

                return (nullableContext & AnnotationsEnabled) != 0;
            }

            return false;
        }

        private static T? GetInstanceMethodDelegate<T>(this Type instanceType, string methodName)
            where T : Delegate
        {
            var invokeMethod = typeof(T).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new ArgumentException("The delegate type must have an Invoke method.");

            var parametersIncludingInstance = invokeMethod.GetParameters();

            var methodParameterTypes = new Type[parametersIncludingInstance.Length - 1];

            for (var i = 0; i < methodParameterTypes.Length; i++)
            {
                methodParameterTypes[i] = parametersIncludingInstance[i + 1].ParameterType;
            }

            return instanceType
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, methodParameterTypes, null)
                ?.CreateDelegate<T>();
        }

        private static T CreateDelegate<T>(this MethodInfo method)
            where T : Delegate
        {
            return (T)method.CreateDelegate(typeof(T));
        }
    }
}
