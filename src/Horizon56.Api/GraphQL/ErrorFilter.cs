using System;
using HotChocolate;

namespace Horizon56.Api.GraphQL
{
    // Without this filter, when domain validation fails (e.g. "Plan name too short"),
    // the client would receive a confusing "Unexpected Execution Error" with no useful detail.
    //
    // This filter intercepts every GraphQL error before it's sent to the client.
    // If the error came from a known domain exception (ArgumentException or InvalidOperationException),
    // we rewrite it into a clean message with a recognisable "DOMAIN_ERROR" code.
    //
    // Example — what the client receives when validation fails:
    //   {
    //     "message": "Plan name must be between 3–100 characters.",
    //     "extensions": { "code": "DOMAIN_ERROR" }
    //   }
    //
    // Registered in Startup.cs via: .AddErrorFilter<DomainErrorFilter>()
    public class DomainErrorFilter : IErrorFilter
    {
        // HotChocolate calls this method for every error that occurs during a GraphQL operation.
        public IError OnError(IError error)
        {
            bool isDomainException =
                error.Exception is ArgumentException ||          // thrown by value objects (e.g. invalid plan name)
                error.Exception is InvalidOperationException;    // thrown when a resource is not found

            if (isDomainException)
            {
                return error
                    .WithMessage(error.Exception.Message)  // use the exception's own message (already user-friendly)
                    .WithCode("DOMAIN_ERROR")              // add a code so clients can handle it programmatically
                    .RemoveException();                    // strip the stack trace — never send that to clients
            }

            // For any other kind of error, pass it through unchanged
            return error;
        }
    }
}
