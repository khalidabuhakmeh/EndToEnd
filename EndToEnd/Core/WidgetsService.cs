using System;
using Raven.Client;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;

namespace EndToEnd.Core
{
    [Authenticate]
    public class WidgetsService
        : Service
    {
        private readonly IDocumentSession _session;

        public WidgetsService(IDocumentSession session)
        {
            _session = session;
        }

        public CreateWidgetResponse Post(CreateWidget input)
        {
            var widget = new Widget { Name = input.Name };
            _session.Store(widget);
            _session.SaveChanges();

            return new CreateWidgetResponse { Widget = widget };
        }
    }

    [Route("/widgets", "POST")]
    public class CreateWidget : IReturn<CreateWidgetResponse>
    {
        public string Name { get; set; }
    }

    public class CreateWidgetResponse
    {
        public CreateWidgetResponse()
        {
            ResponseStatus = new ResponseStatus();
        }

        public Widget Widget { get; set; }
        public ResponseStatus ResponseStatus { get; set; }   
    }

    public class Widget
    {
        public Widget()
        {
            Created = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public class CreateWidgetValidator : AbstractValidator<CreateWidget>
    {
        public CreateWidgetValidator()
        {
            RuleFor(m => m.Name).NotEmpty();
        }
    }
}
