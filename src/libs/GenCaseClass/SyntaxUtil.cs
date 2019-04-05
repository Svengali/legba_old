using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Validation;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;




public static class SU
{

	public static T Or<T>( this Optional<T> opt, T def )
	{
		if( opt.HasValue)
			return opt.Value;

		return def;
	}


	public static T Def<T>( Optional<T> opt, T def )
	{
		if(opt.HasValue)
			return opt.Value;

		return def;
	}



	//*
	internal static ParameterSyntax Optional( ParameterSyntax parameter )
	{

		return parameter
				.WithType(OptionalOf(parameter.Type))
				.WithDefault(SF.EqualsValueClause(SF.DefaultExpression(OptionalOf(parameter.Type))));
	}
	//*/

	//*
	internal static NameSyntax OptionalOf( TypeSyntax type )
	{
		return 
				SF.GenericName(
						SF.Identifier(nameof(Optional)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(type)));
	}
	//*/

	internal static MemberAccessExpressionSyntax OptionalIsDefined( ExpressionSyntax optionalOfTExpression )
	{
		return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, optionalOfTExpression, SF.IdentifierName(nameof(Optional<int>.HasValue)));
	}

	internal static InvocationExpressionSyntax OptionalGetValueOrDefault( ExpressionSyntax optionalOfTExpression, ExpressionSyntax defaultValue )
	{
		return SF.InvocationExpression(
				SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, optionalOfTExpression, SF.IdentifierName(nameof(Or))),
				SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(defaultValue))));
	}

	internal static MemberAccessExpressionSyntax OptionalValue( ExpressionSyntax optionalOfTExpression )
	{
		return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, optionalOfTExpression, SF.IdentifierName(nameof(Optional<int>.Value)));
	}

	/*
	internal static ExpressionSyntax OptionalFor( ExpressionSyntax expression )
	{
		return SF.InvocationExpression(
				SF.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SF.QualifiedName(
								SF.IdentifierName(nameof(ImmutableObjectGraph)),
								SF.IdentifierName(nameof(ImmutableObjectGraph.Optional))),
						SF.IdentifierName(nameof(ImmutableObjectGraph.Optional.For))),
				SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(expression))));
	}
	*/

	/*
	internal static ExpressionSyntax OptionalForIf( ExpressionSyntax expression, bool isOptional )
	{
		return isOptional ? OptionalFor(expression) : expression;
	}
	*/

	/*
	internal static ImmutableArray<DeclarationInfo> GetDeclarationsInSpan( this SemanticModel model, TextSpan span, bool getSymbol, CancellationToken cancellationToken )
	{
		return CSharpDeclarationComputer.GetDeclarationsInSpan(model, span, getSymbol, cancellationToken);
	}
	//*/

	//*
	internal static NameSyntax GetTypeSyntax( Type type )
	{
		Requires.NotNull(type, nameof(type));

		SimpleNameSyntax leafType = SF.IdentifierName(type.IsGenericType ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name);
		if(type.IsGenericType)
		{
			leafType = SF.GenericName(
					( (IdentifierNameSyntax)leafType ).Identifier,
					SF.TypeArgumentList(JoinSyntaxNodes<TypeSyntax>(SyntaxKind.CommaToken, type.GenericTypeArguments.Select(GetTypeSyntax))));
		}

		if(type.Namespace != null)
		{
			NameSyntax namespaceName = null;
			foreach(string segment in type.Namespace.Split('.'))
			{
				var segmentName = SF.IdentifierName(segment);
				namespaceName = namespaceName == null
						? (NameSyntax)segmentName
						: SF.QualifiedName(namespaceName, SF.IdentifierName(segment));
			}

			return SF.QualifiedName(namespaceName, leafType);
		}

		return leafType;
	}
	//*/

	internal static NameSyntax IEnumerableOf( TypeSyntax typeSyntax )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Generic))),
				SF.GenericName(
						SF.Identifier(nameof(IEnumerable<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(typeSyntax))));
	}

	internal static NameSyntax IEnumeratorOf( TypeSyntax typeSyntax )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Generic))),
				SF.GenericName(
						SF.Identifier(nameof(IEnumerator<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(typeSyntax))));
	}

	internal static NameSyntax IEquatableOf( TypeSyntax typeSyntax )
	{
		return SF.QualifiedName(
				SF.IdentifierName(nameof(System)),
				SF.GenericName(
						SF.Identifier(nameof(IEquatable<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(typeSyntax))));
	}

	internal static NameSyntax IEqualityComparerOf( TypeSyntax typeSyntax )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Generic))),
				SF.GenericName(
						SF.Identifier(nameof(IEqualityComparer<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(typeSyntax))));
	}

	internal static NameSyntax ImmutableStackOf( TypeSyntax typeSyntax )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Immutable))),
				SF.GenericName(
						SF.Identifier(nameof(ImmutableStack<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(typeSyntax))));
	}

	internal static NameSyntax FuncOf( params TypeSyntax[] typeArguments )
	{
		return SF.QualifiedName(
				SF.IdentifierName(nameof(System)),
				SF.GenericName(nameof(Func<int>)).AddTypeArgumentListArguments(typeArguments));
	}

	internal static InvocationExpressionSyntax ToList( ExpressionSyntax expression )
	{
		return SF.InvocationExpression(
				// System.Linq.Enumerable.ToList
				SF.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SF.QualifiedName(
								SF.QualifiedName(
										SF.IdentifierName(nameof(System)),
										SF.IdentifierName(nameof(System.Linq))),
								SF.IdentifierName(nameof(Enumerable))),
						SF.IdentifierName(nameof(Enumerable.ToList))),
				SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(expression))));
	}

	internal static NameSyntax IReadOnlyCollectionOf( TypeSyntax elementType )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Generic))),
				SF.GenericName(
						SF.Identifier(nameof(IReadOnlyCollection<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(elementType))));
	}

	internal static NameSyntax IReadOnlyListOf( TypeSyntax elementType )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Generic))),
				SF.GenericName(
						SF.Identifier(nameof(IReadOnlyList<int>)),
						SF.TypeArgumentList(SF.SingletonSeparatedList(elementType))));
	}

	internal static NameSyntax KeyValuePairOf( TypeSyntax keyType, TypeSyntax valueType )
	{
		return SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Generic))),
				SF.GenericName(
						SF.Identifier(nameof(KeyValuePair<int, int>)),
						SF.TypeArgumentList(JoinSyntaxNodes(SyntaxKind.CommaToken, keyType, valueType))));
	}

	internal static ExpressionSyntax CreateDictionary( TypeSyntax keyType, TypeSyntax valueType )
	{
		// System.Collections.Immutable.ImmutableDictionary.Create<TKey, TValue>()
		return SF.InvocationExpression(
				SF.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						GetTypeSyntax(typeof(ImmutableDictionary)),
						SF.GenericName(nameof(ImmutableDictionary.Create)).AddTypeArgumentListArguments(keyType, valueType)),
				SF.ArgumentList());
	}

	internal static ExpressionSyntax CreateImmutableStack( TypeSyntax elementType = null )
	{
		var typeSyntax = SF.QualifiedName(
				SF.QualifiedName(
						SF.QualifiedName(
								SF.IdentifierName(nameof(System)),
								SF.IdentifierName(nameof(System.Collections))),
						SF.IdentifierName(nameof(System.Collections.Immutable))),
				SF.IdentifierName(nameof(ImmutableStack)));

		return SF.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				typeSyntax,
				elementType == null
						? (SimpleNameSyntax)SF.IdentifierName(nameof(ImmutableStack.Create))
						: SF.GenericName(nameof(ImmutableStack.Create)).AddTypeArgumentListArguments(elementType));
	}

	internal static BaseMethodDeclarationSyntax AddKeyword( BaseMethodDeclarationSyntax method, SyntaxKind keyword )
	{
		return method.WithModifiers(method.Modifiers.Insert(0, SF.Token(keyword)));
	}

	internal static MethodDeclarationSyntax AddNewKeyword( MethodDeclarationSyntax method )
	{
		return method.WithModifiers(method.Modifiers.Insert(0, SF.Token(SyntaxKind.NewKeyword)));
	}

	internal static PropertyDeclarationSyntax AddNewKeyword( PropertyDeclarationSyntax method )
	{
		return method.WithModifiers(method.Modifiers.Insert(0, SF.Token(SyntaxKind.NewKeyword)));
	}

	internal static SeparatedSyntaxList<T> JoinSyntaxNodes<T>( SyntaxKind tokenDelimiter, params T[] nodes )
			where T : SyntaxNode
	{
		return SF.SeparatedList<T>(JoinSyntaxNodes<T>(SF.Token(tokenDelimiter), nodes));
	}

	internal static SeparatedSyntaxList<T> JoinSyntaxNodes<T>( SyntaxKind tokenDelimiter, ImmutableArray<T> nodes )
			where T : SyntaxNode
	{
		return SF.SeparatedList<T>(JoinSyntaxNodes<T>(SF.Token(tokenDelimiter), nodes));
	}

	internal static SeparatedSyntaxList<T> JoinSyntaxNodes<T>( SyntaxKind tokenDelimiter, IEnumerable<T> nodes )
			where T : SyntaxNode
	{
		return SF.SeparatedList<T>(JoinSyntaxNodes<T>(SF.Token(tokenDelimiter), nodes.ToArray()));
	}

	internal static SyntaxNodeOrTokenList JoinSyntaxNodes<T>( SyntaxToken separatingToken, IReadOnlyList<T> nodes )
			where T : SyntaxNode
	{
		Requires.NotNull(nodes, nameof(nodes));

		switch(nodes.Count)
		{
			case 0:
			return SF.NodeOrTokenList();
			case 1:
			return SF.NodeOrTokenList(nodes[ 0 ]);
			default:
			var nodesOrTokens = new SyntaxNodeOrToken[ ( nodes.Count * 2 ) - 1 ];
			nodesOrTokens[ 0 ] = nodes[ 0 ];
			for(int i = 1; i < nodes.Count; i++)
			{
				int targetIndex = i * 2;
				nodesOrTokens[ targetIndex - 1 ] = separatingToken;
				nodesOrTokens[ targetIndex ] = nodes[ i ];
			}

			return SF.NodeOrTokenList(nodesOrTokens);
		}
	}

	internal static ParameterListSyntax PrependParameter( this ParameterListSyntax list, ParameterSyntax parameter )
	{
		return SF.ParameterList(SF.SingletonSeparatedList(parameter))
				.AddParameters(list.Parameters.ToArray());
	}

	internal static ArgumentListSyntax PrependArgument( this ArgumentListSyntax list, ArgumentSyntax argument )
	{
		return SF.ArgumentList(SF.SingletonSeparatedList(argument))
				.AddArguments(list.Arguments.ToArray());
	}

	internal static ExpressionSyntax ThisDot( SimpleNameSyntax memberAccess )
	{
		return SF.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SF.ThisExpression(),
				memberAccess);
	}

	internal static ExpressionSyntax BaseDot( SimpleNameSyntax memberAccess )
	{
		return SF.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SF.BaseExpression(),
				memberAccess);
	}

	internal static ExpressionSyntax ChainBinaryExpressions( this IEnumerable<ExpressionSyntax> expressions, SyntaxKind binaryOperator )
	{
		return expressions.Aggregate((ExpressionSyntax)null, ( agg, e ) => agg != null ? SF.BinaryExpression(binaryOperator, agg, e) : e);
	}

	internal static InvocationExpressionSyntax EnumerableExtension( SimpleNameSyntax linqMethod, ExpressionSyntax receiver, ArgumentListSyntax arguments )
	{
		return SF.InvocationExpression(
				SF.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						GetTypeSyntax(typeof(Enumerable)),
						linqMethod),
				arguments.PrependArgument(SF.Argument(receiver)));
	}

	internal static StatementSyntax RequiresNotNull( IdentifierNameSyntax parameter )
	{
		// if (other == null) { throw new System.ArgumentNullException(nameof(other)); }
		return SF.IfStatement(
				SF.BinaryExpression(SyntaxKind.EqualsExpression, parameter, SF.LiteralExpression(SyntaxKind.NullLiteralExpression)),
				SF.ThrowStatement(
						SF.ObjectCreationExpression(GetTypeSyntax(typeof(ArgumentNullException))).AddArgumentListArguments(
								SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(parameter.Identifier.ToString()))))));
	}

	internal static TypeSyntax GetFullName( SyntaxNode typeNode )
	{
		var typeDeclaration = typeNode as TypeDeclarationSyntax;
		if(typeDeclaration != null)
		{
			if(true == typeDeclaration.TypeParameterList?.Parameters.Any())
			{
				var arguments = typeDeclaration.TypeParameterList.Parameters.Select(p => GetFullName(p)).ToArray();
				return SF.GenericName(SF.Identifier(typeDeclaration.Identifier.Text), SF.TypeArgumentList(SF.SeparatedList(arguments)));
			}

			return SF.IdentifierName(typeDeclaration.Identifier.Text);
		}

		var typeParameter = typeNode as TypeParameterSyntax;
		if(typeParameter != null)
		{
			return SF.IdentifierName(typeParameter.Identifier.Text);
		}

		return typeNode as TypeSyntax;
	}
}
