using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StarfishProject.Startup))]
namespace StarfishProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
