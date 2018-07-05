using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class BusinessStrategy : IRoleStrategy
    {
        public UserInfoDTO userInfo;
        public RoleType roleType;

        public BusinessStrategy(RoleType roleType, UserInfoDTO userInfo)
        {
            this.roleType = roleType;
            this.userInfo = userInfo;
        }




        public virtual dynamic GetDimssionValue(AbstractCriteria ac, PropertyInfo propertyInfo)
        {
            dynamic ret = null;
            var attr = (CriteriaPropertyAttribute)propertyInfo.GetCustomAttributes(typeof(CriteriaPropertyAttribute), false).FirstOrDefault();
            dynamic value = propertyInfo.GetValue(ac);

            if (attr.UserFlag == UserData.Normal && !string.IsNullOrEmpty(attr.DefaultValue))
            {
                dynamic defaultValue;
                if (attr.Opration == CriteriaOprations.Eq)
                {
                    defaultValue = attr.DefaultValue;
                }
                else
                {
                    defaultValue = CriteriaHelper.GetDimissionValueByInt(attr.DefaultValue).ToArray();
                }


                switch (attr.Conditions)
                {
                    case CriteriaConditions.BusinessCondition:
                        ret = defaultValue;
                        break;
                    case CriteriaConditions.KBPBusinessCondition:
                        if ((attr.Role == UserRole.Common
                                && (roleType == RoleType.KBPAdmin || roleType == RoleType.KBPUser))
                        || (attr.Role == UserRole.User && roleType == RoleType.KBPUser)
                        || (attr.Role == UserRole.Admin && roleType == RoleType.KBPAdmin)
                        )
                            ret = defaultValue;
                        break;
                    case CriteriaConditions.KDSBusinessCondition:
                        if ((attr.Role == UserRole.Common
                              && (roleType == RoleType.KDSAdmin || roleType == RoleType.KDSUser))
                            || (attr.Role == UserRole.User && roleType == RoleType.KDSUser)
                            || (attr.Role == UserRole.Admin && roleType == RoleType.KDSAdmin)
                            )
                            ret = defaultValue;
                        break;

                    case CriteriaConditions.QueryBusinessCondition:
                        ret = CriteriaHelper.GetDimissionValue(attr.Opration, value, defaultValue, attr.NoDataID);
                        break;
                    case CriteriaConditions.QueryKBPBusinessCondition:
                        if ((attr.Role == UserRole.Common
                                && (roleType == RoleType.KBPAdmin || roleType == RoleType.KBPUser))
                        || (attr.Role == UserRole.User && roleType == RoleType.KBPUser)
                        || (attr.Role == UserRole.Admin && roleType == RoleType.KBPAdmin)
                        )
                            ret = CriteriaHelper.GetDimissionValue(attr.Opration, value, defaultValue, attr.NoDataID);

                        else if ((attr.Role == UserRole.Common
                             && (roleType == RoleType.KDSAdmin || roleType == RoleType.KDSUser))
                             || (attr.Role == UserRole.User && roleType == RoleType.KDSUser)
                            || (attr.Role == UserRole.Admin && roleType == RoleType.KDSAdmin)
                            )
                            ret = value;
                        break;
                    case CriteriaConditions.QueryKDSBusinessCondition:
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


                    default:
                        break;
                }

            }



            return ret;
        }

    }
}
