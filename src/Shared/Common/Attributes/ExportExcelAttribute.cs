namespace Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ExportExcelAttribute : Attribute
{
    public string? HeaderName { get; set; }
    public string? ValueColorMap { get; set; }
    public string? NumericRangeColorMap { get; set; }
    public string? SummaryFunctions { get; set; }
    public string? Format { get; set; }
    public string? BooleanValueMap { get; set; }
    public bool IsLockColumn { get; set; } = false;
}