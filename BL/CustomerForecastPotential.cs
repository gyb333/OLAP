

namespace Wilmar.KDSA.Service.Controllers
{

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Wilmar.Interface.Sync.OLAP;
    using Wilmar.Interface.Sync.OLAP.Criterias;

    public static class CustomerForecastPotential
    {
        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            { "[Dim_BillDates].[Month].[Month].[MEMBER_CAPTION]", "MonthName" },
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "RegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "OrgName" },
            { "[Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistriContractCode" },
            { "[Dim_CompBranches].[Distributor].[Distributor].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Customers].[CustChannelType].[CustChannelType].[MEMBER_CAPTION]","CustChannelTypeName"},
            { "[Dim_Customers].[CustChannel].[CustChannel].[MEMBER_CAPTION]","CustChannelName"},
            { "[Dim_Customers].[Id].[Id].[MEMBER_CAPTION]","CustName"},
    



            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[1]","WeightRH" },
            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[2]","WeightDF" },
            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[3]","WeightMF" },
            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[4]","WeightDM" },
            //{ "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[5]","WeightTWZ" },
            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[6]","WeightYou" },
            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[7]","WeightQT" },
            { "[Measures].[Weight].[DimProdLineProportions].[ProdLineTypeName].&[全部]","WeightALL" } ,

            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[1]","PredictedPotentialValueRH" },
            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[2]","PredictedPotentialValueDF" },
            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[3]","PredictedPotentialValueMF" },
            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[4]","PredictedPotentialValueDM" },
            //{ "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[5]","PredictedPotentialValueTWZ" },
            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[6]","PredictedPotentialValueYou" },
            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[7]","PredictedPotentialValueQT" },
            { "[Measures].[PredictedPotentialValue].[DimProdLineProportions].[ProdLineTypeName].&[全部]","PredictedPotentialValueALL" }



        };


        private static string BuildMdx()
        {
            var mdx = @"WITH 
            SET [SET] AS  NONEMPTY(
                { [DimProdLineProportions].[ProdLineTypeName].[ProdLineTypeName]
				  ,[DimProdLineProportions].[ProdLineTypeName].[全部]
                }, [Measures].[MinUnitQty])

            SET [SETROW] AS
            NONEMPTY(
                        [Dim_BillDates].[Month].[Month].ALLMEMBERS 
						* [Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                        * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
		                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
						* [Dim_CompBranches].[Distributor].[Distributor].ALLMEMBERS		   
						* [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS
						* [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS
						* [Dim_Customers].[Id].[Id].ALLMEMBERS		
						,[Measures].[MinUnitQty])

            MEMBER [Measures].[ALLProportion] AS  
            CASE WHEN [DimProdLineProportions].[ProdLineTypeName].CURRENTMEMBER.NAME='全部'
            THEN NULL
            ELSE[Measures].[VlaueProportion]
            END

            MEMBER[Measures].[Proportion] AS
            CASE WHEN ISEMPTY([Measures].[ALLProportion]) OR[Measures].[ALLProportion]=0 THEN NULL
            ELSE[Measures].[Weight]/[Measures].[ALLProportion]
            END

            MEMBER[Measures].[MAXProportion] AS MAX([SET], [Measures].[Proportion])

            MEMBER[Measures].[LeadWeight] AS
            Case WHEN[Measures].[Proportion] =[Measures].[MAXProportion]
                    THEN[Measures].[Weight]
                    ELSE NUll
            END

            MEMBER[Measures].[LeadProportion] AS
            Case WHEN[Measures].[Proportion] =[Measures].[MaxProportion]
                    THEN[Measures].[ALLProportion]
                    ELSE NUll
            END

            MEMBER[Measures].[MAXLeadWeight] AS MAX([SET], [Measures].[LeadWeight])

            MEMBER[Measures].[MAXLeadProportion] AS MAX([SET], [Measures].[LeadProportion])

            MEMBER[Measures].[PredictedValue] AS
            CASE WHEN[DimProdLineProportions].[ProdLineName].CURRENTMEMBER.NAME ='全部'
            THEN
            SUM([SET], Round([Measures].[MAXLeadWeight] *[Measures].[ALLProportion]/[Measures].[MAXLeadProportion],4))
            ELSE
            Round([Measures].[MAXLeadWeight] *[Measures].[ALLProportion]/[Measures].[MAXLeadProportion],4)
            END

            MEMBER[Measures].[PredictedPotentialValue] AS [Measures].[PredictedValue] -  Round([Measures].[Weight],4)

            SELECT { 
			{[Measures].[Weight],[Measures].[PredictedPotentialValue]}
			* [SET] 
            } ON COLUMNS,
            [SETROW]  ON ROWS ";
            return mdx;
        }

        public static IEnumerable<T> GetCustomerForecastPotentialData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            string mdx = BuildMdx();
            return Utility.Query<T, CustomerForecastPotentialCriteria>(con, request, user, mdx, propertyMap);
        }

    }
}
