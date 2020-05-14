using Autofac;
using ServiceStack.Configuration;

namespace Neo4jManager.Tests
{
    public class AutofacIocAdapter : IContainerAdapter
    {
        private readonly IContainer container;

        public AutofacIocAdapter(IContainer container)
        {
            this.container = container;
        }

        public T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        public T TryResolve<T>()
        {
            if (container.TryResolve(typeof(T), out var result))
            {
                return (T)result;
            }

            return default;
        }
    }
}