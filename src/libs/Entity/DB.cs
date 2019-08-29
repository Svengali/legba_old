using System;
using System.Collections.Immutable;
using static System.Collections.Immutable.ImmutableInterlocked;

/*
	???? Should we have an explicit transaction class/ID?  
*/

namespace ent
{

	enum EntityState
	{
		Invalid,
		Free,
		CheckedOut,
	}

	struct EntityInfo<T>
	{
		public EntityState State { get; set; }
		public Entity<T> Entity { get; set; }
	}

	static public class DB<T>
	{
		static ImmutableDictionary<ent.EntityId, EntityInfo<T>> s_entities;


		// @@@@ TODO This returns an entity that can be changing.  It should be a lazy instantiated copy
		static public ent.IEntity<T> lookup( ent.EntityId id )
		{
			if( s_entities.TryGetValue( id, out EntityInfo<T> ent ) )
			{
				return ent.Entity;
			}
			else
			{
				// LOG
			}

			return null;
		}


		static public void checkout( ent.EntityId id, ref Checkout<T> co )
		{
			if( s_entities.TryGetValue( id, out EntityInfo<T> ent ) )
			{
				var info = ent;
				info.State = EntityState.CheckedOut;

				var updateCount = 0;
				while( !ImmutableInterlocked.TryUpdate( ref s_entities, id, info, info ) )
				{
					++updateCount;
				}

				if( updateCount > 1 ) lib.Log.debug( $"Ent {id} took {updateCount} tries." );

				co.add( ent.Entity );

				//co.add( ent );
				//AddOrUpdate( ref m_checkouts, id, ent, (k, v) => ent );
			}
			else
			{
				lib.Log.warn( $"Could not find Ent {id}" );
			}
		}

		static public void commit( ref Checkout<T> co )
		{
			
		}

	}


	public class Checkout<T>
	{
		public ImmutableList<ent.Entity<T>> Checkouts => m_checkouts;


		internal void add( ent.Entity<T> ent )
		{
			m_checkouts = m_checkouts.Add( ent );
		}



		ImmutableList<ent.Entity<T>> m_checkouts;
	}





}
