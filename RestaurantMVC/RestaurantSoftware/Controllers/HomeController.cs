using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantSoftware.Models;
using System.Data.Entity;
using System.IO;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web.WebPages;

namespace RestaurantSoftware.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int id = 0)   //Tested
        {
            MvcHandler.DisableMvcResponseHeader = true;
            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
            List<Order> Tables = db.Orders.Where(x => x.orderEndDate == null).ToList();
            ViewBag.Staff = db.Staffs.ToList();
            ViewBag.Customers = db.Customers.ToList();
            if (id == 2)
            {
                ViewBag.ErrorMessage = "There was an error creating this order!";
            }
            return View("Index", Tables.ToList());
        }
        #region Items
        #region Add Item
        public ActionResult AddItem(int id = 0) //Tested
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (myCookie["Role"] != "SystemAdmin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    List<ItemType> myItemTypes = db.ItemTypes.ToList();
                    List<SelectListItem> ItemTypes = new List<SelectListItem>();
                    List<Item> items = db.Items.ToList();
                    List<string> itemNames = new List<string>();
                    foreach (Item item in items)
                    {
                        itemNames.Add(item.itemName);
                    }
                    foreach (ItemType itemType in myItemTypes)
                    {
                        ItemTypes.Add(new SelectListItem { Text = itemType.itemTypeName, Value = itemType.itemTypeID.ToString() });
                    }
                    ViewBag.ErrorMessage = "";
                    if (id == 1)
                    {
                        ViewBag.ErrorMessage = "There is already an existing item with this name.";
                    }
                    ViewBag.ItemType = ItemTypes;
                    return View("AddItem");
                }
            }
        }
        [HttpPost]
        public ActionResult AddItem(Item item)
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (myCookie["Role"] != "SystemAdmin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    List<Item> allItems = db.Items.ToList();
                    foreach (Item existingItem in allItems)
                    {
                        if (existingItem.itemName.ToUpper() == item.itemName.ToUpper())
                        {
                            return RedirectToAction("AddItem/1");
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        db.Items.Add(item);
                        db.SaveChanges();
                    }
                    return RedirectToAction("ViewMenu");
                }
            }
        }
        #endregion
        #region Edit Item
        public ActionResult EditItem(int id)    //Tested
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (myCookie["Role"] != "SystemAdmin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Item item = db.Items.Find(id);
                    if (item == null)
                    {
                        var Items = from s in db.Items select s;
                        var Tables = db.Orders.Where(x => x.orderEndDate == null);
                        ViewBag.Tables = Tables.ToList();
                        return RedirectToAction("Index");
                    }
                    List<ItemType> myItemTypes = db.ItemTypes.ToList();
                    List<SelectListItem> ItemTypes = new List<SelectListItem>();
                    foreach (ItemType itemType in myItemTypes)
                    {
                        if (item.itemType == itemType.itemTypeID)
                        {
                            ItemTypes.Add(new SelectListItem { Text = itemType.itemTypeName, Value = itemType.itemTypeID.ToString(), Selected = true });
                        }
                        else
                        {
                            ItemTypes.Add(new SelectListItem { Text = itemType.itemTypeName, Value = itemType.itemTypeID.ToString() });
                        }
                    }
                    ViewBag.ItemType = ItemTypes;
                    return View("EditItem", item);
                }
            }
        }
        [HttpPost]
        public ActionResult EditItem(Item item) //Tested
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (myCookie["Role"] != "SystemAdmin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    if (ModelState.IsValid)
                    {
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("ViewMenu");
                    }
                    else
                    {
                        return View("Index");
                    }
                }
            }
        }
        #endregion
        public ActionResult ViewMenu()          //Tested
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == null || myCookie["Role"] == "Customer" || myCookie["Role"] == "Staff")
            {
                RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                List<Item> Items = db.Items.ToList();
                ViewBag.ItemTypes = db.ItemTypes.ToList();
                return View("ViewMenu", Items.ToList());
            }
            else if (myCookie["Role"] == "SystemAdmin")
            {
                RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                List<Item> Items = db.Items.ToList();
                ViewBag.ItemTypes = db.ItemTypes.ToList();
                return View("StaffMenu", Items.ToList());
            }
            else
            {
                RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                List<Item> Items = db.Items.ToList();
                ViewBag.ItemTypes = db.ItemTypes.ToList();
                return View("ViewMenu", Items.ToList());
            }
        }
        #endregion
        #region Orders
        #region Open Order
        public ActionResult OpenTable()     // Tested
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    ViewBag.Customers = db.Customers.ToList().OrderBy(x=>x.customerEmail);
                    ViewBag.Rank = "Staff";
                    int myID = Int32.Parse(aCookie["ID"]);
                    ViewBag.StaffID = db.Staffs.FirstOrDefault(x => x.staffID == myID).staffID;
                    ViewBag.StaffName = db.Staffs.FirstOrDefault(x => x.staffID == myID).staffName;
                    return View("OpenTable");
                }
                else if (aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    ViewBag.Staff = db.Staffs.ToList();
                    ViewBag.Customers = db.Customers.ToList().OrderBy(x => x.customerEmail);
                    ViewBag.Rank = "SystemAdmin";
                    return View("OpenTable");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

        }
        [HttpPost]
        public ActionResult OpenTable(Order order)  // Tested
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    order.orderStartDate = DateTime.UtcNow;
                    if (ModelState.IsValid)
                    {
                        db.Orders.Add(order);
                        db.SaveChanges();
                        Order orderID = db.Orders.Where(x => ((x.staffID == order.staffID) && (x.customerID == order.customerID))).OrderByDescending(x => x.orderID).First();
                        return RedirectToAction("ManageTable/" + orderID.orderID);
                    }
                    return RedirectToAction("Index/2");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        #endregion
        public ActionResult ManageTable(int id, int errorMessage = 0)   // Tested
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.FirstOrDefault(x => x.orderID == id);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (order.generatedReceipt == 0)
                        {
                            Response.AddHeader("Refresh", "5");
                        }
                        ViewBag.OrderItems = db.OrderLines.Where(x => x.orderID == order.orderID).ToList();
                        ViewBag.Items = db.Items.ToList();
                        ViewBag.StaffName = db.Staffs.FirstOrDefault(x => x.staffID == order.staffID).staffName;
                        int customerID = 0;
                        if (order.customerID == "NULL" || order.customerID == null || order.customerID == "")
                        {

                        }
                        else
                        {
                            customerID = Int32.Parse(order.customerID);
                        }
                        Customer customer = db.Customers.FirstOrDefault(x => x.customerID == customerID);
                        if (errorMessage == 1)
                        {
                            ViewBag.ErrorMessage = "This order has not been paid for, so it is not possible to close it.";
                        }
                        else if (errorMessage == 2)
                        {
                            ViewBag.ErrorMessage = "This order can not be closed until an up-to-date receipt has been printed.";
                        }
                        else if (errorMessage == 3)
                        {
                            ViewBag.ErrorMessage = "Items can not be added to an order that has a valid issued receipt.";
                        }
                        else if (errorMessage == 4)
                        {
                            ViewBag.ErrorMessage = "Items can not be deleted from an order that has a valid issued receipt.";
                        }
                        else if (errorMessage == 5)
                        {
                            ViewBag.ErrorMessage = "Item quantities can not be changed after a valid receipt has been issued.";
                        }
                        else if (errorMessage == 6)
                        {
                            ViewBag.ErrorMessage = "Loyalty point settings can not be changed if a valid receipt has been issued.";
                        }
                        else if (errorMessage == 7)
                        {
                            ViewBag.ErrorMessage = "The staff member can't be changed if a valid receipt has been issued.";
                        }
                        if (customer == default(Customer))
                        {

                        }
                        else
                        {
                            ViewBag.PointsBalance = customer.customerLoyaltyPoints;
                            ViewBag.customerEmail = customer.customerEmail;
                        }
                        ViewBag.StaffType = "Staff";
                        if (aCookie["Role"] == "SystemAdmin")
                        {
                            ViewBag.StaffType = "SystemAdmin";
                        }
                        return View("ManageTable", order);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        #region Add Item To Order
        public ActionResult AddItemToOrder(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.Find(id);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (order.generatedReceipt == 1)
                        {
                            return RedirectToAction("ManageTable/" + id + "/3");
                        }
                        else
                        {
                            ViewBag.orderID = id;
                            ViewBag.StarterItems = db.Items.Where(x => x.itemType == 1);
                            ViewBag.MainItems = db.Items.Where(x => x.itemType == 2);
                            ViewBag.DessertItems = db.Items.Where(x => x.itemType == 3);
                            ViewBag.DrinkItems = db.Items.Where(x => x.itemType == 4);
                            return View();
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult AddItemToOrderJS(int orderID, int itemID, int quantity)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.Find(orderID);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (order.generatedReceipt == 1)
                        {
                            return RedirectToAction("ManageTable/" + order.orderID + "/3");
                        }
                        else
                        {
                            if (quantity <= 0)
                            {

                            }
                            else
                            {
                                OrderLine check = db.OrderLines.FirstOrDefault(x => (x.itemID == itemID && x.orderID == orderID));
                                if (check != default(OrderLine))
                                {
                                    check.quantity += quantity;
                                    db.Entry(check).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else if (check == default(OrderLine))
                                {
                                    OrderLine orderLine = new OrderLine();
                                    orderLine.itemID = itemID;
                                    orderLine.orderID = orderID;
                                    orderLine.quantity = quantity;
                                    db.OrderLines.Add(orderLine);
                                    db.SaveChanges();
                                }
                            }
                            order.generatedReceipt = 0;
                            db.Entry(order).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("ManageTable/" + orderID);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        //[HttpPost]
        //public ActionResult AddItemToOrder(OrderLine orderLine)
        //{
        //    HttpCookie aCookie = Request.Cookies["UserSettings"];
        //    if (aCookie == default(HttpCookie))
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
        //        {
        //            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
        //            Order order = db.Orders.Find(orderLine.orderID);
        //            if (order.generatedReceipt == 1)
        //            {
        //                return RedirectToAction("ManageTable/" + order.orderID + "/3");
        //            }
        //            else
        //            {
        //                if (orderLine.quantity <= 0)
        //                {

        //                }
        //                else
        //                {
        //                    OrderLine check = db.OrderLines.FirstOrDefault(x => (x.itemID == orderLine.itemID && x.orderID == orderLine.orderID));
        //                    if (check != default(OrderLine))
        //                    {
        //                        check.quantity += orderLine.quantity;
        //                        db.Entry(check).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                    }
        //                    else if (check == default(OrderLine))
        //                    {
        //                        db.OrderLines.Add(orderLine);
        //                        db.SaveChanges();
        //                    }
        //                }
        //                order.generatedReceipt = 0;
        //                db.Entry(order).State = EntityState.Modified;
        //                db.SaveChanges();
        //                return RedirectToAction("ManageTable/" + orderLine.orderID);
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index");
        //        }
        //    }
        //}
        #endregion
        public ActionResult CloseTable(int id)      // Tested
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.FirstOrDefault(x => x.orderID == id);
                    if (order == default(Order))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        if (Int32.Parse(aCookie["ID"]) == order.staffID || aCookie["Role"] == "SystemAdmin")
                        {
                            List<OrderLine> orderItems = db.OrderLines.Where(x => x.orderID == id).ToList();
                            if (order.isPaid == 0 && orderItems.Count > 0)
                            {
                                return RedirectToAction("ManageTable/" + id + "/1");
                            }
                            else
                            {
                                order.orderEndDate = DateTime.Now;
                                decimal price = 0;
                                foreach (OrderLine orderItem in orderItems)
                                {
                                    decimal itemPrice = db.Items.Find(orderItem.itemID).itemPrice;
                                    orderItem.price = itemPrice;
                                    db.Entry(orderItem).State = EntityState.Modified;
                                    price = price + (itemPrice * orderItem.quantity);
                                }
                                order.Price = price;
                                db.Entry(order).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }

        public ActionResult DeleteOrderLine(int id) // Tested
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    OrderLine orderLine = db.OrderLines.FirstOrDefault(x => x.id == id);
                    if (orderLine == default(OrderLine))
                    {
                        return RedirectToAction("Index");
                    }
                    Order order = db.Orders.Find(orderLine.orderID);
                    if (order == default(Order))
                    {
                        return RedirectToAction("Index");
                    }
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (order.generatedReceipt == 1)
                        {
                            return RedirectToAction("ManageTable/" + order.orderID + "/4");
                        }
                        else
                        {
                            if (orderLine != default(OrderLine))
                            {

                                db.OrderLines.Remove(orderLine);
                                db.SaveChanges();
                                return RedirectToAction("ManageTable/" + orderLine.orderID);
                            }
                            else
                            {
                                return RedirectToAction("Index");
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult ChangeOrderLineQuantity(int id, int quantity)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    OrderLine orderLine = db.OrderLines.FirstOrDefault(x => x.id == id);
                    if (orderLine == default(OrderLine))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Order order = db.Orders.Find(orderLine.orderID);
                        if (order == default(Order))
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                            {
                                if (order.generatedReceipt == 1)
                                {
                                    return RedirectToAction("ManageTable/" + order.orderID + "/5");
                                }
                                else
                                {
                                    if (quantity <= 0)
                                    {
                                        return RedirectToAction("ManageTable/" + order.orderID + "/5");
                                    }
                                    else
                                    {
                                        orderLine.quantity = quantity;
                                        db.Entry(orderLine).State = EntityState.Modified;
                                        db.SaveChanges();
                                        return RedirectToAction("ManageTable/" + orderLine.orderID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }
        //public ActionResult ChangeOrderLineQuantity(int id)
        //{
        //    HttpCookie aCookie = Request.Cookies["UserSettings"];
        //    if (aCookie == default(HttpCookie))
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
        //        {
        //            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
        //            OrderLine orderLine = db.OrderLines.Find(id);
        //            Order order = db.Orders.Find(orderLine.orderID);
        //            if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
        //            {
        //                if (order.generatedReceipt == 1)
        //                {
        //                    return RedirectToAction("ManageTable/" + order.orderID + "/5");
        //                }
        //                else
        //                {
        //                    ViewBag.itemName = db.Items.Find(orderLine.itemID).itemName;
        //                    if (orderLine == null)
        //                    {
        //                        return RedirectToAction("Index");
        //                    }
        //                    return View("ChangeOrderLineQuantity", orderLine);
        //                }
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index");
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index");
        //        }
        //    }
        //}

        //[HttpPost]
        //public ActionResult ChangeOrderLineQuantity(OrderLine orderLine)
        //{
        //    HttpCookie aCookie = Request.Cookies["UserSettings"];
        //    if (aCookie == default(HttpCookie))
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
        //        {
        //            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
        //            Order order = db.Orders.Find(orderLine.orderID);
        //            if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
        //            {
        //                if (order.generatedReceipt == 1)
        //                {
        //                    return RedirectToAction("ManageTable/" + order.orderID + "/5");
        //                }
        //                else
        //                {
        //                    if (ModelState.IsValid)
        //                    {
        //                        if (orderLine.quantity <= 0)
        //                        {
        //                            OrderLine sample = db.OrderLines.Find(orderLine.id);
        //                            db.OrderLines.Remove(sample);
        //                            db.SaveChanges();
        //                            return RedirectToAction("ManageTable/" + orderLine.orderID);
        //                        }
        //                        else if (orderLine.quantity > 0)
        //                        {
        //                            db.Entry(orderLine).State = EntityState.Modified;
        //                            db.SaveChanges();
        //                            return RedirectToAction("ManageTable/" + orderLine.orderID);
        //                        }
        //                    }
        //                    return RedirectToAction("ManageTable/" + orderLine.orderID);
        //                }
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index");
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index");
        //        }
        //    }
        //}
        public ActionResult ChangePointsSetting(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order myOrder = db.Orders.Find(id);
                    if (myOrder.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (myOrder.isPaid == 1 || myOrder.generatedReceipt == 1)
                        {
                            return RedirectToAction("ManageTable/" + myOrder.orderID + "/6");
                        }
                        else
                        {
                            if (myOrder.pointsChoice == "Save")
                            {
                                myOrder.pointsChoice = "Spend";
                            }
                            else
                            {
                                myOrder.pointsChoice = "Save";
                            }
                            db.Entry(myOrder).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("ManageTable/" + id);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult PayOrder(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.Find(id);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        int errorMessage = 0;
                        if (order.generatedReceipt == 1)
                        {
                            order.isPaid = 1;
                            db.Entry(order).State = EntityState.Modified;
                            db.SaveChanges();
                            List<OrderLine> allOrderLines = db.OrderLines.Where(x => x.orderID == order.orderID).ToList();
                            List<Item> allItems = db.Items.ToList();
                            double totalPrice = 0;
                            foreach (OrderLine orderLine in allOrderLines)
                            {
                                foreach (Item item in allItems)
                                {
                                    if (orderLine.itemID == item.itemID)
                                    {
                                        totalPrice += Convert.ToDouble(item.itemPrice * orderLine.quantity);
                                    }
                                }
                            }
                            int pointsEarned = (int)Math.Floor(totalPrice * 10);
                            int customerID = 0;
                            if (order.customerID != null)
                            {
                                customerID = Int32.Parse(order.customerID);
                            }
                            Customer customer = db.Customers.FirstOrDefault(x => x.customerID == customerID);
                            if (customer != default(Customer))
                            {
                                int customerCurrentPoints = customer.customerLoyaltyPoints;
                                if (order.pointsChoice == "Save")
                                {
                                    customer.customerLoyaltyPoints = customerCurrentPoints + pointsEarned;
                                }
                                else if (order.pointsChoice == "Spend")
                                {
                                    customer.customerLoyaltyPoints = customerCurrentPoints + pointsEarned;
                                    if (totalPrice * 100 >= customer.customerLoyaltyPoints)
                                    {
                                        customer.customerLoyaltyPoints = 0;
                                    }
                                    else
                                    {
                                        customer.customerLoyaltyPoints = customer.customerLoyaltyPoints - (int)(totalPrice * 100);
                                    }
                                }
                                db.Entry(customer).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            return RedirectToAction("CloseTable/" + order.orderID);
                        }
                        else
                        {
                            errorMessage = 2;
                        }
                        return RedirectToAction("ManageTable/" + id + "/" + errorMessage);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult ChangeCustomerID(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order myOrder = db.Orders.FirstOrDefault(x => x.orderID == id);
                    if (myOrder.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (myOrder == default(Order))
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Customers = db.Customers.ToList();
                            return View("ChangeCustomerID", myOrder);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult ChangeCustomerID(string customerID, int orderID)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    int custID = 0;
                    if (customerID == "")
                    {
                    }
                    else
                    {
                        custID = Int32.Parse(customerID);
                    }
                    Customer customer = db.Customers.FirstOrDefault(x => x.customerID == custID);
                    Order order = db.Orders.Find(orderID);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (customerID == "" || customerID == "0")
                        {
                            order.customerID = null;
                            db.Entry(order).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("ManageTable/" + orderID);
                        }
                        else
                        {
                            if (customer == default(Customer))
                            {
                                ViewBag.ErrorMessage = "The customer ID was not found.";
                                return View("ChangeCustomerID", order);
                            }
                            else
                            {
                                order.customerID = customer.customerID.ToString();
                                db.Entry(order).State = EntityState.Modified;
                                db.SaveChanges();
                                return RedirectToAction("ManageTable/" + orderID);
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult GenerateHTMLReceipt(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.Find(id);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        ViewBag.staffName = db.Staffs.FirstOrDefault(x => x.staffID == order.staffID).staffName;
                        ViewBag.allItems = db.Items.ToList();
                        ViewBag.allOrderLines = db.OrderLines.Where(x => x.orderID == id);
                        int customerID = 0;
                        if (order.customerID != null)
                        {
                            customerID = Int32.Parse(order.customerID);
                        }
                        Customer customer = db.Customers.FirstOrDefault(x => x.customerID == customerID);
                        ViewBag.CustomerEmail = "";
                        ViewBag.CustomerID = null;
                        ViewBag.CustomerCurrentPoints = 0;
                        if (customer != default(Customer))
                        {
                            ViewBag.CustomerEmail = customer.customerEmail;
                            ViewBag.CustomerID = customer.customerID;
                            ViewBag.CustomerCurrentPoints = customer.customerLoyaltyPoints;
                        }
                        order.generatedReceipt = 1;
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                        return View("GenerateHTMLReceipt", order);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        #region OriginalReceiptCode
        //public FileStreamResult GenerateReceipt(int id)
        //{
        //    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
        //    Order myOrder = db.Orders.Find(id);
        //    Staff staffMember = db.Staffs.Find(myOrder.staffID);
        //    string customerAccount = "";
        //    if (myOrder.customerID == null)
        //    {
        //        customerAccount = "";
        //    }
        //    else
        //    {
        //        Customer customer = db.Customers.FirstOrDefault(x => x.customerID == myOrder.customerID);
        //        if (customer == default(Customer))
        //        {
        //            customerAccount = "";
        //        }
        //        else
        //        {
        //            customerAccount = customer.customerEmail;
        //        }
        //    }
        //    string log = "<table><tr><td width='100px'><u>Date</u></td><td>" + myOrder.orderStartDate.ToString("dd/MM/yy HH:mm") + "</td></tr><tr><td><u>Order ID</u></td><td>" + myOrder.orderID + "</td></tr><tr><td><u>Staff Member</u></td><td>" + staffMember.staffName + "</td></tr><tr><td><u>Customer<u></td><td>" + customerAccount + "</td></tr><tr><td><br /></td><td></td></tr></table>";
        //    List<Item> allItems = db.Items.ToList();
        //    List<OrderLine> myOrderLines = db.OrderLines.Where(x => x.orderID == id).ToList();
        //    double overallPrice = 0;
        //    log = log + "<table><tr><th width='100px'>Item</th><th width='75px'>Quantity</th><th width='65px'>Price</th></tr>";
        //    foreach (OrderLine orderLine in myOrderLines)
        //    {
        //        foreach (Item item in allItems)
        //        {
        //            if (item.itemID == orderLine.itemID)
        //            {
        //                double totalPrice = orderLine.quantity * Convert.ToDouble(item.itemPrice);
        //                log = log + "<tr><td>" + item.itemName + "</td><td>" + orderLine.quantity + "</td><td align='right'>&euro;" + totalPrice.ToString("0.00") + "</td></tr>";
        //                overallPrice += totalPrice;
        //            }
        //        }
        //    }
        //    log = log + "<tr style='border-top: 1px solid #000'><td></td><td>Total</td><td align='right' style='border-top: 1px solid #000'>&euro;" + overallPrice.ToString("0.00") + "</td></tr>";
        //    double earnedPoints = Math.Floor(overallPrice * 10);
        //    double adjustedPrice = overallPrice;
        //    double newPointsBalance = 0;
        //    if (myOrder.pointsChoice == "Spend" && myOrder.customerID != null)
        //    {
        //        Customer customer = db.Customers.Find(myOrder.customerID);
        //        double currentPoints = customer.customerLoyaltyPoints;
        //        double totalPointSavings = currentPoints + earnedPoints;
        //        if ((totalPointSavings) / 100 > overallPrice)
        //        {
        //            totalPointSavings = Math.Ceiling(overallPrice * 100);
        //            adjustedPrice = 0.00;
        //            newPointsBalance = currentPoints + earnedPoints - totalPointSavings;
        //        }
        //        else
        //        {
        //            adjustedPrice = adjustedPrice - (totalPointSavings / 100);
        //            newPointsBalance = 0;
        //        }
        //        log = log + "<tr><td></td><td>Savings</td><td align='right'>&euro;" + (overallPrice-adjustedPrice).ToString("0.00") + "</td></tr>";
        //        log = log + "<tr><td></td><td>Total</td><td align='right' style='border-top: 1px solid #000'>&euro;" + adjustedPrice.ToString("0.00") + "</td></tr>";
        //    }
        //    else if (myOrder.pointsChoice == "Save" && myOrder.customerID != null)
        //    {
        //        Customer customer = db.Customers.Find(myOrder.customerID);
        //        double currentPoints = customer.customerLoyaltyPoints;
        //        newPointsBalance = currentPoints + earnedPoints;
        //    }
        //    log = log + "</table>";
        //    if (myOrder.customerID == null)
        //    {
        //        log = log + "<br /><div style='width:300px;'>You could have earned " + earnedPoints + " loyalty points, worth &euro;" + (earnedPoints/100).ToString("0.00") + " if you were signed up for our loyalty scheme.</div>";
        //    }
        //    else
        //    {
        //        Customer customer = db.Customers.Find(myOrder.customerID);
        //        if (myOrder.pointsChoice == "Save")
        //        {
        //            log = log + "<br /><div style='width:300px;'>You earned " + earnedPoints + " loyalty points, worth &euro;" + (earnedPoints/100).ToString("0.00") + ".<br />Old Balance: " + customer.customerLoyaltyPoints + " points<br />New Balance: " + newPointsBalance + " points</div>";

        //        }
        //        else if (myOrder.pointsChoice == "Spend")
        //        {
        //            log = log + "<br /><div style='width:300px;'>You saved &euro;" + (overallPrice - adjustedPrice).ToString("0.00") + " on your meal today.<br />Old Balance: " + customer.customerLoyaltyPoints + " points<br />New Balance: " + newPointsBalance + " points</div>";
        //        }
        //    }
        //    var byteArray = Encoding.ASCII.GetBytes(log);
        //    var stream = new MemoryStream(byteArray);
        //    myOrder.generatedReceipt = 1;
        //    db.Entry(myOrder).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return File(stream, "html", "Order" + id + ".html");
        //}
        #endregion
        public ActionResult DiscardReceipt(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.Find(id);
                    if (order.staffID == Int32.Parse(aCookie["ID"]) || aCookie["Role"] == "SystemAdmin")
                    {
                        if (order.isPaid == 1)
                        {
                            return RedirectToAction("ManageTable/" + id + "/7");
                        }
                        else
                        {
                            order.generatedReceipt = 0;
                            db.Entry(order).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("ManageTable/" + id);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult ChangeStaff(int id)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    Order order = db.Orders.FirstOrDefault(x => x.orderID == id);
                    if (order.generatedReceipt == 0)
                    {
                        if (order == default(Order))
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.allStaff = db.Staffs.ToList();
                            return View("ChangeStaff", order);
                        }
                    }
                    else
                    {
                        return RedirectToAction("ManageTable/" + id + "/7");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult ChangeStaff(Order order)
        {
            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
            Order originalOrder = db.Orders.FirstOrDefault(x => x.orderID == order.orderID);
            if (originalOrder == default(Order))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (originalOrder.generatedReceipt == 0)
                {
                    originalOrder.staffID = order.staffID;
                    db.Entry(originalOrder).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ManageTable/" + order.orderID);
                }
                else
                {
                    return RedirectToAction("ManageTable/" + order.orderID + "/7");
                }
            }
        }
        #endregion
        #region Customer
        public ActionResult CreateCustomerAccount(int id = 0)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                if (id == 1)
                {
                    ViewBag.ErrorMessage = "An account with this email already exists!";
                }
                else if (id == 2)
                {
                    ViewBag.ErrorMessage = "The data you entered was not valid!";
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
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
        [HttpPost]
        public ActionResult CreateCustomerAccount(Customer customer)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                if (customer.customerEmail == null || customer.customerAddress == null || customer.customerPass == null || customer.customerPhone == null)
                {
                    return RedirectToAction("CreateCustomerAccount/2");
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        Customer prevCustomer = db.Customers.FirstOrDefault(x => x.customerEmail == customer.customerEmail);
                        if (prevCustomer == default(Customer))
                        {
                            using (MD5 hash = MD5.Create())
                            {
                                customer.customerPass = GetMd5Hash(hash, customer.customerPass);
                            }
                            db.Customers.Add(customer);
                            db.SaveChanges();
                            customer.customerID = db.Customers.First(x => x.customerEmail == customer.customerEmail).customerID;
                            HttpCookie myCookie = new HttpCookie("UserSettings");
                            myCookie.Values["Email"] = customer.customerEmail;
                            myCookie.Values["Role"] = "Customer";
                            myCookie.Values["ID"] = customer.customerID.ToString();
                            Response.SetCookie(myCookie);
                            return RedirectToAction("CustomerAccount");
                        }
                        else
                        {
                            return RedirectToAction("CreateCustomerAccount/1");
                        }
                    }
                    else
                    {
                        return RedirectToAction("CreateCustomerAccount/2");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        #endregion
        #region Staff
        public ActionResult CreateStaffAccount(int id = 0)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "SystemAdmin")
                {
                    if (id == 1)
                    {
                        ViewBag.ErrorMessage = "The name you entered was invalid.";
                    }
                    if (id == 2)
                    {
                        ViewBag.ErrorMessage = "The password you entered is too short. It must be at least 6 characters long.";
                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult CreateStaffAccount(Staff staff)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    if (staff.staffName == null)
                    {
                        return RedirectToAction("CreateStaffAccount/1");
                    }
                    else
                    {
                        if (ModelState.IsValid)
                        {
                            Staff existingStaff = db.Staffs.FirstOrDefault(x => x.staffName == staff.staffName);
                            if (existingStaff == default(Staff))
                            {
                                if (staff.password == null || staff.password.Length < 6)
                                {
                                    return RedirectToAction("CreateStaffAccount/2");
                                }
                                else
                                {
                                    using (MD5 hash = MD5.Create())
                                    {
                                        staff.password = GetMd5Hash(hash, staff.password);
                                    }
                                    db.Staffs.Add(staff);
                                    db.SaveChanges();
                                    return RedirectToAction("Index");
                                }
                            }
                            else
                            {
                                return RedirectToAction("CreateStaffAccount/1");
                            }
                        }
                        else
                        {
                            return RedirectToAction("CreateStaffAccount/1");
                        }
                    }
                }
                return RedirectToAction("Index");
            }
        }
        public ActionResult CustomerLogin()
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return View("CustomerLogin");
            }
            else
            {
                if (myCookie["Role"] == "Customer")
                {
                    return RedirectToAction("CustomerAccount");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult CustomerLogin(string customerEmail, string customerPassword)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                using (MD5 hash = MD5.Create())
                {
                    customerPassword = GetMd5Hash(hash, customerPassword);
                }
                Customer myCustomer = db.Customers.FirstOrDefault(x => x.customerEmail == customerEmail && x.customerPass == customerPassword);
                if (myCustomer == default(Customer))
                {
                    return View("CustomerLogin");
                }
                else
                {
                    HttpCookie myCookie = new HttpCookie("UserSettings");
                    myCookie.Values["Email"] = myCustomer.customerEmail;
                    myCookie.Values["Role"] = "Customer";
                    myCookie.Values["ID"] = myCustomer.customerID.ToString();
                    Response.SetCookie(myCookie);
                    return RedirectToAction("CustomerAccount");
                }
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("CustomerAccount");
                }
            }
        }
        public ActionResult LogOut()
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                myCookie.Expires = DateTime.UtcNow.AddDays(-1D);
                Response.SetCookie(myCookie);
                return RedirectToAction("Index");
            }
        }
        public ActionResult StaffLogin()
        {
            HttpCookie myCookie = Request.Cookies["UserSettings"];
            if (myCookie == default(HttpCookie))
            {
                return View("StaffLogin");
            }
            else
            {
                if (myCookie["Role"] == "Customer")
                {
                    return RedirectToAction("CustomerAccount");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult StaffLogin(string staffName, string staffPassword, string accountType)
        {
            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
            using (MD5 hash = MD5.Create())
            {
                staffPassword = GetMd5Hash(hash, staffPassword);
            }
            if (accountType == "Staff")
            {
                Staff staffMember = db.Staffs.FirstOrDefault(x => x.staffName == staffName && x.password == staffPassword);
                if (staffMember == default(Staff))
                {
                    return View("StaffLogin");
                }
                else
                {
                    HttpCookie myCookie = new HttpCookie("UserSettings");
                    myCookie.Values["Name"] = staffMember.staffName;
                    myCookie.Values["Role"] = "Staff";
                    myCookie.Values["ID"] = staffMember.staffID.ToString();
                    Response.SetCookie(myCookie);
                }
            }
            else if (accountType == "Admin")
            {
                SystemAdmin mySystemAdmin = db.SystemAdmins.FirstOrDefault(x => x.name == staffName && x.password == staffPassword);
                if (mySystemAdmin == default(SystemAdmin))
                {
                    return View("StaffLogin");
                }
                else
                {
                    HttpCookie myCookie = new HttpCookie("UserSettings");
                    myCookie.Values["Name"] = mySystemAdmin.name;
                    myCookie.Values["Role"] = "SystemAdmin";
                    myCookie.Values["ID"] = mySystemAdmin.id.ToString();
                    Response.SetCookie(myCookie);
                }
            }
            return RedirectToAction("Index");
        }
        public ActionResult News()
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "SystemAdmin")
                {
                    return View("News");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult News(News news)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "SystemAdmin")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    news.date = DateTime.Now;
                    if (ModelState.IsValid)
                    {
                        db.News.Add(news);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult CustomerAccount(int id = 0)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Staff" || aCookie["Role"] == "SystemAdmin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    string customerEmail = aCookie["Email"];
                    string customerID = aCookie["ID"];
                    Customer customerDetails = db.Customers.FirstOrDefault(x => x.customerEmail == customerEmail);
                    ViewBag.Email = customerEmail;
                    ViewBag.Password = customerDetails.customerPass;
                    ViewBag.Address = customerDetails.customerAddress;
                    ViewBag.Phone = customerDetails.customerPhone;
                    ViewBag.Points = customerDetails.customerLoyaltyPoints;
                    List<Order> ordersList = db.Orders.Where(x => (x.customerID == customerID) && (x.orderEndDate != null)).ToList();
                    List<OrderLine> orderLinesList = new List<OrderLine>();
                    foreach (Order myOrder in ordersList)
                    {
                        List<OrderLine> miniList = db.OrderLines.Where(x => x.orderID == myOrder.orderID).ToList();
                        foreach (OrderLine myOL in miniList)
                        {
                            orderLinesList.Add(myOL);
                        }
                    }
                    ViewBag.Items = db.Items.ToList();
                    ViewBag.Orders = ordersList;
                    ViewBag.OrderLines = orderLinesList;
                    ViewBag.ErrorMessage = "";
                    if (id == 1)
                    {
                        ViewBag.ErrorMessage = "This email address is already in use.";
                    }
                    if (id == 2)
                    {
                        ViewBag.ErrorMessage = "An error occurred. Your account details were not changed.";
                    }
                    if (id == 3)
                    {
                        ViewBag.ErrorMessage = "Your password has been updated!";
                    }
                    if (id == 4)
                    {
                        ViewBag.ErrorMessage = "Your account details have been updated!";
                    }
                    return View();
                }
            }
        }
        [HttpPost]
        public ActionResult CustomerAccount(Customer customer)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] == "Customer")
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    string currentEmail = aCookie["Email"];
                    Customer myCustomer = db.Customers.FirstOrDefault(x => x.customerEmail == currentEmail);
                    if (myCustomer == default(Customer))
                    {
                        return RedirectToAction("CustomerAccount");
                    }
                    else
                    {
                        Customer checkEmail = db.Customers.FirstOrDefault(x => x.customerEmail == customer.customerEmail);
                        if (checkEmail == default(Customer) || checkEmail.customerID == myCustomer.customerID)
                        {
                            myCustomer.customerEmail = customer.customerEmail;
                            myCustomer.customerAddress = customer.customerAddress;
                            myCustomer.customerPhone = customer.customerPhone;
                            aCookie["Email"] = myCustomer.customerEmail;
                            Response.SetCookie(aCookie);
                            db.Entry(myCustomer).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("CustomerAccount/4");
                        }
                        else
                        {
                            return RedirectToAction("CustomerAccount/1");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPass1, string newPass2)
        {
            HttpCookie aCookie = Request.Cookies["UserSettings"];
            if (aCookie == default(HttpCookie))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (aCookie["Role"] != "Customer")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
                    using (MD5 hash = MD5.Create())
                    {
                        currentPassword = GetMd5Hash(hash, currentPassword);
                    }
                    string customerEmail = aCookie["Email"];
                    Customer myCustomer = db.Customers.FirstOrDefault(x=>((x.customerEmail == customerEmail) &&(x.customerPass == currentPassword)));
                    if (myCustomer == default(Customer))
                    {
                        return RedirectToAction("CustomerAccount/2");
                    }
                    else
                    {
                        using (MD5 hash = MD5.Create())
                        {
                            newPass1 = GetMd5Hash(hash, newPass1);
                        }
                        myCustomer.customerPass = newPass1;
                        db.Entry(myCustomer).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("CustomerAccount/3");
                    }
                }
            }
        }
        #endregion
    }
}