using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Client.Helpers.ExportFile
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreExportAttribute : Attribute
    {
    }
}
