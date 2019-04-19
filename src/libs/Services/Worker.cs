using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace svc
{







public class Worker : Handler
{
	public bool Running => m_running;

	public virtual void run()
	{

	}

	public void stop()
	{
		m_running = false;
	}

	volatile bool m_running = true;
}

struct WorkerThreads<TWORKER>
{
	public TWORKER Worker { get; set; }
	public Thread Thread { get; set; }
}

public class WorkerMgr<TWORKER> where TWORKER: Worker
{
	public WorkerMgr()
	{
	}

	public void createWorkers( int count, Func<TWORKER> fnCreate )
	{
		for( int i = 0; i < count; ++i )
		{
			var w = fnCreate();

			var start = new ThreadStart( w.run );

			var t = new Thread( start );

			t.Start();

		}

	}


	ImmutableList<WorkerThreads<TWORKER>> m_workers;
}


}
