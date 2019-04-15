using System;

using System.Windows;

namespace EntityTest
{
	class Program
	{
		static void Main( string[] args )
		{
			Console.WriteLine( "Hello World!" );

			var e = ent.Entity.create();

			//var nc = new NetCore.Class1();

			//var e = ent.Entity.create(testStringOpt: "Howdy");

			var mutE = new ent.Mut<ent.Entity>(ref e);

			//mutE.mut( () => { return e.with(testStringOpt: "mut"); });

			//mutE.mut( () => e.with(testStringOpt: "mut 2") );

			change( ref e );

			var v = e.TestVal;

			//var healthOpt = e.Com<ent.IComHealth>();

			/*
			if( healthOpt.HasValue )
			{
				if( healthOpt.Value.Health < 10 )
				{
				}
			}
			*/

			//var ver = e.Ver

			//var ver = e.Version;

			//Console.WriteLine( $"int is {e.testInt}" );
		}


		static void change( ref ent.Entity ent )
		{
			//ent = ent.with(testIntOpt: 10);
		}
	}
}
