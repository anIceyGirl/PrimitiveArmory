using System;

namespace PrimitiveArmory
{
    [System.SerializableAttribute]
    public class EnumExtenderNotFound : Exception
    {
        public EnumExtenderNotFound() { }
        public EnumExtenderNotFound(string message) : base(message) { }
        public EnumExtenderNotFound(string message, Exception inner) : base(message, inner) { }
        protected EnumExtenderNotFound(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.SerializableAttribute]
    public class ModDependencyNotFound : Exception
    {
        public ModDependencyNotFound() { }
        public ModDependencyNotFound(string message) : base(message) { }
        public ModDependencyNotFound(string message, Exception inner) : base(message, inner) { }
        protected ModDependencyNotFound(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
