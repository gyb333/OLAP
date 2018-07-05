using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wilmar.KDSA.Service.Controllers;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class KBPUserStrategy : PermissionsStrategy
    {
        public KBPUserStrategy(RoleType roleType, UserInfoDTO userInfo) : base(roleType, userInfo)
        {

        }

        
    }
}
