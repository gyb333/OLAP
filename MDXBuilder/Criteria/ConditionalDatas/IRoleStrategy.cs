using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public interface IRoleStrategy
    {
         
       

        dynamic GetDimssionValue(AbstractCriteria ac, PropertyInfo propertyInfo);
    }
}
