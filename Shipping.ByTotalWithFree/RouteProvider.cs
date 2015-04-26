using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.ByTotalWithFree {
  public class RouteProvider: IRouteProvider {
    public void RegisterRoutes( RouteCollection routes ) {
      routes.MapRoute(
        "Plugin.Shipping.ByTotalWithFree.AddShippingRate",
        "Plugins/ShippingByTotalWithFree/AddShippingRate",
        new { controller = "ShippingByTotalWithFree", action = "AddShippingRate" },
        new[] { "Nop.Plugin.Shipping.ByTotalWithFree.Controllers" }
      );
      routes.MapRoute(
        "Plugin.Shipping.ByTotalWithFree.SaveGeneralSettings",
        "Plugins/ShippingByTotalWithFree/SaveGeneralSettings",
        new { controller = "ShippingByTotalWithFree", action = "SaveGeneralSettings" },
        new[] { "Nop.Plugin.Shipping.ByTotalWithFree.Controllers" }
      );
    }

    public int Priority {
      get { return 0; }
    }
  }
}
