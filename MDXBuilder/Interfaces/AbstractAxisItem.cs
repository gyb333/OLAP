namespace Wilmar.SSAS.MDXBuilder.Interfaces
{

    /// <summary>
    /// MDX轴项
    /// </summary>
    public abstract class AbstractAxisItem  : IMDXAxisItem
    {

        /// <summary>
        /// 生成语句
        /// </summary>
        /// <returns></returns>
        abstract public string Build();
    }
}
