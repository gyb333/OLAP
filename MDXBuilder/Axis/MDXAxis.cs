namespace Wilmar.SSAS.MDXBuilder.Axis
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// 查询轴
    /// </summary>
    public class MDXAxis : IMDXAxis
    {
        public const string ROW_AXIS = "ROWS";
        public const string COLUMN_AXIS = "COLUMNS";
        public const string SLICE_AXIS = "SLICE";

        public MDXAxis()
        {
        }

        public MDXAxis(string type)
        {
            this.AxisType = type;
        }

        public string Build()
        {
            if (this.AxisItem == null) return string.Empty;

 	        return $"{this.AxisItem.Build()} ON {this.AxisType}";
        }

        public IMDXAxisItem AxisItem { get; set;}

        public string AxisType { get; set; }
    }
}
