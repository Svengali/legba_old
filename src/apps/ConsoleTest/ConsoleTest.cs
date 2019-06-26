using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Konsole;
using Konsole.Drawing;

namespace ConsoleTest
{
static partial class Util
{
	// TODO Write this!
	public static int Count<T>( this IEnumerable<T> en )
	{
		return 0;
	}

}



	class ConsoleTest
	{


		static void Main( string[] args )
		{
			Console.Clear();

			

			Console.WriteLine($"Make zero size files in directory {args[ 1 ]} from directory {args[ 0 ]}");

			var pathFrom = args[ 0 ];
			var pathTo = args[ 1 ];

			var files = Directory.GetFiles( pathFrom );

			var tasks = new List<Task>();
			var bars = new ConcurrentBag<ProgressBar>();

			var con = Window.Open( 0, 5, 132, 40, "Errors", 
                  LineThickNess.Double,ConsoleColor.White,ConsoleColor.Blue);

			tasks.Add( Task.Run( () => {
				
				var bar = new ProgressBar( PbStyle.DoubleLine, files.Length );

				bars.Add( bar );



				for( int i = 0; i < files.Length; ++i )
				{
					var fullpath = files[i];
					var dir = Path.GetDirectoryName( fullpath );
					var fileShortname = Path.GetFileName( fullpath );

					bar.Refresh( i + 1, fullpath );

					var newFilename = $"{pathTo}\\{fileShortname}";

					try
					{
						using( var newFile = File.Open( newFilename, FileMode.CreateNew ) )
						{
						}

					}
					catch( Exception ex )
					{
						con.WriteLine( $"Caught {ex.Message}" );
					}

				}
			}));

			Task.WaitAll( tasks.ToArray() );
			Console.WriteLine( "done" );

			var timer = new lib.Timer().Start();

			while( timer.Current < 10_000 && !Console.KeyAvailable )
			{
				Thread.Sleep(10);
			}


		}
	}
}
