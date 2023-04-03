namespace PropertyChangedAnalyzers.Test.Helpers;

using System.Threading;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public static partial class PropertyChangedTest
{
    public static class InvokesPropertyChangedFor
    {
        [TestCase("_p1 = value", "P1")]
        [TestCase("_p2 = value", "P2")]
        public static void Assignment(string signature, string propertyName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p1;
        private int _p2;
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get
            {
                return _p1;
            }

            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int P2
        {
            get
            {
                return _p2;
            }

            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged(() => this.P2);
            }
        }

        public int P3
        {
            get { return _p3; }
            set { TrySet(ref _p3, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.Find<ExpressionSyntax>(signature);
            var property = semanticModel.GetDeclaredSymbol(syntaxTree.FindPropertyDeclaration(propertyName));
            Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.InvokesPropertyChangedFor(node, property, semanticModel, CancellationToken.None));
        }

        [TestCase("this.TrySet(ref this.p, value)", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, \"P\")", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, nameof(this.P))", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, nameof(P))", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, null)", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, string.Empty)", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, \"Wrong\")", AnalysisResult.No)]
        public static void TrySetCallerMemberName(string trySetCode, AnalysisResult expected)
        {
            var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.TrySet(ref this.p, value)", trySetCode);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindArgument("ref this.p");
            var property = semanticModel.GetDeclaredSymbol(syntaxTree.FindPropertyDeclaration("P"));
            Assert.AreEqual(expected, PropertyChanged.InvokesPropertyChangedFor(node.Expression, property, semanticModel, CancellationToken.None));
        }

        [TestCase("this.TrySet(ref this.p, value, \"P\")", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, nameof(P))", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, nameof(this.P))", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, null)", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, string.Empty)", AnalysisResult.Yes)]
        [TestCase("this.TrySet(ref this.p, value, \"Wrong\")", AnalysisResult.No)]
        public static void TrySet(string trySetCode, AnalysisResult expected)
        {
            var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value, nameof(P));
        }

        protected bool TrySet<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.TrySet(ref this.p, value, nameof(P))", trySetCode);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindArgument("ref this.p");
            var property = semanticModel.GetDeclaredSymbol(syntaxTree.FindPropertyDeclaration("P"));
            Assert.AreEqual(expected, PropertyChanged.InvokesPropertyChangedFor(node.Expression, property, semanticModel, CancellationToken.None));
        }

        [TestCase("_p1 = value", "P1")]
        [TestCase("_p2 = value", "P2")]
        [TestCase("TrySet(ref _p3, value);", "P3")]
        public static void WhenRecursive(string signature, string propertyName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p1;
        private int _p2;
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get
            {
                return _p1;
            }

            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int P2
        {
            get
            {
                return _p2;
            }

            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged(() => this.P2);
            }
        }

        public int P3
        {
            get { return _p3; }
            set { TrySet(ref _p3, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (TrySet<T>(ref field, value, propertyName))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(property);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
             this.OnPropertyChanged(e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.Find<ExpressionSyntax>(signature);
            var property = semanticModel.GetDeclaredSymbol(syntaxTree.FindPropertyDeclaration(propertyName));
            Assert.AreEqual(AnalysisResult.No, PropertyChanged.InvokesPropertyChangedFor(node, property, semanticModel, CancellationToken.None));
        }
    }
}
