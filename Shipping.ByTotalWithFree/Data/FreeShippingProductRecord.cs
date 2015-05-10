using Nop.Core;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Data {
  public class FreeShippingProductRecord: BaseEntity {
    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public int CountryId { get; set; }
  }
}
