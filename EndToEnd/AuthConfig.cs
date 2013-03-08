using Funq;
using Raven.Client;
using ServiceStack.Authentication.RavenDb;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Configuration;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace EndToEnd
{
    public static class AuthConfig
    {
        public static void Start(IAppHost appHost, Container container)
        {
            var appSettings = new AppSettings();

            //Default route: /auth/{provider}
            appHost.Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[] {
                    new BasicAuthProvider(appSettings),
                    new CredentialsAuthProvider(appSettings)
                }) { IncludeAssignRoleServices = false, HtmlRedirect = null });

            appHost.Plugins.Add(new RegistrationFeature());

            container.Register<ICacheClient>(new MemoryCacheClient());

            container.Register<IUserAuthRepository>(c =>
            {
                var documentStore = c.Resolve<IDocumentStore>();
                return new RavenUserAuthRepository(documentStore);
            });
        }
    }
}