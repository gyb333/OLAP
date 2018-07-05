namespace Wilmar.SSAS.MDXBuilder.Where
{
    using System;
    using System.Text;
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// 多值条件项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public  class InWhereItem : AbstractWhereItem
    {
        /// <summary>
        /// 多值集合
        /// </summary>
        public dynamic Values { get; set; }

        /// <summary>
        /// 生成多值集合条件项
        /// </summary>
        /// <returns></returns>
        public override string Build()
        {
            if(this.Values==null || this.Values.Length == 0)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach(var v in this.Values)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.AppendFormat("{0}.&[{1}]", this.Dimission, v);
            }

            return $"{{{sb.ToString()}}}";
        }
    }
}
