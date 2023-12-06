using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVPIAddin.VisualHelpers
{
    public class EnumVisualHelper
    {
        public static List<string> GetEnumDescriptionList(Type EnumType)
        {
            return EnumType.GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true)
                .Cast<DescriptionAttribute>()).Select(x => x.Description).ToList();
        }
    }
}
