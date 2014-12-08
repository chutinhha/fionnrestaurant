using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestaurantSoftware.Models
{
    public class CustomerApp
    {
        public int customerID;
        public int pointsBalance;
        public string customerEmail;
        public string customerPhone;
        public string customerAddress;
    }
    public class OrderApp
    {
        public int orderID;
        public decimal Price;
        public DateTime orderStartDate;
    }
    public class OrderLineApp
    {
        public int orderLineID;
        public string itemName;
        public decimal Price;
        public int quantity;
    }
}