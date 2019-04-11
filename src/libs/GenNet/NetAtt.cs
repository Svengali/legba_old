using System;
using System.Collections.Generic;
using System.Text;
using CodeGeneration.Roslyn;
using System.Diagnostics;

namespace gen
{

///[Conditional("CodeGeneration")]


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
[CodeGenerationAttribute(typeof(NetViewGen))]
[Conditional("CodeGeneration")]
public class NetViewAttribute : Attribute
{
    public NetViewAttribute()
    {
    }

}


}
