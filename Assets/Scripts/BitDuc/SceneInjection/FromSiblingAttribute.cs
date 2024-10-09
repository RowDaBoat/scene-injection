using System;

namespace BitDuc.SceneInjection
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FromSiblingAttribute : Attribute
    {
        public string Name { get; }

        public FromSiblingAttribute(string name = null) => Name = name;
    }
}