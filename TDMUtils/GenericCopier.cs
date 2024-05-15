using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMUtils
{
    public static class GenericCopier<T>
    {
        public static T DeepCopy(object objectToCopy)
        {
            string Serialized = JsonConvert.SerializeObject(objectToCopy);
            return JsonConvert.DeserializeObject<T>(Serialized);
        }
    }
}
