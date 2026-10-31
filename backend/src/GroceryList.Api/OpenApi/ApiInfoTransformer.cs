using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace GroceryList.Api.OpenApi;

/// <summary>
/// Sets the document's title and description. Scalar shows the description (Markdown) as the
/// introduction of the API reference page.
/// </summary>
internal sealed class ApiInfoTransformer : IOpenApiDocumentTransformer
{
    public const string Title = "Grocery List API";

    private const string Description =
        """
        A private grocery list for every user: add and remove products, and tag them as picked up
        (in the basket) or put them back. Each user only ever sees their own list.

        ## Authentication

        Every endpoint except register and login requires a JWT bearer token.

        1. Call `POST /api/auth/register` to create an account, or `POST /api/auth/login` if you
           already have one.
        2. Copy the `token` value from the response.
        3. Paste it into the **Bearer** token field under **Authentication**. Requests sent from
           this page then include the `Authorization: Bearer <token>` header.

        Tokens expire at `expiresAtUtc` (60 minutes by default); log in again to get a new one.
        There is no logout endpoint: a client logs out by discarding its token.

        ## Errors

        Errors are `application/problem+json` (RFC 9457) with a stable `code` extension, for example
        `GroceryItems.DuplicateName`. Clients should branch on `code`, not on the message.
        """;

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = Title;
        document.Info.Description = Description;

        return Task.CompletedTask;
    }
}
