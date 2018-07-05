namespace Wilmar.SSAS.MDXBuilder.Interfaces
{
    public abstract class AbstractWhereItem: IBuilder
    {
        /// <summary>
        /// 生成语句
        /// </summary>
        /// <returns></returns>
         public abstract string Build();

        /// <summary>
        /// 维度
        /// </summary>
         public virtual string Dimission { get; set; }
    }
}
