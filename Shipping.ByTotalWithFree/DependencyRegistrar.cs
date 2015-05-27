using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.ByTotalWithFree.Data;
using Nop.Plugin.Shipping.ByTotalWithFree.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotalWithFree {
  public class DependencyRegistrar: IDependencyRegistrar {
    public virtual void Register( ContainerBuilder builder, ITypeFinder typeFinder ) {
      builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().InstancePerRequest();

      //data context
      this.RegisterPluginDataContext<PluginObjectContext>( builder, "nop_object_context_shipping_total_with_free" );

      //override required repository with our custom context
      builder.RegisterType<EfRepository<ShippingByTotalRecord>>()
          .As<IRepository<ShippingByTotalRecord>>()
          .WithParameter( ResolvedParameter.ForNamed<IDbContext>( "nop_object_context_shipping_total_with_free" ) )
          .InstancePerRequest();

      //override required repository with our custom context
      builder.RegisterType<EfRepository<FreeShippingProductRecord>>()
          .As<IRepository<FreeShippingProductRecord>>()
          .WithParameter( ResolvedParameter.ForNamed<IDbContext>( "nop_object_context_shipping_total_with_free" ) )
          .InstancePerRequest();
    }

    public int Order {
      get { return 1; }
    }
  }
}
