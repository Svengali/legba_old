using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Validation;

namespace gen
{






public class CaseClassGen : ICodeGenerator
{
    private readonly AttributeData attributeData;
    private readonly ImmutableDictionary<string, TypedConstant> data;
    private readonly string suffix = "CaseClass";

    public CaseClassGen(AttributeData attributeData)
    {
        Requires.NotNull(attributeData, nameof(attributeData));

        this.attributeData = attributeData;
        this.data = this.attributeData.NamedArguments.ToImmutableDictionary(kv => kv.Key, kv => kv.Value);
    }

    public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
    {
        var results = SyntaxFactory.List<MemberDeclarationSyntax>();

        MemberDeclarationSyntax copy = null;
        var applyToClass = context.ProcessingNode as ClassDeclarationSyntax;
        if (applyToClass != null)
        {
            copy = applyToClass
                .WithIdentifier(SyntaxFactory.Identifier(applyToClass.Identifier.ValueText + this.suffix));
        }

        if (copy != null)
        {
            results = results.Add(copy);
        }

        return Task.FromResult(results);
    }
}




}
