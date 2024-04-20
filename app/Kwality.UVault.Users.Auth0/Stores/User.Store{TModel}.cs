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
namespace Kwality.UVault.Users.Auth0.Stores;

using System.Diagnostics.CodeAnalysis;

using global::Auth0.Core.Exceptions;
using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Users.Auth0.Mapping.Abstractions;
using Kwality.UVault.Users.Auth0.Models;
using Kwality.UVault.Users.Auth0.Options;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.Stores.Abstractions;

[UsedImplicitly]
internal sealed class UserStore<TModel>(
    ManagementClient managementClient,
    ApiConfiguration apiConfiguration,
    IModelMapper<TModel> modelMapper,
    Auth0Options options) : IUserStore<TModel, StringKey>
    where TModel : UserModel
{
    public Task<TModel> GetByKeyAsync(StringKey key)
    {
        options.RetryCount = 0;

        return this.GetByKeyInternalAsync(key);
    }

    public Task<IEnumerable<TModel>> GetByEmailAsync(string email)
    {
        options.RetryCount = 0;

        return this.GetByEmailInternalAsync(email);
    }

    public Task<StringKey> CreateAsync(TModel model, IUserOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.CreateInternalAsync(model, mapper);
    }

    public Task UpdateAsync(StringKey key, TModel model, IUserOperationMapper mapper)
    {
        options.RetryCount = 0;

        return this.UpdateInternalAsync(key, model, mapper);
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

            User? user = await apiClient.Users.GetAsync(key.Value)
                                        .ConfigureAwait(false);

            return modelMapper.Map(user);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleGetByKeyInternalRateLimitExceptionAsync(key, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read user: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<TModel> HandleGetByKeyInternalRateLimitExceptionAsync(StringKey key, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to read user: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to read user: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.GetByKeyInternalAsync(key)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException($"Failed to read user: `{key}`.", ex);
        }
    }

    private async Task<IEnumerable<TModel>> GetByEmailInternalAsync(string email)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            IList<User>? users = await apiClient.Users.GetUsersByEmailAsync(email)
                                                .ConfigureAwait(false);

            return users.Select(modelMapper.Map);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleGetByEmailInternalRateLimitExceptionAsync(email, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read user: `{email}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<IEnumerable<TModel>> HandleGetByEmailInternalRateLimitExceptionAsync(string email, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to read user: `{email}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to read user: `{email}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.GetByEmailInternalAsync(email)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException($"Failed to read user: `{email}`.", ex);
        }
    }

    private async Task<StringKey> CreateInternalAsync(TModel model, IUserOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            User user = await apiClient.Users.CreateAsync(mapper.Create<TModel, UserCreateRequest>(model))
                                       .ConfigureAwait(false);

            return new StringKey(user.UserId);
        }
        catch (RateLimitApiException ex)
        {
            return await this.HandleCreateInternalRateLimitExceptionAsync(model, mapper, ex)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create user.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<StringKey> HandleCreateInternalRateLimitExceptionAsync(
        TModel model, IUserOperationMapper mapper, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException("Failed to create user.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException("Failed to create user.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                return await this.CreateInternalAsync(model, mapper)
                                 .ConfigureAwait(false);

            default:
                throw new ReadException("Failed to create user.", ex);
        }
    }

    private async Task UpdateInternalAsync(StringKey key, TModel model, IUserOperationMapper mapper)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.Users.UpdateAsync(key.Value, mapper.Create<TModel, UserUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            await this.HandleUpdateInternalRateLimitExceptionAsync(key, model, mapper, ex)
                      .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update user: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task HandleUpdateInternalRateLimitExceptionAsync(
        StringKey key, TModel model, IUserOperationMapper mapper, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to update user: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to update user: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                await this.UpdateInternalAsync(key, model, mapper)
                          .ConfigureAwait(false);

                break;

            default:
                throw new ReadException($"Failed to update user: `{key}`.", ex);
        }
    }

    private async Task DeleteByKeyInternalAsync(StringKey key)
    {
        try
        {
            using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                            .ConfigureAwait(false);

            await apiClient.Users.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (RateLimitApiException ex)
        {
            await this.HandleDeleteByKeyInternalRateLimitExceptionAsync(key, ex)
                      .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete user: `{key}`.", ex);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task HandleDeleteByKeyInternalRateLimitExceptionAsync(StringKey key, Exception ex)
    {
        switch (options.RateLimitBehaviour)
        {
            case RateLimitBehaviour.Fail:
                throw new ReadException($"Failed to delete user: `{key}`.", ex);

            case RateLimitBehaviour.Retry:
                if (options.RetryCount > options.RateLimitMaxRetryCount)
                {
                    throw new ReadException($"Failed to delete user: `{key}`.", ex);
                }

                options.RetryCount += 1;

                await Task.Delay(options.RateLimitRetryInterval)
                          .ConfigureAwait(false);

                await this.DeleteByKeyInternalAsync(key)
                          .ConfigureAwait(false);

                break;

            default:
                throw new ReadException($"Failed to delete user: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await managementClient.GetTokenAsync(apiConfiguration)
                                                          .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, apiConfiguration.TokenEndpoint.Authority);
    }
}
