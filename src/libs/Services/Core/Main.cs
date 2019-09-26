using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using System.Reflection;

using System.IO;
using Konsole;
using System.Diagnostics;
using System.Linq.Expressions;
using Shielded.Gossip;

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
			for( int i = 0; i < m_runCount; ++i )
			{
				handle( o );
			}
			var endMs = timer.Current;

			lib.Log.info( $"testDirect: {endMs}" );
		}

		public void testInvoke()
		{
			var argTypes = new Type[ 1 ];
			argTypes[0] = typeof( TestMsg );

			var mi = GetType().GetMethod( "handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argTypes, null );

			var o = new TestMsg();

			var timer = new lib.Timer();

			var args = new object[1];
			args[0] = o;

			timer.Start();
			for( int i = 0; i < m_runCount; ++i )
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

			lib.Log.info( $"testDelegate: {endMs}" );
			/*/
			lib.Log.info( $"testDelegate: OFF" );
			//*/


		}

		public void testExpression()
		{
			var args = new Type[1];
			args[0] = typeof( TestMsg );

			var mi = GetType().GetMethod( "handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null );

			ParameterExpression pe = Expression.Parameter( typeof (TestMsg ), "msgIn" );

			var exConvert = Expression.Convert( pe, args[ 0 ] );

			var exParams = new Expression[ 1 ];
			exParams[0] = exConvert;

			var exThis = Expression.Constant( this );
			var exCall = Expression.Call( exThis, mi, exParams );

			var fn = Expression.Lambda<Action<TestMsg>>( exCall, pe ).Compile();

			var o = new TestMsg();

			var timer = new lib.Timer();

			timer.Start();
			for( int i = 0; i < m_runCount; ++i )
			{
				fn( o );
			}
			var endMs = timer.Current;

			lib.Log.info( $"testExpression: {endMs}" );
		}

		public void handle( TestMsg msg )
		{
		}


	}

	#endregion






	public enum ENodeType
	{
		Root,
		Leaf
	}

	[Serializable]
	public class SerializationTest
	{

		public SerializationTest()
		{
			m_strings[0] = "Str 0";
			m_strings[1] = "Str 1";

			m_comList[new lib.Token( "Token" )] = "Token";
			m_comList[new lib.Token( "Token_2" )] = "Token_2";
		}

		String[] m_strings = new String[ 2 ];

		Dictionary<lib.Token, string> m_comList = new Dictionary<lib.Token, string>();
	}

	[Serializable]
	public struct NewService
	{
		public string type;
		public string name;
		public string configPath;
	}

	[Serializable]
	public class ServerCfg : lib.Config
	{
		public ENodeType node = ENodeType.Leaf;
		public string name = "Test";
		public string address = "0.0.0.0";
		public int    port = 8008;

		public res.Ref<svc.MachineCfg> machineCfg;

		public NewService[] services = { new NewService() };
		public string[] demand = { };
	}




	public class Main
	{
		static public Main main;




		static Window s_fullscreenWin;
		static IConsole s_logWin;

		static public void log( lib.LogEvent evt )
		{
			switch( evt.LogType )
			{
				case lib.LogType.Error:
				s_logWin.ForegroundColor = ConsoleColor.Red;
				break;
				case lib.LogType.Warn:
				s_logWin.ForegroundColor = ConsoleColor.Yellow;
				break;
				case lib.LogType.Info:
				s_logWin.ForegroundColor = ConsoleColor.Gray;
				break;

			}

			s_logWin.WriteLine( $"{evt.Msg}" );
		}




		public lib.Clock clock { get; private set; }

		public Main( string configPath )
		{
			main = this;

			s_fullscreenWin = new Window();
			s_fullscreenWin.BackgroundColor = ConsoleColor.DarkGray;
			s_fullscreenWin.Clear( ConsoleColor.DarkGray );

			var xStart = 2;
			var yStart = 2;

			var xSize = s_fullscreenWin.WindowWidth  - xStart * 2;
			var ySize = s_fullscreenWin.WindowHeight - yStart * 2;

			s_logWin = Window.Open( xStart, yStart, xSize, ySize, "Logging" );


			Process p = Process.GetCurrentProcess();

			string logpath = "logs/"+Environment.MachineName+"_"+p.Id+".log";

			lib.Log.create( logpath );

			lib.Log.s_log.addDelegate( log );

			//*
			lib.Log.info( $"Command line {Environment.CommandLine}" );
			lib.Log.info( $"Current working directory {Environment.CurrentDirectory}" );
			lib.Log.info( $"Running as {( Environment.Is64BitProcess ? "64" : "32" )}bit on a {( Environment.Is64BitOperatingSystem ? "64" : "32" )}bit machine." );
			lib.Log.info( $"Running on {Environment.OSVersion}" );
			lib.Log.info( $"This machine has {Environment.ProcessorCount} processors." );
			lib.Log.info( $"Running as {Environment.UserName}" );

			lib.Log.info( $"Running on CLR {Environment.Version}" );
			lib.Log.info( $"Currently given {Environment.WorkingSet} memory" );
			//*/


			/*
			var test = new TestCalls();
			test.runAllTests();
			/*/
			lib.Log.info( $"Skipping tests." );
			//*/

			Serializer.Use( new svc.CerasSerializerForShielded() );





			res.Mgr.startup();
			lib.Config.startup( "server_config.cfg" );



			lib.Util.checkAndAddDirectory( "logs" );
			// save/static and save/dynamic are created when they dont exist in order to create the universe
			lib.Util.checkAndAddDirectory( "save/players" );


			lib.Util.checkAndAddDirectory( "save/archive/static" );
			lib.Util.checkAndAddDirectory( "save/archive/dynamic" );
			lib.Util.checkAndAddDirectory( "save/archive/players" );

			clock = new lib.Clock( 0 );

			m_svcMgr = new svc.Mgr();

			//Load configs
			lib.Log.info( $"Loading config {configPath}" );
			//m_cfg = lib.Config.load<ServerCfg>( configPath );
			m_cfg = res.Mgr.lookup<ServerCfg>( configPath );

			lib.Log.info( $"Listening on port {m_cfg.res.port}" );
			var localEP = new IPEndPoint( IPAddress.Parse( m_cfg.res.address ), m_cfg.res.port );

			m_listener = new TcpListener( localEP );
			m_listener.Start();

			/*
			foreach( NetworkInterface nic in
					NetworkInterface.GetAllNetworkInterfaces() )
			{
				Console.WriteLine( nic.Name );
				foreach( UnicastIPAddressInformation addrInfo in
						nic.GetIPProperties().UnicastAddresses )
				{
					Console.WriteLine( "\t" + addrInfo.Address );
				}
			}
			*/

			var ep = (IPEndPoint)m_listener.LocalEndpoint;

			string machineName = m_cfg.res.name+"/"+ep.Address.ToString() + ":" + ep.Port;

			//First of all start the machine service

			//This is now done 
			//var machineCfg = res.Mgr.load<svc.MachineCfg>( m_cfg.res.machineCfg );

			m_machine = new svc.Machine( new lib.Token( machineName ), m_cfg.res.machineCfg );
			svc.Service.s_mgr.start( m_machine );

			//TODO: Move these into machine startup.
			tick();
			Thread.Sleep( 1000 );

			//Now startup all the listed services
			foreach( var s in m_cfg.res.services )
			{
				var start = new svmsg.StartService( new svmsg.FilterType<svc.Machine>() );
				start.type = s.type;
				start.configPath = s.configPath;
				start.name = s.name;

				m_machine.send( start );
			}


		}

		public void shutdown()
		{
			foreach( var c in m_cnx )
			{
				c.Stream.Close();
				c.Sock.Close();
			}

			var shutdown = new svmsg.Shutdown();

			m_machine.send( shutdown );

			svc.Service.s_mgr.processMessages();
		}



		public void startup()
		{

			Thread thread = new Thread( new ThreadStart( this.run ) );
			thread.Start();
		}

		public void run()
		{
			while( true )
			{
				tick();
			}
		}

		public void tick()
		{
			clock.tick();

			//msg.Tick tick = new msg.Tick();

			//svc.Service.mgr.send( tick );

			svc.Service.s_mgr.procMsg_block( 1000 );

			//lib.Util.checkAndAddDirectory( "" );

			if( m_listener.Pending() )
			{
				lib.Log.info( "Client connected" );

				Socket socket = m_listener.AcceptSocket();


				net.Conn conn = new net.Conn( socket );

				m_cnx.Add( conn );

				conn.recieveThread();
			}
		}


		res.Ref<ServerCfg> m_cfg;

		TcpListener m_listener;

		List<net.Conn> m_cnx = new List<net.Conn>();

		svc.Mgr m_svcMgr;

		svc.Machine m_machine;


	}



}
