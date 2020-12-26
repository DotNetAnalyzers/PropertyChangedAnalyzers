using System;
using Gu.Roslyn.Asserts;

[assembly: CLSCompliant(false)]

[assembly: MetadataReference(typeof(object), new[] { "global", "mscorlib" })]
[assembly: MetadataReference(typeof(System.Diagnostics.Debug), new[] { "global", "System" })]
[assembly: TransitiveMetadataReferences(typeof(Gu.Roslyn.CodeFixExtensions.DocumentEditorAction))]
[assembly: MetadataReferences(
    typeof(System.Linq.Enumerable),
    typeof(System.Net.WebClient),
    typeof(System.Drawing.Bitmap),
    typeof(System.Data.Common.DbConnection),
    typeof(System.Xml.Serialization.XmlSerializer),
    typeof(System.Runtime.Serialization.DataContractSerializer),
    typeof(System.Windows.Media.Brush),
    typeof(System.Windows.Controls.Control),
    typeof(System.Windows.Media.Matrix),
    typeof(System.Xaml.XamlLanguage),
    typeof(Gu.Reactive.ICondition),
    typeof(Gu.Wpf.Reactive.AsyncCommand),
    typeof(NUnit.Framework.Assert))]
