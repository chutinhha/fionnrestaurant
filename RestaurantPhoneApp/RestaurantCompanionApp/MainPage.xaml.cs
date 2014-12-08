using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RestaurantCompanionApp.Resources;
using RestaurantCompanionApp.Models;
using System.IO;
using System.Threading;

namespace RestaurantCompanionApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        Settings mySettings = new Settings();
        private const String serviceURI = "http://fionnrestaurant.azurewebsites.net";
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }
        private void Button_Click_Home(object sender, RoutedEventArgs e)
        {
            if (MyVariables.loggedIn == true)
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Visible;
                LogOutPanel.Visibility = System.Windows.Visibility.Visible;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Collapsed;
                LogOutPanel.Visibility = System.Windows.Visibility.Collapsed;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Visible;
            }
            WelcomeGrid.Visibility = System.Windows.Visibility.Visible;
            ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
            LoginGrid.Visibility = System.Windows.Visibility.Collapsed;
            OrderList.Visibility = System.Windows.Visibility.Collapsed;
            ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
            Header.Text = "Home";
        }
        private async void Button_Click_News(object sender, RoutedEventArgs e)
        {
            if (MyVariables.loggedIn == true)
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Visible;
                LogOutPanel.Visibility = System.Windows.Visibility.Visible;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Collapsed;
                LogOutPanel.Visibility = System.Windows.Visibility.Collapsed;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Visible;
            }
            WelcomeGrid.Visibility = System.Windows.Visibility.Collapsed;
            ViewNewsGrid.Visibility = System.Windows.Visibility.Visible;
            LoginGrid.Visibility = System.Windows.Visibility.Collapsed;
            OrderList.Visibility = System.Windows.Visibility.Collapsed;
            ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
            Header.Text = "News";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(serviceURI);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync("api/CustomerApp/GetNews");
                    if (response.IsSuccessStatusCode)
                    {
                        List<News> myNews = await response.Content.ReadAsAsync<List<News>>();
                        ViewNewsGrid.ItemsSource = myNews;
                    }
                    else
                    {
                        //fill this in....
                    }
                }
            }
            catch (Exception ex)
            {
                //something in here
            }
        }
        private async void Button_Click_View_Orders(object sender, RoutedEventArgs e)
        {
            if (MyVariables.loggedIn == true)
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Visible;
                LogOutPanel.Visibility = System.Windows.Visibility.Visible;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Collapsed;
                LogOutPanel.Visibility = System.Windows.Visibility.Collapsed;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Visible;
            }
            if (MyVariables.loggedIn == true)
            {
                OrderList.Visibility = System.Windows.Visibility.Visible;
                ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
                WelcomeGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewOrderLinesGrid.Visibility = System.Windows.Visibility.Collapsed;
                Header.Text = "Your Orders";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(serviceURI);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = await client.GetAsync("api/CustomerApp/GetAllOrders?customerID=" + MyVariables.custID);
                        if (response.IsSuccessStatusCode)
                        {
                            List<Order> myOrder = await response.Content.ReadAsAsync<List<Order>>();
                            OrderList.ItemsSource = myOrder;
                        }
                        else
                        {
                            //fill this in....
                        }
                    }
                }
                catch (Exception ex)
                {
                    //something in here
                }
            }
        }
        private async void Button_Click_View_OrderLines(object sender, RoutedEventArgs e)
        {
            if (MyVariables.loggedIn == true)
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Visible;
                LogOutPanel.Visibility = System.Windows.Visibility.Visible;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Collapsed;
                LogOutPanel.Visibility = System.Windows.Visibility.Collapsed;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Visible;
            }
            if (MyVariables.loggedIn == true)
            {
                OrderList.Visibility = System.Windows.Visibility.Collapsed;
                ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
                WelcomeGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewOrderLinesGrid.Visibility = System.Windows.Visibility.Visible;
                ViewOrderLines.Visibility = System.Windows.Visibility.Visible;
                Button myButton = (Button)sender;
                string id = myButton.CommandParameter.ToString();
                Header.Text = "Order #" + id;
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(serviceURI);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = await client.GetAsync("api/CustomerApp/GetAllOrderLinesForOrder?orderID=" + id);
                        if (response.IsSuccessStatusCode)
                        {
                            var myOrder = await response.Content.ReadAsAsync<IEnumerable<OrderLine>>();
                            ViewOrderLines.ItemsSource = new ObservableCollection<OrderLine>(myOrder);
                        }
                        else
                        {
                            //fill this in....
                        }
                    }
                }
                catch (Exception ex)
                {
                    //something in here
                }
            }
        }
        private void Button_Click_Login_Page(object sender, RoutedEventArgs e)
        {
            if (MyVariables.loggedIn == true)
            {

            }
            else
            {
                WelcomeGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
                LoginGrid.Visibility = System.Windows.Visibility.Visible;
                ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
                Header.Text = "Login";
            }
        }
        private async void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            string email = UserEmail.Text;
            string password = UserPassword.Password;
            if (email == "" || password == "")
            {
                LoginMessage.Text = "The email/password combination was not recognised.";
            }
            else
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(serviceURI);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = await client.GetAsync("api/CustomerApp/CheckCredentials?email=" + email + "&password=" + password + "");
                        if (response.IsSuccessStatusCode)
                        {
                            int[] details = new int[2];
                            details = await response.Content.ReadAsAsync<int[]>();
                            if (details[0] == 1)
                            {
                                MyVariables.loggedIn = true;
                                MyVariables.custID = details[1];
                                MyVariables.custPass = password;
                                MyVariables.custEmail = email;
                                LoginGrid.Visibility = System.Windows.Visibility.Collapsed;
                                ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
                                LoggedInPanel.Visibility = System.Windows.Visibility.Visible;
                                LogOutPanel.Visibility = System.Windows.Visibility.Visible;
                                LoggedOutPanel.Visibility = System.Windows.Visibility.Collapsed;
                                OrderList.Visibility = System.Windows.Visibility.Visible;
                                ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
                                ViewOrderLinesGrid.Visibility = System.Windows.Visibility.Collapsed;
                                Header.Text = "Your Orders";
                                LoginMessage.Text = "";
                                UserEmail.Text = "";
                                UserPassword.Password = "";
                                //Populate Orders
                                HttpResponseMessage getOrdersMessage = await client.GetAsync("api/CustomerApp/GetAllOrders?customerID=" + MyVariables.custID);
                                if (getOrdersMessage.IsSuccessStatusCode)
                                {
                                    List<Order> myOrder = await getOrdersMessage.Content.ReadAsAsync<List<Order>>();
                                    OrderList.ItemsSource = myOrder;
                                }
                                else
                                {
                                    //fill this in....
                                }
                            }
                            else
                            {
                                MyVariables.loggedIn = false;
                                MyVariables.custID = 0;
                                LoginMessage.Text = "The email/password combination was not recognised.";
                            }
                        }
                        else
                        {
                            //fill this in....
                        }
                    }
                }
                catch (Exception ex)
                {
                    //something in here
                }
            }
        }
        private void Button_Click_LogOut(object sender, RoutedEventArgs e)
        {
            MyVariables.loggedIn = false;
            MyVariables.custID = 0;
            MyVariables.custPass = "";
            MyVariables.custEmail = "";
            LoggedInPanel.Visibility = System.Windows.Visibility.Collapsed;
            ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
            LoggedOutPanel.Visibility = System.Windows.Visibility.Visible;
            LogOutPanel.Visibility = System.Windows.Visibility.Collapsed;
            OrderList.Visibility = System.Windows.Visibility.Collapsed;
            WelcomeGrid.Visibility = System.Windows.Visibility.Visible;
            ViewAccountGrid.Visibility = System.Windows.Visibility.Collapsed;
            ViewOrderLinesGrid.Visibility = System.Windows.Visibility.Collapsed;
            OrderList.ItemsSource = null;
        }
        private async void Button_Click_Account(object sender, RoutedEventArgs e)
        {
            if (MyVariables.loggedIn == true)
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Visible;
                LogOutPanel.Visibility = System.Windows.Visibility.Visible;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LoggedInPanel.Visibility = System.Windows.Visibility.Collapsed;
                LogOutPanel.Visibility = System.Windows.Visibility.Collapsed;
                LoggedOutPanel.Visibility = System.Windows.Visibility.Visible;
            }
            if (MyVariables.loggedIn == true)
            {
                OrderList.Visibility = System.Windows.Visibility.Collapsed;
                ViewNewsGrid.Visibility = System.Windows.Visibility.Collapsed;
                WelcomeGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewOrderLinesGrid.Visibility = System.Windows.Visibility.Collapsed;
                ViewAccountGrid.Visibility = System.Windows.Visibility.Visible;
                ViewAccount.Visibility = System.Windows.Visibility.Visible;
                Header.Text = "Your Account";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(serviceURI);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = await client.GetAsync("api/CustomerApp/GetCustomerDetails?custID=6");
                        if (response.IsSuccessStatusCode)
                        {
                            Customer myCustomer = await response.Content.ReadAsAsync<Customer>();
                            List<Customer> returnCustomer = new List<Customer>();
                            returnCustomer.Add(myCustomer);
                            ViewAccount.ItemsSource = returnCustomer;
                        }
                        else
                        {
                            //fill this in....
                        }
                    }
                }
                catch (Exception ex)
                {
                    //something in here
                }
            }
        }
    }
}