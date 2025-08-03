#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kairou.Editor")]
[assembly: InternalsVisibleTo("Kairou.Tests.EditMode")]
[assembly: InternalsVisibleTo("Kairou.Tests.Runtime")]
#endif