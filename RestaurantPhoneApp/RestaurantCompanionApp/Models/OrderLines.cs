using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCompanionApp.Models
{
    public class OrderLine
    {
        public int orderLineID { get; set; }
        public string itemName { get; set; }
        public decimal Price { get; set; }
        public int quantity { get; set; }
    }
}
