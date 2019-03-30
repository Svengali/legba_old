using System;
using System.Collections.Generic;
using System.Text;
using CodeGeneration.Roslyn;
using System.Diagnostics;

namespace gen
{

///[Conditional("CodeGeneration")]


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[CodeGenerationAttribute(typeof(CaseClassGen))]
[Conditional("CodeGeneration")]
public class CaseClassAttribute : Attribute
{
    public CaseClassAttribute()
    {
    }

}


}
