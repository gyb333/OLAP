using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    /// <summary>
    /// 共有的权限条件
    /// </summary>
    public class PermissionsStrategy : BusinessStrategy
    {
        public PermissionsStrategy(RoleType roleType, UserInfoDTO userInfo) : base(roleType, userInfo)
        {

        }


      

        public override dynamic GetDimssionValue(AbstractCriteria ac, PropertyInfo propertyInfo)
        {
            dynamic ret = null;
            var attr = (CriteriaPropertyAttribute)propertyInfo.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault();
            dynamic value = propertyInfo.GetValue(ac);
            if (attr.UserFlag== UserData.Normal && attr.EntityID != 0)
            {
                dynamic defaultValue = PermissionsConditionBL.GetWhereByEntityID(userInfo.UserID, attr);

                switch (attr.Conditions)
                {

                    case CriteriaConditions.PermissionsCondition:
                        ret = defaultValue;
                        break;
                    
                    case CriteriaConditions.KBPPermissionsCondition:
                        if ((attr.Role == UserRole.Common
                             && (roleType == RoleType.KBPAdmin || roleType == RoleType.KBPUser))
                       || (attr.Role == UserRole.User && roleType == RoleType.KBPUser)
                       || (attr.Role == UserRole.Admin && roleType == RoleType.KBPAdmin)
                       )
                            ret = defaultValue;
                        break;
                    

                    case CriteriaConditions.KDSPermissionsCondition:
                        if ((attr.Role == UserRole.Common
                            && (roleType == RoleType.KDSAdmin || roleType == RoleType.KDSUser))
                      || (attr.Role == UserRole.User && roleType == RoleType.KDSUser)
                      || (attr.Role == UserRole.Admin && roleType == RoleType.KDSAdmin)
                      )
                            ret = defaultValue;
                        break;


                    case CriteriaConditions.QueryPermissionsCondition:
                        ret = CriteriaHelper.GetDimissionValue(attr.Opration, value, defaultValue,attr.NoDataID);
                        break;

 
                    case CriteriaConditions.QueryKBPPermissionsCondition:
                        if ((attr.Role == UserRole.Common
                             && (roleType == RoleType.KBPAdmin || roleType == RoleType.KBPUser))
                       || (attr.Role == UserRole.User && roleType == RoleType.KBPUser)
                       || (attr.Role == UserRole.Admin && roleType == RoleType.KBPAdmin)
                       )
                            ret = CriteriaHelper.GetDimissionValue(attr.Opration, value, defaultValue, attr.NoDataID);

                        else if ((attr.Role == UserRole.Common
                             && (roleType == RoleType.KDSAdmin || roleType == RoleType.KDSUser))
                             ||(attr.Role == UserRole.User && roleType == RoleType.KDSUser)
                            || (attr.Role == UserRole.Admin && roleType == RoleType.KDSAdmin)
                            )
                            ret = value;
                        break;


  
                    case CriteriaConditions.QueryKDSPermissionsCondition:
                        if ((attr.Role == UserRole.Common
                            && (roleType == RoleType.KDSAdmin || roleType == RoleType.KDSUser))
                      || (attr.Role == UserRole.User && roleType == RoleType.KDSUser)
                      || (attr.Role == UserRole.Admin && roleType == RoleType.KDSAdmin)
                      )
                            ret = CriteriaHelper.GetDimissionValue(attr.Opration, value, defaultValue, attr.NoDataID);

                        else if ((attr.Role == UserRole.Common
                             && (roleType == RoleType.KBPAdmin || roleType == RoleType.KBPUser))
                             || (attr.Role == UserRole.User && roleType == RoleType.KBPUser)
                            || (attr.Role == UserRole.Admin && roleType == RoleType.KBPAdmin)
                            )
                            ret = value;
                        break;

                    default:break;

                }


            }
            if (ret == null)
                ret = base.GetDimssionValue(ac,propertyInfo);
            return ret;
            
        }
    }
}
