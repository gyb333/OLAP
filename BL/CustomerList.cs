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
    public static class CustomerList
    {

        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "SalesRegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "SalesOrgName" },
            { "[Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistributorCode" },
            { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Customers].[CustChannelType].[CustChannelType].[MEMBER_CAPTION]", "CustChannelTypeName" },
            { "[Dim_Customers].[CustChannel].[CustChannel].[MEMBER_CAPTION]", "CustChannelName" },
            { "[Dim_Customers].[Code].[Code].[MEMBER_CAPTION]", "CustomerCode" },
            { "[Dim_Customers].[Id].[Id].[MEMBER_CAPTION]", "CustomerName" },
            { "[Measures].[CurMonthSalesQty]","CurMonthSalesQty" },
            { "[Measures].[CurMonthSalesWeight]","CurMonthSalesWeight" },
            { "[Measures].[LastSixMonthSalesQty]","LastSixMonthSalesQty" },
            { "[Measures].[LastFiveMonthSalesQty]","LastFiveMonthSalesQty" },
            { "[Measures].[LastFourMonthSalesQty]","LastFourMonthSalesQty" },
            { "[Measures].[LastThreeMonthSalesQty]","LastThreeMonthSalesQty" },
            { "[Measures].[LastTwoMonthSalesQty]","LastTwoMonthSalesQty" },
            { "[Measures].[LastOneMonthSalesQty]","LastOneMonthSalesQty" },
            { "[Measures].[LastSixMonthSalesWeight]","LastSixMonthSalesWeight" },
            { "[Measures].[LastFiveMonthSalesWeight]","LastFiveMonthSalesWeight" },
            { "[Measures].[LastFourMonthSalesWeight]","LastFourMonthSalesWeight" },
            { "[Measures].[LastThreeMonthSalesWeight]","LastThreeMonthSalesWeight" },
            { "[Measures].[LastTwoMonthSalesWeight]","LastTwoMonthSalesWeight" },
            { "[Measures].[LastOneMonthSalesWeight]","LastOneMonthSalesWeight" }
        };

        private static string buildMdx(string rowAxis)
        {
            var mdx = @"WITH 
                        MEMBER [Measures].[Before3AllMonthSalesQty] as
						AGGREGATE( LastPeriods(9999, [Dim_ICBillDates].[YMD].CurrentMember), ([Measures].[BaseUnitQty]))
						-AGGREGATE( LastPeriods(3, [Dim_ICBillDates].[YMD].CurrentMember), ([Measures].[BaseUnitQty]))

                        MEMBER [Measures].[3~6MonthSalesQty] as
						AGGREGATE( LastPeriods(6, [Dim_ICBillDates].[YMD].CurrentMember), ([Measures].[BaseUnitQty]))
						-AGGREGATE( LastPeriods(3, [Dim_ICBillDates].[YMD].CurrentMember), ([Measures].[BaseUnitQty]))

                        MEMBER [Measures].[CurMonthSalesQty] AS  
	                        [Measures].[BaseUnitQty]

                        MEMBER [Measures].[CurMonthSalesWeight] AS  
	                        [Measures].[Weight]


                        MEMBER [Measures].[LastSixMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(6, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[BaseUnitQty])
                        )

                        MEMBER [Measures].[LastFiveMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(5, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[BaseUnitQty])
                        )

                        MEMBER [Measures].[LastFourMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(4, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[BaseUnitQty])
                        )

                        MEMBER [Measures].[LastThreeMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(3, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[BaseUnitQty])
                        )


                        MEMBER [Measures].[LastTwoMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(2, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[BaseUnitQty])
                        )

                        MEMBER [Measures].[LastOneMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(1, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[BaseUnitQty])
                        )


                        MEMBER [Measures].[LastSixMonthSalesWeight]
                        AS
                        AGGREGATE(
                         LastPeriods(6, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[Weight])
                        )

                        MEMBER [Measures].[LastFiveMonthSalesWeight]
                        AS
                        AGGREGATE(
                         LastPeriods(5, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[Weight])
                        )

                        MEMBER [Measures].[LastFourMonthSalesWeight]
                        AS
                        AGGREGATE(
                         LastPeriods(4, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[Weight])
                        )

                        MEMBER [Measures].[LastThreeMonthSalesWeight]
                        AS
                        AGGREGATE(
                         LastPeriods(3, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[Weight])
                        )


                        MEMBER [Measures].[LastTwoMonthSalesWeight]
                        AS
                        AGGREGATE(
                         LastPeriods(2, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[Weight])
                        )

                        MEMBER [Measures].[LastOneMonthSalesWeight]
                        AS
                        AGGREGATE(
                         LastPeriods(1, [Dim_ICBillDates].[YMD].CurrentMember),
                        ([Measures].[Weight])
                        )
                    SELECT  { 
                        [Measures].[CurMonthSalesQty],
                        [Measures].[CurMonthSalesWeight],
                        [Measures].[LastSixMonthSalesQty],
                        [Measures].[LastFiveMonthSalesQty],
                        [Measures].[LastFourMonthSalesQty],
                        [Measures].[LastThreeMonthSalesQty],
                        [Measures].[LastTwoMonthSalesQty],
                        [Measures].[LastOneMonthSalesQty],
                        [Measures].[LastSixMonthSalesWeight],
                        [Measures].[LastFiveMonthSalesWeight],
                        [Measures].[LastFourMonthSalesWeight],
                        [Measures].[LastThreeMonthSalesWeight],
                        [Measures].[LastTwoMonthSalesWeight],
                        [Measures].[LastOneMonthSalesWeight]
                    } ON COLUMNS,
                                              NONEMPTY(
		                                            ({ROW_AXIS}), [Measures].[BaseUnitQty]
                                              ) ON ROWS ";

            return mdx.Replace("{ROW_AXIS}", rowAxis);
        }



        private static string buildChartsMdx(string rowAxis)
        {
            var mdx = @"WITH 

                        MEMBER [Measures].[LastThreeMonthSalesQty]
                        AS
                        AGGREGATE(
                         LastPeriods(3, [Dim_ICBillDates].[YMD].CurrentMember),
                        ( [Measures].[CustomerCount])
                        )
                    SELECT  { 
                        [Measures].[CustomerCount],
                        [Measures].[LastThreeMonthSalesQty]
                    } ON COLUMNS,
                                              NONEMPTY(
		                                            ({ROW_AXIS}), [Measures].[BaseUnitQty]
                                              ) ON ROWS ";

            return mdx.Replace("{ROW_AXIS}", rowAxis);
        }


        /// <summary>
        /// 获取流失客户清单报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetLostCustomerData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();

            var mdx = buildMdx(@"FILTER( NonEmpty([Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                * [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS 
                * [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS 
                * [Dim_Customers].[Code].[Code].ALLMEMBERS 
                * [Dim_Customers].[Id].[Id].ALLMEMBERS,[Measures].[BaseUnitQty]),
				  ISEMPTY([Measures].[LastThreeMonthSalesQty]) 
                    AND [Measures].[3~6MonthSalesQty]>0  
                )");

            return Utility.Query<T, CustomerListCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }



        /// <summary>
        /// 获取新增客户清单报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetNewlyCustomerData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();

            var mdx = buildMdx(@"FILTER( NonEmpty([Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                        * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
		                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                        * [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS 
                        * [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS 
		                * [Dim_Customers].[Id].[Id].ALLMEMBERS,[Measures].[BaseUnitQty]),
				   NOT ISEMPTY([Measures].[LastThreeMonthSalesQty]) 
                    AND [Measures].[Before3AllMonthSalesQty]=0
                    )");

            return Utility.Query<T, CustomerListCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);

        }


        /// <summary>
        /// 获取流失客户率＿分公司报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetLostCustomerRateByCompanyData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();


            var mdx = buildMdx(@"FILTER( NonEmpty([Dim_ICBillDates].[YMD].[Month].MEMBERS,[Measures].[BaseUnitQty]),
				  ISEMPTY([Measures].[LastThreeMonthSalesQty]))");


            var maps = new Dictionary<string, string>()
            {
                { "[Dim_ICBillDates].[YMD].[Month].[MEMBER_CAPTION]", "Dimission" },
                { "[Measures].[LastThreeMonthSalesQty]","Measure1" }
            };

            return Utility.Query<T, CustomerListCriteria>(
                con,
                request,
                user,
                mdx,
            maps);

        }

        /// <summary>
        /// 获取流失客户率＿分公司报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetLostCustomerRateByDistributorData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {


            var mdx = buildChartsMdx(@"FILTER( NonEmpty([Dim_CompBranches].[DistributorContract].[DistributorContract].MEMBERS , [Measures].[BaseUnitQty] ),
				  ISEMPTY([Measures].[LastThreeMonthSalesQty]))");

            var maps = new Dictionary<string, string>()
            {
                { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "Dimission" },
                { "[Measures].[BaseUnitQty]","Measure1" }
            };

            return Utility.Query<T, CustomerListCriteria>(
                con,
                request,
                user,
                mdx,
            maps);

        }



        /// <summary>
        /// 获取新增客户新增率_分公司报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetNewlyCustomerRateByCompanyData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();



            var mdx = buildChartsMdx(@"FILTER( NonEmpty([Dim_ICBillDates].[YMD].[Month].MEMBERS, [Measures].[BaseUnitQty] ),
				   NOT ISEMPTY([Measures].[LastThreeMonthSalesQty]))");

            var maps = new Dictionary<string, string>()
            {
                { "[Dim_ICBillDates].[YMD].[Month].[MEMBER_CAPTION]", "Dimission" },
                { "[Measures].[BaseUnitQty]","Measure1" }
            };


            return Utility.Query<T, CustomerListCriteria>(
                con,
                request,
                user,
                mdx,
            maps);
        }

        /// <summary>
        /// 获取经销商新增客户新增率报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetNewlyCustomerRateByDistributorData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildChartsMdx(@"FILTER( NonEmpty([Dim_CompBranches].[DistributorContract].[DistributorContract].MEMBERS, [Measures].[BaseUnitQty] ),
				   NOT ISEMPTY([Measures].[LastThreeMonthSalesQty]))");

            var maps = new Dictionary<string, string>()
            {
                { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "Dimission" },
                { "[Measures].[BaseUnitQty]","Measure1" }
            };

            return Utility.Query<T, CustomerListCriteria>(
                con,
                request,
                user,
                mdx,
            maps);
        }

    }
}
