using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using lib;

namespace svc
{



public interface ISvcClient: IService
{

}


[Serializable]
public class ClientCfg : lib.Config
{
	[lib.Desc( "0: Worker count is cores - 1\n<0: Worker count is cores - coresAdj\n>0: Absolute worker count" )]
	public int coresAdj = 0;
}

public partial class Client : ServiceWithConfig<ClientCfg>, ISvcClient
{
	public Client( lib.Token _id, res.Ref<ClientCfg> _cfg )
		: base( _id, _cfg )
	{
		addHandler( typeof(svmsg.SpawnEnt), handleAll );

		m_mgr = new WorkerMgr<ClientWorker>();

		m_mgr.createWorkers( Environment.ProcessorCount / 2, create );
	}

	ClientWorker create() => new ClientWorker( this );

	public void handleAll( msg.Msg msg )
	{

	}


	public override void run()
	{
		
	}

	WorkerMgr<ClientWorker> m_mgr;
}

//Handlers
public partial class ClientWorker : Worker
{
	public void handle( svmsg.SpawnEnt msg )
	{
	}
}

public partial class ClientWorker : Worker
{
	public ClientWorker( Client phy )
	{
		m_Client = phy;


	}

	
	public override Service service()
	{
		return m_Client;
	}



	public override void run()
	{
		while( Running )
		{
			Thread.Sleep( 10 );
		}
	}


	


	Client m_Client;

}





}
