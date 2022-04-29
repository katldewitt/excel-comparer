using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Data_Structures
{
    public class InDataStruct
    {
        public string Key { get; set; }
        public Dictionary<string, Datum> Data = new Dictionary<string, Datum>();
    }

    public class OutDataStruct
    {
        public string Key { get; set; }
        public Dictionary<string, OData> Data  = new Dictionary<string, OData>();//{ get; set; }
    }
}
