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

[gen.CaseClass]
public partial class Component
{
	public readonly int test = 3;
}


[gen.CaseClass]
public partial class ComTest : Component
{

}

[gen.CaseClass]
public partial class ComHealth : Component
{
	public readonly float health;
}

[gen.CaseClass]
public partial class ComPhysical : Component
{
	public readonly float health;
}



[gen.CaseClass]
public partial class Entity
{
	public readonly Test testTest;
	public readonly int testInt = 7;
	public readonly string testString = "Hey, Im a test";
	public readonly ImmutableDictionary<string, Component> coms;
}










public class EntityTest
{
    public EntityTest()
    {
        //var ent_1 = new Entity();
    }
}


}
