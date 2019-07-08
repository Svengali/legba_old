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
/*
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
*/
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

public struct MsgContext
{
	public svmsg.Server m;
	//This allows our asker to wait until the other service has returned an answer
	public EventWaitHandle wait;
	public Answer response;
	public List<Task<Answer>> task;

	static public MsgContext msg( svmsg.Server _msg )
	{
		return new MsgContext( _msg, false );
	}

	static public MsgContext ask( svmsg.Server _msg )
	{
		return new MsgContext( _msg, true );
	}

	public MsgContext( svmsg.Server _msg, bool isAsk )
	{
		m = _msg;
		wait = isAsk ? new EventWaitHandle(false, EventResetMode.AutoReset) : null;
		response = default;
		task = new List<Task<Answer>>();
	}
}

public partial class Handler
{


	// Single threaded.  Non-reentrent
	void procMsg( int maxCount )
	{
		var args = new Type[ 1 ];
		var thisType = GetType();

		if(m_qMax < m_q.Count)
		{
			lib.Log.warn( $"Service Q hit highwater of {m_q.Count} in {GetType()}." );
			m_qMax = (uint)m_q.Count;
		}

		maxCount = Math.Max( maxCount, m_q.Count );

		while(maxCount-- > 0 && m_q.Count > 0)
		{
			MsgContext c;
			m_q.TryDequeue(out c);

			if(c.m != null)
			{
				if(c.wait == null)
				{
					args[ 0 ] = c.m.GetType();
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

							fn( c.m );

							//mi.Invoke( this, mm_params );
						}
						catch(Exception e)
						{
							lib.Log.error( $"Exception while calling { c.m.GetType()}.  {e}" );
						}
					}
					else
					{
						unhandled( c.m );
					}
				}
				else
				{
					args[ 0 ] = c.m.GetType();

					Func<svmsg.Server, object> fn;

					if( !m_handlingAsk.TryGetValue( args[0], out fn ) )
					{
						var mi = thisType.GetMethod("handleAsk", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);

						if( mi != null )
						{
							ParameterExpression pe = Expression.Parameter(typeof(svmsg.Server), "msgIn");

							var exConvert = Expression.Convert(pe, args[ 0 ]);

							var exParams = new Expression[ 1 ];
							exParams[ 0 ] = exConvert;

							var exThis = Expression.Constant(this);
							var exCall = Expression.Call(exThis, mi, exParams);

							fn = Expression.Lambda<Func<svmsg.Server, object>>(exCall, pe).Compile();

							m_handlingAsk[ args[ 0 ] ] = fn;
						}
					}

					if(fn != null)
					{
						try
						{
							object resp = fn( c.m );

							var svc = service();

							c.response = new Answer(new Ref<Service>(svc), resp);

							c.wait.Set();

							//retEWH(c.wait);
						}
						catch(Exception ex)
						{
							lib.Log.warn( $"Exception while calling {c.m.GetType()}.  Ex {ex}" );
						}
					}
					else
					{
						unhandled( c.m );
					}

						//time.Stop();
						//lib.Log.info( $"{time.DurationMS} to handleAsk" );
					}
				}
		}

		if(m_q.IsEmpty)
		{
			//m_event.Reset();
		}
	}

	public virtual Service service()
	{
		throw new NotImplementedException();
	}

	private void unhandled( svmsg.Server msg )
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

	public void addHandler( Type msgType, Action<svmsg.Server> fn )
	{
		m_handlingMethod[ msgType ] = fn;
	}


	//This event allows us to have a very light service that mostly sleeps until it gets a message
	protected EventWaitHandle m_event = new EventWaitHandle( false, EventResetMode.ManualReset );

	protected ConcurrentQueue<MsgContext> m_q															= new ConcurrentQueue<MsgContext>();
	protected Dictionary<Type, Action<svmsg.Server>> m_handlingMethod			= new Dictionary<Type, Action<svmsg.Server>>();
	protected Dictionary<Type, Func<svmsg.Server, object>> m_handlingAsk	= new Dictionary<Type, Func<svmsg.Server, object>>();

	protected uint m_qMax = 10000;
}



public partial class Service : Handler, IService
{
	public static Mgr s_mgr = null;

	public lib.Token id { get; private set; }
	public Ref<Service> sref { get { return new Ref<Service>(this); } }

	public bool QueueHasMessages { get { return !m_q.IsEmpty; } }

	public ImmutableList<Type> Services { get; private set; }

	public Service( lib.Token _id )
	{
		id = _id;
		gatherServices();
	}

	
	public override Service service()
	{
		return this;
	}

	void gatherServices()
	{
		var iserviceType = typeof( IService );

		var allInterfaces = GetType().GetInterfaces();

		var bldServices = ImmutableList<Type>.Empty.ToBuilder();

		foreach( var iface in allInterfaces )
		{
			if( iface == iserviceType ) continue;

			if( iserviceType.IsAssignableFrom( iface ) )
			{
				bldServices.Add( iface );
			}
		}

		Services = bldServices.ToImmutable();
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
		s_mgr.send_fromService(msg);
	}

	public Task<Answer[]> ask( svmsg.Server msg, [CallerFilePath]string callerFilePath = "", [CallerMemberName]string callerMemberName = "", [CallerLineNumber]int callerLineNumber = 0 )
	{
		msg.setSender_fromService(this);
		msg.setCaller_fromService(callerFilePath, callerMemberName, callerLineNumber);
		return s_mgr.ask_fromService(msg);
	}


	public void deliver( svmsg.Server msg )
	{
		MsgContext c = MsgContext.msg( msg );
		m_q.Enqueue(c);
		m_event.Set();
	}

	public Task<Answer> deliverAsk( svmsg.Server msg )
	{
		MsgContext c = MsgContext.ask( msg );
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

	//delegate void fnHandleGeneric<T>( svmsg.Server msg, Action<T> fn ) where T : class;

	/*
	void handleGeneric<T>( svmsg.Server msg, Action<T> fn ) where T : class
	{
		fn(msg as T);
	}
	*/


	//internal StRunning Running => new StRunning();

	Random m_rand = new Random();




}

// Handlers
public partial class Service
{
	public virtual void handle( svmsg.ServiceReady ready )
	{

	}

	public object handleAsk( svmsg.Ping ping )
	{
		var dt = 0UL; //(ulong)sv.Main.main.clock.ms - ping.time;

		lib.Log.info( $"Got ping {dt}" );

		return ping;
	}
}


}
