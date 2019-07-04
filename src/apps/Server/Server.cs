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
	struct POINTAPI
	{
		public Int32 x;
		public Int32 y;
	}

	struct MSG
	{
		public Int32 hwmd;
		public Int32 message;
		public Int32 wParam;
		public Int32 lParam;
		public Int32 time;
		public POINTAPI pt;
	}

	[DllImport( "user32.dll", SetLastError = true )]
	private static extern bool PeekMessage(
																 ref MSG lpMsg,
													 Int32 hwnd,
							 Int32 wMsgFilterMin,
							 Int32 wMsgFilterMax,
							 PeekMessageOption wRemoveMsg );

	[DllImport( "user32.dll", SetLastError = true )]
	private static extern bool TranslateMessage( ref MSG lpMsg );

	[DllImport( "user32.dll", SetLastError = true )]
	private static extern Int32 DispatchMessage( ref MSG lpMsg );

	private enum PeekMessageOption
	{
		PM_NOREMOVE = 0,
		PM_REMOVE
	}

	private static Int32 WM_QUIT = 0x12;


	private static bool s_running = true;

	private static void onAppExit(object sender, EventArgs e)
	{
		s_running = false;
	}


	static void Main( string[] args  )
	{
		sv.Main server = new sv.Main( args[0] );

		server.startup();

		MSG msg = new MSG();
		while( s_running )
		{
			while( PeekMessage( ref msg, 0, 0, 0, PeekMessageOption.PM_REMOVE ) )
			{
				if( msg.message == WM_QUIT )
				{
					break;
				}

				TranslateMessage( ref msg );
				DispatchMessage( ref msg );
			}

			System.Threading.Thread.Sleep( 1 );
		}

		server.shutdown();
	}
}

}
