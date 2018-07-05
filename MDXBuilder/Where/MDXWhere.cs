namespace Wilmar.SSAS.MDXBuilder
{
    using System.Collections.Generic;
    using System.Text;
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// MDX 条件生成器
    /// </summary>
    public class MDXWhere:IBuilder
    {
        private IList<AbstractWhereItem> items=new List<AbstractWhereItem>();


        /// <summary>
        /// AND条件项
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public MDXWhere And(AbstractWhereItem Item)
        {
            this.items.Add(Item);
            return this;
        }
 
        /// <summary>
        /// 生成where串
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            var sb = new StringBuilder();
            foreach(var item in this.items)
            {
                var itemStr = item.Build();
                if (!string.IsNullOrEmpty(itemStr))
                {
                    if (sb.Length > 0 )
                    {
                        sb.Append(",");
                       
                    }

                    sb.Append(itemStr);
                }
            }

            if (sb.Length > 0)
            {
                return $"({sb.ToString()})";
            }

            return null;
        }
    }
}
