using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IDMSPhotoLink.Startup))]
namespace IDMSPhotoLink
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
