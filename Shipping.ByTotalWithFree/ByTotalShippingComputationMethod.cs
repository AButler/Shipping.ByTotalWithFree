using System;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.ByTotalWithFree.Data;
using Nop.Plugin.Shipping.ByTotalWithFree.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.ByTotalWithFree {
  public class ByTotalShippingComputationMethod: BasePlugin, IShippingRateComputationMethod {
    private readonly IShippingService _shippingService;
    private readonly IStoreContext _storeContext;
    private readonly IShippingByTotalService _shippingByTotalService;
    private readonly ShippingByTotalWithFreeSettings _shippingByTotalWithFreeSettings;
    private readonly ShippingByTotalObjectContext _objectContext;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;

    internal const int ZipPostalCodeMaxLength = 400;

    public ByTotalShippingComputationMethod( IShippingService shippingService,
        IStoreContext storeContext,
        IShippingByTotalService shippingByTotalService,
        ShippingByTotalWithFreeSettings shippingByTotalWithFreeSettings,
        ShippingByTotalObjectContext objectContext,
        IPriceCalculationService priceCalculationService,
        ILogger logger,
        ISettingService settingService ) {
      _shippingService = shippingService;
      _storeContext = storeContext;
      _shippingByTotalService = shippingByTotalService;
      _shippingByTotalWithFreeSettings = shippingByTotalWithFreeSettings;
      _objectContext = objectContext;
      _priceCalculationService = priceCalculationService;
      _logger = logger;
      _settingService = settingService;
    }

    public ShippingRateComputationMethodType ShippingRateComputationMethodType {
      get { return ShippingRateComputationMethodType.Offline; }
    }

    /// <summary>
    /// Gets the rate for the shipping method
    /// </summary>
    private decimal? GetRate( decimal subtotal, int shippingMethodId, int storeId, int countryId, int stateProvinceId, string zipPostalCode ) {
      decimal? shippingTotal = null;

      var shippingByTotalRecord = _shippingByTotalService.FindShippingByTotalRecord( shippingMethodId, storeId, countryId, subtotal, stateProvinceId, zipPostalCode );

      if ( shippingByTotalRecord == null ) {
        if ( _shippingByTotalWithFreeSettings.LimitMethodsToCreated ) {
          return null;
        }

        return decimal.Zero;
      }

      if ( shippingByTotalRecord.UsePercentage && shippingByTotalRecord.ShippingChargePercentage <= decimal.Zero ) {
        return decimal.Zero;
      }

      if ( !shippingByTotalRecord.UsePercentage && shippingByTotalRecord.ShippingChargeAmount <= decimal.Zero ) {
        return decimal.Zero;
      }

      shippingTotal = shippingByTotalRecord.UsePercentage
        ? CalculatePercentage( subtotal, shippingByTotalRecord.ShippingChargePercentage )
        : shippingByTotalRecord.ShippingChargeAmount;

      if ( shippingTotal < decimal.Zero ) {
        shippingTotal = decimal.Zero;
      }

      return shippingTotal;
    }

    /// <summary>
    ///  Gets available shipping options
    /// </summary>
    /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
    /// <returns>Represents a response of getting shipping rate options</returns>
    public GetShippingOptionResponse GetShippingOptions( GetShippingOptionRequest getShippingOptionRequest ) {
      if ( getShippingOptionRequest == null ) {
        throw new ArgumentNullException( "getShippingOptionRequest" );
      }

      var response = new GetShippingOptionResponse();

      if ( getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0 ) {
        response.AddError( "No shipment items" );
        return response;
      }
      if ( getShippingOptionRequest.ShippingAddress == null ) {
        response.AddError( "Shipping address is not set" );
        return response;
      }

      var storeId = _storeContext.CurrentStore.Id;
      var countryId = getShippingOptionRequest.ShippingAddress.CountryId.GetValueOrDefault( 0 );
      var stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId.GetValueOrDefault( 0 );
      var zipPostalCode = getShippingOptionRequest.ShippingAddress.ZipPostalCode;

      var subTotal = decimal.Zero;
      foreach ( var packageItem in getShippingOptionRequest.Items ) {
        if ( !packageItem.ShoppingCartItem.IsShipEnabled ) {
          continue;
        }
        if ( IsFreeShippingAllowed( getShippingOptionRequest.ShippingAddress, packageItem ) ) {
          continue;
        }

        subTotal += _priceCalculationService.GetSubTotal( packageItem.ShoppingCartItem );
      }

      var shippingMethods = _shippingService.GetAllShippingMethods( countryId );
      foreach ( var shippingMethod in shippingMethods ) {
        var rate = GetRate( subTotal, shippingMethod.Id, storeId, countryId, stateProvinceId, zipPostalCode );
        if ( rate.HasValue ) {
          var shippingOption = new ShippingOption {
            Name = shippingMethod.GetLocalized( x => x.Name ),
            Description = shippingMethod.GetLocalized( x => x.Description ),
            Rate = rate.Value
          };
          response.ShippingOptions.Add( shippingOption );
        }
      }

      return response;
    }

    private bool IsFreeShippingAllowed( Address shippingAddress, GetShippingOptionRequest.PackageItem packageItem ) {
      if ( !packageItem.ShoppingCartItem.IsFreeShipping ) {
        return false;
      }

      if ( !shippingAddress.CountryId.HasValue ) {
        // Don't know the country
        return false;
      }

      // Check whether the country allows free shipping or not
      var countryId = shippingAddress.CountryId.Value;
      return _shippingByTotalWithFreeSettings.CountriesWithFreeShipping.Contains( countryId );
    }

    /// <summary>
    /// Gets a route for provider configuration
    /// </summary>
    /// <param name="actionName">Action name</param>
    /// <param name="controllerName">Controller name</param>
    /// <param name="routeValues">Route values</param>
    public void GetConfigurationRoute( out string actionName, out string controllerName, out RouteValueDictionary routeValues ) {
      actionName = "Configure";
      controllerName = "ShippingByTotalWithFree";
      routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Shipping.ByTotalWithFree.Controllers" }, { "area", null } };
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override void Install() {
      var settings = new ShippingByTotalWithFreeSettings {
        LimitMethodsToCreated = false
      };
      _settingService.SaveSetting( settings );

      _objectContext.Install();

      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.AddNewRecordTitle", "Add new 'Shipping By Total' record" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Country", "Country" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.DisplayOrder", "Display Order" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.DisplayOrder.Hint", "The display order for the shipping rate. Rates with lower display order values will be used if multiple rates match. If display orders match, the older rate is used." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.From", "Order total From" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.From.Hint", "Order total from." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.LimitMethodsToCreated.Hint", "If you check this option, your customers will be limited to the shipping options configured here. Unchecked and they'll be able to choose any existing shipping options even if it's not configured here (shipping methods not configured here will have shipping fees of zero)." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargeAmount", "Charge amount" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargeAmount.Hint", "Charge amount." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargePercentage", "Charge percentage (of subtotal)" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargePercentage.Hint", "Charge percentage (of subtotal)." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod", "Shipping method" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod.Hint", "The shipping method." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.StateProvince", "State / province" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.StateProvince.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state / province." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Store", "Store" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Store.Hint", "This shipping rate will apply to all stores if an asterisk is selected." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.To", "Order total To" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.To.Hint", "Order total to." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.UsePercentage", "Use percentage" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.UsePercentage.Hint", "Check to use 'charge percentage' value." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ZipPostalCode", "ZIP / postal code" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ZipPostalCode.Hint", "If ZIP / postal code is empty, this shipping rate will apply to all customers from the given country or state / province, regardless of the ZIP / postal code. The ZIP / postal codes can be entered in multiple formats: single (11111), multiple comma separated (11111, 22222), wildcard characters (S4? ???), starts with wildcard (S4*), numeric ranges (10000:30000), or combinations of the preceding formats (11111, 100??, 11111:22222, 33333)." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.FreeShippingCountries", "Countries" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.FreeShippingCountries.Hint", "The list of countries that free shipping on a product level is allowed for" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.FreeShippingCountriesTitle", "Free Shipping Countries" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied", "Access denied" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AddFailed", "Failed to add record." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.Saved", "Saved" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.StatesFailed", "Failed to retrieve states." );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Reset", "Reset" );
      this.AddOrUpdatePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.SettingsTitle", "Shipping By Total Settings" );

      base.Install();

      _logger.Information( string.Format( "Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName ) );
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override void Uninstall() {
      _settingService.DeleteSetting<ShippingByTotalWithFreeSettings>();

      _objectContext.Uninstall();

      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.AddNewRecordTitle" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Country" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Country.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.DisplayOrder" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.DisplayOrder.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.From" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.From.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.LimitMethodsToCreated" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.LimitMethodsToCreated.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargeAmount" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargeAmount.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargePercentage" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingChargePercentage.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ShippingMethod.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.StateProvince" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.StateProvince.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Store" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.Store.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.To" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.To.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.UsePercentage" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.UsePercentage.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ZipPostalCode" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Fields.ZipPostalCode.Hint" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AddFailed" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.Saved" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.StatesFailed" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.Reset" );
      this.DeletePluginLocaleResource( "Plugins.Shipping.ByTotalWithFree.SettingsTitle" );

      base.Uninstall();
    }

    private static decimal CalculatePercentage( decimal subtotal, decimal percentage ) {
      return Math.Round( (decimal) ( ( ( (float) subtotal ) * ( (float) percentage ) ) / 100f ), 2 );
    }

    #region Not Supported
    /// <summary>
    /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
    /// </summary>
    /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
    /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
    public decimal? GetFixedRate( GetShippingOptionRequest getShippingOptionRequest ) {
      return null;
    }

    public IShipmentTracker ShipmentTracker {
      get { return null; }
    }
    #endregion
  }
}
