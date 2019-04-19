using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

using static net.Views;

namespace ent
{






public partial interface IComponent
{

}

public partial class Component : IComponent
{
	public readonly int test = 3;
}


public partial class ComTest : Component
{

}



public partial interface IComHealth
{
	float Health { get; }
}

public partial interface IComHealth_Rule : IComHealth
{
}


public partial interface IComHealth_Edge : IComHealth
{

}

public partial interface IComHealth_Client : IComHealth
{

}


[gen.NetView( Rule )]
[gen.NetDist( Rule, Edge ), gen.NetDist( Edge, Client )]
public partial class ComHealth : Component, IComHealth
{
	float m_health;
}

public partial class ComHealth_Rule : ComHealth, IComHealth_Rule
{
}





[gen.NetView( Rule )]
public partial class ComPhysical : Component
{
	math.Vec3 m_pos;
}


[gen.NetView( Rule )]
public partial class ComAdmin : Component
{
}





public partial interface IEntity
{
	EntityId Id { get; }
	Optional<T> Com<T>() where T : class;
}

public struct EntityId
{
	private ulong m_id;

	static public readonly EntityId None = new EntityId( 0 );

	public EntityId( ulong id )
	{
		m_id = id;
	}

	public ulong Id()
	{
		return m_id;
	}
}


[gen.NetView( All )]
public partial class Entity : IEntity
{
	public EntityId Id => m_id;


	EntityId m_id;
	ImmutableDictionary<string, Component> m_coms;




}










public class EntityTest
{
    public EntityTest()
    {
        //var ent_1 = new Entity();
    }
}


}
