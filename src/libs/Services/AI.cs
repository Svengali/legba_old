using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace svc
{


[Serializable]
public class AICfg : lib.Config
{
	public uint NPCcount = 0;
}




public class AI : ServiceWithConfig<AICfg>
{
	public AI( lib.Token _id, res.Ref<AICfg> _cfg )
		: base(_id, _cfg)
	{
		m_clock = new lib.Clock(0);
	}

	override public void run()
	{
		///var ready = new svmsg.ServiceReady( new svmsg.FilterType<Universe>() );
		///ready.sref = sref;
		///send( ready );

		//TODO META: Should we just taskify each object update?

		//var syncContext = new SynchronizationContext();
		//SynchronizationContext.SetSynchronizationContext( new SynchronizationContext() );

		var baseMs = m_clock.ms;
		var nextTick = baseMs + 33;

		while(m_running)
		{
			var curMs = m_clock.ms;

			if(curMs >= nextTick)
			{
				var dtMs = curMs - baseMs;
				var dtSec = (double)dtMs / 1000.0;

				/*
				foreach( var kv in m_ents )
				{
					kv.Value.tick( dtSec );
				}
				*/

				var procMs = m_clock.ms;

				baseMs = curMs;
				nextTick = baseMs + 33;
			}

			if(m_tasks.Count < cfg.res.NPCcount)
			{
				doInterestingThings();
			}

			procMsg_block(1);
		}
	}

	void handle( svmsg.Shutdown stop )
	{
		m_running = false;
	}

	void doInterestingThings()
	{
		/*
		Action action = async () => {

			Thread.Sleep( (int)(1000.0 * m_rnd.NextDouble()) );

			//TODO: Load AIs instead of always spawning them.  
			var spawn = new svmsg.SpawnEnt( new svmsg.FilterRoundRobin( new svmsg.FilterType<Game>() ) );
			spawn.config = "config/ents/Player.cfg";
			spawn.prefix = "AI";

			var ans = await ask( spawn );

			if( ans.Length != 1 )
			{
				lib.Log.warn( "Could not spawn NPC." );
				return;
			}

			var resp = ans[ 0 ].obj as Game.LoadEntResp;

			var ent = resp.eref;


			while( true )
			{
				Thread.Sleep( (int)( m_rnd.NextDouble() * 10000.0 ) + 5000 );

				var x = (float)m_rnd.NextDouble() * 500.0f - 250.0f;
				var y = (float)m_rnd.NextDouble() * 500.0f - 250.0f;
				var z = (float)m_rnd.NextDouble() * 4.0f - 2.0f;

				var newPos = new math.Vec3f( x, y, z );

				var move = new msg.MoveTo();
				var toGame = new svmsg.ToGame();
				toGame.ent = ent;
				toGame.msg = move;

				//ent.owner.deliver( toGame );
				sendTo( toGame, ent.owner );

				await Task.Yield();
			}
		};

		//Task.Factory.StartNew(

		//var ts = new TaskScheduler();

		Task t = new Task( action, TaskCreationOptions.LongRunning );
		//t.Start( TaskScheduler.FromCurrentSynchronizationContext() );
		t.Start();
		m_tasks.Add( t );

		*/
	}

	private Random m_rnd = new Random();
	private bool m_running = true;

	private lib.Clock m_clock;

	private List<Task> m_tasks = new List<Task>();
}







}
