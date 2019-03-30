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

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace gen
{






	public class CaseClassGen : ICodeGenerator
	{
		private readonly AttributeData m_attributeData;
		private readonly ImmutableDictionary<string, TypedConstant> m_data;

		private TransformationContext m_context;

		private Dictionary<TypeSyntax, VariableDeclaratorSyntax> m_fields = new Dictionary<TypeSyntax, VariableDeclaratorSyntax>();  



		public CaseClassGen( AttributeData attributeData )
		{
			Requires.NotNull(attributeData, nameof(attributeData));

			m_attributeData = attributeData;
			m_data = attributeData.NamedArguments.ToImmutableDictionary(kv => kv.Key, kv => kv.Value);
		}

		public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync( TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken )
		{


			m_context = context;

			//m_context.SemanticModel.

			//m_classDecl = m_context.SemanticModel.GetDeclarationDiagnostics();
			
			

			//.GetDeclarationsInSpan(TextSpan.FromBounds(0, this.semanticModel.SyntaxTree.Length), true, this.cancellationToken);

			var results = SyntaxFactory.List<MemberDeclarationSyntax>();

			ClassDeclarationSyntax copy = null;
			var applyToClass = context.ProcessingNode as ClassDeclarationSyntax;

			var applyToClassIdentifier = applyToClass.Identifier;

			if(applyToClass != null)
			{
				var fieldsComment = "";

				foreach( var member in applyToClass.Members )
				{
					var field = member as FieldDeclarationSyntax;
					if( field != null )
					{
						var decl = field.Declaration;

						var type = decl.Type;

						var v = decl.Variables[0];

						m_fields.Add( type, v );

						fieldsComment += $"({type.GetType().Name}){type} ({v.GetType().Name}){v}\n";

					}
				}

				var leadingTrivia = SF.Comment( $"/*\n{fieldsComment}\n*/" );

				copy = SF.ClassDeclaration( applyToClassIdentifier )
					.WithModifiers( SyntaxTokenList.Create( SF.Token( SyntaxKind.PartialKeyword ) ) )
					.WithLeadingTrivia( leadingTrivia );

			}
			else
			{
				// TODO ERROR 
			}


			if(copy != null)
			{
				results = results.Add( copy );
			}

			return Task.FromResult(results);
		}
	}




}
