using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Alchub.Models.Catalog
{
    public partial record ProductSearchOverviewModel : BaseNopEntityModel
    {
        public ProductSearchOverviewModel()
        {
            DefaultPictureModel = new PictureModel();
        }

        public string Name { get; set; }
        public string SeName { get; set; }

        //picture
        public PictureModel DefaultPictureModel { get; set; }
    }
}
