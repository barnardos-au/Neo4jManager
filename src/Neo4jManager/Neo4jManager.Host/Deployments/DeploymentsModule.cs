using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Nancy;
using Nancy.ModelBinding;
using Neo4jManager.Host.Versions;

namespace Neo4jManager.Host.Deployments
{
    public class DeploymentsModule : NancyModule
    {
        public DeploymentsModule(INeo4jDeploymentsPool pool, IMapper mapper) : base("/deployments")
        {
            // Get all deployments
            Get("/", _ =>
            {
                var viewModel = new DeploymentsViewModel
                {
                    Deployments = mapper.Map<IEnumerable<Deployment>>(pool)
                };

                return Negotiate
                    .WithModel(viewModel)
                    .WithView("Deployments");
            });

            // Get single deployment
            Get("/{Id}", ctx =>
            {
                string id = ctx.Id.ToString();
                var viewModel = mapper.Map<Deployment>(pool.Single(d => d.Key == id));

                return Negotiate
                    .WithModel(viewModel)
                    .WithView("Deployment");
            });

            // Create new deployment
            Get("/create", _ =>
            {
                var viewModel = new DeploymentRequest
                {
                    Versions = mapper.Map<IEnumerable<Version>>(Neo4jVersions.GetVersions()),
                    Id = $"{pool.Count + 1}" 
                };

                return Negotiate
                    .WithModel(viewModel)
                    .WithView("Create");
            });

            // Create deployment
            Post("/create", async (ctx, ct) =>
            {
                var deployment = this.Bind<DeploymentRequest>();
                await Task.Run(() => pool.Create(Neo4jVersions.GetVersions().Single(v => v.Version == deployment.Version), deployment.Id));

                var location = string.Format("{0}/{1}", ModulePath, deployment.Id);
                return Response.AsRedirect(location);
            });

            // Delete all deployments
            Delete("/all", async (ctx, ct) =>
            {
                await Task.Run(() => pool.DeleteAll());
                await Task.Run(() => Helper.KillNeo4jServices());
                return (Response)null;
            });

            // Delete single deployment
            Delete("/{Id}", async (ctx, ct) =>
            {
                string id = ctx.Id.ToString();
                await Task.Run(() => pool.Delete(id));
                return (Response) null;
            });

            
            // Start instance
            Post("/{Id}/start", async (ctx, ct) =>
            {
                string id = ctx.Id.ToString();
                await pool[id].Start(ct);
                return (Response) null;
            });

            // Stop instance
            Post("/{Id}/stop", async (ctx, ct) =>
            {
                string id = ctx.Id.ToString();
                await pool[id].Stop(ct);
                return (Response)null;
            });

            // Restart instance
            Post("/{Id}/restart", async (ctx, ct) =>
            {
                string id = ctx.Id.ToString();
                await pool[id].Restart(ct);
                return (Response)null;
            });

            // Clear instance (delete data)
            Post("/{Id}/clear", async (ctx, ct) =>
            {
                string id = ctx.Id.ToString();
                await pool[id].Clear(ct);
                return (Response)null;
            });

            // Backup instance data
            Post("/{Id}/backup", async (ctx, ct) =>
            {
                var backup = this.Bind<BackupRequest>();
                await pool[backup.Id].Backup(ct, backup.DestinationPath, backup.StopInstanceBeforeBackup);
                return (Response)null;
            });

            // Restore instance data
            Post("/{Id}/restore", async (ctx, ct) =>
            {
                var restore = this.Bind<RestoreRequest>();
                await pool[restore.Id].Restore(ct, restore.SourcePath);
                return (Response)null;
            });

            // Modify instance config
            Post("/{Id}/config", async (ctx, ct) =>
            {
                var config = this.Bind<ConfigureRequest>();
                await Task.Run(() => pool[config.Id].Configure(config.ConfigFile, config.Key, config.Value));
                return (Response)null;
            });
        }
    }
}
