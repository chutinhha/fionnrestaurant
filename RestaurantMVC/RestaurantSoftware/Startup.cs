using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RestaurantSoftware.Startup))]
namespace RestaurantSoftware
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
