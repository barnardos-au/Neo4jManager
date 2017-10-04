using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Neo4jManager.Host.Deployments;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host
{
    public static class Extensions
    {
        public static IEnumerable<VersionInfo> AsVersionInfos(this IEnumerable<Neo4jVersion> versions)
        {
            return versions.Select(Mapper.Map<VersionInfo>);
        }

        public static IEnumerable<DeploymentInfo> AsDeploymentInfos(this Dictionary<string, INeo4jInstance> deployments)
        {
            return deployments.Select(neo4JInstance => neo4JInstance.AsDeploymentInfo());
        }

        public static DeploymentInfo AsDeploymentInfo(this KeyValuePair<string, INeo4jInstance> kvp)
        {
            return new DeploymentInfo
            {
                Id = kvp.Key,
                DataPath = kvp.Value.DataPath,
                Version = Mapper.Map<VersionInfo>(kvp.Value.Version),
                Endpoints = Mapper.Map<EndpointsInfo>(kvp.Value.Endpoints)
            };
        }

    }
}
