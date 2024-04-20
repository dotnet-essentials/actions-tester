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
namespace Kwality.UVault.APIs.Auth0.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using Kwality.UVault.APIs.Auth0.Extensions;
using Kwality.UVault.APIs.Auth0.Mapping.Abstractions;
using Kwality.UVault.APIs.Auth0.Models;
using Kwality.UVault.APIs.Auth0.Operations.Mappers;
using Kwality.UVault.APIs.Auth0.Options;
using Kwality.UVault.APIs.Auth0.QA.Internal.Factories;
using Kwality.UVault.APIs.Managers;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[Collection("Auth0")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class ApiManagementAuth0Tests
{
    private const int rateLimitMaxRetryCount = 5;
    private const int rateLimitDelaySeconds = 2;
    private readonly ApiManager<Model, StringKey> apiManager;

    public ApiManagementAuth0Tests()
    {
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        this.apiManager = new ApiManagerFactory().Create<Model, StringKey>(options =>
            options.UseAuth0Store<Model, ModelMapper>(apiConfiguration,
                static () => new Auth0Options
                {
                    RateLimitBehaviour = RateLimitBehaviour.Retry,
                    RateLimitRetryInterval = TimeSpan.FromSeconds(rateLimitDelaySeconds),
                    RateLimitMaxRetryCount = rateLimitMaxRetryCount,
                }));
    }

    [AutoData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(StringKey key)
    {
        // ACT.
        Func<Task<Model>> act = () => this.apiManager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            // ACT.
            key = await this.apiManager.CreateAsync(model, new CreateOperationMapper())
                            .ConfigureAwait(true);

            // ASSERT.
            (await this.apiManager.GetByKeyAsync(key)
                       .ConfigureAwait(true)).Should()
                                             .BeEquivalentTo(model);
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await this.apiManager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(StringKey key)
    {
        // ACT.
        await this.apiManager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => this.apiManager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApiManager<Model, StringKey> manager
            = new ApiManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                     .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`.")
                 .ConfigureAwait(true);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.AUTH0_TOKEN_ENDPOINT), Environment.AUTH0_CLIENT_ID,
            Environment.AUTH0_CLIENT_SECRET, Environment.AUTH0_AUDIENCE);
    }

    internal sealed class Model(StringKey name) : ApiModel(name);

#pragma warning disable CA1812
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(ResourceServer resourceServer)
        {
            return new Model(new StringKey(resourceServer.Identifier));
        }
    }

    private sealed class CreateOperationMapper : Auth0ApiCreateOperationMapper
    {
        protected override ResourceServerCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ResourceServerCreateRequest { Identifier = model.Key.Value };
            }

            throw new CreateException($"Invalid {nameof(IApiOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        fixture.Customize<Model>(static composer => composer.OmitAutoProperties());

        return fixture;
    });
}
