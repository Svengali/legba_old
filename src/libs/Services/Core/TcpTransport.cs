using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Shielded.Gossip;
using Shielded;
using System.Collections.Immutable;
using System.Text;

namespace svc
{






	/// <summary>
	/// Implementation of <see cref="ITransport"/> using a very simple TCP-based protocol.
	/// </summary>
	public class TcpTransport : ITransport
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ownId">The ID of this server.</param>
		/// <param name="serverIPs">The dictionary with server IDs and IP endpoints, including this server. Do not make later
		/// changes to any of the IPEndPoint objects!</param>
		public TcpTransport( string ownId, IList<IPEndPoint> serverIPs )
		{
			OwnId = ownId ?? throw new ArgumentNullException( nameof( ownId ) );

			if( serverIPs == null )
				throw new ArgumentNullException( nameof( serverIPs ) );


			//LocalEndpoint = serverIPs[ownId];

			/*
			(ServerIPs, _clientConnections) = Shield.InTransaction( () =>
			{
				var ips = new ShieldedDict<string, IPEndPoint>(serverIPs.Where(kvp => !StringComparer.InvariantCultureIgnoreCase.Equals(kvp.Key, ownId)));

				var clients = new ConcurrentDictionary<string, TcpClientConnection>(
										ips.Select(kvp => new KeyValuePair<string, TcpClientConnection>(kvp, new TcpClientConnection(this, kvp))));

				Shield.PreCommit( () => ips.TryGetValue( "any", out var _ ) || true,
						() => Shield.SyncSideEffect( UpdateClientConnections ) );
				return (ips, clients);
			} );
			*/


		}

		public string OwnId { get; private set; }
		public readonly IPEndPoint LocalEndpoint;

		/// <summary>
		/// Other servers known to this one. You may make changes to the dictionary, but please treat all IPEndPoint objects
		/// as immutable! Create a new IPEndPoint if you wish to change the address of a server.
		/// </summary>

		//public readonly ShieldedDict<string, IPEndPoint> TestServerIPs;


		public ICollection<string> Servers => (ICollection<string>)_servers.Select( v => v.Id );


		public readonly ShieldedSeqNc<IPEndPoint> PotentialServers = new ShieldedSeqNc<IPEndPoint>();

		/// <summary>
		/// Timeout in milliseconds for detecting a half-open connection. Default is 30 seconds.
		/// </summary>
		public int ReceiveTimeout { get; set; } = 30_000;

		/// <summary>
		/// Every this many milliseconds we transmit a keep-alive message over our active persistent connections,
		/// if nothing else gets sent. Default is 15 seconds. Should be smaller than the <see cref="ReceiveTimeout"/>, of course.
		/// </summary>
		public int KeepAliveInterval { get; set; } = 15_000;

		public MessageHandler MessageHandler { get; set; }

		private TcpListener _listener;
		private readonly object _listenerLock = new object();


		//private readonly ConcurrentDictionary<string, TcpClientConnection> _clientConnections;
		//private readonly ConcurrentDictionary<TcpClient, object> _serverConnections = new ConcurrentDictionary<TcpClient, object>();

		// @@@ TODO this should probably be immutable
		//private readonly ConcurrentBag<TcpClientConnection> _connections = new ConcurrentBag<TcpClientConnection>();

		ImmutableList<TcpClientConnection> _attempts = ImmutableList<TcpClientConnection>.Empty;

		ImmutableList<TcpClientConnection> _servers = ImmutableList<TcpClientConnection>.Empty;

		/*
		static bool Contains<T>( this ConcurrentBag<T> bag, Predicate<T> pred )
		{
			foreach( var thing in bag )
			{
				
			}

			return false;
		}
		*/

		private void UpdateClientConnections()
		{
			var currentAttempts = _attempts;

			foreach( var server in PotentialServers.Consume )
			{
				if( _servers.Exists( other => other.TargetEndPoint == server ) )
				{
					// TODO LOG 
					continue;
				}

				if( _attempts.Exists( other => other.TargetEndPoint == server ) )
				{
					// TODO LOG 
					continue;
				}

				var newConn = new TcpClientConnection( this, server );

				_attempts = _attempts.Add( newConn );

				byte[] bytes = Encoding.ASCII.GetBytes( OwnId );

				newConn.Send( bytes );
			}

		}

