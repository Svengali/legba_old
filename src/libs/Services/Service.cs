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
using svmsg;
using System.Collections.Immutable;

namespace svc
{

[Serializable]
public struct Ref<T> where T : Service
{
	public Type type { get { return m_type; } }

	public T r
	{
		get
		{
			T svc = m_ref.Target as T;
			return svc;
		}
	}

	public Ref( Ref<Service> sref )
	{
		m_ref = sref.m_ref;
		m_type = sref.m_type;
	}

	public Ref( T svc )
	{
		m_ref = new WeakReference(svc);
		m_type = svc.GetType();
	}

	public void deliverDirectly( svmsg.Server msg )
	{
		T svc = m_ref.Target as T;

		if(svc != null)
		{
			svc.deliver(msg);
		}
		else
		{
			lib.Log.warn("Object was deleted in ref of type");
		}
	}

	private WeakReference m_ref;
	private Type m_type;
}

#region MixinsAndStates

[AttributeUsage(validOn: AttributeTargets.Class,  AllowMultiple = true)]
public class MixinAttribute : Attribute
{
	public Type type;
	public object[] parms;
}

public class Retry : state.State<StBase, Context>
{
		public override void onEnter( StBase oldState )
		{
			base.onEnter( oldState );
		}
	
}

public class Track : state.State<StBase, Context>
{
		public override void onEnter( StBase oldState )
		{
			base.onEnter( oldState );
		}
	
}

public class Context : state.Context<Context, StBase>
{
	public Service Service;
}


public class StBase : state.State<StBase, Context>
{

		public override void onEnter( StBase oldState )
		{
			base.onEnter( oldState );
		}

		void handle( svmsg.StartService msg ) { }



}

[Mixin(type = typeof(Retry), parms = new object[] {10, "hello"} )]
[Mixin(type = typeof(Track), parms = new object[] {10, "hello"} )]
public class StStarting : StBase
{

	// This is a possible 
	private void onEnter_Retry( StBase oldState)
	{
		base.onEnter( oldState );
	}

	private void onEnter_StStarting( StBase oldState)
	{
		Console.WriteLine( $"StStarting.onEnter" );

		onEnter_Retry( oldState );
	}

	public override void onEnter( StBase oldState )
	{
		onEnter_StStarting( oldState );
	}

	virtual public bool isDone()
	{
		return false;
	}

	public void checkDone()
	{
		if( isDone() )
		{
			var state = Context.Service.Running;

			Context.fsm.Transition( state );
		}
	}


}

public class StRunning : StBase
{





}

#endregion


[Serializable]
public struct Answer
{
	public readonly Ref<Service> svc;
	public readonly object obj;

	public Answer( Ref<svc.Service> _svc, object _obj )
	{
		svc = _svc;
		obj = _obj;
	}
}

struct MsgContext
{
	public svmsg.Server msg;
	//This allows our asker to wait until the other service has returned an answer
	public EventWaitHandle wait;
	public Answer response;
	public List<Task<Answer>> task;

	public MsgContext( svmsg.Server _msg )
	{
		msg = _msg;
		wait = new EventWaitHandle(false, EventResetMode.AutoReset);
		response = default;
		task = new List<Task<Answer>>();
	}

	public MsgContext( svmsg.Server _msg, bool isAsk )
	{
		msg = _msg;
		wait = isAsk ? new EventWaitHandle(false, EventResetMode.AutoReset) : null;
		response = default;
		task = new List<Task<Answer>>();
	}
}


public class Service
{
	public static Mgr mgr = null;

	public lib.Token id { get; private set; }
	public Ref<Service> sref { get { return new Ref<Service>(this); } }

	public bool QueueHasMessages { get { return !m_q.IsEmpty; } }

	public Service( lib.Token _id )
	{
		id = _id;
	}

