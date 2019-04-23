using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace svmsg
{
#region Filters
[Serializable]
public class Filter
{
	virtual public bool pass( svc.Service svc )
	{
		return false;
	}

	virtual public void deliver( Server msg )
	{
	}

	virtual public Task<svc.Answer> deliverAsk( Server msg  )
	{
		return null;
	}
}

[Serializable]
public class FilterAll : Filter
{
	public static Filter filter = new FilterAll();

	override public bool pass( svc.Service svc )
	{
		return true;
	}
}

[Serializable]
public class FilterType<T> : Filter
{
	override public bool pass( svc.Service svc )
	{
		return svc.GetType().Equals( typeof( T ) ) || svc.GetType().IsSubclassOf( typeof( T ) );
	}
}

[Serializable]
public class FilterRoundRobin : Filter
{
	static long s_count = 0;
	
	public FilterRoundRobin( Filter filter )
	{
		m_filter = filter;
	}

	override public bool pass( svc.Service svc )
	{
		bool passes = m_filter.pass( svc );

		if( passes ) m_svcs.Add( svc );

		return false;
	}


	override public void deliver( Server msg )
	{
		if( m_svcs.Count > 0 )
		{
			int index = (int)( ++s_count % m_svcs.Count );

			m_svcs[ index ].deliver( msg );
		}
		else
		{
		}
	}

	override public Task<svc.Answer> deliverAsk( Server msg )
	{
		if( m_svcs.Count > 0 )
		{
			int index = (int)( ++s_count % m_svcs.Count );

			return m_svcs[ index ].deliverAsk( msg );
		}

		return null;
	}

	Filter m_filter;

	private List<svc.Service> m_svcs = new List<svc.Service>( 8 );
}
#endregion


#region Messages

[Serializable]
public class Server : msg.Msg
{
	public svc.Ref<svc.Service> sender { get; private set; }
	public Filter filter { get; private set; }

	public string caller { get; private set; }

	public Server() 
	{ 
		filter = FilterAll.filter; 
	}

	public Server( Filter _filter ) { filter = _filter; }

	public void setSender_fromService( svc.Service _sender )
	{
		sender = new svc.Ref<svc.Service>( _sender );
	}

	public void setCaller_fromService( string callerFilePath = "", string callerMemberName = "", int callerLineNumber = 0 )
	{
		caller = String.Format( "{0}: {1}: in {2}", callerFilePath, callerLineNumber, callerMemberName );
	}

	private void setCaller()
	{
		
	}
}

[Serializable]
public class Ping : svmsg.Server
{
	public ulong time;

	public Ping( Filter _filter ) : base( _filter ) { }
}

[Serializable]
public class MsgTest : svmsg.Server
{
	public ulong time;

	public MsgTest( Filter _filter ) : base( _filter ) { }
}

[Serializable]
public class MsgsProcessed : svmsg.Server
{
	public ulong count;

	public MsgsProcessed( Filter _filter ) : base( _filter ) { }
}

[Serializable]
class Shutdown : svmsg.Server
{
}

[Serializable]
class StartService : Server
{
	public string type;
	public string configPath;
	public string name;

	public StartService( Filter _filter ) : base( _filter ) { }

}

/*
[Serializable]
class ServicesAvailable : Server
{
	public string[] services;
}
*/

[Serializable]
public class ServiceReady : Server
{
	public svc.Ref<svc.Service> sref;
	public bool dontReply;

	public ServiceReady( Filter _filter ) : base( _filter ) { }
}

[Serializable]
class MsgRecv : Server
{
	///public uint			id;
	///public svc.Conn c;
	///public msg.Msg	msg;
}

[Serializable]
public class SpawnViewer : Server
{
	public SpawnViewer( Filter _filter ) : base( _filter ) { }

	public math.Vec3 pos;
	public float radius;
	public uint context;
}

[Serializable]
public class RemoveViewer : Server
{
	public RemoveViewer( Filter _filter ) : base( _filter ) { }

	public uint context;
}





[Serializable]
public class SpawnEnt : Server
{
	public string config;
	public string prefix;
	public math.Vec3d pos;
	public math.Vec3d dir;

	public SpawnEnt( Filter _filter ) : base( _filter ) { }
}

[Serializable]
public class DespawnEnt : Server
{
	///public ent.Ref ent;

	public DespawnEnt( Filter _filter ) : base( _filter ) { }
}

[Serializable]
public class ReconnectEnt : Server
{
	public string name;

	public ReconnectEnt( Filter _filter ) : base( _filter ) { }
}

[Serializable]
public class LoadEnt : Server
{
	public string name;

	public LoadEnt( Filter _filter ) : base( _filter ) { }
}

[Serializable]
public class ToConn : Server
{
	public uint context;
	///public ent.Ref ent;
	public msg.Msg msg;
}

[Serializable]
public class ToGame : Server
{
	///public ent.Ref ent;
	public msg.Msg msg;
}

[Serializable]
public class DebugTeleportTo : Server
{
	///public ent.Ref ent;
	public math.Vec3 to;
}

[Serializable]
public class EntSnapshot : Server
{
	public uint context;
	///public ent.Ref ent;
	public math.Vec3 pos;
	public math.Vec3 vel;
	public float rot;
	public float damage;
	public float energyUse;
}



#endregion

}
