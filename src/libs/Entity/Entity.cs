using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ent
{



public class Test
{

}


[gen.CaseClass]
public partial class Entity
{
	public readonly Test testTest;
	public readonly int testInt;
	public readonly string testString;
	public readonly ImmutableDictionary<int, string> testDict;
}










public class EntityTest
{
    public EntityTest()
    {
        var ent_1 = new Entity();
    }
}


}
