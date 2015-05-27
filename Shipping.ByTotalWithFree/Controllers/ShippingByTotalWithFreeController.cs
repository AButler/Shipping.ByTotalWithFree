using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Shipping.ByTotalWithFree.Data;
using Nop.Plugin.Shipping.ByTotalWithFree.Models;
using Nop.Plugin.Shipping.ByTotalWithFree.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotalWithFree.Controllers {
  [AdminAuthorize]
  public class ShippingByTotalWithFreeController: BasePluginController {
    private readonly IShippingService _shippingService;
    private readonly IStoreService _storeService;
    private readonly ISettingService _settingService;
    private readonly IShippingByTotalService _shippingByTotalService;
    private readonly ShippingByTotalWithFreeSettings _shippingByTotalWithFreeSettings;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly IProductService _productService;

    public ShippingByTotalWithFreeController( IShippingService shippingService,
        IStoreService storeService,
        ISettingService settingService,
        IShippingByTotalService shippingByTotalService,
        ShippingByTotalWithFreeSettings shippingByTotalWithFreeSettings,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IProductService productService ) {
      _shippingService = shippingService;
      _storeService = storeService;
      _settingService = settingService;
      _shippingByTotalService = shippingByTotalService;
      _shippingByTotalWithFreeSettings = shippingByTotalWithFreeSettings;
      _countryService = countryService;
      _stateProvinceService = stateProvinceService;
      _currencyService = currencyService;
      _currencySettings = currencySettings;
      _permissionService = permissionService;
      _localizationService = localizationService;
      _productService = productService;
    }

    protected override void Initialize( System.Web.Routing.RequestContext requestContext ) {
      //always set culture to 'en-US' (Telerik Grid has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
      CommonHelper.SetTelerikCulture();

      base.Initialize( requestContext );
    }

    [ChildActionOnly]
    public ActionResult Configure() {
      var shippingMethods = _shippingService.GetAllShippingMethods();
      if ( shippingMethods.Count == 0 ) {
        return Content( "No shipping methods can be loaded" );
      }

      var model = new ShippingByTotalListModel();

      // stores
      model.AvailableStores.Add( new SelectListItem { Text = "*", Value = "0" } );
      foreach ( var store in _storeService.GetAllStores() ) {
        model.AvailableStores.Add( new SelectListItem { Text = store.Name, Value = store.Id.ToString() } );
      }

      // shipping methods
      foreach ( var sm in shippingMethods ) {
        model.AvailableShippingMethods.Add( new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() } );
      }

      // countries
      model.AvailableCountriesWithAll.Add( new SelectListItem { Text = "*", Value = "0" } );
      var countries = _countryService.GetAllCountries( true );
      foreach ( var c in countries ) {
        model.AvailableCountries.Add( new SelectListItem { Text = c.Name, Value = c.Id.ToString() } );
        model.AvailableCountriesWithAll.Add( new SelectListItem { Text = c.Name, Value = c.Id.ToString() } );
      }

      model.AvailableStates.Add( new SelectListItem { Text = "*", Value = "0" } );
      model.LimitMethodsToCreated = _shippingByTotalWithFreeSettings.LimitMethodsToCreated;
      model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById( _currencySettings.PrimaryStoreCurrencyId ).CurrencyCode;

      return View( "~/Plugins/Shipping.ByTotalWithFree/Views/ShippingByTotalWithFree/Configure.cshtml", model );
    }

    [HttpPost]
    public ActionResult RatesList( DataSourceRequest command ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Content( _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) );
      }

      var records = _shippingByTotalService.GetAllShippingByTotalRecords( command.Page - 1, command.PageSize );
      var sbtModel = records.Select( x => {
        var m = new ShippingByTotalModel {
          Id = x.Id,
          StoreId = x.StoreId,
          ShippingMethodId = x.ShippingMethodId,
          CountryId = x.CountryId,
          DisplayOrder = x.DisplayOrder,
          From = x.From,
          To = x.To,
          UsePercentage = x.UsePercentage,
          ShippingChargePercentage = x.ShippingChargePercentage,
          ShippingChargeAmount = x.ShippingChargeAmount,
        };

        // shipping method
        var shippingMethod = _shippingService.GetShippingMethodById( x.ShippingMethodId );
        m.ShippingMethodName = ( shippingMethod != null ) ? shippingMethod.Name : "Unavailable";

        // store
        var store = _storeService.GetStoreById( x.StoreId );
        m.StoreName = ( store != null ) ? store.Name : "*";

        // country
        var c = _countryService.GetCountryById( x.CountryId );
        m.CountryName = ( c != null ) ? c.Name : "*";
        m.CountryId = x.CountryId;

        // state/province
        var s = _stateProvinceService.GetStateProvinceById( x.StateProvinceId );
        m.StateProvinceName = ( s != null ) ? s.Name : "*";
        m.StateProvinceId = x.StateProvinceId;

        // ZIP / postal code
        m.ZipPostalCode = ( !String.IsNullOrEmpty( x.ZipPostalCode ) ) ? x.ZipPostalCode : "*";

        return m;
      } ).ToList();

      var gridModel = new DataSourceResult {
        Data = sbtModel,
        Total = records.TotalCount
      };

      return Json( gridModel );
    }

    [HttpPost]
    public ActionResult RateUpdate( ShippingByTotalModel model ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Content( _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) );
      }

      var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById( model.Id );
      shippingByTotalRecord.ZipPostalCode = model.ZipPostalCode == "*" ? null : model.ZipPostalCode;
      shippingByTotalRecord.DisplayOrder = model.DisplayOrder;
      shippingByTotalRecord.From = model.From;
      shippingByTotalRecord.To = model.To;
      shippingByTotalRecord.UsePercentage = model.UsePercentage;
      shippingByTotalRecord.ShippingChargePercentage = model.UsePercentage ? model.ShippingChargePercentage : 0;
      shippingByTotalRecord.ShippingChargeAmount = !model.UsePercentage ? model.ShippingChargeAmount : 0;
      shippingByTotalRecord.ShippingMethodId = model.ShippingMethodId;
      shippingByTotalRecord.StoreId = model.StoreId;
      shippingByTotalRecord.StateProvinceId = model.StateProvinceId;
      shippingByTotalRecord.CountryId = model.CountryId;
      _shippingByTotalService.UpdateShippingByTotalRecord( shippingByTotalRecord );

      return new NullJsonResult();
    }

    [HttpPost]
    public ActionResult RateDelete( int id ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Content( _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) );
      }

      var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById( id );
      if ( shippingByTotalRecord != null ) {
        _shippingByTotalService.DeleteShippingByTotalRecord( shippingByTotalRecord );
      }
      return new NullJsonResult();
    }

    [HttpPost]
    public ActionResult FreeShippingList( DataSourceRequest command ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Content( _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) );
      }

      var records = _shippingByTotalService.GetAllFreeShippingProductRecords( command.Page - 1, command.PageSize );
      var fspModel = records.Select( x => {
        var m = new FreeShippingProductModel {
          Id = x.Id,
          StoreId = x.StoreId,
          CountryId = x.CountryId,
          ProductId = x.ProductId
        };

        // store
        var store = _storeService.GetStoreById( x.StoreId );
        m.StoreName = ( store != null ) ? store.Name : "*";

        // country
        var c = _countryService.GetCountryById( x.CountryId );
        m.CountryName = ( c != null ) ? c.Name : "*";

        // product
        var p = _productService.GetProductById( x.ProductId );
        m.ProductName = p.Name;

        return m;
      } ).ToList();

      var gridModel = new DataSourceResult {
        Data = fspModel,
        Total = records.TotalCount
      };

      return Json( gridModel );
    }

    [HttpPost]
    public ActionResult FreeShippingDelete( int id ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Content( _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) );
      }

      var freeShippingProductRecord = _shippingByTotalService.GetFreeShippingProductRecordById( id );
      if ( freeShippingProductRecord != null ) {
        _shippingByTotalService.DeleteFreeShippingProductRecord( freeShippingProductRecord );
      }
      return new NullJsonResult();
    }

    [HttpPost]
    public ActionResult AddShippingRate( ShippingByTotalListModel model ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Json( new { Result = false, Message = _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) } );
      }

      var zipPostalCode = model.AddZipPostalCode;

      if ( zipPostalCode != null ) {
        int zipMaxLength = ByTotalShippingComputationMethod.ZipPostalCodeMaxLength;
        zipPostalCode = zipPostalCode.Trim();
        if ( zipPostalCode.Length > zipMaxLength ) {
          zipPostalCode = zipPostalCode.Substring( 0, zipMaxLength );
        }
      }

      var shippingByTotalRecord = new ShippingByTotalRecord {
        ShippingMethodId = model.AddShippingMethodId,
        StoreId = model.AddStoreId,
        CountryId = model.AddCountryId,
        StateProvinceId = model.AddStateProvinceId,
        ZipPostalCode = zipPostalCode,
        DisplayOrder = model.AddDisplayOrder,
        From = model.AddFrom,
        To = model.AddTo,
        UsePercentage = model.AddUsePercentage,
        ShippingChargePercentage = ( model.AddUsePercentage ) ? model.AddShippingChargePercentage : 0,
        ShippingChargeAmount = ( model.AddUsePercentage ) ? 0 : model.AddShippingChargeAmount
      };
      _shippingByTotalService.InsertShippingByTotalRecord( shippingByTotalRecord );

      return Json( new { Result = true } );
    }

    [HttpPost]
    public ActionResult AddFreeShippingProduct( ShippingByTotalListModel model ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Json( new { Result = false, Message = _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) } );
      }

      var freeShippingProductRecord = new FreeShippingProductRecord {
        StoreId = model.AddStoreId,
        ProductId = model.AddProductId,
        CountryId = model.AddFreeShippingCountryId
      };

      _shippingByTotalService.InsertFreeShippingProductRecord( freeShippingProductRecord );

      return Json( new { Result = true } );
    }

    [HttpPost]
    public ActionResult SaveGeneralSettings( ShippingByTotalListModel model ) {
      if ( !_permissionService.Authorize( StandardPermissionProvider.ManageShippingSettings ) ) {
        return Json( new { Result = false, Message = _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.AccessDenied" ) } );
      }

      //save settings
      _shippingByTotalWithFreeSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
      _settingService.SaveSetting( _shippingByTotalWithFreeSettings );

      return Json( new { Result = true, Message = _localizationService.GetResource( "Plugins.Shipping.ByTotalWithFree.ManageShippingSettings.Saved" ) } );
    }
  }
}