	public void sendTo<T>( svmsg.Server msg, svc.Ref<T> sref, [CallerFilePath]string callerFilePath = "", [CallerMemberName]string callerMemberName = "", [CallerLineNumber]int callerLineNumber = 0 ) where T : Service
	{
		msg.setSender_fromService(this);
		msg.setCaller_fromService(callerFilePath, callerMemberName, callerLineNumber);
		sref.deliverDirectly(msg);
	}

	public void send( svmsg.Server msg, [CallerFilePath]string callerFilePath = "", [CallerMemberName]string callerMemberName = "", [CallerLineNumber]int callerLineNumber = 0 )
	{
		msg.setSender_fromService(this);
		msg.setCaller_fromService(callerFilePath, callerMemberName, callerLineNumber);
		mgr.send_fromService(msg);
	}

	public Task<Answer[]> ask( svmsg.Server msg, [CallerFilePath]string callerFilePath = "", [CallerMemberName]string callerMemberName = "", [CallerLineNumber]int callerLineNumber = 0 )
	{
		msg.setSender_fromService(this);
		msg.setCaller_fromService(callerFilePath, callerMemberName, callerLineNumber);
		return mgr.ask_fromService(msg);
	}


	public void deliver( svmsg.Server msg )
	{
		MsgContext c = new MsgContext( msg );
		m_q.Enqueue(c);
		m_event.Set();
	}

	public Task<Answer> deliverAsk( svmsg.Server msg )
	{
		MsgContext c = new MsgContext( msg, isAsk: true );
		m_q.Enqueue(c);


		var a = new Func<svc.Answer>(() =>
		{
			c.wait.WaitOne();
			return c.response;
		});

		var t = new Task<svc.Answer>(a, TaskCreationOptions.LongRunning);
		t.Start();

		return t;
	}

	virtual public void run()
	{
	}

	delegate void fnHandleGeneric<T>( svmsg.Server msg, Action<T> fn ) where T : class;

	/*
	void handleGeneric<T>( svmsg.Server msg, Action<T> fn ) where T : class
	{
		fn(msg as T);
	}
	*/

