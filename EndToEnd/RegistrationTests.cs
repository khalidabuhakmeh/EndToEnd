using System;
using EndToEnd.Core;
using FluentAssertions;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface.Auth;
using Xunit;

namespace EndToEnd
{
    public class RegistrationTests
        : ServiceStackTestBase
    {
        [Fact]
        public void Can_register_a_new_user()
        {
            var client = new JsonServiceClient(ListeningOn);

            var response = client.Send(new Registration
            {
                UserName = "newuser",
                Password = "p@55w0rd",
                DisplayName = "New User",
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User"
            });

            response.Should().NotBeNull();
            response.UserId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Can_create_a_widget()
        {
            var response = Client.Send(new CreateWidget
            {
                Name = "Widget One"
            });

            response.Should().NotBeNull();
            response.Widget.Id.Should().NotBeNullOrEmpty();
            response.Widget.Name.Should().Be("Widget One");
        }

        [Fact]
        public void Throws_validation_exception_when_bad_widget()
        {
            var validator = Host.Container.Resolve<IValidator<CreateWidget>>();
            validator.Should().NotBeNull();


            try
            {
                var response = Client.Post(new CreateWidget
                {
                    Name = null
                });
                throw new Exception("Should Not Get Here!");
            }
            catch (WebServiceException wex)
            {
                wex.StatusCode.Should().Be(400);
                wex.ErrorMessage.Should().Be("'Name' should not be empty.");
            }
        }
    }
}
