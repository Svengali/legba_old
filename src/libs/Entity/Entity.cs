using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ent
{
	public partial class Entity
	{


		public Optional<Component> Com<T>()
		{
			var name = typeof(T).Name;

			Component com = null;

			var hasCom = coms.TryGetValue( name, out com );

			return com;
		}

		public Entity MutCom<T>(Func<T> fn) where T : Component
		{
			var name = typeof(T).Name;

			Component com = null;

			var hasCom = coms.TryGetValue( name, out com );

			if( hasCom )
			{
				var newCom = fn();

				var newComs = coms.SetItem( name, newCom );

				return with( comsOpt: newComs );
			}
			else
			{
				// TODO LOG

				return this;
			}
		}

		



	}

	// Interesting.  But not toally sure its necessary
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
