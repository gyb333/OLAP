namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    using Axis;
    using Dapper;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Net.Http;
    using Wilmar.Interface.Sync.OLAP;
    using Wilmar.KDSA.Service.Controllers;
    using Wilmar.SSAS.MDXBuilder.Cube;
    using Wilmar.SSAS.MDXBuilder.Interfaces;
    using Wilmar.SSAS.MDXBuilder.Where;

    /// <summary>
    /// 条件对象抽象类
    /// </summary>
    public abstract class AbstractCriteria : PermissionsCriteria
    {

        public virtual void SetDimissionValuesToSubCube(UserInfoDTO userInfo)
        {
            SetDimissionValues(userInfo, CriteriaBuildType.SubCube);
        }

        public virtual void SetDimissionValuesToWhere(UserInfoDTO userInfo)
        {
            SetDimissionValues(userInfo, CriteriaBuildType.Where);
        }

        private  void SetDimissionValues(UserInfoDTO userInfo, CriteriaBuildType criteriaBuildType)
        {
            // 获取所有的需要转未子查询的属性
            var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var subCubePros = properties.Where(p => p.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault(t => ((CriteriaPropertyAttribute)t).Type == criteriaBuildType) != null);
            foreach (var s in subCubePros)
            {
                var attr = (CriteriaPropertyAttribute)s.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault();
                dynamic value = s.GetValue(this);

                switch (attr.Conditions)
                {
                    case CriteriaConditions.UnCondition:
                    case CriteriaConditions.QueryCondition:
                        break;

                    default:
                        value = new UserRoleContext(userInfo).UserRoleSetValue(this, s);
                        break;
                }
                if(value!=null)
                    s.SetValue(this, value);
            }
        }


       



        public virtual MDXCube ToSubCube(string cubeName)
        {
            //获取所有的需要转未子查询的属性
            var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var subCubePros = properties.Where(p => p.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault(t => ((CriteriaPropertyAttribute)t).Type == CriteriaBuildType.SubCube) != null);

            //构造子查询Cube
            MDXCube cube = new MDXCube(cubeName);
            foreach (var s in subCubePros)
            {
                var attr = (CriteriaPropertyAttribute)s.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault();
                dynamic value = s.GetValue(this);

                switch (attr.Opration)
                {
                    case CriteriaOprations.Eq:
                        if (value != null)
                        {
                            var axis = this.ToAxis(attr, value);
                            cube = new MDXCube(cube, axis);
                        }
                        break;
                    default:
                        if (value != null && value.Length > 0)
                        {
                            var axis = this.ToAxis(attr, value);
                            cube = new MDXCube(cube, axis);
                        }
                        break;
                }
            }
            return cube;
        }

        public virtual MDXWhere ToWhere()
        {
            //获取所有的需要转未子查询的属性
            var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var whereItmPros = properties.Where(p => p.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault(t => ((CriteriaPropertyAttribute)t).Type == CriteriaBuildType.Where) != null);

            //构造子查询Cube
            MDXWhere where = new MDXWhere();
            foreach (var s in whereItmPros)
            {
                var attr = (CriteriaPropertyAttribute)s.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault();
                dynamic value = s.GetValue(this);
                switch (attr.Opration)
                {
                    case CriteriaOprations.Eq:
                        if (value != null)
                        {
                            var item = this.ToWhereItem(attr, value);
                            where.And(item);
                        }
                        break;
                    default:
                        if (value != null && value.Length > 0)
                        {
                            var item = this.ToWhereItem(attr, value);
                            where.And(item);
                        }
                        break;
                }
            }

            return where;
        }





        private void GetWhere(CriteriaPropertyAttribute attr, dynamic value, MDXWhere where)
        {

            switch (attr.Opration)
            {
                case CriteriaOprations.Eq:
                    if (value != null)
                    {
                        var item = this.ToWhereItem(attr, value);
                        where.And(item);
                    }
                    break;
                default:
                    if (value != null && value.Length > 0)
                    {
                        var item = this.ToWhereItem(attr, value);
                        where.And(item);
                    }
                    break;
            }
        }


        private static IEnumerable<T> Query<T, CriteriaT>(DbConnection con, HttpRequestMessage request, UserInfoDTO user, string mdx, Dictionary<string, string> maps) where CriteriaT : AbstractCriteria, new()
        {

#if DEBUG&&TEST
            dynamic data = new CriteriaT();
            //  data.BillDates = new int?[] {  201801  };
            data.CompanyIDs = new int[] { 210002 };
#else
            //非管理员
            dynamic data = request.Deserialize<CriteriaT>();
            if (data == null)
            {
                data = new CriteriaT();
            }

            if (user.UserType != 12 && user.UserType != 161)
            {
                data.CompanyIDs = new int[] { user.CompanyID.Value };
            }
            data.BillStatusIDs = new int[] { 7 };
            data.BussLineIDs = new int[] { 2 };
            data.CustChannelTypeIDs = new int[] { 5, 6, 7, 8, 9, 10, 11, 12 };
#endif

            //设置映射关系
            typeof(T).Map(maps);


            //日期查询特殊处理
            if (data.BillDates != null)
            {
                if (data.BillDates.Length > 0)
                {
                    mdx = mdx.Replace("{0}", data.BillDates[0].ToString());
                }

                if (data.BillDates.Length > 1)
                {
                    mdx = mdx.Replace("{1}", data.BillDates[1].ToString());
                }
            }



            //构造完整MDX
            var builder = new MDXBuilder();
            builder.Mdx(mdx)
                    .Cube(data.ToSubCube("[KDS3_CY]"))
                    .Where(data.ToWhere());

            var sql = builder.Build();

            //var table = con.GetSchemaTable(sql);
            return con.Query<T>(sql).ToList();
        }


        private AbstractWhereItem ToWhereItem(CriteriaPropertyAttribute attr, dynamic val)
        {
            AbstractWhereItem result = null;
            switch (attr.Opration)
            {
                case CriteriaOprations.In:
                    result = new InWhereItem
                    {
                        Dimission = attr.Dimission,
                        Values = val
                    };
                    break;
                case CriteriaOprations.Range:
                    result = new RangeWhereItem
                    {
                        Dimission = attr.Dimission,
                        ValueFrom = val[0],
                        ValueTo = val.Length > 1 ? val[1] : null
                    };
                    break;
                default:
                    result = new EqualWhereItem
                    {
                        Dimission = attr.Dimission,
                        Value = val
                    };
                    break;
            }

            return result;
        }


        private IMDXAxis ToAxis(CriteriaPropertyAttribute attr, dynamic val)
        {
            AbstractWhereItem result = ToWhereItem(attr, val);
            var axis = new MDXAxis
            {
                AxisType = MDXAxis.COLUMN_AXIS
            };

            if (attr.Opration == CriteriaOprations.Eq)
            {
                axis.AxisItem = new StringAxisItem($"({{{result.Build()}}})");
            }
            else
            {
                axis.AxisItem = new StringAxisItem($"({result.Build()})");
            }
            return axis;
        }
    }
}
