using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]

// how does this bypass the need for publicity stunt?
// fuckin beats me lol ¯\_(ツ)_/¯ 

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}