using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OnedrivePhotoOrganizer
{
    /// <summary>
    /// Represents the authentication helper.
    /// </summary>
    internal class AuthenticationHelper
    {
        private GraphServiceClient graphServiceClient;
        private readonly IPublicClientApplication application;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationHelper"/> class.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="redirectUri">The redirect uri.</param>
        public AuthenticationHelper(string appId, string redirectUri) =>
            application = PublicClientApplicationBuilder
                .Create(appId)
                .WithRedirectUri(redirectUri)
                .Build();

        /// <summary>
        /// Gets the graph client.
        /// </summary>
        /// <returns>An instance of the <see cref="GraphServiceClient"/>.</returns>
        public GraphServiceClient GetGraphClient()
        {
            if(graphServiceClient != null)
            {
                return graphServiceClient;
            }

            graphServiceClient = new GraphServiceClient(
                "https://graph.microsoft.com/v1.0",
                new DelegateAuthenticationProvider(
                    async (request) =>
                    {
                        var token = await GetTokenAsync();
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }));

            return graphServiceClient;
        }

        private async Task<string> GetTokenAsync()
        {
            var scopes = new[] { "Files.ReadWrite.All" };
            var accounts = await application.GetAccountsAsync();
            AuthenticationResult result;

            try
            {
                result = await application
                    .AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await application
                    .AcquireTokenInteractive(scopes)
                    .WithUseEmbeddedWebView(false)
                    .WithSystemWebViewOptions(new SystemWebViewOptions
                    {
                        HtmlMessageError = "<p> An error occured: {0}. Details {1}</p>",
                        HtmlMessageSuccess = "<p>Successfully authenticated! You may close this window now.</p>",
                    })
                    .ExecuteAsync();
            }

            return result != null ? result.AccessToken : throw new Exception("Unable to acquire token.");
        }
    }
}
