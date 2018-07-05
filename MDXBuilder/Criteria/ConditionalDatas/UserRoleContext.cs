using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class UserRoleContext
    {
        private IRoleStrategy strategy;

        
        public UserRoleContext(UserInfoDTO userInfo)
        {
            if (userInfo == null)
                throw new Exception("UserRoleContext构造函数userInfo参数不能为空!");

            strategy = RoleStrategyFactory.GetRoleStrategy(userInfo);
 
        }

         

        public dynamic UserRoleSetValue(AbstractCriteria ac,PropertyInfo property)
        {
            return strategy.GetDimssionValue(ac,property);
        }
    }
}
