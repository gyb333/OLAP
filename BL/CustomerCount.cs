namespace Wilmar.KDSA.Service.Controllers
{
    using Dapper;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Net.Http;
    using Wilmar.Interface.Sync.OLAP;
    using Wilmar.Interface.Sync.OLAP.Criterias;
    using Wilmar.SSAS.MDXBuilder;

    /// <summary>
    /// 销售回顾
    /// </summary>
    public static class CustomerCount
    {

        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            {"[Dim_ICBillDates].[YMD].[Month].[MEMBER_CAPTION]","SalesDate" },
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "SalesRegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "SalesOrgName" },
            { "[Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistributorCode" },
            { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Customers].[CustChannelType].[CustChannelType].[MEMBER_CAPTION]", "CustChannelTypeName" },
            { "[Dim_Customers].[CustChannel].[CustChannel].[MEMBER_CAPTION]", "CustChannelName" },
            { "[Measures].[QueryTimeCustomerCount]","QueryTimeCustomerCount" },
            { "[Measures].[SamePeriodPrevYearCustomerCount]","SamePeriodPrevYearCustomerCount" },
            { "[Measures].[CustomerCountYOY]","CustomerCountYOY" },
            { "[Measures].[CurYearCustomerCount]","CurYearCustomerCount" },
             { "[Measures].[CustomerCount]","CurYearCustomerCount" }
        };


      

        /// <summary>
        /// 获取终端数量报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerCountData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();
            var mdx = @"WITH MEMBER [Measures].[QueryTimeCustomerCount] AS  [Measures].[CustomerCount]

                                            MEMBER [Measures].[SamePeriodPrevYearCustomerCount] AS
                                                    SUM(
                                                     {
                                                                ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                   [Dim_ICBillDates].[YMD].[Month].&[{0}]
	                                                   ):
	                                                  ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                [Dim_ICBillDates].[YMD].[Month].&[{0}]
	                                                   )
	                                                  },

                                                      [Measures].[CustomerCount]
	                                                )

                                             MEMBER [Measures].[CustomerCountYOY] AS
                                                Iif(
                                                   IsEmpty([Measures].[QueryTimeCustomerCount])
                                                   OR IsEmpty([Measures].[SamePeriodPrevYearCustomerCount])
                                                   OR[Measures].[SamePeriodPrevYearCustomerCount] = 0
                                                   , NULL
                                                   , ([Measures].[QueryTimeCustomerCount]/[Measures].[SamePeriodPrevYearCustomerCount]-1)*100
                                                 )


                                                   MEMBER [Measures].[CurYearCustomerCount] AS
												   AGGREGATE(
												PERIODSTODATE([Dim_ICBillDates].[YMD].[Year],
												[Dim_ICBillDates].[YMD].CurrentMember
												),
												([Measures].[CustomerCount])
												)


                                             SELECT  { 
		                                            [Measures].[QueryTimeCustomerCount],
		                                            [Measures].[SamePeriodPrevYearCustomerCount],
		                                            [Measures].[CustomerCountYOY], 
		                                            [Measures].[CurYearCustomerCount]
                                                } ON COLUMNS,
                                              NONEMPTY(
		                                            ([Dim_ICBillDates].[YMD].[Month].MEMBERS
													*[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                        * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
		                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		                * [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS 
		                * [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS),[Measures].[CustomerCount]
                                              ) ON ROWS ";


            return Utility.Query<T, CustomerCountCriteria>(
                    con,
                    request,
                    user,
                     mdx,
                     propertyMap);
        }



        /// <summary>
        /// 按渠道分类获取客户数量报表表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerCountByChanelData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            var mdx = @" SELECT  
                        { 
	                        [Measures].[CustomerCount]
                        } ON COLUMNS,
                        NON EMPTY{
	                        [Dim_Customers].[CustChannel].[CustChannel].MEMBERS
                        } ON ROWS 
				   ";



            var maps = new Dictionary<string, string>()
            {
                { "[Dim_Customers].[CustChannel].[CustChannel].[MEMBER_CAPTION]", "Dimission" },
                { "[Measures].[CustomerCount]","Measure1" }
            };

            return Utility.Query<T, CustomerCountCriteria>(
                   con,
                   request,
                   user,
                    mdx,
                    maps);
        }

        /// <summary>
        /// 按客户类型获取客户数量报表表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerCountByChanelTypeData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            var mdx = @" SELECT  
                        { 
	                        [Measures].[CustomerCount]
                        } ON COLUMNS,
                        NON EMPTY{
	                        [Dim_Customers].[CustChannelType].[CustChannelType].MEMBERS
                        } ON ROWS 
				   ";

            var maps = new Dictionary<string, string>()
            {
                { "[Dim_Customers].[CustChannelType].[CustChannelType].[MEMBER_CAPTION]", "Dimission" },
                { "[Measures].[CustomerCount]","Measure1" }
            };

            return Utility.Query<T, CustomerCountCriteria>(
                   con,
                   request,
                   user,
                    mdx,
                    maps);
        }

    }
}
