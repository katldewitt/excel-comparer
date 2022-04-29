using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Data_Structures
{
    public class Delta
    {
        public int DeltaValue { get; set; }
        public DeltaType DeltaType { get; set; }
    }

    public enum DeltaType
    {
        BOOL,
        STRING,
        NUMERIC,
        UNCOMPARABLE
    }
}
