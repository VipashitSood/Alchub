using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.NivoSlider
{
    public partial class NivoSliderSettings : ISettings
    {
        //for mobile
        public int MobilePicture1Id { get; set; }
        public string MobileText1 { get; set; }
        public string MobileLink1 { get; set; }
        public string MobileAltText1 { get; set; }

        public int MobilePicture2Id { get; set; }
        public string MobileText2 { get; set; }
        public string MobileLink2 { get; set; }
        public string MobileAltText2 { get; set; }

        public int MobilePicture3Id { get; set; }
        public string MobileText3 { get; set; }
        public string MobileLink3 { get; set; }
        public string MobileAltText3 { get; set; }

        public int MobilePicture4Id { get; set; }
        public string MobileText4 { get; set; }
        public string MobileLink4 { get; set; }
        public string MobileAltText4 { get; set; }

        public int MobilePicture5Id { get; set; }
        public string MobileText5 { get; set; }
        public string MobileLink5 { get; set; }
        public string MobileAltText5 { get; set; }
    }
}