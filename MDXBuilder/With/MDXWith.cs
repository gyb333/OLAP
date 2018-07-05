

namespace Wilmar.SSAS.MDXBuilder.With
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Wilmar.SSAS.MDXBuilder.Interfaces;


    public class MDXWith:IBuilder
    {
        private IList<IMDXWithItem> WithList;

        public MDXWith()
        {
            this.WithList = new List<IMDXWithItem>();
        }

        public void AddItem(IMDXWithItem Item)
        {
            this.WithList.Add(Item);
        }

        public int Count
        {
            get
            {
                return this.WithList.Count;
            }
        }

        public string Build()
        {
            if (this.WithList == null || this.WithList.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach(var w in this.WithList)
            {
                if (sb.Length > 0) sb.Append(Environment.NewLine);

                sb.Append(w.Build());

            }

            return $" WITH {sb.ToString()}";
        }
    }
}
