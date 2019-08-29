using System;

namespace Core3Test
{
	class Program
	{
		static void Main( string[] args )
		{
			Console.WriteLine( "Hello World!" );

			var words = new string[]
			{
											// index from start    index from end
					"The",      // 0                   ^9
					"quick",    // 1                   ^8
					"brown",    // 2                   ^7
					"fox",      // 3                   ^6
					"jumped",   // 4                   ^5
					"over",     // 5                   ^4
					"the",      // 6                   ^3
					"lazy",     // 7                   ^2
					"dog"       // 8                   ^1
			};              // 9 (or words.Length) ^0

			var qbf = words[1..4];

			var lastWord = words[^1];

			Console.WriteLine( $"last {lastWord} | qbf {qbf[2]}" );


		}
	}
}
