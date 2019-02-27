using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace WindowsFormsApp2
{
    [DataContract]
    class FormerValues
    {
        public const string dbpath = "C:\\Users\\MadFox\\source\repos\\WindowsFormsApp2\\" +
            "WindowsFormsApp2\\former_values.data";

        [DataMember]
        public string path { get; set; }
        [DataMember]
        public string template { get; set; }
        [DataMember]
        public string content { get; set; }

        public FormerValues(string path, string template,string content)
        {
            this.path = path;
            this.template = template;
            this.content = content;
        }
    }
}
