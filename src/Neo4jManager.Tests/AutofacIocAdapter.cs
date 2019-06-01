using Autofac;
using ServiceStack.Configuration;

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
        return container.TryResolve<T>(out var result) ? result : default;
    }
}
