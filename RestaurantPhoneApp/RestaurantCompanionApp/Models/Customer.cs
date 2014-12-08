using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCompanionApp.Models
{
    public class Customer
    {
        public int customerID { get; set; }
        public int pointsBalance { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public string customerAddress { get; set; }
    }
}
