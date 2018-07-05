using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public abstract class PermissionsCriteria : BusinessCriteria
    {
        /// <summary>
        /// 账号ID
        /// </summary>
        /// 或者[Dim_Distrbutors].[Company]
        [CriteriaProperty(Dimission = "[Dim_CompBranches].[Company]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
            , Conditions = CriteriaConditions.QueryKDSPermissionsCondition,Role = UserRole.Common,UserFlag = UserData.CompanyID)]
        public int[] CompanyIDs { get; set; }

        /// <summary>
        /// 经销商ID
        /// </summary>
        /// 或者 [Dim_Distrbutors].[DistributorID]
        [CriteriaProperty(Dimission = "[Dim_CompBranches].[Distributor]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
           , Conditions = CriteriaConditions.QueryKDSPermissionsCondition,Role = UserRole.User,EntityID =606)]
        public int[] DistributorIDs { get; set; }



        ///// <summary>
        ///// 户头
        ///// </summary>
        //[CriteriaProperty(Dimission = " [Dim_CompBranches].[CompBranch]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
        //   , Conditions = CriteriaConditions.QueryKDSPermissionsCondition, Role = UserRole.User, EntityID =607)]
        //public int[] CompBranchIDs { get; set; }

        /// <summary>
        /// 户头
        /// </summary>
        [CriteriaProperty(Dimission = " [Dim_CompBranches].[CompBranch]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
           , Conditions = CriteriaConditions.QueryKDSPermissionsCondition, Role = UserRole.User, EntityID = 607)]
        public int[] CompanyBranIDs { get; set; }


        /// <summary>
        /// 业务线
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[BussLine]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
            , Conditions = CriteriaConditions.QueryPermissionsCondition, Role = UserRole.User, EntityID =631)]
        public int[] BussLineIDs { get; set; }


        /// <summary>
        /// 分公司
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Orgs].[Org]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
            , Conditions = CriteriaConditions.QueryKBPPermissionsCondition, Role = UserRole.User, EntityID =601)]
        public int[] OrgIDs { get; set; }


        /// <summary>
        /// 业务员
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_CompanyStaffs].[Emp]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
            , Conditions = CriteriaConditions.QueryKDSPermissionsCondition, Role = UserRole.User, EntityID = 627,UserFlag = UserData.EmpID)]
        public int[] EmpIDs { get; set; }

    }
}
