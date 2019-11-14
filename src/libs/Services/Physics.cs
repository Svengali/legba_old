using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using lib;

namespace svc
{
	public interface ISvcPhysics : IService
	{

	}


	[Serializable]
	public class PhysicsCfg : lib.Config
	{
		[lib.Desc( "0: Worker count is cores - 1\n<0: Worker count is cores - coresAdj\n>0: Absolute worker count" )]
		public int coresAdj = 0;
	}

	public partial class Physics : ServiceWithConfig<PhysicsCfg>, ISvcPhysics
	{
		public Physics( lib.Token _id, res.Ref<PhysicsCfg> _cfg )
			: base( _id, _cfg )
		{
			addHandler( typeof( svmsg.SpawnEnt ), handleAll );

			m_mgr = new WorkerMgr<PhysicsWorker>();

			m_mgr.createWorkers( Environment.ProcessorCount / 2, create );
		}

		PhysicsWorker create() => new PhysicsWorker( this );

		public void handleAll( msg.Msg msg )
		{

		}


		public override void run()
		{

		}

		WorkerMgr<PhysicsWorker> m_mgr;
	}

	//Handlers
	public partial class PhysicsWorker : Worker
	{
		public void handle( svmsg.SpawnEnt msg )
		{
		}
	}

	public partial class PhysicsWorker : Worker
	{
		public PhysicsWorker( Physics phy )
		{
			m_physics = phy;


		}


		public override Service service()
		{
			return m_physics;
		}



		public override void run()
		{
			while( Running )
			{
				Thread.Sleep( 10 );
			}
		}





		Physics m_physics;

	}





}
