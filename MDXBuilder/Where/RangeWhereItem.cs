namespace Wilmar.SSAS.MDXBuilder.Where
{
    using System;
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// 范围条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeWhereItem : AbstractWhereItem
    {
        /// <summary>
        /// 开始值
        /// </summary>
        public dynamic ValueFrom { get; set; }

        /// <summary>
        /// 结束范围值
        /// </summary>
        public dynamic ValueTo { get; set; }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public override string Build()
        {
            if (this.ValueFrom==null && this.ValueTo==null) return null;

            var item1 = "";
            var item2 = "";
            if (this.ValueFrom!=null)
            {
                item1= $"{this.Dimission}.&[{this.ValueFrom}]";
            }
            else
            {
                item1 = $"{this.Dimission}.&[null]";
            }

            if (this.ValueTo!=null)
            {
                item2 = $"{this.Dimission}.&[{this.ValueTo}]";
            }
          

            if (string.IsNullOrEmpty(item2))
            {
                return $"{{{item1}}}";
            }
            else
            {
                return $"{{{item1}:{item2}}}";
            }
           
        }
    }
}
