using Analyzers.Xml;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;

namespace Analyzers.Tests;

internal sealed class SuppressDoNotExposeGenericListsForXmlSerializerTests
{
    [Test]
    [Arguments(
        """
            using System.Collections.Generic;
            using System.Xml.Serialization;

            public class MyModel
            {
                public List<string> Items { get; init; } = [];
            }
            """
    )]
    public async Task ReportOnPublicTypeWithConcreteList(
        string code,
        CancellationToken cancellationToken
    )
    {
        var test = new SuppressorTest
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };

        // TODO: Update test to expect CA1002

        await test.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    [Test]
    [Arguments(
        """
            using System.Collections.Generic;
            using System.Xml.Serialization;

            [XmlRoot("Test")]
            public class MyModel
            {
                public List<string> Items { get; init; } = [];
            }
            """
    )]
    [Arguments(
        """
            using System.Collections.Generic;
            using System.Xml.Serialization;

            public class MyModel
            {
                [XmlArray("Test")]
                public List<string> Items { get; init; } = [];
            }
            """
    )]
    [Arguments(
        """
            using System.Collections.Generic;
            using System.Xml.Serialization;

            public class MyModel
            {
                [XmlArrayItem("TestItem")]
                public List<string> Items { get; init; } = [];
            }
            """
    )]
    [Arguments(
        """
            using System.Collections.Generic;
            using System.Xml.Serialization;

            public class MyModel
            {
                [XmlElement("Test")]
                public string Test { get; init; } = "";
                public List<string> Items { get; init; } = [];
            }
            """
    )]
    public async Task DoNotReportOnPublicTypeWithConcreteListWithXmlSerializerAttributes(
        string code,
        CancellationToken cancellationToken
    )
    {
        var test = new SuppressorTest
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };

        // TODO: Update test to expect no diagnostics since CA1002 should be suppressed by SuppressDoNotExposeGenericListsForXmlSerializer

        await test.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    private sealed class SuppressorTest
        : CSharpAnalyzerTest<SuppressDoNotExposeGenericListsForXmlSerializer, DefaultVerifier>
    {
        // We need to load CA1002 (Do not expose generic lists) to test suppressing it
        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() =>
            base.GetDiagnosticAnalyzers().Append(new DoNotExposeGenericLists());
    }
}
