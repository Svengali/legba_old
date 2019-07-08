using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Net;

namespace svc
{


public interface IMachine: IService
{
	void handle( svmsg.StartService start );
}



[Serializable]
public class MachineCfg : lib.Config
{
	public string	connectToAddress	= "0.0.0.0";
	public ushort	connectToPort			= 0;

}

public class Machine : ServiceWithConfig<MachineCfg>, IMachine, lib.IProcess
	{
	public Machine( lib.Token _id, res.Ref<MachineCfg> _cfg )
		: 
		base( _id, _cfg )
	{
	}

	override public void run()
	{
		//TODO: Make a way to close services off.
		while( m_running )
		{
			procMsg_block( 2 );
		}
	}

		void handle( svmsg.Shutdown stop )
		{
			m_running = false;
		}

		void handle( svmsg.Hello hello )
		{
			lib.Log.info( $"Got hello" );
		}

		public void handle( svmsg.StartService start )
	{
		Type[] types = new Type[ 2 ];
		object[] parms = new object[ 2 ];

		types[ 0 ] = typeof( lib.Token );

		Type svcType = Type.GetType( start.type );

		if( svcType != null )
		{
			Type cfgType = svcType.BaseType.GenericTypeArguments[0];

			//res.Ref cfg = res.Mgr.lookup( start.configPath, cfgType );

			var refGenType = typeof(res.Ref<>);

			var refType = refGenType.MakeGenericType( cfgType );

			var cfg = Activator.CreateInstance( refType, start.configPath );

			if( cfg != null )
			{
				types[ 1 ] = cfg.GetType();

				ConstructorInfo cons = svcType.GetConstructor( types );

				try
				{
					parms[0] = new lib.Token( start.name );
					parms[1] = cfg;

					lib.Log.info( $"Starting service {start.name} of type {refType.Name} using config {start.configPath}" );

					svc.Service s = (svc.Service)cons.Invoke( parms );

					svc.Service.s_mgr.start( s );
				}
				catch( Exception ex )
				{
					lib.Log.error( $"Exception while calling service constructor {ex}" );
				}
			}
			else
			{
				lib.Log.warn( $"Could not find service of type {start.type}" );
			}
		}
		else
		{
			lib.Log.warn( $"Could not find service of type {start.type}" );
		}

		if( cfg.res.connectToPort != 0 )
		{
			lib.Log.info( $"Connecting to {cfg.res.connectToAddress}:{cfg.res.connectToPort}" );

			m_sock = new Socket( SocketType.Stream, ProtocolType.Tcp );

			var didParse = IPAddress.TryParse( cfg.res.connectToAddress, out var ipAddress );

			m_sock.Connect( ipAddress, cfg.res.connectToPort );

			m_conn = new net.Conn( m_sock );

			m_sock.LingerState = new LingerOption( false, 0 );
			m_sock.NoDelay = true;

			var msg = new svmsg.Hello( new svmsg.FilterType<Machine>() );

			m_conn.send( msg );

		}


	}

	public void process( object obj )
	{
		var msg = obj as svmsg.Server;

		send( msg );

	}

	bool m_running = true;

	Socket m_sock;
	net.Conn m_conn;

	//TcpClient m_client;



	}


}
