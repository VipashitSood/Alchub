using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.NivoSlider.Models
{
    public partial record ConfigurationModel : BaseNopModel
    {
        //for mobile
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Picture")]
        [UIHint("Picture")]
        public int MobilePicture1Id { get; set; }
        public bool MobilePicture1Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Text")]
        public string MobileText1 { get; set; }
        public bool MobileText1_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Link")]
        public string MobileLink1 { get; set; }
        public bool MobileLink1_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.AltText")]
        public string MobileAltText1 { get; set; }
        public bool MobileAltText1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Picture")]
        [UIHint("Picture")]
        public int MobilePicture2Id { get; set; }
        public bool MobilePicture2Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Text")]
        public string MobileText2 { get; set; }
        public bool MobileText2_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Link")]
        public string MobileLink2 { get; set; }
        public bool MobileLink2_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.AltText")]
        public string MobileAltText2 { get; set; }
        public bool MobileAltText2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Picture")]
        [UIHint("Picture")]
        public int MobilePicture3Id { get; set; }
        public bool MobilePicture3Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Text")]
        public string MobileText3 { get; set; }
        public bool MobileText3_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Link")]
        public string MobileLink3 { get; set; }
        public bool MobileLink3_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.AltText")]
        public string MobileAltText3 { get; set; }
        public bool MobileAltText3_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Picture")]
        [UIHint("Picture")]
        public int MobilePicture4Id { get; set; }
        public bool MobilePicture4Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Text")]
        public string MobileText4 { get; set; }
        public bool MobileText4_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Link")]
        public string MobileLink4 { get; set; }
        public bool MobileLink4_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.AltText")]
        public string MobileAltText4 { get; set; }
        public bool MobileAltText4_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Picture")]
        [UIHint("Picture")]
        public int MobilePicture5Id { get; set; }
        public bool MobilePicture5Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Text")]
        public string MobileText5 { get; set; }
        public bool MobileText5_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.Link")]
        public string MobileLink5 { get; set; }
        public bool MobileLink5_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Mobile.AltText")]
        public string MobileAltText5 { get; set; }
        public bool MobileAltText5_OverrideForStore { get; set; }
    }
}