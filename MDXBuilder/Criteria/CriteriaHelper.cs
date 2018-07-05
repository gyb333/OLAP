using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;
using Wilmar.SSAS.MDXBuilder.Interfaces;
using Wilmar.SSAS.MDXBuilder.Where;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public static class CriteriaHelper
    {

        public static List<int> GetDimissionValueByInt(string strValue)
        {
            return strValue.Split(',').Select(Int32.Parse).ToList();
        }

        public static dynamic GetDimissionValue(CriteriaOprations Opration, dynamic value, dynamic defaultValue, int noDataID)
        {
            dynamic ret = null;
            if (Opration == CriteriaOprations.Eq)
            {
                if (defaultValue != null)
                {
                    if (value != null && value != defaultValue)
                        ret = "NULL";
                    else
                        ret = defaultValue;
                }
                else
                    ret = value;
                
            }
            else
            {
                if (defaultValue != null)
                {
                    if (value != null&&value.Length > 0)
                    {
                        List<int> l1 = new List<int>(defaultValue);
                        List<int> l2 = new List<int>(value);
                        var temp = l1.Intersect(l2).ToArray(); // 交集
                        if (temp.Length > 0)
                            ret = temp;
                        else
                            ret = new int[] { noDataID };
                    }
                    else
                        ret = defaultValue;

                }
                else
                    ret = value;


            }
            return ret;
        }
        public static dynamic GetDimissionValueByUserData(AbstractCriteria ac, PropertyInfo propertyInfo, RoleType roleType, UserInfoDTO userInfo)
        {
            dynamic ret = null;

            var attr = (CriteriaPropertyAttribute)propertyInfo.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault();
            dynamic value = propertyInfo.GetValue(ac);

            if ((attr.Role == UserRole.Common
                    && (roleType == RoleType.KDSAdmin || roleType == RoleType.KDSUser))
               || (attr.Role == UserRole.User && roleType == RoleType.KDSUser)
                || (attr.Role == UserRole.Admin && roleType == RoleType.KDSAdmin)
                )
            {
                if (attr.UserFlag == UserData.CompanyID)
                {
                    int CompanyID = 0;
                    if (userInfo.CompanyID != null) CompanyID = userInfo.CompanyID.Value;
                    dynamic defalutValue= new int[] { CompanyID };
                    if (attr.Conditions == CriteriaConditions.QueryKDSPermissionsCondition)
                        ret = GetDimissionValue(attr.Opration, value, defalutValue, attr.NoDataID);
                    else if (attr.Conditions == CriteriaConditions.KDSPermissionsCondition)
                        ret = defalutValue;
                }
                else if (attr.UserFlag == UserData.EmpID && attr.EntityID != 0)
                {
                    int EmpID = 0;
                    if (userInfo.EmpID != null) EmpID = userInfo.EmpID.Value;
                    dynamic defaultValue = PermissionsConditionBL.GetWhereByEntityID(userInfo.UserID, attr, EmpID);

                    if (attr.Conditions == CriteriaConditions.QueryKDSPermissionsCondition)
                        ret = GetDimissionValue(attr.Opration, value, defaultValue, attr.NoDataID);
                    else if (attr.Conditions == CriteriaConditions.KDSPermissionsCondition)
                        ret = defaultValue;
                }

            }


            return ret;
        }

        public static void GetWhere(CriteriaPropertyAttribute attr, dynamic value, MDXWhere where)
        {

            switch (attr.Opration)
            {
                case CriteriaOprations.Eq:
                    if (value != null)
                    {
                        var item = ToWhereItem(attr, value);
                        where.And(item);
                    }
                    break;
                default:
                    if (value != null && value.Length > 0)
                    {
                        var item = ToWhereItem(attr, value);
                        where.And(item);
                    }
                    break;
            }
        }

        public static AbstractWhereItem ToWhereItem(CriteriaPropertyAttribute attr, dynamic val)
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
    }
}
