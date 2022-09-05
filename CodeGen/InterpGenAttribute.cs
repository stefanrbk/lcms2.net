using System;

namespace CodeGen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MyAttribute: Attribute
    { }
}
