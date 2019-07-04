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

namespace sv
{







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
		m_strings[ 0 ] = "Str 0";
		m_strings[ 1 ] = "Str 1";

		m_comList[ new lib.Token( "Token" ) ] = "Token";
		m_comList[ new lib.Token( "Token_2" ) ] = "Token_2";
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
	public int		port = 8008;

	public res.Ref<svc.MachineCfg> machineCfg;

	public NewService[] services = { new NewService() };
	public string[] demand = { };
}


public class Main
{
	static public Main main;

	public lib.Clock clock { get; private set; }

	public Main( string configPath )
	{
		main = this;

		/*
		{
			var serTest = new SerializationTest();

			var filestream = new FileStream( "serTest.xml", FileMode.Create );

			var formatter = new lib.XmlFormatter2();

			formatter.Serialize( filestream, serTest );

			filestream.Close();

			filestream = new FileStream( "serTest.xml", FileMode.Open );

			formatter = new lib.XmlFormatter2();

			var serTestLoad = formatter.Deserialize( filestream ) as SerializationTest;

			filestream.Close();

		}
		//*/



		lib.Util.checkAndAddDirectory( "logs" );
		// save/static and save/dynamic are created when they dont exist in order to create the universe
		lib.Util.checkAndAddDirectory( "save/players" );


		lib.Util.checkAndAddDirectory( "save/archive/static" );
		lib.Util.checkAndAddDirectory( "save/archive/dynamic" );
		lib.Util.checkAndAddDirectory( "save/archive/players" );

		clock = new lib.Clock( 0 );

		m_svcMgr = new svc.Mgr();

		//Load configs
		lib.Log.info( "Loading config {0}", configPath );
		//m_cfg = lib.Config.load<ServerCfg>( configPath );
		m_cfg = res.Mgr.lookup<ServerCfg>( configPath );

		lib.Log.info( $"Starting {m_cfg.res.port}" );
		IPEndPoint localEP = new IPEndPoint( IPAddress.Parse( m_cfg.res.address ), m_cfg.res.port ); 

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

		IPEndPoint ep = (IPEndPoint)m_listener.LocalEndpoint;

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
