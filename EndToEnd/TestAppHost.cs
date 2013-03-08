using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EndToEnd.Core;
using Funq;
using Raven.Client;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;

namespace EndToEnd
{
    public class TestAppHost
        : AppHostHttpListenerBase
    {
        private readonly IDocumentStore _documentStore;

        public TestAppHost(IDocumentStore documentStore)
            : base("Test AppHost Api", typeof(TestAppHost).Assembly)
        {
            _documentStore = documentStore;
        }

        public override void Configure(Container container)
        {
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

            // Register RavenDB things
            container.Register(_documentStore);
            container.Register(c =>
            {
                var db = c.Resolve<IDocumentStore>();
                return db.OpenSession();
            }).ReusedWithin(ReuseScope.Request);

            Plugins.Add(new ValidationFeature());
            container.RegisterValidators(typeof(CreateWidgetValidator).Assembly);

            // todo: register all of your plugins here
            AuthConfig.Start(this, container);
        }
    }
}
