namespace Neo4jManager.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceFactory.CreateHost(new Neo4jManagerService()).Run();
        }
    }
}
