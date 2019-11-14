/*
	M A I N

	The core server class.  

*/
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
using System.Collections.Concurrent;

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
	public class NewService
	{
		public string type = "<unknown>";
		public string name = "<unknown>";
		public string configPath = "<unknown>";
	}

	[Serializable]
	public class ServiceOnDemand
	{
		public NewService service = new NewService();
	}

	[Serializable]
	public class RemoteMachine
	{
		public string address = "0.0.0.0";
		public int    port = 8008;
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
		public ServiceOnDemand[] servicesOnDemand = { new ServiceOnDemand() };

		public RemoteMachine[] machines = { new RemoteMachine() };
	}




	public class Main
	{
		static public Main main;

		#region Logging
		/*

		//Konsole

		static Window s_fullscreenWin;
		static IConsole s_logWin;
		//*/

		static char getSymbol( lib.LogType type )
		{
			switch( type )
			{
				case lib.LogType.Trace:
				return '.';
				case lib.LogType.Debug:
				return '-';
				case lib.LogType.Info:
				return ' ';
				case lib.LogType.Warn:
				return '+';
				case lib.LogType.Error:
				return '*';
				case lib.LogType.Fatal:
				return '*';
				default:
				return '?';
			}
		}



		static public void log( lib.LogEvent evt )
		{
			/*
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
	
		Invalid = 0,
		Trace = 1,
		Debug = 2,
		Info = 3,
		Warn = 4,
		Error = 5,
		Fatal = 6,
			
			
			/*/
			switch( evt.LogType )
			{
				case lib.LogType.Trace:
				Console.ForegroundColor = ConsoleColor.Gray;
				break;
				case lib.LogType.Debug:
				Console.ForegroundColor = ConsoleColor.Gray;
				break;
				case lib.LogType.Info:
				Console.ForegroundColor = ConsoleColor.White;
				break;
				case lib.LogType.Warn:
				Console.ForegroundColor = ConsoleColor.Yellow;
				break;
				case lib.LogType.Error:
				Console.ForegroundColor = ConsoleColor.Red;
				break;
				case lib.LogType.Fatal:
				Console.ForegroundColor = ConsoleColor.Yellow;
				break;
			}

			char sym = getSymbol( evt.LogType );

			string finalMsg = string.Format( "{0,-6}{1}| {2}", evt.Cat, sym.ToString(), evt.Msg );

			Console.WriteLine( $"{finalMsg}" );

			Console.ForegroundColor = ConsoleColor.Gray;
			//*/


		}

		#endregion Logging


		public lib.Clock clock { get; private set; }

		public Main( string configPath )
		{
			main = this;

			/* Konsole Logging
			s_fullscreenWin = new Window();
			s_fullscreenWin.BackgroundColor = ConsoleColor.DarkGray;
			s_fullscreenWin.Clear( ConsoleColor.DarkGray );

			var xStart = 2;
			var yStart = 2;

			var xSize = s_fullscreenWin.WindowWidth  - xStart * 2;
			var ySize = s_fullscreenWin.WindowHeight - yStart * 2;

			s_logWin = Window.Open( xStart, yStart, xSize, ySize, "Logging" );
			//1*/


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


			//*
			foreach( NetworkInterface nic in
					NetworkInterface.GetAllNetworkInterfaces() )
			{
				if( nic.OperationalStatus == OperationalStatus.Up )
				{
					lib.Log.logProps( nic, "Network Interface (up)", lib.LogType.Info, prefix: "  " );
				}
				else
				{
					lib.Log.info( "Network Interface (down)" );
					lib.Log.info( $"  {nic.Name} {nic.Description}" );
				}

				foreach( UnicastIPAddressInformation addrInfo in
						nic.GetIPProperties().UnicastAddresses )
				{
					//lib.Log.logProps( addrInfo, " Addresses", lib.LogType.Info, prefix: "    " );
					lib.Log.debug( $"    {addrInfo.Address}" );
				}
			}
			//*/

			string machineName = m_cfg.res.name; //+"/"+ep.Address.ToString() + ":" + ep.Port;

			var machines = new List<IPEndPoint>(); // new Dictionary<string, IPEndPoint>();

			//machines[machineName] = new IPEndPoint( IPAddress.Any, m_cfg.res.port );
			var localEndPoint = new IPEndPoint( IPAddress.Any, m_cfg.res.port );


			foreach( var mac in m_cfg.res.machines )
			{
				//var remoteName = $"remote_{mac.address}:{mac.port.ToString()}";

				//machines[remoteName] = new IPEndPoint( IPAddress.Parse( mac.address ), mac.port );

				machines.Add( new IPEndPoint( IPAddress.Parse( mac.address ), mac.port ) );
			}

			connectToOtherMachines( machineName, localEndPoint, machines );






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

		#region Shielded.Gossip
		protected void OnListenerError( object sender, Exception ex )
		{
		}

		private ConcurrentQueue<(string, string, object)> _messages = new ConcurrentQueue<(string, string, object)>();

		protected void OnMessage( string server, object msg )
		{
#if DEBUG
			if( msg is DirectMail dm && dm.Items != null && dm.Items.Length == 1 )
				_messages.Enqueue( (DateTime.Now.ToString( "hh:mm:ss.fff" ), server, dm.Items[0]) );
			else
				_messages.Enqueue( (DateTime.Now.ToString( "hh:mm:ss.fff" ), server, msg) );
#endif
		}

		protected void CheckProtocols()
		{
		}


		GossipBackend CreateBackend( ITransport transport, GossipConfiguration configuration )
		{
			return new GossipBackend( transport, configuration );
		}

		svc.TcpTransport CreateTransport( string ownId, IPEndPoint localEndPoint, IList<IPEndPoint> servers )
		{
			var transport = new svc.TcpTransport(ownId, localEndPoint, servers);
			transport.Error += OnListenerError;
			transport.StartListening();
			return transport;
		}


		void connectToOtherMachines( string ownIp, IPEndPoint localEndPoint, IList<IPEndPoint> servers )
		{
			var transport = CreateTransport( ownIp, localEndPoint, servers );
			var backend = CreateBackend(transport, new GossipConfiguration
			{
				GossipInterval = 250,
				AntiEntropyIdleTimeout = 2000,
			});

			var backendHandler = transport.MessageHandler;
			transport.MessageHandler = msg =>
			{
				OnMessage( ownIp, msg );
				return backendHandler( msg );
			};
		}
		#endregion Shielded.Gossip


		public void shutdown()
		{
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

			svc.Service.s_mgr.procMsg_block( 1000 );

		}


		res.Ref<ServerCfg> m_cfg;

		svc.Mgr m_svcMgr;

		svc.Machine m_machine;




	}



}
