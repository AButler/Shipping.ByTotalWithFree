using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.ByTotalWithFree {
  public class ShippingByTotalWithFreeSettings: ISettings {
    public bool LimitMethodsToCreated { get; set; }
  }
}
