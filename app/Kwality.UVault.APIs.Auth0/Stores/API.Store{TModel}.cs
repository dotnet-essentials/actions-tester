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
namespace Kwality.UVault.APIs.Auth0.Stores;

using global::Auth0.Core.Exceptions;
using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Auth0.Mapping.Abstractions;
using Kwality.UVault.APIs.Auth0.Models;
using Kwality.UVault.APIs.Auth0.Options;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.APIs.Stores.Abstractions;
using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;

[UsedImplicitly]
internal sealed class ApiStore<TModel>(
    ManagementClient client,
    ApiConfiguration configuration,
    IModelMapper<TModel> modelMapper,
    Auth0Options options) : IApiStore<TModel, StringKey>
    where TModel : ApiModel
{
    public Task<TModel> GetByKeyAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.GetByKeyInternalAsync(key);
    }

    public Task<StringKey> CreateAsync(TModel model, IApiOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.CreateInternalAsync(model, mapper);
    }

    public Task DeleteByKeyAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.DeleteByKeyInternalAsync(key);
    }

    private async Task<TModel> GetByKeyInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            ResourceServer? resourceServer = await apiClient.ResourceServers.GetAsync(key.Value)
                                                            .ConfigureAwait(false);

            return modelMapper.Map(resourceServer);
        }
        catch (RateLimitApiException ex)
        {
            switch (options.RateLimitBehaviour)
            {
                case RateLimitBehaviour.Fail:
                    throw new ReadException($"Failed to read API: `{key}`.", ex);

                case RateLimitBehaviour.Retry:
                    if (options.RetryCount > options.RateLimitMaxRetryCount)
                    {
                        throw new ReadException($"Failed to read API: `{key}`.", ex);
                    }

                    options.RetryCount += 1;

                    await Task.Delay(options.RateLimitRetryInterval)
                              .ConfigureAwait(false);

                    return await this.GetByKeyInternalAsync(key)
                                     .ConfigureAwait(false);

                default:
                    throw new ReadException($"Failed to read API: `{key}`.", ex);
            }
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read API: `{key}`.", ex);
        }
    }

    private async Task<StringKey> CreateInternalAsync(TModel model, IApiOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            ResourceServer resourceServer = await apiClient
                                                  .ResourceServers.CreateAsync(
                                                      mapper.Create<TModel, ResourceServerCreateRequest>(model))
                                                  .ConfigureAwait(false);

            return new StringKey(resourceServer.Id);
        }
        catch (RateLimitApiException ex)
        {
            switch (options.RateLimitBehaviour)
            {
                case RateLimitBehaviour.Fail:
                    throw new ReadException("Failed to create API.", ex);

                case RateLimitBehaviour.Retry:
                    if (options.RetryCount > options.RateLimitMaxRetryCount)
                    {
                        throw new ReadException("Failed to create API.", ex);
                    }

                    options.RetryCount += 1;

                    await Task.Delay(options.RateLimitRetryInterval)
                              .ConfigureAwait(false);

                    return await this.CreateInternalAsync(model, mapper)
                                     .ConfigureAwait(false);

                default:
                    throw new ReadException("Failed to create API.", ex);
            }
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create API.", ex);
        }
    }

    private async Task DeleteByKeyInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.ResourceServers.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            switch (options.RateLimitBehaviour)
            {
                case RateLimitBehaviour.Fail:
                    throw new ReadException($"Failed to delete API: `{key}`.", ex);

                case RateLimitBehaviour.Retry:
                    if (options.RetryCount > options.RateLimitMaxRetryCount)
                    {
                        throw new ReadException($"Failed to delete API: `{key}`.", ex);
                    }

                    options.RetryCount += 1;

                    await Task.Delay(options.RateLimitRetryInterval)
                              .ConfigureAwait(false);

                    await this.DeleteByKeyInternalAsync(key)
                              .ConfigureAwait(false);

                    break;

                default:
                    throw new ReadException($"Failed to delete API: `{key}`.", ex);
            }
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete API: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await client.GetTokenAsync(configuration)
                                                .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, configuration.TokenEndpoint.Authority);
    }
}
