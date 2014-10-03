using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Trivia.Startup))]
namespace Trivia
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
