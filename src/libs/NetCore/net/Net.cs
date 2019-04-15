using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net
{





public interface IApi
{

}

public enum Views
{
	None		= 0,
	Client	= 1,
	Edge		= 2,
	Shadow	= 4,
	Rule		= 8,

	All			= 15,
}







public class View
{
	public int test;
}

public class ClientView : View
{

}

public class EdgeView : View
{

}

public class ShadowView : View
{

}

public interface UserAccount_Client
{

	string Name { get; }
	
}

public interface UserAccount_Edge
{

	string Name { get; }
	
}

/*
interface UserAccountServerView
{

	string Name { get; }
	float Money { get; }

	void takeMoney( float amount );

}
*/



public class UserAccount : UserAccount_Client //, UserAccountServerView
{
	public string Name { get; private set; }
	public float Money { get; private set; }

	public void takeMoney( float amount )
	{

	}


}




















}
