using ClosedXML.Excel;
namespace Nop.Services.ExportImport.Help
{
    public partial class PropertyManager<T>
    {
        public virtual void ReadFromSpecificationAttributesXlsx(IXLWorksheet worksheet, int row, int position = 0)
        {
            if (worksheet?.Cells() == null)
                return;
            if (position == 0)
                return;
            foreach (var prop in _properties.Values)
            {
                prop.PropertyValue = worksheet.Row(row).Cell(position).Value;
                position++;
            }
        }
    }
}
