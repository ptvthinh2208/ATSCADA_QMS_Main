using CallQueue.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.DTOs
{
    public class PrintTemplate
    {
        public string PrinterName { get; set; }
        public string Text { get; set; }
        public HeightSize HeightSize { get; set; }
        public WidthSize WidthSize { get; set; }
        public JustifyMode Justify { get; set; }
        public PrintMode PrintMode { get; set; }
    }
}
