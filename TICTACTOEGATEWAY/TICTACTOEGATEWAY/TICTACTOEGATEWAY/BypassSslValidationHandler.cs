using System.Net.Http;

namespace TICTACTOEGATEWAY;

public class BypassSslValidationHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        InnerHandler ??= new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return base.SendAsync(request, cancellationToken);
    }
}
