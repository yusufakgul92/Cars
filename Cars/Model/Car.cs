using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cars.Model
{
    /// <summary>
    /// All Cars Data
    /// </summary>
    public class Car
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string PriceDrop { get; set; }
        public string EstMonth { get; set; }
        public string Seller { get; set; }
        public string Rating { get; set; }
        public string Condition { get; set; }
        public string MileAge { get; set; }
    }
}
