using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ent
{



public class Test
{

}

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

public partial interface IComHealth : IComponent
{
	float Health { get; }
}

public partial class ComHealth : Component, IComHealth
{
	float m_health;
}

public partial class ComPhysical : Component
{
}

public partial interface IEntity
{
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

public partial class Entity : IEntity
{
	public EntityId m_id;
	public ImmutableDictionary<string, Component> m_coms;

}










public class EntityTest
{
    public EntityTest()
    {
        //var ent_1 = new Entity();
    }
}


}
