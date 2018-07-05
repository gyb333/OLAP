

namespace Wilmar.KDSA.Service.Controllers
{
    using Dapper;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Wilmar.Interface.Sync.OLAP.Criterias;
    using Wilmar.SSAS.MDXBuilder;
    using Wilmar.Interface.Sync.OLAP;
    using Wilmar.SSAS.MDXBuilder.Criteria;

    /// <summary>
    /// 经销商库存报表
    /// </summary>
    public static class DistributorStock
    {
        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "RegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "OrgName" },
            { "[Dim_Distrbutors].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistriContractCode" },
            { "[Dim_Distrbutors].[Distributor].[Distributor].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Products].[ProdLine].[ProdLine].[MEMBER_CAPTION]","ProdLineName"},
            {  "[Dim_Products].[ProdCategory].[ProdCategory].[MEMBER_CAPTION]","ProdCategoryName"},
            { "[Dim_Products].[ProdBrand].[ProdBrand].[MEMBER_CAPTION]","ProdBrandName"},
            { "[Dim_Products].[Id].[Id].[MEMBER_CAPTION]","ProductName"},
            { "[Dim_Products].[ProdCode].[ProdCode].[MEMBER_CAPTION]","ProductCode"},
            { "[Measures].[RealBaseUnitQty]","RealBaseUnitQty" },
            { "[Measures].[AvgDayBaseUnitQty]","AvgDayBaseUnitQty" },
            { "[Measures].[BaseUnitQtyDays]","BaseUnitQtyDays" },
            { "[Measures].[RealWeight]","RealWeight" },
            { "[Measures].[AvgDayWeight]","AvgDayWeight" },
            { "[Measures].[WeightDays]","WeightDays" }
        };

