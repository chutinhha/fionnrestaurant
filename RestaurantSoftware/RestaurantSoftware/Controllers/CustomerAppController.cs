using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RestaurantSoftware.Models;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantSoftware.Controllers
{
    public class CustomerAppController : ApiController
    {
        RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
        [HttpGet]
        public int[] CheckCredentials(string email, string password)
        {
            int[] defaultValue = new int[2];
            string encryptedPass;
            using (MD5 hash = MD5.Create())
            {
                encryptedPass = GetMd5Hash(hash, password);
            }
            int myCount = db.Customers.Where(x => ((x.customerEmail == email) && (x.customerPass == encryptedPass))).Count();
            int custID = 0;
            if (myCount == 1)
            {
                custID = db.Customers.FirstOrDefault(x=>x.customerEmail == email).customerID;
            }
            defaultValue[0] = myCount;
            defaultValue[1] = custID;
            return defaultValue;
        }
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
        public IEnumerable<OrderApp> GetAllOrdersForCustomer(string customerID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Order> myOrders = db.Orders.Where(x=>((x.customerID == customerID) && (x.orderEndDate != null))).ToList();
            List<OrderApp> myOrdersForApp = new List<OrderApp>();
            foreach (Order order in myOrders)
            {
                OrderApp myOrderApp = new OrderApp();
                myOrderApp.orderID = order.orderID;
                myOrderApp.orderStartDate = order.orderStartDate;
                myOrderApp.Price = order.Price;
                myOrdersForApp.Add(myOrderApp);
            }
            return myOrdersForApp;
        }
        public IEnumerable<OrderLineApp> GetAllOrderLinesForOrder(int orderID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<OrderLine> myOrderLines = db.OrderLines.Where(x => x.orderID == orderID).ToList();
            List<Item> allItems = db.Items.ToList();
            List<OrderLineApp> orderLineAppList = new List<OrderLineApp>();
            foreach (OrderLine orderLine in myOrderLines)
            {
                OrderLineApp myOrderLineApp = new OrderLineApp();
                myOrderLineApp.orderLineID = orderLine.id;
                foreach (Item item in allItems)
                {
                    if (item.itemID == orderLine.itemID)
                    {
                        myOrderLineApp.itemName = item.itemName;
                        break;
                    }
                }
                myOrderLineApp.Price = orderLine.price*orderLine.quantity;
                myOrderLineApp.quantity = orderLine.quantity;
                orderLineAppList.Add(myOrderLineApp);
            }
            return orderLineAppList;
        }
        public CustomerApp GetCustomerDetails(int custID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            Customer customerDetails = db.Customers.Find(custID);
            CustomerApp myCustomer = new CustomerApp();
            myCustomer.customerID = custID;
            myCustomer.pointsBalance = customerDetails.customerLoyaltyPoints;
            myCustomer.customerAddress = customerDetails.customerAddress;
            myCustomer.customerPhone = customerDetails.customerPhone;
            myCustomer.customerEmail = customerDetails.customerEmail;
            return myCustomer;
        }
    }
}