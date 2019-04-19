using System;
using System.Collections.Immutable;
using static System.Collections.Immutable.ImmutableInterlocked;


namespace ent
{

public class DB
{

	public ent.IEntity lookup( ent.EntityId id )
	{
		if( m_entities.TryGetValue( id, out Entity ent ) )
		{
			return ent;
		}
		else
		{
			// LOG
		}

		return null;
	}

	
	public void checkout( ent.EntityId id, ref Checkout co )
	{
		if( m_entities.TryGetValue( id, out Entity ent ) )
		{
			co.add( ent );
			AddOrUpdate( ref m_checkouts, id, ent, (k, v) => ent );
		}
		else
		{
			// LOG
		}
	}
	
	public void checkout( ent.IEntity ient, ref Checkout co )
	{
		if( m_entities.TryGetValue( ient.Id, out Entity ent ) )
		{
			co.add( ent );
			AddOrUpdate( ref m_checkouts, ient.Id, ent, (k, v) => ent );
		}
		else
		{
			// LOG
		}
	}





	ImmutableDictionary<ent.EntityId, ent.Entity> m_entities;

	ImmutableDictionary<ent.EntityId, ent.Entity> m_checkouts;

}


public class Checkout
{
	public ImmutableList<ent.Entity> Checkouts => m_checkouts;


	internal void add( ent.Entity ent )
	{
		m_checkouts = m_checkouts.Add( ent );
	}



	ImmutableList<ent.Entity> m_checkouts;
}












}
