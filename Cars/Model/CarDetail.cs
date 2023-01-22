using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cars.Model
{
    /// <summary>
    /// Specific Car Data
    /// </summary>
    public class CarDetail
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string PriceDrop { get; set; }
        public string Seller { get; set; }
        public string Condition { get; set; }
        public string MileAge { get; set; }
        public List<string> Convenience { get; set; }
        public List<string> Entertainment { get; set; }
        public List<string> Exterior { get; set; }
        public List<string> Safety { get; set; }
        public List<string> Seating { get; set; }
        public List<string> AdditionalFeatures { get; set; }
        public List<string> Basics { get; set; }
        public List<string> Features { get; set; }

    }
}
