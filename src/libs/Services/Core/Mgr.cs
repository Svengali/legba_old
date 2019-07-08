using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Immutable;

namespace svc
{

public class Mgr
{
	public Mgr()
	{
		Service.s_mgr = this;
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

		var c = MsgContext.ask( msg );
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
			//lib.Log.info( $"{time.DurationMS} to task ask_fromService" );

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
			lib.Log.warn( $"Service Q hit highwater of {m_q.Count} in {GetType()}." );
			m_qMax = (uint)m_q.Count;
		}

		while(m_q.Count > 0)
		{
			MsgContext c;
			m_q.TryDequeue(out c);

			if(c.m != null)
			{
				if(c.wait == null)
				{
					procMsg(c.m);
				}
				else
				{
					foreach(var p in m_services)
					{
						if(c.m.filter.pass(p.Value))
						{
							var t = p.Value.deliverAsk(c.m);
							if(t != null)
								c.task.Add(t);
						}
					}

					var tf = c.m.filter.deliverAsk( c.m );
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
				lib.Log.info( $"Starting service {svc}" );

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
