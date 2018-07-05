namespace Wilmar.SSAS.MDXBuilder.Where
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// 等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqualWhereItem: AbstractWhereItem
    {
        /// <summary>
        /// 值
        /// </summary>
        public dynamic Value { get; set; }

        /// <summary>
        /// 构建相等的条件项
        /// </summary>
        /// <returns></returns>
        public override string Build()
        {
            if (this.Value==null) return null;

            return $"{this.Dimission}.&[{this.Value}]";
        }
    }
}