	// Single threaded.  Non-reentrent
	void procMsg( int maxCount )
	{
		var args = new Type[ 1 ];
		var thisType = GetType();

		if(m_qMax < m_q.Count)
		{
			lib.Log.warn("Service Q hit highwater of {0} in {1}.", m_q.Count, GetType());
			m_qMax = (uint)m_q.Count;
		}

		maxCount = math.fn.Max(maxCount, m_q.Count);

		while(maxCount-- > 0 && m_q.Count > 0)
		{
			MsgContext c;
			m_q.TryDequeue(out c);

			if(c.msg != null)
			{
				if(c.wait == null)
				{
					args[ 0 ] = c.msg.GetType();
					Action<svmsg.Server> fn = null;

					if(!m_handlingMethod.TryGetValue(args[ 0 ], out fn))
					{
						var mi = thisType.GetMethod("handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);

						ParameterExpression pe = Expression.Parameter(typeof(svmsg.Server), "msgIn");

						var exConvert = Expression.Convert(pe, args[ 0 ]);

						var exParams = new Expression[ 1 ];
						exParams[ 0 ] = exConvert;

						var exThis = Expression.Constant(this);
						var exCall = Expression.Call(exThis, mi, exParams);

						fn = Expression.Lambda<Action<svmsg.Server>>(exCall, pe).Compile();

						m_handlingMethod[ args[ 0 ] ] = fn;
					}

					if(fn != null)
					{
						try
						{
							//mm_params[ 0 ] = c.msg;

							fn(c.msg);

							//mi.Invoke( this, mm_params );
						}
						catch(Exception e)
						{
							lib.Log.warn("Exception while calling {0}.  {1}", c.msg.GetType(), e);
						}
					}
					else
					{
						unhandled(c.msg);
					}
				}
				else
				{
					args[ 0 ] = c.msg.GetType();

					Func<svmsg.Server, object> fn;

					if( !m_handlingAsk.TryGetValue( args[0], out fn ) )
					{
						var mi = thisType.GetMethod("handleAsk", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);

						ParameterExpression pe = Expression.Parameter(typeof(svmsg.Server), "msgIn");

						var exConvert = Expression.Convert(pe, args[ 0 ]);

						var exParams = new Expression[ 1 ];
						exParams[ 0 ] = exConvert;

						var exThis = Expression.Constant(this);
						var exCall = Expression.Call(exThis, mi, exParams);

						fn = Expression.Lambda<Func<svmsg.Server, object>>(exCall, pe).Compile();

						m_handlingAsk[ args[ 0 ] ] = fn;
					}

					if(fn != null)
					{
						try
						{
							object resp = fn( c.msg );

							c.response = new Answer(new Ref<Service>(this), resp);

							c.wait.Set();

							//retEWH(c.wait);
						}
						catch(Exception e)
						{
							lib.Log.warn("Exception while calling {0}.  {1}", c.msg.GetType(), e);
						}
					}
					else
					{
						unhandled( c.msg );
					}

					//time.Stop();
					//lib.Log.info( "{0} to handleAsk", time.DurationMS );
				}
			}
		}

		if(m_q.IsEmpty)
		{
			//m_event.Reset();
		}
	}

	private void unhandled( Server msg )
	{
		throw new NotImplementedException();
	}

	public void procMsg_block( int wait )
	{
		procMsg( 1000 );
		m_event.WaitOne(wait);
	}

	public void procMsg_block()
	{
		procMsg_block( 1000 );
	}


	public virtual void handle( svmsg.ServiceReady ready )
	{

	}

	public object handleAsk( svmsg.Ping ping )
	{
		var dt = 0UL; //(ulong)sv.Main.main.clock.ms - ping.time;

		lib.Log.info("Got ping {0}", dt);

		return ping;
	}

	public void addHandler( Type msgType, Action<svmsg.Server> fn )
	{
		m_handlingMethod[ msgType ] = fn;
	}

	internal StRunning Running => new StRunning();

	Random m_rand = new Random();


	ConcurrentQueue<MsgContext> m_q = new ConcurrentQueue<MsgContext>();

	//This event allows us to have a very light service that mostly sleeps until it gets a message
	EventWaitHandle m_event = new EventWaitHandle( false, EventResetMode.ManualReset );

	Dictionary<Type, Action<svmsg.Server>> m_handlingMethod	= new Dictionary<Type, Action<svmsg.Server>>();
	Dictionary<Type, Func<svmsg.Server, object>> m_handlingAsk		= new Dictionary<Type, Func<svmsg.Server, object>>();

	uint m_qMax = 100;
}

public class ServiceWithConfig<TCfg> : Service where TCfg : class
{
	public res.Ref<TCfg> cfg { get; protected set; }

	public ServiceWithConfig( lib.Token _id, res.Ref<TCfg> _cfg ) : base(_id)
	{
		cfg = _cfg;
	}
}

#if false

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


#endif


public class Mgr
{
	public Mgr()
	{
		Service.mgr = this;
	}

	public void start( Service svc )
	{
		m_pendingService.Enqueue(svc);
	}

	public void send_fromService( svmsg.Server msg )
	{
		/*
		var c = new MsgContext();
		c.msg = msg;
		m_q.Enqueue( c );
		m_wait.Set();
		*/

		procMsg(msg);
	}

