using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using Subtext.TestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestaurantSoftware;
using RestaurantSoftware.Controllers;
using System.IO;
using MockJockey;
using System.Web.Routing;
using System.Web.SessionState;
using Moq;
using RestaurantSoftware.Models;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Security.Cryptography;

namespace RestaurantSoftware.Tests.Controllers
{
    public class MockContext
    {
        public Mock<RequestContext> RoutingRequestContext { get; private set; }
        public Mock<HttpContextBase> Http { get; private set; }
        public Mock<HttpServerUtilityBase> Server { get; private set; }
        public Mock<HttpResponseBase> Response { get; private set; }
        public Mock<HttpRequestBase> Request { get; private set; }
        public Mock<HttpSessionStateBase> Session { get; private set; }
        public Mock<ActionExecutingContext> ActionExecuting { get; private set; }
        public HttpCookieCollection Cookies { get; private set; }

        public MockContext()
        {
            this.RoutingRequestContext = new Mock<RequestContext>(MockBehavior.Loose);
            this.ActionExecuting = new Mock<ActionExecutingContext>(MockBehavior.Loose);
            this.Http = new Mock<HttpContextBase>(MockBehavior.Loose);
            this.Server = new Mock<HttpServerUtilityBase>(MockBehavior.Loose);
            this.Response = new Mock<HttpResponseBase>(MockBehavior.Loose);
            this.Request = new Mock<HttpRequestBase>(MockBehavior.Loose);
            this.Session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
            this.Cookies = new HttpCookieCollection();

            this.RoutingRequestContext.SetupGet(c => c.HttpContext).Returns(this.Http.Object);
            this.ActionExecuting.SetupGet(c => c.HttpContext).Returns(this.Http.Object);
            this.Http.SetupGet(c => c.Request).Returns(this.Request.Object);
            this.Http.SetupGet(c => c.Response).Returns(this.Response.Object);
            this.Http.SetupGet(c => c.Server).Returns(this.Server.Object);
            this.Http.SetupGet(c => c.Session).Returns(this.Session.Object);
            this.Request.Setup(c => c.Cookies).Returns(Cookies);
        }

    }
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.AreEqual("Index", result.ViewName);
        }
        [TestMethod]
        public void IndexError()
        {
            HomeController controller = new HomeController();
            ViewResult result = controller.Index(2) as ViewResult;
            Assert.AreEqual("There was an error creating this order!", result.ViewBag.ErrorMessage);
        }
        [TestMethod]
        public void NoCookieAddItem()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.AddItem() as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void CustomerStaffCookieAddItem()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Staff"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.AddItem() as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void AdminCookieAddItem()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=SystemAdmin"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.AddItem() as ViewResult;
            Assert.AreEqual("AddItem", result.ViewName);
        }
        [TestMethod]
        public void AdminCookieWarningAddItem()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=SystemAdmin"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.AddItem(1) as ViewResult;
            Assert.AreEqual("There is already an existing item with this name.", result.ViewBag.ErrorMessage);
        }
        [TestMethod]
        public void AdminViewMenu()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=SystemAdmin"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.ViewMenu() as ViewResult;
            Assert.AreEqual("StaffMenu", result.ViewName);
        }
        [TestMethod]
        public void OtherViewMenu()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.ViewMenu() as ViewResult;
            Assert.AreEqual("ViewMenu", result.ViewName);
        }
        [TestMethod]
        public void CookielessEditItem()
        {
            MockContext mockContext = new MockContext();
            //mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Staff"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.EditItem(1) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void CustomerCookieEditItem()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.EditItem(1) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void AdminEditItem()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=SystemAdmin"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.EditItem(1) as ViewResult;
            Assert.AreEqual("EditItem", result.ViewName);
        }
        [TestMethod]
        public void AdminEditNotExistingItem()
        {
            MockContext mockContext = new MockContext();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=SystemAdmin"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.EditItem(0) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void CookielessEditItemDetails()
        {
            MockContext mockContext = new MockContext();
            Item myItem = new Item();
            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
            myItem = db.Items.Find(1);
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.EditItem(myItem) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void CustomerEditItemDetails()
        {
            MockContext mockContext = new MockContext();
            Item myItem = new Item();
            RestaurantDatabaseEntities db = new RestaurantDatabaseEntities();
            myItem = db.Items.Find(1);
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.EditItem(myItem) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void ManageTable()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.ManageTable(32) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            result = homeController.ManageTable(32) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            HttpCookie myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            var newResult = homeController.ManageTable(32) as ViewResult;
            Assert.AreEqual("ManageTable", newResult.ViewName);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 1) as ViewResult;
            Assert.AreEqual("This order has not been paid for, so it is not possible to close it.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 2) as ViewResult;
            Assert.AreEqual("This order can not be closed until an up-to-date receipt has been printed.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 3) as ViewResult;
            Assert.AreEqual("Items can not be added to an order that has a valid issued receipt.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 4) as ViewResult;
            Assert.AreEqual("Items can not be deleted from an order that has a valid issued receipt.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 5) as ViewResult;
            Assert.AreEqual("Item quantities can not be changed after a valid receipt has been issued.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 6) as ViewResult;
            Assert.AreEqual("Loyalty point settings can not be changed if a valid receipt has been issued.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            newResult = homeController.ManageTable(32, 7) as ViewResult;
            Assert.AreEqual("The staff member can't be changed if a valid receipt has been issued.", newResult.ViewBag.ErrorMessage);

            mockContext.Cookies.Clear();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Staff&ID=2"));
            result = homeController.ManageTable(32) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }

        [TestMethod]
        public void OpenTable()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };

            //act
            var result = homeController.OpenTable() as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            result = homeController.OpenTable() as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            HttpCookie myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "Staff";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            var newResult = homeController.OpenTable() as ViewResult;
            Assert.AreEqual("OpenTable", newResult.ViewName);

            mockContext.Cookies.Clear();
            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=SystemAdmin"));
            newResult = homeController.OpenTable() as ViewResult;
            Assert.AreEqual("OpenTable", newResult.ViewName);
        }
        [TestMethod]
        public void OpenTableOrder()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };
            Order myOrder = new Order();
            myOrder.customerID = "6";
            myOrder.generatedReceipt = 0;
            myOrder.isPaid = 0;
            myOrder.orderStartDate = DateTime.UtcNow;
            myOrder.pointsChoice = "Save";
            myOrder.staffID = 1;
            //act
            var result = homeController.OpenTable(myOrder) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            result = homeController.OpenTable(myOrder) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            HttpCookie myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "Staff";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            result = homeController.OpenTable(myOrder) as RedirectToRouteResult;
            string action = result.RouteValues["Action"].ToString();
            bool response = action.Contains("ManageTable");
            Assert.AreEqual(response, true);
        }
        [TestMethod]
        public void CloseTable()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };
            var result = homeController.CloseTable(30) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            result = homeController.CloseTable(30) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            HttpCookie myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "Staff";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            result = homeController.CloseTable(30) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        [TestMethod]
        public void DeleteOrderLine()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };
            var result = homeController.DeleteOrderLine(1) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            result = homeController.DeleteOrderLine(1) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            HttpCookie myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            result = homeController.DeleteOrderLine(1) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
        }
        [TestMethod]
        public void ChangeOrderLineQuantity()
        {
            MockContext mockContext = new MockContext();
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockContext.Http.Object
                }
            };
            var result = homeController.ChangeOrderLineQuantity(21,3) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Add(new HttpCookie("UserSettings", "Role=Customer"));
            result = homeController.ChangeOrderLineQuantity(21,3) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            HttpCookie myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            result = homeController.ChangeOrderLineQuantity(20,3) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);

            mockContext.Cookies.Clear();
            myCookie = new HttpCookie("UserSettings");
            myCookie.Values["Role"] = "SystemAdmin";
            myCookie.Values["ID"] = "1";
            mockContext.Cookies.Add(myCookie);
            result = homeController.ChangeOrderLineQuantity(21, 3) as RedirectToRouteResult;
            string response = result.RouteValues["Action"].ToString();
            bool check = response.Contains("ManageTable");
            Assert.AreEqual(check, true);
        }
        [TestMethod]
        public void CheckCredentials()
        {
            var controller = new CustomerAppController();
            var result = controller.CheckCredentials("customer@restaurant.com", "password");
            Assert.AreEqual(6, result[1]);

            result = controller.CheckCredentials("fionn@email.com", "password");
            Assert.AreEqual(0, result[0]);
        }
        [TestMethod]
        public void CheckCustomerDetails()
        {
            var controller = new CustomerAppController();
            var result = controller.GetCustomerDetails(6);
            Assert.AreEqual("customer@restaurant.com", result.customerEmail);
        }
    }
}