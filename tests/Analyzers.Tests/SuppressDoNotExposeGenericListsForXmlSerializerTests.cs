using Analyzers.Xml;
using Microsoft.CodeAnalysis;
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
            """,
        6,
        25,
        6,
        30
    )]
    public async Task ReportOnPublicTypeWithConcreteList(
        string code,
        int startLine,
        int startColumn,
        int endLine,
        int endColumn,
        CancellationToken cancellationToken
    )
    {
        var test = new SuppressorTest
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CA1002", DiagnosticSeverity.Warning).WithSpan(
                startLine,
                startColumn,
                endLine,
                endColumn
            )
        );

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
            """,
        7,
        25,
        7,
        30
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
            """,
        7,
        25,
        7,
        30
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
            """,
        7,
        25,
        7,
        30
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
            """,
        8,
        25,
        8,
        30
    )]
    public async Task DoNotReportOnPublicTypeWithConcreteListWithXmlSerializerAttributes(
        string code,
        int startLine,
        int startColumn,
        int endLine,
        int endColumn,
        CancellationToken cancellationToken
    )
    {
        var test = new SuppressorTest
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CA1002", DiagnosticSeverity.Warning)
                .WithSpan(startLine, startColumn, endLine, endColumn)
                .WithIsSuppressed(true)
        );

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
