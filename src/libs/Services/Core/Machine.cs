﻿using System;
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

public class Machine : ServiceWithConfig<MachineCfg>, IMachine
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

			m_client = new TcpClient( cfg.res.connectToAddress, cfg.res.connectToPort );
			m_client.Connect( cfg.res.connectToAddress, cfg.res.connectToPort );

			m_client.LingerState = new LingerOption( false, 0 );
			m_client.NoDelay = true;

			lib.Log.expected( m_client.Connected, " Connected." );

			lib.Log.info( $"Connected: {m_client.Connected}" );

		}


	}

	bool m_running = true;

	TcpClient m_client;



	}


}
