using System;
using CodeGeneration.Roslyn;
using System.Diagnostics;

namespace gen
{

///[Conditional("CodeGeneration")]


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
[CodeGenerationAttribute(typeof(NetViewGen))]
[Conditional("CodeGeneration")]
public class NetViewAttribute : Attribute
{
	/*
	public net.Views PrimaryView { get; private set; }
	//public (net.Views from, net.Views to)[] Distribution { get; private set; }

	public NetViewAttribute( net.Views primaryView )
	{
		PrimaryView = primaryView;

		/*
		var halfLength = distribution.Length / 2;

		Distribution = new (net.Views from, net.Views to)[halfLength];

		for( int i = 0; i < halfLength; ++i )
		{
			Distribution[i] = (distribution[i * 2 + 0], distribution[i * 2 + 1]);
		}
		* /
	}
	*/

}


/*
[CodeGenerationAttribute(typeof(NetViewGen))]
[Conditional("CodeGeneration")]
*/

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class NetDistAttribute : Attribute
{
	/*
	public net.Views Set { get; private set; }
	public net.Views Get { get; private set; }

	public NetDistAttribute( net.Views set = net.Views.None, net.Views get = net.Views.None )
	{
		Set = set;
		Get = get;
	}
	*/

}



}
