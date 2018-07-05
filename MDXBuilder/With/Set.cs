

namespace Wilmar.SSAS.MDXBuilder.With
{
    using Axis;
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// 
    /// </summary>
    public class Set :IMDXWithItem
    {
        private IMDXAxisItem _Set;
        private string _Name;


        public Set(string Name, IMDXAxisItem Set)
        {
            this._Set = Set;
            this._Name = Name;
        }

        public string Build()
        {
            if (this._Set == null)
            {
                return string.Empty;
            }

            var txt = this._Set.Build();
            var retTxt = $"SET { this._Name } AS ";
            if (this._Set is SetAxisItem)
            {
                retTxt = txt;
            }
            else
            {
                retTxt = $"'{txt }'";
            }

            return retTxt;
        }
    }
}
