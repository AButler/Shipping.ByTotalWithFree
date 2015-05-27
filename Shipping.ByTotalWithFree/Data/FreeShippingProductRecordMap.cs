using Nop.Data.Mapping;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Data {
  public class FreeShippingProductRecordMap: NopEntityTypeConfiguration<FreeShippingProductRecord> {
    public FreeShippingProductRecordMap() {
      ToTable( "ShippingByTotalWithFree_FreeShippingProduct" );
      HasKey( x => x.Id );
    }
  }
}