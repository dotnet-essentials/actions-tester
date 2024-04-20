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
namespace Kwality.UVault.Grants.Auth0.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Auth0.Extensions;
using Kwality.UVault.Grants.Auth0.Mapping.Abstractions;
using Kwality.UVault.Grants.Auth0.Models;
using Kwality.UVault.Grants.Auth0.Operations.Filters;
using Kwality.UVault.Grants.Auth0.Operations.Mappers;
using Kwality.UVault.Grants.Auth0.Options;
using Kwality.UVault.Grants.Auth0.QA.Internal.Factories;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[Collection("Auth0")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class GrantManagementAuth0Tests
{
    private const int rateLimitMaxRetryCount = 5;
    private readonly GrantManager<Model, StringKey> grantManager;

    public GrantManagementAuth0Tests()
    {
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        this.grantManager = new GrantManagerFactory().Create<Model, StringKey>(options =>
            options.UseAuth0Store<Model, ModelMapper>(apiConfiguration,
                static () => new Auth0Options
                {
                    RateLimitBehaviour = RateLimitBehaviour.Retry,
                    RateLimitRetryInterval = TimeSpan.FromSeconds(2),
                    RateLimitMaxRetryCount = rateLimitMaxRetryCount,
                }));
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.grantManager.CreateAsync(model,
                                new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                            .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this.grantManager.GetAllAsync(0, 3)
                                                     .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeFalse();

            result.ResultSet.Count()
                  .Should()
                  .Be(2);

            result.ResultSet.Skip(1)
                  .Take(1)
                  .First()
                  .Should()
                  .BeEquivalentTo(model, static options => options.Excluding(static grant => grant.Scopes)
                                                                  .Excluding(static grant => grant.Key));
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                await this.grantManager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.grantManager.CreateAsync(model,
                                new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                            .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this.grantManager.GetAllAsync(1, 10)
                                                     .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeFalse();

            result.ResultSet.Count()
                  .Should()
                  .Be(0);
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                await this.grantManager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        StringKey? keyOne = null;
        StringKey? keyTwo = null;

        try
        {
            keyOne = await this.grantManager.CreateAsync(modelOne,
                                   new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                               .ConfigureAwait(true);

            keyTwo = await this.grantManager.CreateAsync(modelTwo,
                                   new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_2_CLIENT_ID))
                               .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this.grantManager.GetAllAsync(1, 1)
                                                     .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeTrue();

            result.ResultSet.Count()
                  .Should()
                  .Be(1);

            result.ResultSet.Take(1)
                  .First()
                  .Should()
                  .BeEquivalentTo(modelOne, static options => options.Excluding(static grant => grant.Scopes)
                                                                     .Excluding(static grant => grant.Key));
        }
        finally
        {
            // Cleanup: Remove the client grants in Auth0.
            if (keyOne != null)
            {
                await this.grantManager.DeleteByKeyAsync(keyOne)
                          .ConfigureAwait(true);
            }

            if (keyTwo != null)
            {
                await this.grantManager.DeleteByKeyAsync(keyTwo)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        StringKey? keyOne = null;
        StringKey? keyTwo = null;

        try
        {
            keyOne = await this.grantManager.CreateAsync(modelOne,
                                   new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                               .ConfigureAwait(true);

            keyTwo = await this.grantManager.CreateAsync(modelTwo,
                                   new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_2_CLIENT_ID))
                               .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this
                                                 .grantManager.GetAllAsync(0, 10,
                                                     new OperationFilter(modelTwo.Audience))
                                                 .ConfigureAwait(true);

            // ASSERT.
            result.ResultSet.Count()
                  .Should()
                  .Be(3);

            result.ResultSet.Should()
                  .ContainEquivalentOf(modelOne, static options => options.Excluding(static grant => grant.Key));

            result.ResultSet.Should()
                  .ContainEquivalentOf(modelTwo, static options => options.Excluding(static grant => grant.Key));
        }
        finally
        {
            // Cleanup: Remove the client grants in Auth0.
            if (keyOne != null)
            {
                await this.grantManager.DeleteByKeyAsync(keyOne)
                          .ConfigureAwait(true);
            }

            if (keyTwo != null)
            {
                await this.grantManager.DeleteByKeyAsync(keyTwo)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            // ACT.
            key = await this.grantManager.CreateAsync(model,
                                new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                            .ConfigureAwait(true);

            // ASSERT.
            (await this.grantManager.GetAllAsync(0, 100)
                       .ConfigureAwait(true)).ResultSet.Should()
                                             .ContainEquivalentOf(model,
                                                 static options => options.Excluding(static grant => grant.Key));
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                await this.grantManager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.grantManager.CreateAsync(model,
                                new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                            .ConfigureAwait(true);

            // ACT.
            model.Scopes = new[] { "read:authentication_methods" };

            await this.grantManager.UpdateAsync(key, model, new UpdateOperationMapper())
                      .ConfigureAwait(true);

            (await this.grantManager.GetAllAsync(0, 100)
                       .ConfigureAwait(true)).ResultSet.Should()
                                             .ContainEquivalentOf(model,
                                                 static options => options.Excluding(static grant => grant.Key));
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                await this.grantManager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(StringKey key, Model model)
    {
        // ACT.
        Func<Task> act = () => this.grantManager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update client grant: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey key = await this.grantManager.CreateAsync(model,
                                      new CreateOperationMapper(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                                  .ConfigureAwait(true);

        // ACT.
        await this.grantManager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.grantManager.GetAllAsync(0, 100)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .NotContainEquivalentOf(model);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.AUTH0_TOKEN_ENDPOINT), Environment.AUTH0_CLIENT_ID,
            Environment.AUTH0_CLIENT_SECRET, Environment.AUTH0_AUDIENCE);
    }

    internal sealed class Model(StringKey key, IEnumerable<string> scopes) : GrantModel(key)
    {
        public IEnumerable<string> Scopes { get; set; } = scopes;
        public string Audience { get; } = Environment.AUTH0_AUDIENCE;
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(ClientGrant clientGrant)
        {
            return new Model(new StringKey(clientGrant.Id), clientGrant.Scope);
        }
    }

    private sealed class OperationFilter(string audience) : Auth0GrantFilter
    {
        protected override GetClientGrantsRequest Map()
        {
            return new GetClientGrantsRequest { Audience = audience };
        }
    }

    private sealed class CreateOperationMapper(string clientId) : Auth0GrantCreateOperationMapper
    {
        protected override ClientGrantCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ClientGrantCreateRequest
                {
                    Scope = model.Scopes.ToList(), Audience = model.Audience, ClientId = clientId,
                };
            }

            throw new CreateException($"Invalid {nameof(IGrantOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    private sealed class UpdateOperationMapper : Auth0GrantUpdateOperationMapper
    {
        protected override ClientGrantUpdateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ClientGrantUpdateRequest { Scope = model.Scopes.ToList() };
            }

            throw new UpdateException($"Invalid {nameof(IGrantOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }
}
