using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Data_Structures
{
    public class OData
    {
        public OData()
        {
        }

        /// <summary>
        /// The uniquely identified key of the column from which the datum is derived.
        /// </summary>
        /// 
        public string colKey { get; set; }

        public Source_Comparison Source { get; set; }
        public Datum original { get; set; }
        public Datum newer { get; set; }
        public Delta delta { get; set; }

    }

    public enum Source_Comparison
    {
        ORIG = 1,
        NEW = 2,
        BOTH = 3,
    }
}
