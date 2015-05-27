using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Models {
  public class FreeShippingProductModel: BaseNopEntityModel {
    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.Store" )]
    public int StoreId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.Store" )]
    public string StoreName { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.FreeShippingCountry" )]
    public int CountryId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.FreeShippingCountry" )]
    public string CountryName { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.Product" )]
    public int ProductId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.Product" )]
    public string ProductName { get; set; }
  }
}
