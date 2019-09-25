using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

using System.Net.Sockets;

using System.Diagnostics;

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Reflection;
using Konsole;

namespace sv
{


	static class Program
	{

		private static bool s_running = true;

		private static void onAppExit( object sender, EventArgs e )
		{
			s_running = false;
		}


		static void Main( string[] args )
		{
			sv.Main server = new sv.Main( args[0] );

			server.startup();

			while( s_running )
			{
				System.Threading.Thread.Sleep( 1 );
			}

			server.shutdown();
		}
	}

}
