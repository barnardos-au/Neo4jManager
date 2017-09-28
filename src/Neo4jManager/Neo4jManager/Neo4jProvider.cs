using System.Diagnostics.CodeAnalysis;

namespace Neo4jManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Neo4jProvider
    {
        ServiceInstanceProvider,
        JavaInstanceProviderV3,
        PowerShellInstanceProvider
    }
}
