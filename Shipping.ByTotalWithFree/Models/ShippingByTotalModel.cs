using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Models
{
    public class ShippingByTotalModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.Country")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.StateProvince")]
        public int StateProvinceId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.From")]
        public decimal From { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.To")]
        public decimal To { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargePercentage")]
        public decimal ShippingChargePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargeAmount")]
        public decimal ShippingChargeAmount { get; set; }
    }
}
