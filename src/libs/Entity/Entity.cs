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

}




[gen.CaseClass]
public partial class Entity
{
	public readonly Test testTest;
	public readonly int testInt = 7;
	public readonly string testString = "Hey, Im a test";
	public readonly ImmutableDictionary<int, string> testDict;
}










public class EntityTest
{
    public EntityTest()
    {
        //var ent_1 = new Entity();
    }
}


}
