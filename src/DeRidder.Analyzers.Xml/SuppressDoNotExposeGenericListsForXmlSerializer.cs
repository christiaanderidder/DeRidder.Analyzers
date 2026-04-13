using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DeRidder.Analyzers.Xml;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuppressDoNotExposeGenericListsForXmlSerializer : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor Rule = new(
        "RID1002",
        "CA1002",
        "Suppress CA1002 when the type is used by XmlSerializer. XmlSerializer requires public types and properties as well as concrete collection types."
    );

    private static readonly ImmutableArray<string> XmlSerializerAttributes = ImmutableArray.Create(
        "System.Xml.Serialization.XmlRootAttribute",
        "System.Xml.Serialization.XmlTypeAttribute",
        "System.Xml.Serialization.XmlElementAttribute",
        "System.Xml.Serialization.XmlArrayAttribute",
        "System.Xml.Serialization.XmlArrayItemAttribute",
        "System.Xml.Serialization.XmlAttributeAttribute",
        "System.Xml.Serialization.XmlTextAttribute",
        "System.Xml.Serialization.XmlIgnoreAttribute",
        "System.Xml.Serialization.XmlIncludeAttribute",
        "System.Xml.Serialization.XmlEnumAttribute",
        "System.Xml.Serialization.XmlAnyElementAttribute",
        "System.Xml.Serialization.XmlAnyAttributeAttribute",
        "System.Xml.Serialization.XmlChoiceIdentifierAttribute"
    );

    private static readonly ImmutableArray<SuppressionDescriptor> Suppressions =
        ImmutableArray.Create(Rule);

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => Suppressions;

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        var knownTypes = XmlSerializerAttributes
            .Select(context.Compilation.GetTypeByMetadataName)
            .Where(type => type is not null)
            .ToImmutableHashSet<INamedTypeSymbol?>(SymbolEqualityComparer.Default);

        if (knownTypes.IsEmpty)
            return;

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var node = diagnostic
                .Location.SourceTree?.GetRoot(context.CancellationToken)
                .FindNode(diagnostic.Location.SourceSpan);

            if (node?.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not { } classDeclaration)
                continue;

            var model = context.GetSemanticModel(classDeclaration.SyntaxTree);

            if (
                model.GetDeclaredSymbol(classDeclaration, context.CancellationToken) is { } symbol
                && HasAnyXmlAttribute(symbol, knownTypes)
            )
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }

    private static bool HasAnyXmlAttribute(
        INamedTypeSymbol classSymbol,
        ImmutableHashSet<INamedTypeSymbol?> knownTypes
    )
    {
        return classSymbol.GetAttributes().Any(a => knownTypes.Contains(a.AttributeClass))
            || classSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Any(p => p.GetAttributes().Any(a => knownTypes.Contains(a.AttributeClass)));
    }
}
