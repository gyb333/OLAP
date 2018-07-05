using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public enum CriteriaConditions
    {

        QueryCondition =0,          //查询条件控制

        PermissionsCondition =1,    //公共权限条件控制 
        KDSPermissionsCondition =2,
        KBPPermissionsCondition =3,

        BusinessCondition =4,       //公共业务条件控制

        KDSBusinessCondition = 5,   //KDS独有的业务权限

        KBPBusinessCondition =6,    //KBP独有的业务权限

       



        QueryPermissionsCondition = 7,     //前端查询并且权限条件控制

        QueryKDSPermissionsCondition =8,

        QueryKBPPermissionsCondition =9,

 


        QueryBusinessCondition = 10,       //前端查询并且业务条件控制

        QueryKDSBusinessCondition = 11,

        QueryKBPBusinessCondition = 12,


        UnCondition = 13    //特殊情况，排除后端条件控制



    }
}
