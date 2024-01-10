
namespace Nop.Services.ExportImport
{
    public class ImportSpecificationAttribute
    {
        /// <summary>
        /// Gets or sets vintage
        /// </summary>
        public string Att_Vintage { get; set; }

        /// <summary>
        /// Gets or sets country of origin
        /// </summary>
        public string Att_CountryofOrigin { get; set; }

        /// <summary>
        /// Gets or sets color
        /// </summary>
        public string Att_Color { get; set; }

        /// <summary>
        /// Gets or sets abv
        /// </summary>
        public string Att_ABV { get; set; }

        /// <summary>
        /// Gets or sets state of origin
        /// </summary>
        public string Att_StateofOrigin { get; set; }

        /// <summary>
        /// Gets or sets region
        /// </summary>
        public string Att_Region { get; set; }

        /// <summary>
        /// Gets or sets district (date: 9-01-22: changed to sub type)
        /// </summary>
        public string Att_SubType { get; set; }

        /// <summary>
        /// Gets or sets flavor
        /// </summary>
        public string Att_Flavor { get; set; }

        /// <summary>
        /// Gets or sets alchohol proof
        /// </summary>
        public string Att_AlcoholProof { get; set; }

        /// <summary>
        /// Gets or sets type
        /// </summary>
        public string Att_Type { get; set; }

        /// <summary>
        /// Gets or sets speciality
        /// </summary>
        public string Att_Specialty { get; set; }

        /// <summary>
        /// Gets or sets ratings
        /// </summary>
        public string Att_Ratings { get; set; }

        /// <summary>
        /// Gets or sets appellation
        /// </summary>
        public string Att_Appellation { get; set; }

        /// <summary>
        /// Gets or sets food pairing
        /// </summary>
        public string Att_FoodPairing { get; set; }

        /// <summary>
        /// Gets or sets body
        /// </summary>
        public string Att_Body { get; set; }

        /// <summary>
        /// Gets or sets tasting notes
        /// </summary>
        public string Att_TastingNotes { get; set; }

        /// <summary>
        /// Gets or sets container
        /// </summary>
        public string Att_Container { get; set; }

        /// <summary>
        /// Gets or sets base unit closure
        /// </summary>
        public string Att_BaseUnitClosure { get; set; }

        /// <summary>
        /// Gets or sets base unit closure
        /// </summary>
        public string Att_BrandDescription { get; set; }

        /// <summary>
        /// Gets or sets base unit closure
        /// </summary>
        public string Att_Size { get; set; }

        /// <summary>
        /// Gets or sets variant.
        //Note: Its special specification attribute, mostly use for group products.
        /// </summary>
        public string Att_Variant { get; set; }
    }
}
