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
using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;
using global::Auth0.ManagementApi.Paging;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Auth0.Mapping.Abstractions;
using Kwality.UVault.M2M.Auth0.Models;
using Kwality.UVault.M2M.Auth0.Options;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.Stores.Abstractions;

[UsedImplicitly]
internal sealed class ApplicationStore<TModel>(
    ManagementClient managementClient,
    ApiConfiguration apiConfiguration,
    IModelMapper<TModel> modelMapper,
    Auth0Options options) : IApplicationStore<TModel, StringKey>
    where TModel : ApplicationModel
{
    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IApplicationFilter? filter)
    {
        options.RetryCount = 0;

        return this.GetAllInternalAsync(pageIndex, pageSize, filter);
    }

    public Task<TModel> GetByKeyAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.GetByKeyInternalAsync(key);
    }

    public Task<StringKey> CreateAsync(TModel model, IApplicationOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.CreateInternalAsync(model, mapper);
    }

    public Task UpdateAsync(StringKey key, TModel model, IApplicationOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.UpdateInternalAsync(key, model, mapper);
    }

    public Task DeleteByKeyAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.DeleteByKeyInternalAsync(key);
    }

    public Task<TModel> RotateClientSecretAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.RotateClientSecretInternalAsync(key);
    }

    private async Task<PagedResultSet<TModel>> GetAllInternalAsync(
        int pageIndex, int pageSize, IApplicationFilter? filter)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            GetClientsRequest request = filter == null ? new GetClientsRequest() : filter.Create<GetClientsRequest>();

            IPagedList<Client>? clients = await apiClient
                                                .Clients.GetAllAsync(request,
                                                    new PaginationInfo(pageIndex, pageSize, true))
                                                .ConfigureAwait(false);

            IList<TModel> models = clients.Select(modelMapper.Map)
                                          .ToList();

            return new PagedResultSet<TModel>(models, clients.Paging.Total > (pageIndex + 1) * pageSize);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleGetAllInternalRateLimitExceptionAsync(pageIndex, pageSize, filter, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ReadException("Failed to read applications.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<PagedResultSet<TModel>> HandleGetAllInternalRateLimitExceptionAsync(
        int pageIndex, int pageSize, IApplicationFilter? filter, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException("Failed to read applications.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException("Failed to read applications.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.GetAllInternalAsync(pageIndex, pageSize, filter)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException("Failed to read applications.", ex);
        }
    }

    private async Task<TModel> GetByKeyInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            Client? client = await apiClient.Clients.GetAsync(key.Value)
                                            .ConfigureAwait(false);

            return modelMapper.Map(client);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleGetByKeyInternalRateLimitExceptionAsync(key, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read application: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<TModel> HandleGetByKeyInternalRateLimitExceptionAsync(StringKey key, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to read application: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to read application: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.GetByKeyInternalAsync(key)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException($"Failed to read application: `{key}`.", ex);
        }
    }

    private async Task<StringKey> CreateInternalAsync(TModel model, IApplicationOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            Client client = await apiClient.Clients.CreateAsync(mapper.Create<TModel, ClientCreateRequest>(model))
                                           .ConfigureAwait(false);

            return new StringKey(client.ClientId);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleCreateInternalRateLimitExceptionAsync(model, mapper, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create application.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<StringKey> HandleCreateInternalRateLimitExceptionAsync(
        TModel model, IApplicationOperationMapper mapper, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException("Failed to create application.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException("Failed to create application.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.CreateInternalAsync(model, mapper)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException("Failed to create application.", ex);
        }
    }

    private async Task UpdateInternalAsync(StringKey key, TModel model, IApplicationOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.Clients.UpdateAsync(key.Value, mapper.Create<TModel, ClientUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            await this.HandleUpdateInternalRateLimitExceptionAsync(key, model, mapper, ex)
                      .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update application: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task HandleUpdateInternalRateLimitExceptionAsync(
        StringKey key, TModel model, IApplicationOperationMapper mapper, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to update application: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to update application: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                await this.UpdateInternalAsync(key, model, mapper)
                          .ConfigureAwait(false);

                break;

            default:
                throw new ReadException($"Failed to update application: `{key}`.", ex);
        }
    }

    private async Task DeleteByKeyInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.Clients.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            await this.HandleDeleteByKeyInternalRateLimitExceptionAsync(key, ex)
                      .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete application: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task HandleDeleteByKeyInternalRateLimitExceptionAsync(StringKey key, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to delete application: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to delete application: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                await this.DeleteByKeyInternalAsync(key)
                          .ConfigureAwait(false);

                break;

            default:
                throw new ReadException($"Failed to delete application: `{key}`.", ex);
        }
    }

    private async Task<TModel> RotateClientSecretInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            Client? client = await apiClient.Clients.RotateClientSecret(key.Value)
                                            .ConfigureAwait(false);

            return modelMapper.Map(client);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleRotateClientSecretInternalRateLimitExceptionAsync(key, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update application: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<TModel> HandleRotateClientSecretInternalRateLimitExceptionAsync(StringKey key, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to update application: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to update application: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.RotateClientSecretInternalAsync(key)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException($"Failed to update application: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await managementClient.GetTokenAsync(apiConfiguration)
                                                          .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, apiConfiguration.TokenEndpoint.Authority);
    }
}
