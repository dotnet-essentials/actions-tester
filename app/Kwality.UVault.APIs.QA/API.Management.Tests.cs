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
namespace Kwality.UVault.APIs.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.APIs.Extensions;
using Kwality.UVault.APIs.Managers;
using Kwality.UVault.APIs.Models;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.APIs.QA.Internal.Factories;
using Kwality.UVault.APIs.Stores.Abstractions;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class ApiManagementTests
{
    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefault_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>());

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManager_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(
            static options => { options.UseManager<Manager>(); },
                ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(
            static options => { options.UseManager<Manager>(); },
                ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(
            static options => { options.UseManager<Manager>(); },
                ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStore_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store>(ServiceLifetime.Singleton);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store>(ServiceLifetime.Scoped);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store>(ServiceLifetime.Transient);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApiManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(ApiManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When NO manager configured, it can be resolved.")]
    internal void ResolveDefaultManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>());

        // ACT.
        Func<ApiManager<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                            .GetRequiredService<ApiManager<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveCustomManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager>();
        }));

        // ACT.
        Func<Manager> act = () => services.BuildServiceProvider()
                                          .GetRequiredService<Manager>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveCustomManagerWithStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager>();
            options.UseStore<Store>();
        }));

        // ACT.
        Func<Manager> act = () => services.BuildServiceProvider()
                                          .GetRequiredService<Manager>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

#pragma warning disable CA1812
    private sealed class Manager(IApiStore<Model, IntKey> store) : ApiManager<Model, IntKey>(store);
#pragma warning restore CA1812
#pragma warning disable CA1812
    private sealed class Store : IApiStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly Dictionary<IntKey, Model> collection = new();

        public Task<Model> GetByKeyAsync(IntKey key)
        {
            if (!this.collection.TryGetValue(key, out Model? value))
            {
                throw new ReadException($"Custom: Failed to read API: `{key}`. Not found.");
            }

            return Task.FromResult(value);
        }

        public Task<IntKey> CreateAsync(Model model, IApiOperationMapper mapper)
        {
            this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

            return Task.FromResult(model.Key);
        }

        public Task DeleteByKeyAsync(IntKey key)
        {
            this.collection.Remove(key);

            return Task.CompletedTask;
        }
    }

#pragma warning disable CA1812
    internal sealed class Model(IntKey name) : ApiModel<IntKey>(name);
#pragma warning restore CA1812

    private sealed class CreateOperationMapper : IApiOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IApiOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new TypeRelay(typeof(IServiceCollection), typeof(ServiceCollection)));

        return fixture;
    });
}
