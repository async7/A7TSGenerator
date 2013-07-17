using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A7TSGenerator.Models
{
    public class Service
    {
        public Service()
        {
            this.ServiceMethods = new HashSet<ServiceMethod>();
            this.Models = new Dictionary<string, Type>();
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public ICollection<ServiceMethod> ServiceMethods{ get; set; }
        public IDictionary<string, Type> Models { get; set; }
    }
}
