using System.Collections.Generic;
using System.Linq;
using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.ByTotalWithFree {
  public class ShippingByTotalWithFreeSettings: ISettings {
    public bool LimitMethodsToCreated { get; set; }
    public string CountriesWithFreeShippingRaw { get; set; }

    public IReadOnlyCollection<int> CountriesWithFreeShipping {
      get {
        if ( string.IsNullOrEmpty( CountriesWithFreeShippingRaw ) ) {
          return new List<int>();
        }

        return CountriesWithFreeShippingRaw
          .Split( ',' )
          .Select( int.Parse )
          .ToList();
      }
    }
  }
}
