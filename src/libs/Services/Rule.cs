using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using lib;

namespace svc
{

public interface ISvcRule
{

}

[Serializable]
public class RuleCfg : lib.Config
{
	[lib.Desc( "0: Worker count is cores - 1\n<0: Worker count is cores - coresAdj\n>0: Absolute worker count" )]
	public int coresAdj = 0;
}

public partial class Rule : ServiceWithConfig<RuleCfg>, ISvcRule
{
	public Rule( lib.Token _id, res.Ref<RuleCfg> _cfg )
		: base( _id, _cfg )
	{
		addHandler( typeof(svmsg.SpawnEnt), handleAll );

		m_mgr = new WorkerMgr<RuleWorker>();

		m_mgr.createWorkers( Environment.ProcessorCount / 2, create );
	}

	RuleWorker create() => new RuleWorker( this );

	public void handleAll( msg.Msg msg )
	{

	}


	public override void run()
	{
		
	}

	WorkerMgr<RuleWorker> m_mgr;
}

//Handlers
public partial class RuleWorker : Worker
{
	public void handle( svmsg.SpawnEnt msg )
	{
	}
}

public partial class RuleWorker : Worker
{
	public RuleWorker( Rule phy )
	{
		m_rule = phy;


	}

	
	public override Service service()
	{
		return m_rule;
	}



	public override void run()
	{
		while( Running )
		{
			Thread.Sleep( 10 );
		}
	}


	


	Rule m_rule;

}





}
