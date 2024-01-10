using Nop.Core.Caching;

namespace Nop.Plugin.Api.Cache
{
    public static partial class NopModelCacheDefaults
    {
        public static CacheKey ProductAttributeImageSquarePictureModelKey => new("Nop.pres.productattribute.imagesquare.picture-{0}-{1}-{2}", ProductAttributeImageSquarePicturePrefixCacheKey);
        public static string ProductAttributeImageSquarePicturePrefixCacheKey => "Nop.pres.productattribute.imagesquare.picture";

        public static CacheKey ProductReviewsModelKey => new("Nop.pres.product.reviews-{0}-{1}", ProductReviewsPrefixCacheKey, ProductReviewsPrefixCacheKeyById);
        public static string ProductReviewsPrefixCacheKey => "Nop.pres.product.reviews";
        public static string ProductReviewsPrefixCacheKeyById => "Nop.pres.product.reviews-{0}-";

        public static CacheKey ProductDefaultPictureModelKey => new("Nop.pres.product.detailspictures-{0}-{1}-{2}-{3}-{4}-{5}", ProductDefaultPicturePrefixCacheKey, ProductDefaultPicturePrefixCacheKeyById);
        public static string ProductDefaultPicturePrefixCacheKey => "Nop.pres.product.detailspictures";
        public static string ProductDefaultPicturePrefixCacheKeyById => "Nop.pres.product.detailspictures-{0}-";

        public static CacheKey ProductDetailsPicturesModelKey => new("Nop.pres.product.picture-{0}-{1}-{2}-{3}-{4}-{5}", ProductDetailsPicturesPrefixCacheKey, ProductDetailsPicturesPrefixCacheKeyById);
        public static string ProductDetailsPicturesPrefixCacheKey => "Nop.pres.product.picture";
        public static string ProductDetailsPicturesPrefixCacheKeyById => "Nop.pres.product.picture-{0}-";

    }
}