	public Task<Answer[]> ask_fromService( svmsg.Server msg )
	{

		var c = new MsgContext();
		c.msg = msg;
		c.wait = new EventWaitHandle(false, EventResetMode.AutoReset);
		c.task = new List<Task<Answer>>();
		m_q.Enqueue(c);
		m_wait.Set();



		var a = new Func<svc.Answer[]>(() =>
		{
			//var time = new lib.Timer();
			//time.Start();

			c.wait.WaitOne();
			Task<Answer>[] tasks = c.task.ToArray();
			Task.WaitAll(tasks);

			var list = new List<Answer>();
			for(uint i = 0; i < tasks.Length; ++i)
			{
				if(tasks[ i ].Result.obj != null)
				{
					list.Add(tasks[ i ].Result);
				}
			}

			/*
			Answer[] arr = new Answer[ tasks.Length ];
			for( uint i = 0; i < tasks.Length; ++i )
			{
				arr[i] = tasks[i].Result;
			}
			*/

			//time.Stop();
			//lib.Log.info( "{0} to task ask_fromService", time.DurationMS );

			return list.ToArray();
		});

		var t = new Task<svc.Answer[]>(a, TaskCreationOptions.LongRunning);
		t.Start();

		return t;
	}

	public void procMsg_block( int maxMS )
	{
		var early = m_wait.WaitOne(maxMS);
		processMessages();
	}


	public void procMsg( svmsg.Server msg )
	{

		var services = m_services;
		
		foreach(var p in services)
		{
			if(msg.filter.pass(p.Value))
			{
				p.Value.deliver(msg);
			}
		}

			msg.filter.deliver(msg);
	}

	public void processMessages()
	{
		if(m_qMax < m_q.Count)
		{
			lib.Log.warn("Service Q hit highwater of {0} in {1}.", m_q.Count, GetType());
			m_qMax = (uint)m_q.Count;
		}

		while(m_q.Count > 0)
		{
			MsgContext c;
			m_q.TryDequeue(out c);

			if(c.msg != null)
			{
				if(c.wait == null)
				{
					procMsg(c.msg);
				}
				else
				{
					foreach(var p in m_services)
					{
						if(c.msg.filter.pass(p.Value))
						{
							var t = p.Value.deliverAsk(c.msg);
							if(t != null)
								c.task.Add(t);
						}
					}

					var tf = c.msg.filter.deliverAsk(c.msg);
					if(tf != null)
						c.task.Add(tf);

					c.wait.Set();
					//c.response = c.task.Result;

				}
			}
		}

		while(!m_pendingService.IsEmpty)
		{
			Service svc = null;

			m_pendingService.TryDequeue(out svc);

			if(svc != null)
			{
				lib.Log.info("Starting service {0}", svc.ToString());

				ImmutableInterlocked.AddOrUpdate( ref m_services, svc.id, svc, (k, v) => svc );

				var thread = new Thread(new ThreadStart(svc.run));

				thread.Start();
			}
		}
	}

	/*

	public void save( cm.I_Savable obj )
	{
		var filename = obj.savename();

		var path = "save/" + filename + ".xml";

		if( File.Exists( path ) )
		{
			var savepath = "save/archive/"+filename+"_"+DateTime.Now.ToBinary()+".xml";

			File.Move( path, savepath );
		}

		var filestream = new FileStream( path, FileMode.CreateNew );

		var formatter = new lib.XmlFormatter2();

		formatter.Serialize( filestream, obj );

		filestream.Close();
	}

	*/

	public object load( string filename )
	{
		var path = "save/" + filename + ".xml";

		if(!File.Exists(path))
			return false;

		var filestream = new FileStream(path, FileMode.Open);

		var formatter = new lib.XmlFormatter2();

		object obj = formatter.Deserialize(filestream);

		filestream.Close();

		return obj;
	}

	//private lib.XmlFormatter2 m_formatter = new lib.XmlFormatter2();
	//private Dictionary<lib.Token, Service> m_services = new Dictionary<lib.Token, Service>();
	ImmutableDictionary<lib.Token, Service> m_services = ImmutableDictionary<lib.Token, Service>.Empty;


	private ConcurrentQueue<MsgContext> m_q = new ConcurrentQueue<MsgContext>();
	private ConcurrentQueue<svc.Service> m_pendingService = new ConcurrentQueue<svc.Service>();
	private EventWaitHandle m_wait = new EventWaitHandle( true, EventResetMode.AutoReset );

	private uint m_qMax = 10000;

	//private ConcurrentDictionary<
}







}
