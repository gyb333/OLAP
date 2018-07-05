using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class RoleStrategyFactory
    {
        public static IRoleStrategy GetRoleStrategy(UserInfoDTO userInfo)
        {
            IRoleStrategy strategy = null;
            switch (userInfo.UserType)
            {
                case 12: //KBP管理员
                    strategy = new KBPAdminStrategy(RoleType.KBPAdmin,userInfo);
                    break;
                case 161:   //营销公司职员
                    strategy = new KBPUserStrategy(RoleType.KBPUser, userInfo);
                    break;

                case 13: //KDS经销商管理员
                    strategy = new KDSAdminStrategy(RoleType.KDSAdmin, userInfo);
                    break;
                case 14:    //KDS业务员权限
                    strategy = new KDSUserStrategy(RoleType.KDSUser, userInfo);
                    break;
                default:
                    strategy = new NullStrategy();
                    break;
            }
            return strategy;
        }
    }
}
