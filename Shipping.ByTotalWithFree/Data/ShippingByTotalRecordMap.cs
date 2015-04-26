using Nop.Data.Mapping;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Data {
  public class ShippingByTotalRecordMap: NopEntityTypeConfiguration<ShippingByTotalRecord> {
    public ShippingByTotalRecordMap() {
      ToTable( "ShippingByTotalWithFree" );
      HasKey( x => x.Id );

      Property( x => x.ZipPostalCode ).HasMaxLength( ByTotalShippingComputationMethod.ZipPostalCodeMaxLength );
    }
  }
}
