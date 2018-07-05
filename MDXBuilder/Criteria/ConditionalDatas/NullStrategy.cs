using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public class NullStrategy : IRoleStrategy
    {
        
        public dynamic GetDimssionValue(AbstractCriteria ac, PropertyInfo propertyInfo)
        {
            return null;
        }
    }
}
