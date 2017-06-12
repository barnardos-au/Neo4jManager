namespace Neo4jManager
{
    public interface IFileCopy
    {
        void MirrorFolders(string source, string destination);
    }
}