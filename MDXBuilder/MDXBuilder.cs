namespace Wilmar.SSAS.MDXBuilder
{
    using Axis;
    using Errors;
    using System.Collections.Generic;
    using System.Text;
    using Wilmar.SSAS.MDXBuilder.Cube;
    using Wilmar.SSAS.MDXBuilder.Interfaces;
    using With;

    /// <summary>
    /// 构建MDX语句
    /// </summary>
    public class MDXBuilder : IBuilder
    {
        private MDXWith MDXWith;
        private MDXWhere MDXWhere;
        private MDXCube MDXCube;
        private List<IMDXAxis> AxisList;
        private string MDXSql;

        /// <summary>
        /// Constructor
        /// </summary>
        public MDXBuilder()
        {
            this.AxisList = new List<IMDXAxis>();

        }

        public MDXBuilder Mdx( string mdx)
        {
            this.MDXSql = mdx;
            return this;
        }

        public MDXBuilder With()
        {
            this.MDXWith = new MDXWith();
            return this;
        }

        public MDXBuilder Member(IMDXWithItem Item)
        {
            this.MDXWith.AddItem(Item);
            return this;
        }
        public MDXBuilder Member(string identifier, string expresion)
        {
            var member = new Member(identifier, expresion);
            this.MDXWith.AddItem(member);
            return this;
        }


        public MDXBuilder Set(IMDXWithItem Item)
        {
            this.MDXWith.AddItem(Item);
            return this;
        }

        public MDXBuilder Set(string identifier, string expresion)
        {
            var exp = new StringAxisItem(expresion);
            var set = new Set(identifier, new SetAxisItem(exp));
            this.MDXWith.AddItem(set);
            return this;
        }


        public MDXBuilder Axis(IMDXAxis axis)
        {
            if (axis != null)
            {
                this.AxisList.Add(axis);
            }
            return this;
        }

        public MDXBuilder Axis(string axis, string type)
        {
            if (axis != null)
            {
                this.AxisList.Add(new MDXAxis
                {
                    AxisItem = new NonEmptyAxisItem(new StringAxisItem(axis)),
                    AxisType = type
                });
            }
            return this;
        }


        public MDXBuilder Cube(MDXCube cube)
        {
            this.MDXCube = cube;
            return this;
        }

        public MDXBuilder Cube(string cube)
        {
            this.MDXCube = new MDXCube(cube);
            return this;
        }


        public MDXBuilder Where(MDXWhere where)
        {
            this.MDXWhere = where;
            return this;
        }

        public string Build()
        {
            //if (this.AxisList.Count == 0)
            //{
            //    throw new MDXException("未指定查询抽！");
            //}

            if (this.MDXCube ==null)
            {
                throw new MDXException("未指定多维数据集或子查询！");
            }

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(this.MDXSql))
            {
                sb.AppendLine(this.MDXSql);
            }
            else
            {
                if (this.MDXWith != null && this.MDXWith.Count > 0)
                {
                    sb.AppendLine(this.MDXWith.Build());
                }

                //构造查询抽
                sb.AppendLine(" SELECT ");
                var isFirstAxis = true;
                foreach (var a in this.AxisList)
                {
                    if (!isFirstAxis)
                    {
                        sb.Append(",");
                    }
                    sb.AppendLine(a.Build());
                    isFirstAxis = false;
                }
            }


            //构造查询数据集
            sb.AppendLine($" FROM {this.MDXCube.Build()}");

            //构造条件
            if (this.MDXWhere != null)
            {
                var where = this.MDXWhere.Build();
                if (!string.IsNullOrEmpty(where))
                {
                    sb.AppendLine($" WHERE {where}");
                }
            }

            return sb.ToString();
        }
    }
}
