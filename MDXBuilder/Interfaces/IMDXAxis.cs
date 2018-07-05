namespace Wilmar.SSAS.MDXBuilder.Interfaces
{
    public interface IMDXAxis: IBuilder
    {
     

        /// <summary>
        /// 
        /// </summary>
        IMDXAxisItem AxisItem { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string AxisType { get; set; }
    }
}
