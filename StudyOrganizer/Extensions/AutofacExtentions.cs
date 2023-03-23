using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace StudyOrganizer.Extensions;

public static class AutofacExtentions
{
    public static void RegisterDbContextPool<T>(this ContainerBuilder builder, 
        Action<DbContextOptionsBuilder> options) where T : DbContext {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContextPool<T>(options, poolSize: 16);
        builder.Populate(serviceCollection);
    }
}