        private static string BuildMdx(string rowAxis)
        {
            var mdx = @"WiTH 
SET [SET] AS
  NONEMPTY(
						[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                        * [Dim_Orgs].[Org].[Org].ALLMEMBERS 					 
		                * [Dim_Distrbutors].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
						* [Dim_Distrbutors].[Distributor].[Distributor].ALLMEMBERS
		                * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
                        * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS
		                * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS						
						* [Dim_Products].[Id].[Id].ALLMEMBERS
						* [Dim_Products].[ProdCode].[ProdCode].ALLMEMBERS
						,[Measures].[MinUnitQty])

MEMBER [Measures].[RealBaseUnitQty] AS
  ([Dim_BillDates].[YMD].[Day].&[{0}].Parent.Lag(2),[Measures].[EndBaseUnitQty]) 
  + 
  Aggregate({[Dim_BillDates].[YMD].[Day].&[{0}].Parent.Lag(1).FirstChild
 :[Dim_BillDates].[YMD].[Day].&[{0}].Lag(1)}, [Measures].[ICVoucherDetls_BaseUnitQty]) 
  
 MEMBER [Measures].[RealWeight] AS
  ([Dim_BillDates].[YMD].[Day].&[{0}].Parent.Lag(2),[Measures].[EndWeight]) 
  + Aggregate({[Dim_BillDates].[YMD].[Day].&[{0}].Parent.Lag(1).FirstChild
 :[Dim_BillDates].[YMD].[Day].&[{0}].Lag(1)}, [Measures].[ICVoucherDetls_Weight]) 
 
 

MEMBER [Measures].[Days] AS
IIF (CSTR([Measures].[MinDoBillDateID])<CSTR('{1}'),90,[Measures].[EndDays]- [Measures].[MinDateDays])
 
 MEMBER [Measures].[90DayBaseUnitQty] AS 
SUM({ParallelPeriod([Dim_BillDates].[YMD].[Day], 90, [Dim_BillDates].[YMD].[Day].&[{0}] )
	:
	ParallelPeriod([Dim_BillDates].[YMD].[Day],1,[Dim_BillDates].[YMD].[Day].&[{0}])
	    },[Measures].[BaseUnitQty])    
 
 MEMBER [Measures].[90DayWeight] AS 
SUM({ParallelPeriod([Dim_BillDates].[YMD].[Day], 90, [Dim_BillDates].[YMD].[Day].&[{0}] )
	:
	ParallelPeriod([Dim_BillDates].[YMD].[Day],1,[Dim_BillDates].[YMD].[Day].&[{0}])
	    },[Measures].[Weight])    

 MEMBER [Measures].[AvgDayBaseUnitQty] AS 
 CASE WHEN ISEMPTY([Measures].[Days]) OR [Measures].[Days]=0
 THEN NULL 
 ELSE [Measures].[90DayBaseUnitQty] /[Measures].[Days] END

 MEMBER [Measures].[AvgDayWeight] AS 
 CASE WHEN ISEMPTY([Measures].[Days]) OR [Measures].[Days]=0
 THEN NULL 
 ELSE [Measures].[90DayWeight]/[Measures].[Days] END
 
 MEMBER [Measures].[BaseUnitQtyDays] AS 
 CASE WHEN ISEMPTY([Measures].[AvgDayBaseUnitQty]) OR [Measures].[AvgDayBaseUnitQty]=0
 THEN NULL 
 ELSE [Measures].[RealBaseUnitQty]/[Measures].[AvgDayBaseUnitQty] END

 MEMBER [Measures].[WeightDays]  AS   
 CASE WHEN ISEMPTY([Measures].[AvgDayWeight] ) OR [Measures].[AvgDayWeight] =0
 THEN NULL 
 ELSE [Measures].[RealWeight]/  [Measures].[AvgDayWeight]  END

SELECT  
{ 
[Measures].[RealBaseUnitQty],
[Measures].[AvgDayBaseUnitQty],
[Measures].[BaseUnitQtyDays],
[Measures].[RealWeight] ,
[Measures].[AvgDayWeight],
[Measures].[WeightDays]              
				  } ON COLUMNS,
  {ROW_AXIS}  ON ROWS ";
            return mdx.Replace("{ROW_AXIS}", rowAxis);
        }

        private static IEnumerable<T> Query<T, CriteriaT>(DbConnection con, HttpRequestMessage request, UserInfoDTO user, string rowAxis) 
            where CriteriaT : AbstractCriteria, new()
        {
#if DEBUG && TEST
            dynamic data = Common<CriteriaT>.CriteriaT;
#else

            //非管理员
            dynamic data = request.Deserialize<DistributorStockCriteria>();
            if(data==null){
                data = new DistributorStockCriteria();
            }

#endif
            data.SetDimissionValuesToSubCube(user);
            data.SetDimissionValuesToWhere(user);

            //设置映射关系
            typeof(T).Map(propertyMap);

            string strDate = null;
            if (data != null && data.BillDates != null && data.BillDates.Length > 1)
                strDate = data.BillDates[1].ToString();

            if (string.IsNullOrEmpty(strDate))
                throw new Exception("查询参数日期不能为空！");


            string before90Date = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture)
                .AddDays(-90).ToString("yyyyMMdd");


            //生成mdx前部分语句
            var mdx = BuildMdx(rowAxis);
            mdx = mdx.Replace("{0}", strDate)
                .Replace("{1}", before90Date);

            string subCube = @" [KDS3_CY] ";
            subCube = subCube.Replace("{0}", strDate);

            //构造完整MDX
            var builder = new MDXBuilder();
            builder.Mdx(mdx)
                    .Cube(data.ToSubCube(subCube))
                    .Where(data.ToWhere());

            var sql = builder.Build();

#if DEBUG&&TEST
            var table = con.GetSchemaTable(sql);
#else
#endif

            return con.Query<T>(sql).ToList();
        }



        public static IEnumerable<T> GetDistributorStockData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();
            string rowAxis = @"[SET]";
            return Query<T, DistributorStockCriteria>(con, request, user, rowAxis);
        }

    }
}
