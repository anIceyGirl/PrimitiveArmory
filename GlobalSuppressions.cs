// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Fuck conventions, break the mold.")]
[assembly: SuppressMessage("EditAndContinue", "ENC1003", Justification = "Does it really matter if I'm making edits to my project while it's running?")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Every parameter is required for a hook to work properly, even if it's not used in the hook itself.")]
[assembly: SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Who uses Readonly anyways")]
[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reserved for later.")]
[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "<Pending>")]
