dotnet pack ./Neo4jManager.ServiceModel/Neo4jManager.ServiceModel.csproj -c Release
dotnet pack ./Neo4jManager.Client/Neo4jManager.Client.csproj -c Release
dotnet publish ./Neo4jManager.Host/Neo4jManager.Host.csproj -c Release
