using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCompanionApp.Models
{
    public class News
    {
        public int ID { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public DateTime date { get; set; }
    }
}
