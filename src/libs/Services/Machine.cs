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

namespace svc
{

[Serializable]
public class MachineCfg : lib.Config
{
}

public class Machine : ServiceWithConfig<MachineCfg>
{
	public Machine( lib.Token _id, res.Ref<MachineCfg> _cfg )
		: base( _id, _cfg )
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

	void handle( svmsg.StartService start )
	{
		Type[] types = new Type[ 2 ];
		object[] parms = new object[ 2 ];

		types[ 0 ] = typeof( lib.Token );

		Type svcType = Type.GetType( start.type );

		if( svcType != null )
		{
			Type cfgType = svcType.BaseType.GenericTypeArguments[0];

			res.Ref cfg = res.Mgr.load( start.configPath, cfgType );

			if( cfg != null )
			{
				types[ 1 ] = cfg.GetType();

				ConstructorInfo cons = svcType.GetConstructor( types );

				try
				{
					parms[0] = new lib.Token( start.name );
					parms[1] = cfg;

					svc.Service s = (svc.Service)cons.Invoke( parms );

					svc.Service.mgr.start( s );
				}
				catch( Exception e )
				{
					lib.Log.warn( "Exception while calling service constructor {0}", e );
				}
			}
			else
			{
				lib.Log.warn( "Could not find service of type {0}", start.type );
			}
		}
		else
		{
			lib.Log.warn( "Could not find service of type {0}", start.type );
		}
	}

	private bool m_running = true;
}


}
