using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SPFS.Startup))]
namespace SPFS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
