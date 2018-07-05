

namespace Wilmar.SSAS.MDXBuilder.Axis
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    public class NonEmptyAxisItem : AbstractAxisItem
    {
        private IMDXAxisItem item;

        public NonEmptyAxisItem(IMDXAxisItem  item)
        {
            this.item = item;
        }

        public override string Build()
        {
            return $"NON EMPTY {this.item.Build()} ";
        }
    }
}
