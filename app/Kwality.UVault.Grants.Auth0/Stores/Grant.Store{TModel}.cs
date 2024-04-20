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
namespace Kwality.UVault.Grants.Auth0.Stores;

using System.Diagnostics.CodeAnalysis;

using global::Auth0.Core.Exceptions;
using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;
using global::Auth0.ManagementApi.Paging;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Auth0.Mapping.Abstractions;
using Kwality.UVault.Grants.Auth0.Models;
using Kwality.UVault.Grants.Auth0.Options;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.Stores.Abstractions;


#pragma warning disable CA1812
internal sealed class GrantStore<TModel>(
#pragma warning restore CA1812
    ManagementClient managementClient,
    ApiConfiguration apiConfiguration,
    IModelMapper<TModel> modelMapper,
    Auth0Options options) : IGrantStore<TModel, StringKey>
    where TModel : GrantModel
{
    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
    {
        options.RetryCount = 0;

        return this.GetAllInternalAsync(pageIndex, pageSize, filter);
    }

    public Task<StringKey> CreateAsync(TModel model, IGrantOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.CreateInternalAsync(model, mapper);
    }

    public Task UpdateAsync(StringKey key, TModel model, IGrantOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.UpdateInternalAsync(key, model, mapper);
    }

    public Task DeleteByKeyAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.DeleteByKeyInternalAsync(key);
    }

    [ExcludeFromCodeCoverage]
    private async Task<PagedResultSet<TModel>> GetAllInternalAsync(int pageIndex, int pageSize, IGrantFilter? filter)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            GetClientGrantsRequest request
                = filter == null ? new GetClientGrantsRequest() : filter.Create<GetClientGrantsRequest>();

            IPagedList<ClientGrant>? clientGrants = await apiClient
                                                          .ClientGrants.GetAllAsync(request,
                                                              new PaginationInfo(pageIndex, pageSize, true))
                                                          .ConfigureAwait(false);

            IList<TModel> models = clientGrants.Select(modelMapper.Map)
                                               .ToList();

            return new PagedResultSet<TModel>(models, clientGrants.Paging.Total > (pageIndex + 1) * pageSize);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleGetAllInternalRateLimitExceptionAsync(pageIndex, pageSize, filter, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ReadException("Failed to read client grants.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<PagedResultSet<TModel>> HandleGetAllInternalRateLimitExceptionAsync(
        int pageIndex, int pageSize, IGrantFilter? filter, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException("Failed to read client grants.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException("Failed to read client grants.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.GetAllInternalAsync(pageIndex, pageSize, filter)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException("Failed to read client grants.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<StringKey> CreateInternalAsync(TModel model, IGrantOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            ClientGrant clientGrant = await apiClient
                                            .ClientGrants.CreateAsync(
                                                mapper.Create<TModel, ClientGrantCreateRequest>(model))
                                            .ConfigureAwait(false);

            return new StringKey(clientGrant.Id);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleCreateInternalRateLimitExceptionAsync(model, mapper, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create client grant.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<StringKey> HandleCreateInternalRateLimitExceptionAsync(
        TModel model, IGrantOperationMapper mapper, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException("Failed to create client grant.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException("Failed to create client grant.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.CreateInternalAsync(model, mapper)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException("Failed to create client grant.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task UpdateInternalAsync(StringKey key, TModel model, IGrantOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.ClientGrants.UpdateAsync(key.Value, mapper.Create<TModel, ClientGrantUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            await this.HandleUpdateInternalRateLimitExceptionAsync(key, model, mapper, ex)
                      .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update client grant: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task HandleUpdateInternalRateLimitExceptionAsync(
        StringKey key, TModel model, IGrantOperationMapper mapper, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to update client grant: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to update client grant: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                await this.UpdateInternalAsync(key, model, mapper)
                          .ConfigureAwait(false);

                break;

            default:
                throw new ReadException($"Failed to update client grant: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task DeleteByKeyInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.ClientGrants.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            await this.HandleDeleteByKeyInternalRateLimitExceptionAsync(key, ex)
                      .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete client grant: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task HandleDeleteByKeyInternalRateLimitExceptionAsync(StringKey key, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to delete client grant: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to delete client grant: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                await this.DeleteByKeyInternalAsync(key)
                          .ConfigureAwait(false);

                break;

            default:
                throw new ReadException($"Failed to delete client grant: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await managementClient.GetTokenAsync(apiConfiguration)
                                                          .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, apiConfiguration.TokenEndpoint.Authority);
    }
}
