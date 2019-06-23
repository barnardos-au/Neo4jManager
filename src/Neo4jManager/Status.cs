using System.ComponentModel;

namespace Neo4jManager
{
    public enum Status
    {
        [Description("Installing")]
        Installing,

        [Description("Stopping")]
        Stopping,

        [Description("Stopped")]
        Stopped,

        [Description("Starting")]
        Starting,

        [Description("Started")]
        Started,

        [Description("Clearing")]
        Clearing,

        [Description("Backing Up")]
        Backup,

        [Description("Restoring")]
        Restore,
        
        [Description("Deleted")]
        Deleted
    }
}
