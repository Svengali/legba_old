using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
//using lib;

namespace svc
{

public interface ISvcTextOutput: IService
{






}




[Serializable]
public class DiscordCfg : lib.Config
{
	[lib.Desc( "0: Worker count is cores - 1\n<0: Worker count is cores - coresAdj\n>0: Absolute worker count" )]
	public int coresAdj = 0;
}

public partial class Discord : ServiceWithConfig<DiscordCfg>, ISvcTextOutput
{
	public Discord( lib.Token _id, res.Ref<DiscordCfg> _cfg )
		: base( _id, _cfg )
	{
		addHandler( typeof(svmsg.SpawnEnt), handleAll );

		m_mgr = new WorkerMgr<DiscordWorker>();

		m_mgr.createWorkers( Environment.ProcessorCount / 2, create );

		// It is recommended to Dispose of a client when you are finished
		// using it, at the end of your app's lifetime.
		m_client = new DiscordSocketClient();

		m_client.Log += CBLogAsync;
		m_client.Ready += Ready_cb;
		m_client.MessageReceived += MessageReceived_cb;

	}

	DiscordWorker create() => new DiscordWorker( this );

	void handleAll( msg.Msg msg )
	{

	}


	void handle( svmsg.StartService start )
	{
	}


	public override void run()
	{
		
	}


  private Task CBLogAsync(LogMessage log)
  {
      Console.WriteLine(log.ToString());
      return Task.CompletedTask;
  }

  // The Ready event indicates that the client has opened a
  // connection and it is now safe to access the cache.
  private Task Ready_cb()
  {
      Console.WriteLine($"{m_client.CurrentUser} is connected!");

      var guilds = m_client.Guilds;

      foreach( var guild in guilds )
      {
      }



      return Task.CompletedTask;
  }

  // This is not the recommended way to write a bot - consider
  // reading over the Commands Framework sample.
  private async Task MessageReceived_cb(SocketMessage message)
  {
      // The bot should never respond to itself.
      if (message.Author.Id == m_client.CurrentUser.Id)
          return;

      if (message.Content == "!ping")
          await message.Channel.SendMessageAsync("pong!");
  }


	WorkerMgr<DiscordWorker> m_mgr;

  readonly DiscordSocketClient m_client;

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
