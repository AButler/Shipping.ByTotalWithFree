using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Models {
  public class ShippingByTotalListModel: BaseNopModel {
    public ShippingByTotalListModel() {
      AvailableCountries = new List<SelectListItem>();
      AvailableStates = new List<SelectListItem>();
      AvailableShippingMethods = new List<SelectListItem>();
      AvailableStores = new List<SelectListItem>();
      FreeShippingCountries = new List<SelectListItem>();
    }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.Store" )]
    public int AddStoreId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.Country" )]
    public int AddCountryId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.StateProvince" )]
    public int AddStateProvinceId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.ZipPostalCode" )]
    public string AddZipPostalCode { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.DisplayOrder" )]
    public int AddDisplayOrder { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod" )]
    public int AddShippingMethodId { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.From" )]
    public decimal AddFrom { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.To" )]
    public decimal AddTo { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.UsePercentage" )]
    public bool AddUsePercentage { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargePercentage" )]
    public decimal AddShippingChargePercentage { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargeAmount" )]
    public decimal AddShippingChargeAmount { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.LimitMethodsToCreated" )]
    public bool LimitMethodsToCreated { get; set; }

    public string PrimaryStoreCurrencyCode { get; set; }

    public IList<SelectListItem> AvailableCountries { get; set; }
    public IList<SelectListItem> AvailableStates { get; set; }
    public IList<SelectListItem> AvailableShippingMethods { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    [NopResourceDisplayName( "Plugins.Shipping.ByTotalWithFree.Fields.FreeShippingCountries" )]
    public IList<SelectListItem> FreeShippingCountries { get; set; }
  }
}