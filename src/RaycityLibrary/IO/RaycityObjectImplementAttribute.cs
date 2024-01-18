using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.IO
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RaycityObjectImplementAttribute : Attribute
    {
        public CreateObjectFunc? CreateObjectMethod;

        public RaycityObjectImplementAttribute()
        {
            this.CreateObjectMethod = null;
            
        }

        public RaycityObjectImplementAttribute(CreateObjectFunc? createObjectMethod)
        {
            this.CreateObjectMethod = createObjectMethod;
        }
    }

    public delegate RaycityObject CreateObjectFunc();
}
