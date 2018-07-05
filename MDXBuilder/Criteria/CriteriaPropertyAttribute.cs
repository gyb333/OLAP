namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    using System;
    using System.Collections.Generic;

    public  class CriteriaPropertyAttribute : Attribute
    {
        public string Dimission { get; set; }

        public CriteriaBuildType Type { get; set; }

        public CriteriaOprations Opration { get; set; }

        public int Order { get; set; }

        private CriteriaConditions conditions = CriteriaConditions.QueryCondition;
        public  CriteriaConditions Conditions { get { return conditions; } set { conditions = value; } }

        public int EntityID { get; set; }

        public string DefaultValue { get; set; }

        private UserRole role = UserRole.Common;
        public UserRole Role { get { return role; } set { role = value; } }

        /// <summary>
        /// 根据用户获取条件
        /// </summary>
        private UserData userFlag =  UserData.Normal;
        public UserData UserFlag { get { return userFlag; } set { userFlag = value; } }

        private int noData = -9999;
        public int NoDataID { get { return noData; } set { noData = value; } }
    }

 
   
}
