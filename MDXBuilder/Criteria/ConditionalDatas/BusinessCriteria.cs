using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public abstract class BusinessCriteria
    {
         
        /// <summary>
        /// 状态
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_BillStatus].[BillStatus]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 1
            ,Conditions = CriteriaConditions.QueryBusinessCondition, Role = UserRole.Common,DefaultValue ="7,8,9")]
        public int[] BillStatusIDs { get; set; }

        /// <summary>
        /// 是否集团产品
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[IsGroupFlag]", Opration = CriteriaOprations.Eq, Type = CriteriaBuildType.SubCube, Order = 2
            , Conditions = CriteriaConditions.QueryKBPBusinessCondition, Role = UserRole.Common,DefaultValue ="True")]
        public string IsGroupFlag { get; set; }
    }
}
