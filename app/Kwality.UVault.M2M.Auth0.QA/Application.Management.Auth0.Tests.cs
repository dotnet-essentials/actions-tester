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
namespace Kwality.UVault.M2M.Auth0.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Auth0.Extensions;
using Kwality.UVault.M2M.Auth0.Mapping.Abstractions;
using Kwality.UVault.M2M.Auth0.Models;
using Kwality.UVault.M2M.Auth0.Operations.Filters;
using Kwality.UVault.M2M.Auth0.Operations.Mappers;
using Kwality.UVault.M2M.Auth0.Options;
using Kwality.UVault.M2M.Auth0.QA.Internal.Factories;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[Collection("Auth0")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class ApplicationManagementAuth0Tests
{
    private const int rateLimitMaxRetryCount = 5;
    private readonly ApplicationManager<Model, StringKey> manager;

    public ApplicationManagementAuth0Tests()
    {
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        this.manager = new ApplicationManagerFactory().Create<Model, StringKey>(options =>
            options.UseAuth0Store<Model, ModelMapper>(apiConfiguration,
                static () => new Auth0Options
                {
                    RateLimitBehaviour = RateLimitBehaviour.Retry,
                    RateLimitRetryInterval = TimeSpan.FromSeconds(2),
                    RateLimitMaxRetryCount = rateLimitMaxRetryCount,
                }));
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.manager.CreateAsync(model, new CreateOperationMapper(ClientApplicationType.NonInteractive))
                            .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 5)
                                                     .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeFalse();

            result.ResultSet.Count()
                  .Should()
                  .Be(5);

            result.ResultSet.Skip(3)
                  .Take(1)
                  .First()
                  .Should()
                  .BeEquivalentTo(model,
                      static options => options.Excluding(static application => application.ClientSecret));
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await this.manager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.manager.CreateAsync(model, new CreateOperationMapper(ClientApplicationType.NonInteractive))
                            .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this.manager.GetAllAsync(1, 10)
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
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await this.manager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 3, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.manager.CreateAsync(model, new CreateOperationMapper(ClientApplicationType.NonInteractive))
                            .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this.manager.GetAllAsync(3, 1)
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
                  .BeEquivalentTo(model,
                      static options => options.Excluding(static application => application.ClientSecret));
        }
        finally
        {
            // Cleanup: Remove the applications in Auth0.
            if (key != null)
            {
                await this.manager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        StringKey? keyOne = null;
        StringKey? keyTwo = null;

        try
        {
            keyOne = await this.manager.CreateAsync(modelOne, new CreateOperationMapper(ClientApplicationType.Native))
                               .ConfigureAwait(true);

            keyTwo = await this.manager.CreateAsync(modelTwo,
                                   new CreateOperationMapper(ClientApplicationType.NonInteractive))
                               .ConfigureAwait(true);

            // ACT.
            PagedResultSet<Model> result = await this
                                                 .manager.GetAllAsync(0, 10,
                                                     new OperationFilter(ClientApplicationType.NonInteractive))
                                                 .ConfigureAwait(true);

            // ASSERT.
            result.ResultSet.Count()
                  .Should()
                  .Be(1);

            result.ResultSet.First()
                  .Should()
                  .BeEquivalentTo(modelTwo,
                      static options => options.Excluding(static application => application.ClientSecret));
        }
        finally
        {
            // Cleanup: Remove the applications in Auth0.
            if (keyOne != null)
            {
                await this.manager.DeleteByKeyAsync(keyOne)
                          .ConfigureAwait(true);
            }

            if (keyTwo != null)
            {
                await this.manager.DeleteByKeyAsync(keyTwo)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(StringKey key)
    {
        // ACT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        StringKey? key = null;

        try
        {
            // ACT.
            key = await this.manager.CreateAsync(model, new CreateOperationMapper(ClientApplicationType.NonInteractive))
                            .ConfigureAwait(true);

            // ASSERT.
            (await this.manager.GetByKeyAsync(key)
                       .ConfigureAwait(true)).Should()
                                             .BeEquivalentTo(model,
                                                 static options => options.Excluding(static application =>
                                                     application.ClientSecret));
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await this.manager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.manager.CreateAsync(model, new CreateOperationMapper(ClientApplicationType.NonInteractive))
                            .ConfigureAwait(true);

            // ACT.
            model.Name = "UVault (Sample application)";

            await this.manager.UpdateAsync(key, model, new UpdateOperationMapper())
                      .ConfigureAwait(true);

            (await this.manager.GetByKeyAsync(key)
                       .ConfigureAwait(true)).Should()
                                             .BeEquivalentTo(model, static options => options
                                                 .Excluding(static application => application.ClientSecret)
                                                 .Excluding(static application => application.Key));
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await this.manager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(StringKey key, Model model)
    {
        // ACT.
        Func<Task> act = () => this.manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey key = await this
                              .manager.CreateAsync(model,
                                  new CreateOperationMapper(ClientApplicationType.NonInteractive))
                              .ConfigureAwait(true);

        // ACT.
        await this.manager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Rotate client secret raises an exception when the key is NOT found.")]
    internal async Task RotateClientSecret_UnknownKey_RaisesException(StringKey key)
    {
        // ACT.
        Func<Task<Model>> act = () => this.manager.RotateClientSecretAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Rotate client secret succeeds.")]
    internal async Task RotateClientSecret_Succeeds(Model model)
    {
        // ARRANGE.
        StringKey? key = null;

        try
        {
            key = await this.manager.CreateAsync(model, new CreateOperationMapper(ClientApplicationType.NonInteractive))
                            .ConfigureAwait(true);

            Model initialApplication = await this.manager.GetByKeyAsync(key)
                                                 .ConfigureAwait(true);

            // ACT.
            await this.manager.RotateClientSecretAsync(key)
                      .ConfigureAwait(true);

            // ASSERT.
            Model application = await this.manager.GetByKeyAsync(key)
                                          .ConfigureAwait(true);

            initialApplication.ClientSecret.Should()
                              .NotMatch(application.ClientSecret);
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await this.manager.DeleteByKeyAsync(key)
                          .ConfigureAwait(true);
            }
        }
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.AUTH0_TOKEN_ENDPOINT), Environment.AUTH0_CLIENT_ID,
            Environment.AUTH0_CLIENT_SECRET, Environment.AUTH0_AUDIENCE);
    }

    internal sealed class Model : ApplicationModel
    {
        public Model(StringKey name)
            : base(name)
        {
            this.Name = name.Value;
        }
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(Client client)
        {
            return new Model(new StringKey(client.Name)) { Name = client.Name, ClientSecret = client.ClientSecret };
        }
    }

    private sealed class OperationFilter(ClientApplicationType applicationType) : Auth0ApplicationFilter
    {
        protected override GetClientsRequest Map()
        {
            return new GetClientsRequest { AppType = [applicationType] };
        }
    }

    private sealed class CreateOperationMapper(ClientApplicationType applicationType)
        : Auth0ApplicationCreateOperationMapper
    {
        protected override ClientCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ClientCreateRequest { Name = model.Key.Value, ApplicationType = applicationType };
            }

            throw new CreateException(
                $"Invalid {nameof(IApplicationOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    private sealed class UpdateOperationMapper : Auth0ApplicationUpdateOperationMapper
    {
        protected override ClientUpdateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ClientUpdateRequest { Name = model.Name };
            }

            throw new UpdateException(
                $"Invalid {nameof(IApplicationOperationMapper)}: Source is NOT `{nameof(Model)}`.");
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