		/// <summary>
		/// Stop the server. Safe to call if already stopped.
		/// </summary>
		public void StopListening()
		{
			lock( _listenerLock )
			{
				var listener = _listener;
				if( listener != null )
				{
					try
					{
						listener.Stop();
					}
					catch { }
					_listener = null;
				}
			}
		}

		/// <summary>
		/// Calls <see cref="StopListening"/>, and also disposes all outgoing persistent connections.
		/// </summary>
		public void Dispose()
		{
			StopListening();

			foreach( var attempts in _attempts )
				attempts.Dispose();
			foreach( var servers in _servers )
				servers.Dispose();

			_servers = _servers.Clear();
		}

		/// <summary>
		/// Start the server. Safe to call if already running, does nothing then.
		/// </summary>
		public async void StartListening()
		{
			TcpListener listener;
			lock( _listenerLock )
			{
				if( _listener != null )
					return;
				listener = _listener = new TcpListener( LocalEndpoint );
			}
			try
			{
				listener.Start();


				while( true )
				{
					var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

					client.ReceiveTimeout = ReceiveTimeout;

					var newConn = new TcpClientConnection( this, client );

					_servers = _servers.Add( newConn );

					var stream = client.GetStream();
					MessageLoop( newConn, client, msg => SendFramed( stream, msg ),
							( c, ex ) =>
							{
								//TODO HACK remove it later
								//if( _serverConnections.TryRemove( c, out var _ ) && ex != null )
									RaiseError( ex );
							} );
				}
			}
			catch( ObjectDisposedException ) { }
			catch( Exception ex )
			{
				StopListening();
				RaiseError( ex );
			}
		}

		internal async void MessageLoop( TcpClientConnection connection, TcpClient client, Func<byte[], Task> sender, Action<TcpClient, Exception> onCloseOrError )
		{
			async Task<bool> ReceiveBuffer( NetworkStream ns, byte[] buff )
			{
				int done = 0;
				while( done < buff.Length )
				{
					var read = await ns.ReadAsync(buff, done, buff.Length - done).ConfigureAwait(false);
					if( read == 0 )
						return false;
					done += read;
				}
				return true;
			}

			try
			{
				//TODO pull this out
				var hasSaidHello = false;

				var stream = client.GetStream();
				while( client.Connected )
				{
					byte[] buffer = null;
					var lengthBytes = new byte[4];
					if( !await ReceiveBuffer( stream, lengthBytes ).ConfigureAwait( false ) )
					{
						onCloseOrError( client, null );
						return;
					}
					var length = BitConverter.ToInt32(lengthBytes, 0);
					if( length == 0 )
						continue;

					buffer = new byte[length];
					if( !await ReceiveBuffer( stream, buffer ).ConfigureAwait( false ) )
					{
						onCloseOrError( client, null );
						return;
					}

					if( hasSaidHello )
					{
						var reply = Receive(buffer);
						if( reply != null )
							await sender( reply ).ConfigureAwait( false );
					}
					else
					{
						hasSaidHello = true;

						string id = Encoding.ASCII.GetString( buffer );

						connection.Id = id;
					}

				}
			}
			catch( Exception ex )
			{
				onCloseOrError( client, ex );
			}
		}

		private byte[] Receive( byte[] msg )
		{
			if( MessageHandler == null )
				return null;
			var response = MessageHandler(Serializer.Deserialize(msg));
			return response == null ? null : Serializer.Serialize( response );
		}

		/// <summary>
		/// Event raised when any error occurs. May be a listener or a sender error.
		/// </summary>
		public event EventHandler<Exception> Error;

		public void RaiseError( Exception ex )
		{
			Error?.Invoke( this, ex );
		}

		public void Broadcast( object msg )
		{
			var bytes = Serializer.Serialize(msg);

			var servers = _servers;

			foreach( var conn in servers )
				conn.Send( bytes );
		}

		//*
		public void Send( string server, object msg, bool replyExpected )
		{
			/*
			if( _clientConnections.TryGetValue( server, out var conn ) )
				conn.Send( Serializer.Serialize( msg ) );
				*/


		}
		//*/

		internal static async Task SendFramed( NetworkStream stream, byte[] bytes )
		{
			var lengthBytes = BitConverter.GetBytes(bytes.Length);
			await stream.WriteAsync( lengthBytes, 0, 4 ).ConfigureAwait( false );
			if( bytes.Length > 0 )
				await stream.WriteAsync( bytes, 0, bytes.Length ).ConfigureAwait( false );
		}
	}
}
