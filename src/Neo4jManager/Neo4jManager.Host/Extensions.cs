using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host
{
    public static class Extensions
    {
        public static IEnumerable<VersionInfo> AsVersionInfos(this IEnumerable<Neo4jVersion> versions)
        {
            return versions.Select(Mapper.Map<VersionInfo>);
        }
    }
}
