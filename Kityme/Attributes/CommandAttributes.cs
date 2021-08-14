using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kityme.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CommandTypeAttribute : Attribute
    {
        public string Type { get; set; }
        public CommandTypeAttribute(string type)
        {
            Type = type;
        }
    }
}
