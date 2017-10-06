namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    internal class Dump
    {
        [Test]
        public void CollectionTypes()
        {
            foreach (var type in typeof(SeparatedSyntaxList<>).Assembly
                                                              .GetExportedTypes().Where(x => x.IsValueType && x.IsPublic)
                                                              .Where(x => typeof(IEnumerable).IsAssignableFrom(x)))
            {
                if (type.IsGenericType)
                {
                    Console.WriteLine($"{{ \"{type.Name.Split('`')[0]}<T>\", \"T\" }},");
                }
                else
                {
                    var @interface = type.GetInterface("IEnumerable`1");
                    Console.WriteLine($"{{ \"{type.Name}\", \"{@interface.GenericTypeArguments[0].Name}\" }},");
                }
            }

            ////        var typeNames = new Dictionary<string, string>
            ////        {
            ////            { "IReadOnlyList<T>", "T" },
            ////            { "ImmutableArray<T>", "T" },
            ////            { "ChildSyntaxList", "SyntaxNodeOrToken" },
            ////            { "SeparatedSyntaxList<T>", "T" },
            ////            { "SyntaxList<T>", "T" },
            ////            { "SyntaxNodeOrTokenList", "SyntaxNodeOrToken" },
            ////            { "SyntaxTokenList", "SyntaxToken" },
            ////            { "SyntaxTriviaList", "SyntaxTrivia" },
            ////        };
        }
    }
}