using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class KDSUserStrategy : PermissionsStrategy
    {
        public KDSUserStrategy(RoleType roleType, UserInfoDTO userInfo) :base(roleType, userInfo)
        {

        }

        public override dynamic GetDimssionValue(AbstractCriteria ac, PropertyInfo propertyInfo)
        {
            dynamic ret = null;            
            ret = CriteriaHelper.GetDimissionValueByUserData(ac, propertyInfo, roleType, userInfo);
            if (ret == null)
                ret = base.GetDimssionValue(ac, propertyInfo);
            return ret;
        }

    }
}
