using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Diagnostics.Tracing;
using lib.Net;
using Validation;

namespace ent
{




public partial class ComHealth : Component
{
	public float Health => m_health;



}










public partial class Entity
{
	private int m_testVal = 10;

	public int TestVal => m_testVal;



	public Optional<T> Com<T>() where T : class
	{
		var name = typeof(T).Name;

		Component com = null;

		var hasCom = m_coms.TryGetValue( name, out com );

		return com as T;
	}

	Optional<T> IEntity.Com<T>()
	{
		Requires.ValidState( typeof(T).IsInterface, $"{typeof(T).Name} must be an interface" );

		var comOpt = Com<T>();

		var com = comOpt.Value;

		return com;
	}





	/*
	public Entity MutCom<T>(Func<T> fn) where T : Component
	{
		var name = typeof(T).Name;

		Component com = null;

		var hasCom = m_coms.TryGetValue( name, out com );

		if( hasCom )
		{
			var newCom = fn();

			var newComs = m_coms.SetItem( name, newCom );

			return with( comsOpt: newComs );
		}
		else
		{
			// TODO LOG

			return this;
		}
	}
	*/
}

public class Amazing
{ }


// Interesting.  But not totally sure its necessary
public partial class Mut<T>
{
	private T t;

	public Mut(ref T newT)
	{
		t = newT;
	}

	public void mut(Func<T> fn)
	{
			t = fn();
	}
			

}



}
