

namespace Wilmar.SSAS.MDXBuilder.Axis
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    public class StringAxisItem : AbstractAxisItem
    {
        private string text;

        public StringAxisItem(string text)
        {
            this.text = text;
        }

        public override string Build()
        {
            return this.text;
        }
    }
}
