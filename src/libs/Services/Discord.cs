using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using lib;

namespace svc
{



[Serializable]
public class DiscordCfg : lib.Config
{
	[lib.Desc( "0: Worker count is cores - 1\n<0: Worker count is cores - coresAdj\n>0: Absolute worker count" )]
	public int coresAdj = 0;
}

public partial class Discord : ServiceWithConfig<DiscordCfg>
{
	public Discord( lib.Token _id, res.Ref<DiscordCfg> _cfg )
		: base( _id, _cfg )
	{
		addHandler( typeof(svmsg.SpawnEnt), handleAll );

		m_mgr = new WorkerMgr<DiscordWorker>();

		m_mgr.createWorkers( Environment.ProcessorCount / 2, create );
	}

	DiscordWorker create() => new DiscordWorker( this );

	public void handleAll( msg.Msg msg )
	{

	}


	public override void run()
	{
		
	}

	WorkerMgr<DiscordWorker> m_mgr;
}

//Handlers
public partial class DiscordWorker : Worker
{
	public void handle( svmsg.SpawnEnt msg )
	{
	}
}






public partial class DiscordWorker : Worker
{
	public DiscordWorker( Discord phy )
	{
		m_discord = phy;


	}

	
	public override Service service()
	{
		return m_discord;
	}



	public override void run()
	{
		while( Running )
		{
			Thread.Sleep( 10 );
		}
	}


	


	Discord m_discord;

}





}
