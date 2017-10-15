namespace PropertyChangedAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class InvokesPropertyChangedFor
        {
            [TestCase("_value1 = value", "Value1")]
            [TestCase("_value2 = value", "Value2")]
            //[TestCase("SetValue(ref _value3, value);", "Value3")]
            public void WhenTrue(string signature, string propertyName)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _value1;
        private int _value2;
        private int _value3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value1
        {
            get
            {
                return _value1;
            }

            set
            {
                if (value == _value1)
                {
                    return;
                }

                _value1 = value;
                OnPropertyChanged();
            }
        }

        public int Value2
        {
            get
            {
                return _value2;
            }

            set
            {
                if (value == _value2)
                {
                    return;
                }

                _value2 = value;
                OnPropertyChanged(() => this.Value2);
            }
        }

        public int Value3
        {
            get { return _value3; }
            set { SetValue(ref _value3, value); }
        }

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var node = syntaxTree.FindBestMatch<SyntaxNode>(signature);
                var property = semanticModel.GetDeclaredSymbol(syntaxTree.FindPropertyDeclaration(propertyName));
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.InvokesPropertyChangedFor(node, property, semanticModel, CancellationToken.None));
            }

            [TestCase("_value1 = value", "Value1")]
            [TestCase("_value2 = value", "Value2")]
            [TestCase("SetValue(ref _value3, value);", "Value3")]
            public void WhenRecursive(string signature, string propertyName)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _value1;
        private int _value2;
        private int _value3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value1
        {
            get
            {
                return _value1;
            }

            set
            {
                if (value == _value1)
                {
                    return;
                }

                _value1 = value;
                OnPropertyChanged();
            }
        }

        public int Value2
        {
            get
            {
                return _value2;
            }

            set
            {
                if (value == _value2)
                {
                    return;
                }

                _value2 = value;
                OnPropertyChanged(() => this.Value2);
            }
        }

        public int Value3
        {
            get { return _value3; }
            set { SetValue(ref _value3, value); }
        }

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (SetValue<T>(ref field, newValue, propertyName))
            {
                return false;
            }

            field = newValue;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var node = syntaxTree.FindBestMatch<SyntaxNode>(signature);
                var property = semanticModel.GetDeclaredSymbol(syntaxTree.FindPropertyDeclaration(propertyName));
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.InvokesPropertyChangedFor(node, property, semanticModel, CancellationToken.None));
            }
        }
    }
}