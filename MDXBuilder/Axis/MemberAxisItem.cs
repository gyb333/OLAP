namespace Wilmar.SSAS.MDXBuilder.Axis
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// MDX 成员
    /// </summary>
    public class MemberAxisItem : AbstractAxisItem
    {

        public string Member { get; set; }


        public MemberAxisItem(IMDXAxisItem AxisItem)
        {
        }

        public MemberAxisItem(string m)
        {
            this.Member = m;
        }

        public override string Build()
        {
            return this.Member ?? "";
        }
    }
}
