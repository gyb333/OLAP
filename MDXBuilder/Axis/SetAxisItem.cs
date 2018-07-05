namespace Wilmar.SSAS.MDXBuilder.Axis
{
    using System.Collections.Generic;
    using System.Text;
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    public class SetAxisItem : AbstractAxisItem
    {
        private List<IMDXAxisItem> MemberList;

        public void AddMember()
        {
        }


        public SetAxisItem(IMDXAxisItem AxisItem)
        {
            MemberList = new List<IMDXAxisItem>();
            if (AxisItem != null)
            {
                MemberList.Add(AxisItem);
            }
        }


        public void AddAxisItem(IMDXAxisItem AxisItem)
        {
            if (AxisItem == null) return;
            this.MemberList.Add(AxisItem);
        }

 
        public void AddAxisItemList(List<IMDXAxisItem> AxisItemList)
        {
            this.MemberList.AddRange(AxisItemList);
        }


        public override string Build()
        {
            var sb = new StringBuilder();
            foreach(var m in this.MemberList)
            {
                if (sb.Length > 0) sb.Append(",");

                sb.Append(m.Build());
            }

            return $"{{{sb.ToString()}}}";
        }
    }
}
