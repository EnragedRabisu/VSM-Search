using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DBProjectWebsite.Startup))]
namespace DBProjectWebsite
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
