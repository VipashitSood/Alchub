using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.JCarousel
{
    public class JCarouselPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IJCarouselService _jcarouselService;
        private readonly IWebHelper _webHelper;
        private readonly WidgetSettings _widgetSettings;
        #endregion
        #region Ctor
        public JCarouselPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IJCarouselService jcarouselService,
            IWebHelper webHelper,
            WidgetSettings widgetSettings)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _jcarouselService = jcarouselService;
            _webHelper = webHelper;
            _widgetSettings = widgetSettings;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.BodyStartHtmlTagAfter, PublicWidgetZones.HomepageAfterBestSellers });
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/JCarousel/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            //head widget
            if (widgetZone.Equals(PublicWidgetZones.BodyStartHtmlTagAfter))
                return JCarouselDefaults.HEAD_REFERENCE_VIEW_COMPONENT;

            //default carousel widget
            return JCarouselDefaults.VIEW_COMPONENT;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new JCarouselPluginSettings
            {
                Enable = true,
                LazyLoadNumberOfProductInCarousel = 20
            };

            //add plugin in active widget
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(JCarouselDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(JCarouselDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.SaveSettingAsync(settings);
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.JCarousel.Hint"] = "JCarousel",
                ["Plugins.Widgets.JCarousel.Settings"] = "Settings",
                ["Plugins.Widgets.JCarousel.ManageJCarousel"] = "Manage Jacrousel",
                ["Plugins.Widgets.JCarousel.JCarousel"] = "Jacrousel",
                ["Plugins.Widgets.JCarousel.Save"] = "Save",
                ["Plugins.Widgets.JCarousel.Saved"] = "The plugin has been updated successfully.",
                ["Plugins.Widgets.JCarousel.View"] = "View",
                ["Plugins.Widgets.JCarousel.Edit"] = "Edit",
                ["Plugins.Widgets.JCarousel.Delete"] = "Delete",
                ["Plugins.Widgets.JCarousel.Search"] = "Search",
                ["Plugins.Widgets.JCarousel.SaveContinue"] = "Save and Continue",
                ["Plugins.Widgets.JCarousel.Added"] = "Jcarousel added",
                ["Plugins.Widgets.JCarousel.Products.Fields.Name"] = "Product Name",
                ["Plugins.Widgets.JCarousel.Products.Fields.Published"] = "Published",
                ["Plugins.Widgets.JCarousel.Products.List.SearchProductName"] = "Product name",
                ["Plugins.Widgets.JCarousel.Products.List.SearchProductName.hint"] = "A Product name.",
                ["Plugins.Widgets.JCarousel.Products.List.SearchCategory"] = "Category",
                ["Plugins.Widgets.JCarousel.Products.List.SearchCategory.hint"] = "Search by a specific category.",
                ["Plugins.Widgets.JCarousel.Products.List.SearchManufacturer"] = "Manufacturer",
                ["Plugins.Widgets.JCarousel.Products.List.SearchManufacturer.hint"] = "Search by a specific manufacturer.",
                ["Plugins.Widgets.JCarousel.Products.List.SearchStore"] = "Store",
                ["Plugins.Widgets.JCarousel.Products.List.SearchStore.hint"] = "Search by a specific store.",
                ["Plugins.Widgets.JCarousel.Products.List.SearchVendor"] = "Vendor",
                ["Plugins.Widgets.JCarousel.Products.List.SearchVendor.hint"] = "Search by a specific vendor.",
                ["Plugins.Widgets.JCarousel.Products.List.SearchProductType"] = "Product type",
                ["Plugins.Widgets.JCarousel.Products.List.SearchProductType.hint"] = "Search by a product type.",
                ["Plugins.Widgets.JCarousel.Widgets.List.SearchWidgetName"] = "Widget name",
                ["Plugins.Widgets.JCarousel.Products.List.SearchProductType.hint"] = "A Widget name.",
                ["Plugins.Widgets.JCarousel.Widgets.List.SearchJCarouselName"] = "Jcarousel name",
                ["Plugins.Widgets.JCarousel.Widgets.List.SearchJCarouselName.hint"] = "A Jcarousel name",
                ["Plugins.Widgets.JCarousel.ProductId"] = "Product Id",
                ["Plugins.Widgets.JCarousel.JCarouselId"] = "Jcarousel Id",
                ["Plugins.Widgets.JCarousel.AddNewCarousel"] = "Add New JCarousel",
                ["Plugins.Widgets.JCarousel.AddNew"] = "Add New",
                ["Plugins.Widgets.JCarousel.BackToList"] = "Back to carousel list",
                ["Plugins.Widgets.JCarousel.EditJcarouselDetails"] = "Edit JCarousel details",
                ["Plugins.Widgets.JCarousel.Products.Fields.Product"] = "Product",
                ["Plugins.Widgets.JCarousel.Products.Fields.Isfeaturedproduct"] = "IsFeature Product",
                ["Plugins.Widgets.JCarousel.Products.Fields.Displayorder"] = "Display order",
                ["Plugins.Widgets.JCarousel.Products.AddNew"] = "Add a new product",
                ["Plugins.Widgets.JCarousel.Enabled"] = "Enable the JCarousel",
                ["Plugins.Widgets.JCarousel.Enabled.Hint"] = "Check to enable the plugin",
                ["Plugins.Widgets.JCarousel.LazyLoadNumberOfProductInCarousel"] = "Lazy Load Number Of Product In Carousel",
                ["Plugins.Widgets.JCarousel.LazyLoadNumberOfProductInCarousel.Hint"] = "Lazy Load Number Of Product In each Carousel",
                ["Plugins.Widgets.JCarousel.Name"] = "Name",
                ["Plugins.Widgets.JCarousel.Name.Hint"] = "Specifies the name of the JCarousel.",
                ["Plugins.Widgets.JCarousel.DataSourceType"] = "Data source type",
                ["Plugins.Widgets.JCarousel.DataSourceTypeId"] = "Data source type",
                ["Plugins.Widgets.JCarousel.DataSourceTypeId.Hint"] = "Specifies Data source type",
                ["Plugins.Widgets.JCarousel.MaxItems"] = "Maximum number of items",
                ["Plugins.Widgets.JCarousel.MaxItems.Hint"] = "The maximum number of items that will contain the JCarousel.",
                ["Plugins.Widgets.JCarousel.VisibleItems"] = "Number of visible items in the JCarousel",
                ["Plugins.Widgets.JCarousel.VisibleItems.Hint"] = "Specify the number of visible items at a time, that are going to be shown in the JCarousel.",
                ["Plugins.Widgets.JCarousel.Skin"] = "Skin",
                ["Plugins.Widgets.JCarousel.Skin.Hint"] = "Specufies a custom class name for the current JCarousel.",
                ["Plugins.Widgets.JCarousel.StartIndex"] = "Item start index",
                ["Plugins.Widgets.JCarousel.StartIndex.Hint"] = "Item start index",
                ["Plugins.Widgets.JCarousel.ScrollItems"] = "Number of items to scroll by",
                ["Plugins.Widgets.JCarousel.ScrollItems.Hint"] = "The number of items to scroll by.",
                ["Plugins.Widgets.JCarousel.DotNevigation"] = "	Dot navigation",
                ["Plugins.Widgets.JCarousel.DotNevigation.Hint"] = "Show dot navigation.",
                ["Plugins.Widgets.JCarousel.ArrowNevigation"] = "	Arrows navigation",
                ["Plugins.Widgets.JCarousel.ArrowNevigation.Hint"] = "Prev/Next Arrows navigation.",
                ["Plugins.Widgets.JCarousel.DisplayOrder"] = "Display Order",
                ["Plugins.Widgets.JCarousel.DisplayOrder.Hint"] = "Display Order of Jcarousel",
                ["Plugins.Widgets.JCarousel.SelectedCategoryId"] = "Category",
                ["Plugins.Widgets.JCarousel.SelectedCategoryId.Hint"] = "Selected Category for the Jcarousel",
                ["Plugins.Widgets.JCarousel.Viewall"] = "View All",
                ["Plugins.Widgets.Jcarousel.Fields.Name.Required"] = "Jcarousel Name required.",
                ["Plugins.Widgets.Jcarousel.Fields.MaxItems.GreaterThanOrEqualZero"] = "Max items must be greater than zero.",
                ["Plugins.Widgets.Jcarousel.Fields.DisplayOrder.GreaterThanOrEqualZero"] = "Display Order must be greater than zero.",
                ["Plugins.Widgets.Jcarousel.Admin.Configuration.Settings"] = "JCarousel Configuration",
                ["Plugins.Widgets.JCarousel.Name.AlreadyExists"] = "Jcarousel Name already exists",
                ["Plugins.Widgets.Jcarousel.Categories.Fields.Select"] = "Please Select Category",
                ["Plugins.Widgets.JCarousel.IsBestSeller"] = "Bestseller",
                ["Plugins.Widgets.JCarousel.IsBestSeller.Hint"] = "Bestseller",
                ["Plugins.Widgets.JCarousel.EnableViewAll"] = "Enable redirect at the end of the scroll",
                ["Plugins.Widgets.JCarousel.EnableViewAll.Hint"] = "Enable redirect at the end of the scroll."
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<JCarouselPluginSettings>();

            //remove plugin from active widget
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(JCarouselDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(JCarouselDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.JCarousel");
            await base.UninstallAsync();
        }
        /// <summary>
        /// Create SiteMap
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "Manage Jcarousel",
                Title = await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.ManageJCarousel"),
                ControllerName = "",
                ActionName = "#",
                IconClass = "far fa-dot-circle",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } },
            };
            var subMenuItem = new SiteMapNode()
            {
                SystemName = "JCarousel.Settings",
                Title = await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.Settings"),
                ControllerName = "JCarousel",
                ActionName = "Configure",
                IconClass = "far fa-dot-circle",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } },
            };
            var subMenuItema = new SiteMapNode()
            {
                SystemName = "Jcarousel.List",
                Title = await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.JCarousel"),
                ControllerName = "JCarousel",
                ActionName = "List",
                IconClass = "far fa-dot-circle",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } },
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
            {
                pluginNode.ChildNodes.Add(menuItem);
                menuItem.ChildNodes.Add(subMenuItem);
                menuItem.ChildNodes.Add(subMenuItema);
            }
            else
            {
                rootNode.ChildNodes.Add(subMenuItem);
            }
        }

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;
        #endregion
    }
}
