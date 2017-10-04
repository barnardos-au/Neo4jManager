namespace Neo4jManager.Host.Deployments
{
    public class ConfigureRequest
    {
        public string Id { get; set; }
        public string ConfigFile { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
