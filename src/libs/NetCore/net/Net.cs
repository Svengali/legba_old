using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetId = lib.Id<net.Id>;

namespace net
{


	public enum Id
	{
	}

	static public class Util
	{
		public static lib.Id<Id> Generate()
		{
			return lib.Id<Id>.Generate();
		}

		static public void DoThings( NetId id )
		{
		}
	}



















}
