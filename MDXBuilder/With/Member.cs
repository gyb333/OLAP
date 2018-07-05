

namespace Wilmar.SSAS.MDXBuilder.With
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;

    /// <summary>
    /// 
    /// </summary>
    public class Member : IMDXWithItem
    {
        private string _MemberIdentifier;
        private string _Expresion;
        private Member.Format _Format;

        public enum Format
        {
            /// <summary>
            /// Without options
            /// </summary>
            NONE,
            /// <summary>
            /// 
            /// </summary>
            Percent,
            /// <summary>
            /// 
            /// </summary>
            Currency
        };

        public Member(string MemberIdentifier, string Expresion, Member.Format Format = Member.Format.NONE) 
        {
            this._MemberIdentifier = MemberIdentifier;
            this._Expresion = Expresion;
            this._Format = Format;
        }

        public string Build()
        {
            return $"MEMBER {this._MemberIdentifier } AS {this._Expresion } {((this._Format != Member.Format.NONE) ? ", FORMAT_STRING = '" + this._Format.ToString() + "'" : "")}";
        }
    }
}
