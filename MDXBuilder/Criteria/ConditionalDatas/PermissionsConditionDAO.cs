using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wilmar.Interface;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class PermissionsConditionDAO : BaseDAO
    {


        public static dynamic GetPermissionData(int entityID, int userID)
        {
            dynamic result = null;
            using (var conn = ConfigureManager.CreateConnection(strConn))
            {
                string strSQL = @"SELECT DataID FROM userentitydata 
                                    WHERE EntityID=@EntityID AND UserID=@UserID;";
                result = conn.Query<int>(strSQL, new { EntityID = entityID, UserID = userID }).ToList();
                              
            }
            return result;
        }



    }
}
