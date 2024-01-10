using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Topics;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.Checkout;
using Nop.Plugin.Api.DTO.HomeScreen;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.DTO.Manufacturers;
using Nop.Plugin.Api.DTO.OrderItems;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.DTO.ProductAttributes;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.DTO.SpecificationAttributes;
using Nop.Plugin.Api.DTO.Stores;
using Nop.Plugin.Api.DTOs.Categories;
using Nop.Plugin.Api.DTOs.HomeScreen.V1;
using Nop.Plugin.Api.DTOs.JCarousel;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.DTOs.Topics;
using Nop.Plugin.Api.Models.OrdersParameters;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Widgets.JCarousel.Domain;

namespace Nop.Plugin.Api.Helpers
{
    public interface IDTOHelper
    {
        Task<ProductDto> PrepareProductDTOAsync(Product product);
        Task<CategoryDto> PrepareCategoryDTOAsync(Category category);

        /// <summary>
        /// Prepare mobile category hirachy
        /// </summary>
        /// <param name="categoriesDtos"></param>
        /// <param name="rootCategoryId"></param>
        /// <returns></returns>
        Task<List<CategoryHierarchyModel>> PrepareCategoryHierarchyAsync(List<CategoryDto> categoriesDtos, int rootCategoryId, bool includeProductCount = false);

        Task<OrderDto> PrepareOrderDTOAsync(Order order);
        Task<ShoppingCartItemDto> PrepareShoppingCartItemDTOAsync(ShoppingCartItem shoppingCartItem);
        Task<OrderItemDto> PrepareOrderItemDTOAsync(OrderItem orderItem, Order order);
        Task<StoreDto> PrepareStoreDTOAsync(Store store);
        Task<LanguageDto> PrepareLanguageDtoAsync(Language language);
        Task<CurrencyDto> PrepareCurrencyDtoAsync(Currency currency);
        ProductAttributeDto PrepareProductAttributeDTO(ProductAttribute productAttribute);
        ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute);
        SpecificationAttributeDto PrepareSpecificationAttributeDto(SpecificationAttribute specificationAttribute);
        Task<ManufacturerDto> PrepareManufacturerDtoAsync(Manufacturer manufacturer);
        TopicDto PrepareTopicDTO(Topic topic);
        Task<TopCategoryDto> PrepareTopCategoryDTOAsync(Category category);
        Task<FeaturedProductDto> PrepareFeaturedProductDTOAsync(Product product);
        Task<DealsOfTheDayDto> PrepareDealsOfTheDayProductDTOAsync(Product product);
        Task<IList<HomeBannerDto>> PrepareHomeBannerDtoAsync();
        Task<ProductListDto> PrepareProductListDTOAsync(Product product, Customer customer);

        /// <summary>
        /// Prepare product search result model
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <param name="preparePictureModel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<ProductsSearchResultDto> PrepareProductSearchResultDTOAsync(Product product, Customer customer, bool preparePictureModel = true);

        /// <summary>
        /// Prepare search reult dto 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <param name="preparePictureModel"></param>
        /// <returns></returns>
        Task<ProductsSearchResultDto> PrepareProductSearchResultDtoElasticAsync(Product product, Customer customer, bool preparePictureModel = true);

        Task<CustomerRewardPointsDto> PrepareCustomerRewardPointsAsync(CustomerRewardPointsModel customerRewardPointsModel);

        #region ShoppingCart

        /// <summary>
        /// Prepare the order totals dto
        /// </summary>
        /// <param name="customer">customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order totals model
        /// </returns>
        Task<OrderTotalDto> PrepareOrderTotalsDtoAsync(Customer customer, Store store);

        /// <summary>
        /// Prepare discountbox dto
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<DiscountBoxDto> PrepareDiscountBoxDtoAsync(Customer customer);

        /// <summary>
        /// Prepare giftcard box dto
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        Task<GiftCardBoxDto> PrepareGiftCardBoxDto(Customer customer, Store store);

        #endregion

        #region Filter

        Task<CatalogProductsModel> PrepareAllFilters(int categoyId, int? orderById, List<int> availableVendorIds);

        #endregion

        #region Checkout

        /// <summary>
        /// Prepare checkout shipping method dto
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="shippingAddress"></param>
        /// <param name="customer"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        Task<CheckoutShippingMethodDto> PrepareCheckoutShippingMethodDtoAsync(IList<ShoppingCartItem> cart, Address shippingAddress, Customer customer, Store store);

        /// <summary>
        /// Prepare payment method dto
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="filterByCountryId">Filter by country identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment method model
        /// </returns>
        Task<CheckoutPaymentMethodDto> PrepareCheckoutPaymentMethodDtoAsync(IList<ShoppingCartItem> cart, int filterByCountryId, Customer customer, Store store);

        #endregion

        #region Local Resources

        /// <summary>
        /// Prepare local resource values
        /// </summary>
        /// <param name="language"></param>
        /// <param name="resourceNamePrefix"></param>
        /// <returns></returns>
        Task<LocalResourceDto> PrepareLocalResourceDtosAsync(Language language, string resourceNamePrefix = "");

        #endregion

        #region JCarousel

        /// <summary>
        /// Prepare jcarousel dtos 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<IList<JCarouselDto>> PrepareJCarouselDTOsAsync(Customer customer);

        /// <summary>
        /// Prepare jcarousel info dtos 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<IList<JCarouselInfoDto>> PrepareJCarouselInfoDTOsAsync(Customer customer);

        /// <summary>
        /// Prepare jcarousel dto
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<JCarouselDto> PrepareJCarouselDTOAsync(Customer customer, JCarouselLog jCarouselLog);

        #endregion
    }
}