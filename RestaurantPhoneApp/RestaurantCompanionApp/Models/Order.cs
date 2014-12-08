using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCompanionApp.Models
{
    public class Order
    {
        public int orderID { get; set; }
        public decimal Price { get; set; }
        public DateTime orderStartDate { get; set; }
    }
}
