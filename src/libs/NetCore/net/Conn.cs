using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.IO;

namespace net
{

public interface I_Savable
{
	string savename();
}





public class Conn : lib.Conn
{
	private bool m_loop = true;


	public Conn( Socket sock, IFormatter formatter )
	: base( sock, formatter )
	{
	}

	public void recieveThread()
	{
		Thread thread = new Thread( new ThreadStart( this.recieveLoop ) );

		thread.Start();
	}

	private byte[] mm_recvBuf = new byte[ 2048 ];

	private void recieveLoop()
	{
		while( m_loop )
		{
			try
			{
				uint sizeReadCount = (uint)Stream.Read( mm_recvBuf, 0, 4 );
				uint len = BitConverter.ToUInt32( mm_recvBuf, 0 );
				uint totalAmountRead = 0;

				//lib.Log.info( "Len {0} needed to read", len );

				var memStream = new MemoryStream( 1024 );

				while( totalAmountRead < len )
				{
					uint amountLeft = len - totalAmountRead;

					uint maxRead = amountLeft < mm_recvBuf.Length ? amountLeft : (uint)mm_recvBuf.Length;

					uint amountRead = (uint)Stream.Read( mm_recvBuf, 0, (int)maxRead );

					memStream.Write( mm_recvBuf, 0, (int)amountRead );

					totalAmountRead += amountRead;
				}

				try
				{
					memStream.Position = 0;

					//var str = System.Text.Encoding.Default.GetString( memStream.GetBuffer() );
					//lib.Log.info( "Received {0} in obj {1}.", totalAmountRead, str );

					var obj = recieveObject( memStream );
					if( obj != null )
					{
						recieve( obj );
					}
					else
					{
					}
				}
				catch( Exception e )
				{
					lib.Log.error( $"Socket {Sock.RemoteEndPoint} had a problem.  Ex {e}." );
				}
			}
			catch( SocketException e )
			{
				lib.Log.error($"Socket {Sock.RemoteEndPoint} had a problem.  Ex {e}.");
				m_loop = false;
			}
			catch( IOException e )
			{
				lib.Log.error($"Socket {Sock.RemoteEndPoint} had a problem.  Ex {e}.");
				m_loop = false;
			}
		}
	}


	private EventWaitHandle mm_recvWait = new EventWaitHandle( true, EventResetMode.AutoReset );
	private IAsyncResult mm_recvRes;
	private void recieveLoop_old()
	{
		while( m_loop )
		{
			mm_recvRes = Stream.BeginRead( mm_recvBuf, 0, 1024, new AsyncCallback( asyncRecv ), null );
			mm_recvWait.WaitOne();
		}
	}

	private void asyncRecv( IAsyncResult ar )
	{
		int packetSize = Stream.EndRead( ar );
		lib.Log.info($"Recieved {packetSize}" );

		if( !Sock.Connected || packetSize == 0 )
		{
			var stuff = Sock.Connected?"Connected":"Disconnected";
			lib.Log.warn($"Socket {Sock.RemoteEndPoint} had a problem. {stuff} recv {packetSize} bytes." );
			m_loop = false;
			return;
		}

		/*
		for( int i = 0; i < packetSize; ++i )
		{
			lib.Log.info( "{0}:{1}:{2}", i, mm_recvBuf[ i ], (char)mm_recvBuf[ i ] );
		}
		*/

		try
		{
			mm_recvBuf[packetSize] = 0;

			//TODO: Make a proper 'MemoryStream'
			var recv = new MemoryStream( 1024 );
			recv.Write( mm_recvBuf, 0, packetSize );
			recv.Position = 0;

			var str = System.Text.Encoding.Default.GetString( mm_recvBuf );
			lib.Log.info( $"Received {packetSize} in obj {str}." );

			/*
			for( int i = 0; i < packetSize; ++i )
			{
				lib.Log.info( "{0,3}:{1,3}:{2}:{3,3}", i, mm_recvBuf[ i ], (char)mm_recvBuf[ i ], mm_memRecvStream.ReadByte() );
			}
			*/

			var obj = recieveObject( recv );
			if( obj != null )
			{
				recieve( obj );
			}
			else
			{
			}
		}
		catch( SocketException e )
		{
			lib.Log.error( $"Socket {Sock.RemoteEndPoint} had a problem.  Ex {e}." );
			m_loop = false;
		}
		catch( IOException e )
		{
			lib.Log.error($"Socket {Sock.RemoteEndPoint} had a problem.  Ex {e}.");
			m_loop = false;
		}
		catch( Exception e )
		{
			lib.Log.error($"Socket {Sock.RemoteEndPoint} had a problem.  Ex {e}.");
		}

			mm_recvWait.Set();
	}

}



}
