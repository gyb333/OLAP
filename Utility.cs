using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Reflection;
using System.Data.Common;
using System.Data;
using Wilmar.KDSA.Service.Controllers;
using Wilmar.SSAS.MDXBuilder.Criteria;
using Wilmar.SSAS.MDXBuilder;

namespace Wilmar.Interface.Sync.OLAP
{
    public static class Utility
    {


        public static IEnumerable<T> Query<T, CriteriaT>(DbConnection con, HttpRequestMessage request, UserInfoDTO user, string mdx, Dictionary<string, string> maps) where CriteriaT : AbstractCriteria, new()
        {

#if DEBUG&&TEST

            dynamic data = Common<CriteriaT>.CriteriaT;
#else
            //非管理员
            dynamic data = request.Deserialize<CriteriaT>();
            if(data==null){
                data = new CriteriaT();
            }


#endif
  
            data.SetDimissionValuesToSubCube(user);
            data.SetDimissionValuesToWhere(user);

            //设置映射关系
            typeof(T).Map(maps);


            //日期查询特殊处理
            if (data.BillDates != null)
            {
                if (data.BillDates.Length > 0)
                {
                    mdx = mdx.Replace("{0}", data.BillDates[0].ToString());
                }

                if (data.BillDates.Length > 1)
                {
                    mdx = mdx.Replace("{1}", data.BillDates[1].ToString());
                }
            }



            //构造完整MDX
            var builder = new MDXBuilder();
            builder.Mdx(mdx)
                    .Cube(data.ToSubCube("[KDS3_CY]"))
                    .Where(data.ToWhere());

            var sql = builder.Build();

#if DEBUG&&TEST
            var table = con.GetSchemaTable(sql);
#else
#endif

            return con.Query<T>(sql).ToList();
        }



        public static DataTable GetSchemaTable(this DbConnection con, string sql)
        {
            var com = con.CreateCommand();
            com.CommandText = sql;
            com.CommandType = CommandType.Text;
            if (con.State != ConnectionState.Open)
                con.Open();
            using (var reader = com.ExecuteReader(CommandBehavior.SchemaOnly))
            {
                return reader.GetSchemaTable();
            }
        }

        public static T Deserialize<T>(this HttpRequestMessage request)
        {
            var streamTask = request.Content.ReadAsStreamAsync();
            streamTask.Wait();
            streamTask.Result.Position = 0;
            var content = new StreamReader(streamTask.Result).ReadToEnd();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public static JToken Deserialize(this HttpRequestMessage request)
        {
            var streamTask = request.Content.ReadAsStreamAsync();
            streamTask.Wait();
            streamTask.Result.Position = 0;
            var content = new StreamReader(streamTask.Result).ReadToEnd();

            return JsonConvert.DeserializeObject(content) as JToken;
        }

        public static void Map(this Type type, Dictionary<string, string> maps)
        {
            lock (MapPairs)
            {
                if (!MapPairs.TryGetValue(type, out MapPair value))
                {
                    value = new MapPair(type);
                    MapPairs.Add(type, value);
                }
                value.InitialCurrent(maps);
            }
        }
        private readonly static Dictionary<Type, MapPair> MapPairs = new Dictionary<Type, MapPair>();


        private class MapPair
        {
            public MapPair(Type type)
            {
                ClrType = type;
                TypeMap = new CustomPropertyTypeMap(type, Retrieval);
                Items = new Dictionary<Dictionary<string, string>, Dictionary<string, PropertyInfo>>();
                Dapper.SqlMapper.SetTypeMap(type, TypeMap);
            }

            private PropertyInfo Retrieval(Type type, string name)
            {
                if (Current != null)
                {
                    if (Current.TryGetValue(name, out PropertyInfo value))
                        return value;
                }
                return null;
            }

            public Type ClrType { get; }

            public CustomPropertyTypeMap TypeMap { get; }

            public Dictionary<string, PropertyInfo> Current { get; private set; }

            public Dictionary<Dictionary<string, string>, Dictionary<string, PropertyInfo>> Items { get; }

            public void InitialCurrent(Dictionary<string, string> data)
            {
                if (!Items.TryGetValue(data, out Dictionary < string, PropertyInfo > value))
                {
                    var type = ClrType;
                    var propertys = data.ToDictionary(a => a.Key, a => type.GetProperty(a.Value));
                    var checkitems = propertys.Where(a => a.Value == null).Select(a => a.Key).ToArray();
                    if (checkitems.Length > 0)
                        throw new Exception($"成员 {string.Join(",", checkitems)} 未找到配置属性。");
                    value = propertys;
                    Items.Add(data, propertys);
                }
                Current = value;
            }
        }
    }
}
