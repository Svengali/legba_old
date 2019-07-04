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


#region Test


class TestMsg
{
}

class TestCalls
{

	private int m_runCount = 10000000;

	public void runAllTests()
	{
		testDirect();
		testInvoke();
		testDelegate();
		testExpression();


	}


	public void testDirect()
	{
		var o = new TestMsg();

		var timer = new lib.Timer();

		timer.Start();
		for( int i = 0; i< m_runCount; ++i )
		{
			handle( o );
		}
		var endMs = timer.Current;

		lib.Log.info( $"testDirect: {endMs}" );
	}

	public void testInvoke()
	{
		var argTypes = new Type[ 1 ];
		argTypes[ 0 ]=typeof( TestMsg );

		var mi = GetType().GetMethod( "handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argTypes, null );

		var o = new TestMsg();

		var timer = new lib.Timer();

		var args = new object[1];
		args[0] = o;

		timer.Start();
		for( int i = 0; i< m_runCount; ++i )
		{
			mi.Invoke( this, args );
		}
		var endMs = timer.Current;

		lib.Log.info( $"testInvoke: {endMs}" );
	}

		public delegate void dlgHandler( TestMsg msg );

	public void testDelegate()
	{
		/*
		var args = new Type[1];
		args[0]=typeof( TestMsg );

		var mi = GetType().GetMethod( "handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null );

		var cb = mi.CreateDelegate( typeof(dlgHandler) );

		var o = new TestMsg();

		var timer = new lib.Timer();

		timer.Start();
		for( int i = 0; i< m_runCount; ++i )
		{
			cb.DynamicInvoke( o );
		}
		var endMs = timer.CurrentMS;

		lib.Log.info( "testDelegate: {0}", endMs );
		/*/
		lib.Log.info( $"testDelegate: OFF" );
		//*/


		}

	public void testExpression()
	{
		var args = new Type[1];
		args[0]=typeof( TestMsg );

		var mi = GetType().GetMethod( "handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null );

		ParameterExpression pe = Expression.Parameter( typeof (TestMsg ), "msgIn" );

		var exConvert = Expression.Convert( pe, args[ 0 ] );

		var exParams = new Expression[ 1 ];
		exParams[ 0 ] = exConvert;

		var exThis = Expression.Constant( this );
		var exCall = Expression.Call( exThis, mi, exParams );

		var fn = Expression.Lambda<Action<TestMsg>>( exCall, pe ).Compile();

		var o = new TestMsg();

		var timer = new lib.Timer();

		timer.Start();
		for( int i = 0; i< m_runCount; ++i )
		{
			fn( o );
		}
		var endMs = timer.Current;

		lib.Log.info($"testExpression: {endMs}");
	}

	public void handle( TestMsg msg )
	{
	}


}

#endregion



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

	/*
	private static RichTextBox s_rtb; 

	public static void Log( String type, String cat, String msg )
	{
		s_rtb.Invoke( (MethodInvoker)( () => {
			s_rtb.AppendText( msg );
			s_rtb.AppendText( "\n" );
		} ) );
	}
	*/

	private static bool s_running = true;

	private static void onAppExit(object sender, EventArgs e)
	{
		s_running = false;
	}


	static Window s_logWin;

	static public void log( lib.LogEvent evt )
	{
		s_logWin.WriteLine( $"{evt.Msg}" );
	}


		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		///	[STAThread]
		static void Main( string[] args  )
	{
		///Application.EnableVisualStyles();
		///Application.SetCompatibleTextRenderingDefault( false );
		///Application.ApplicationExit += onAppExit;

		//TODO: refactor this into another function so this init can all be shared between the GUI and the CLI versions
		///Console c = new Console();


		s_logWin = new Window(); 


		Process p = Process.GetCurrentProcess();

		string logpath = "logs/"+Environment.MachineName+"_"+p.Id+".log";

		lib.Log.create( logpath );

		/*
		lib.Log.log.addDelegate( Log );
		s_rtb = c.RTB;
		c.Show();
		*/

		lib.Log.s_log.addDelegate( log );

		lib.Log.info( $"Command line {Environment.CommandLine}" );
		lib.Log.info( $"Current working directory {Environment.CurrentDirectory}" );
		lib.Log.info( $"Running as {(Environment.Is64BitProcess ? "64":"32")}bit on a {(Environment.Is64BitOperatingSystem?"64":"32")}bit machine." );
		lib.Log.info( $"Running on {Environment.OSVersion}" );
		lib.Log.info( $"This machine has {Environment.ProcessorCount} processors." );
		lib.Log.info( $"Running as {Environment.UserName}" );

		lib.Log.info( $"Running on CLR {Environment.Version}" );
		lib.Log.info( $"Currently given {Environment.WorkingSet} memory" );

		

		var test = new TestCalls();
		test.runAllTests();

		res.Mgr.startup();
		lib.Config.startup();

		sv.Main server = new sv.Main( args[0] );

		server.startup();

		MSG msg = new MSG();
		while( s_running )
		{
			//var time = new lib.Timer();
			//time.Start();

			while( PeekMessage( ref msg, 0, 0, 0, PeekMessageOption.PM_REMOVE ) )
			{
				if( msg.message == WM_QUIT )
				{
					//Application.Exit();
					break;
				}

				TranslateMessage( ref msg );
				DispatchMessage( ref msg );
			}

			//time.Stop();
			//lib.Log.info( "{0} to PeekMessage", time.DurationMS );

			//server.tick();

			System.Threading.Thread.Sleep( 1 );
		}

		server.shutdown();
	}
}

}
