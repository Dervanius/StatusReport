namespace StatusReport.Information
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelExportAttribute : Attribute
    {
        public string DisplayName { get; }

        public ExcelExportAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
