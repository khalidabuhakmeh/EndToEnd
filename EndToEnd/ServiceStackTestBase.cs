using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Funq;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;
using ServiceStack.Authentication.RavenDb;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;

namespace EndToEnd
{
    public abstract class ServiceStackTestBase
        : RavenTestBase
    {
        protected IDocumentStore DocumentStore { get; set; }
        protected TestAppHost Host { get; set; }
        protected JsonServiceClient Client { get; set; }

        protected const string ListeningOn = "http://localhost:1337/";

        protected string Username { get { return "testuser"; } }
        protected string Password { get { return "password"; } }

        protected ServiceStackTestBase()
        {
            DocumentStore = NewDocumentStore();
            IndexCreation.CreateIndexes(typeof(ServiceStackTestBase).Assembly, DocumentStore);
            IndexCreation.CreateIndexes(typeof(RavenUserAuthRepository).Assembly, DocumentStore);

            Host = new TestAppHost(DocumentStore);
            Host.Init();
            Host.Start(ListeningOn);

            Client = new JsonServiceClient(ListeningOn)
            {
                AlwaysSendBasicAuthHeader = true,
                UserName = Username,
                Password = Password
            };

            RegisterUser();

            WaitForIndexing(DocumentStore);
        }

        private void RegisterUser()
        {
            Client.Send(new Registration
            {
                UserName = Username,
                Password = Password,
                DisplayName = "Test User",
                Email = "test@test.com",
                FirstName = "test",
                LastName = "user"
            });
        }

        public override void Dispose()
        {
            DocumentStore.Dispose();
            Host.Dispose();
        }
    }

    public static class MyValidationExtensions
    {
        public static void RegisterValidators(this Container container, params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type validator in Enumerable.Where<Type>((IEnumerable<Type>)assembly.GetTypes(), (Func<Type, bool>)(t => ReflectionExtensions.IsOrHasGenericInterfaceTypeOf(t, typeof(IValidator<>)))))
                    ServiceStack.ServiceInterface.Validation.ValidationExtensions.RegisterValidator(container, validator);
            }
        }

        public static void RegisterValidator(this Container container, Type validator)
        {
            Type baseType = validator.BaseType;
            while (!baseType.IsGenericType)
                baseType = baseType.BaseType;
            Type inFunqAsType = typeof(IValidator<>).MakeGenericType(new Type[1]
      {
        baseType.GetGenericArguments()[0]
      });
            ContainerTypeExtensions.RegisterAutoWiredType(container, validator, inFunqAsType, ReuseScope.None);
        }
    }
}
