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

		private Dictionary<VariableDeclaratorSyntax, TypeSyntax> m_fields = new Dictionary<VariableDeclaratorSyntax, TypeSyntax>();  

		private ClassDeclarationSyntax m_class;

        private INamedTypeSymbol m_sym;

        private INamedTypeSymbol m_baseSym;

		public CaseClassGen( AttributeData attributeData )
		{
			Requires.NotNull(attributeData, nameof(attributeData));

			m_attributeData = attributeData;
			m_data = attributeData.NamedArguments.ToImmutableDictionary(kv => kv.Key, kv => kv.Value);
		}


		public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync( TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken )
		{


			m_context = context;

			m_class = m_context.ProcessingNode as ClassDeclarationSyntax;

			m_sym = m_context.SemanticModel.GetDeclaredSymbol( m_class );

			m_baseSym = m_sym.BaseType;

			var baseIsObject = m_baseSym.SpecialType == SpecialType.System_Object;



			var results = SyntaxFactory.List<MemberDeclarationSyntax>();

			ClassDeclarationSyntax copy = null;

			var applyToClassIdentifier = m_class.Identifier;

			if(m_class != null)
			{
				var fieldsComment = "";

				foreach( var member in m_class.Members )
				{
					var field = member as FieldDeclarationSyntax;
					if( field != null )
					{
						var decl = field.Declaration;

						var type = decl.Type;

						foreach( var v in decl.Variables )
						{
							m_fields.Add( v, type );
							fieldsComment += $"({type.GetType().Name}){type} ({v.GetType().Name}){v}\n\r";
						}
					}
				}


                



				var leadingTrivia = SF.Comment( $"/*\n{fieldsComment}\n*/" );

				//var newUsing = SF.UsingDirective( SF.IdentifierName( "" ) );

				var withMembers = CreateDefault();

				if( baseIsObject )
				{
					withMembers = withMembers.AddRange( CreateVersion() );
				}

				withMembers = withMembers.AddRange( CreateProtectedConstructors() );

				withMembers = withMembers.AddRange( CreateWithFunctions() );
				
				withMembers = withMembers.AddRange( CreateCreateFunctions() );


				copy = SF.ClassDeclaration( applyToClassIdentifier )
					.WithModifiers( SyntaxTokenList.Create( SF.Token( SyntaxKind.PartialKeyword ) ) )
					.WithLeadingTrivia( leadingTrivia )
					.WithMembers( withMembers );

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

		SyntaxToken ModifyToken( string mod, SyntaxToken token )
		{
			return SF.IdentifierName( String.Format( mod, token.Text ) ).Identifier;
		}

		IdentifierNameSyntax ModifyIdentifier( string mod, SyntaxToken token )
		{
			return SF.IdentifierName( String.Format( mod, token ) );
		}

		SyntaxList<StatementSyntax> CreateAssignments( string varPrefix )
		{
			var assignments = new SyntaxList<StatementSyntax>();

			var constructorParams = "";

			var first = true;
			foreach( var f in m_fields )
			{
				if( !first )
				{
					constructorParams += ", ";
				}
				else
				{
					first = false;
				}

				var statement = SF.ParseStatement( $"var {f.Key.Identifier}New = {f.Key.Identifier}Opt.Or({varPrefix}{f.Key.Identifier});" );

				assignments = assignments.Add( statement );

				constructorParams += ModifyToken( "{0}New", f.Key.Identifier );

			}

			var retExp = SF.ParseExpression( $"new {m_class.Identifier}( {constructorParams} )" );

			var ret = SF.ReturnStatement( retExp );

			assignments = assignments.Add( ret );

			return assignments;
		}


		private SeparatedSyntaxList<ParameterSyntax> CreateAssignCheckBlock()
		{
			var paramList = new SeparatedSyntaxList<ParameterSyntax>();

			foreach( var f in m_fields )
			{
				var param = SF.Parameter( ModifyToken( "{0}Opt", f.Key.Identifier ) )
					.WithType( f.Value );

				param = SU.Optional( param );

				paramList = paramList.Add( param );
			}

			return paramList;
		}

		private SyntaxList<MemberDeclarationSyntax> CreateWithFunctions()
		{
			var returnType = m_class.Identifier;

			var retType = SF.IdentifierName( returnType );

			var withFn = SF.MethodDeclaration( retType, "with" );
			

			var paramList = CreateAssignCheckBlock();
		

			withFn = withFn.WithParameterList( SF.ParameterList( paramList ) );

			var block = SF.Block();

			block = block.WithStatements( CreateAssignments( "" ) );

			withFn = withFn.WithBody( block )
				.WithModifiers( SyntaxTokenList.Create( SF.Token( SyntaxKind.PublicKeyword ) ) );

			var list = new SyntaxList<MemberDeclarationSyntax>();

			list = list.Add(withFn);

			return list;
		}

		private SyntaxList<MemberDeclarationSyntax> CreateCreateFunctions()
		{
			var returnType = m_class.Identifier;

			var retType = SF.IdentifierName( returnType );

			var withFn = SF.MethodDeclaration( retType, "create" )
			.WithModifiers( SyntaxTokenList.Create( SF.Token( SyntaxKind.PublicKeyword ) ).Add( SF.Token( SyntaxKind.StaticKeyword ) ) );
			

			var paramList = CreateAssignCheckBlock();
		

			withFn = withFn.WithParameterList( SF.ParameterList( paramList ) );

			var block = SF.Block();

			block = block.WithStatements( CreateAssignments( "def." ) );

			withFn = withFn.WithBody( block );

			var list = new SyntaxList<MemberDeclarationSyntax>();

			list = list.Add(withFn);

			return list;
		}




		private SyntaxList<MemberDeclarationSyntax> CreateProtectedConstructors()
		{
			var list = new SyntaxList<MemberDeclarationSyntax>();

			if( m_fields.Count > 0 )
			{
				BaseMethodDeclarationSyntax cons = SF.ConstructorDeclaration( m_class.Identifier );

				cons = SU.AddKeyword( cons, SyntaxKind.ProtectedKeyword );

				var paramList = new SeparatedSyntaxList<ParameterSyntax>();

				foreach( var f in m_fields )
				{
					var param = SF.Parameter( f.Key.Identifier )
						.WithType( f.Value );

					paramList = paramList.Add( param );
				}

				cons = cons.WithParameterList( SF.ParameterList( paramList ) );

				var block = SF.Block();

				var assignments = new SyntaxList<StatementSyntax>();

				foreach( var f in m_fields )
				{
					var statement = SF.ParseStatement( $"this.{f.Key.Identifier} = {f.Key.Identifier};" );

					assignments = assignments.Add( statement );
				}

				block = block.WithStatements( assignments );

				cons = cons.WithBody( block );


				list = list.Add(cons);
			}

			{
				BaseMethodDeclarationSyntax cons = SF.ConstructorDeclaration( m_class.Identifier );

				cons = SU.AddKeyword( cons, SyntaxKind.ProtectedKeyword );

				var block = SF.Block();

				cons = cons.WithBody( block );

				list = list.Add(cons);
			}


			return list;
		}

		private SyntaxList<MemberDeclarationSyntax> CreateDefault()
		{
			var list = new SyntaxList<MemberDeclarationSyntax>();

			//var st = SF.ParseStatement( $"static public readonly {m_class.Identifier} def = new {m_class.Identifier};" );
			var newClass = SF.ParseExpression( $"new {m_class.Identifier}()" );

			var declarator = SF.VariableDeclarator( "def" )
				.WithInitializer( SF.EqualsValueClause( newClass ) );

			var decl = SF.VariableDeclaration( SF.IdentifierName( m_class.Identifier ), SF.SingletonSeparatedList( declarator ) );

			var keywords = SyntaxTokenList.Create( SF.Token( SyntaxKind.PublicKeyword ) )
				.Add( SF.Token( SyntaxKind.StaticKeyword ) )
				.Add( SF.Token( SyntaxKind.ReadOnlyKeyword ) );

			if( m_baseSym.SpecialType != SpecialType.System_Object )
			{
				keywords = keywords.Add( SF.Token( SyntaxKind.NewKeyword ) );
			}

			var field = SF.FieldDeclaration( decl )
				.WithModifiers( keywords );

			list = list.Add( field );

			return list;
		}


		private SyntaxList<MemberDeclarationSyntax> CreateVersion()
		{
			var list = new SyntaxList<MemberDeclarationSyntax>();

			/*
			var versionField = $"private unsigned long m_version = 0";

			var versionAcc = $"public unsigned long Version => m_version";

			var versionFieldParsed = SF.ParseStatement( versionField );

			var versionAccParsed = SF.ParseStatement( versionAcc );
			*/

			var versionField = SU.Field( "m_version", "long", SF.ParseExpression( "0" ), SyntaxKind.PrivateKeyword );

			var versionAcc = SF.PropertyDeclaration(SF.IdentifierName("long"), "Version");


			versionAcc = versionAcc.WithExpressionBody( SF.ArrowExpressionClause( SF.ParseExpression( "m_version" ) ) )
				.WithSemicolonToken( SF.Token( SyntaxKind.SemicolonToken ) )
				.WithModifiers( new SyntaxTokenList( SF.Token( SyntaxKind.PublicKeyword ) ) );

			list = list.Add( versionField );
			list = list.Add( versionAcc );

			return list;
		}



	}




}
