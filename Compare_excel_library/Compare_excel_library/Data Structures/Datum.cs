using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Data_Structures
{
    public class Datum
    {

        public Datum(string key, Object value)
        {
            this.ColKey = key;
            this.Value = value;
            this.Type = value == null? null : value.GetType();
        }
        /// <summary>
        /// The uniquely identified key of the column from which the datum is derived.
        /// </summary>
        /// 
        public string ColKey { get; set; }
        /// <summary>
        /// the type of the datum. This is important for comparisons
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// The raw value of the datum
        /// </summary>
        public Object Value { get; set; }
         
    }
}
