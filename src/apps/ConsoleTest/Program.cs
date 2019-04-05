using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
	class Program
	{
		static void Main( string[] args )
		{
			Console.WriteLine($"Make zero size files in directory {args[ 1 ]} from directory {args[ 0 ]}");

			var pathFrom = args[ 0 ];
			var pathTo = args[ 1 ];

			var files = Directory.EnumerateFiles(pathFrom);

			foreach(var fullpath in files)
			{
				var dir = Path.GetDirectoryName( fullpath );
				var fileShortname = Path.GetFileName( fullpath );

				Console.WriteLine( $"Processing file {fileShortname}" );

				var newFilename = $"{pathTo}\\{fileShortname}";

				try
				{
					using( var newFile = File.Open( newFilename, FileMode.CreateNew ) )
					{
					}

				}
				catch( Exception ex )
				{
					Console.WriteLine( $"Caught {ex.Message}" );
				}

			}



		}
	}
}
