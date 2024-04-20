// =====================================================================================================================
// = LICENSE:       Copyright (c) 2023 Kevin De Coninck
// =
// =                Permission is hereby granted, free of charge, to any person
// =                obtaining a copy of this software and associated documentation
// =                files (the "Software"), to deal in the Software without
// =                restriction, including without limitation the rights to use,
// =                copy, modify, merge, publish, distribute, sublicense, and/or sell
// =                copies of the Software, and to permit persons to whom the
// =                Software is furnished to do so, subject to the following
// =                conditions:
// =
// =                The above copyright notice and this permission notice shall be
// =                included in all copies or substantial portions of the Software.
// =
// =                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// =                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// =                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// =                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// =                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// =                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// =                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// =                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.UVault.M2M.Auth0.Stores;

using System.Diagnostics.CodeAnalysis;

using global::Auth0.Core.Exceptions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Models;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.M2M.Auth0.Configuration;
using Kwality.UVault.M2M.Auth0.Mapping.Abstractions;
using Kwality.UVault.M2M.Auth0.Options;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Stores.Abstractions;


internal sealed class ApplicationTokenStore<TToken>(
    ManagementClient managementClient,
    M2MConfiguration m2MConfiguration,
    IModelTokenMapper<TToken> modelMapper,
    Auth0Options options) : IApplicationTokenStore<TToken>
    where TToken : TokenModel
{
    private const string readError = "Failed to retrieve an access token.";

    public async Task<TToken> GetAccessTokenAsync(
        string clientId, string clientSecret, string audience, string grantType)
    {
        options.RetryCount = 0;

        try
        {
            ArgumentNullException.ThrowIfNull(clientId);
            ArgumentNullException.ThrowIfNull(clientSecret);
            ArgumentNullException.ThrowIfNull(audience);
            ArgumentNullException.ThrowIfNull(grantType);

            return await this.GetAccessTokenInternalAsync(clientId, clientSecret, audience, grantType)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ReadException(readError, ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<TToken> GetAccessTokenInternalAsync(
        string clientId, string clientSecret, string audience, string grantType)
    {
        try
        {
            ApiManagementToken managementApiToken = await managementClient
                                                          .GetM2MTokenAsync(m2MConfiguration.TokenEndpoint, grantType,
                                                              clientId, clientSecret, audience)
                                                          .ConfigureAwait(false);

            return modelMapper.Map(managementApiToken);
        }
        catch (RateLimitApiException ex)
        {
            return await this
                         .HandleRotateClientSecretInternalRateLimitExceptionAsync(clientId, clientSecret, audience,
                             grantType, ex)
                         .ConfigureAwait(false);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<TToken> HandleRotateClientSecretInternalRateLimitExceptionAsync(
        string clientId, string clientSecret, string audience, string grantType, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException(readError, ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException(readError, ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.GetAccessTokenInternalAsync(clientId, clientSecret, audience, grantType)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException(readError, ex);
        }
    }
}
