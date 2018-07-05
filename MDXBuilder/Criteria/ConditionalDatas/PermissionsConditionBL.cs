using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class PermissionsConditionBL
    {
        /// <summary>
        /// 通用查询条件
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="attr"></param>
        /// <param name="where"></param>
        public static dynamic GetWhereByEntityID(int userID, CriteriaPropertyAttribute attr, int? empID=null)
        {
            dynamic value = null;

            var result = PermissionsConditionDAO.GetPermissionData(attr.EntityID, userID);
            if(empID!=null)
                result.Add(empID.Value);
            if (result != null && result.Count > 0)
            {
                if (!result.Contains(-999))
                    value = result.ToArray();
            }else
            {
                value = new int[] {  attr.NoDataID };
            }
               

            return value;
        }



        



    }
}
