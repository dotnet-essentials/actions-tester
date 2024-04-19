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

using JetBrains.Annotations;

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
    [Theory(DisplayName = "When a custom manager is configured, it's registered.")]
    internal void UseManager_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager (with a custom store) is configured, it's registered.")]
    internal void UseManagerStore_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey>>();
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ManagerStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager<Model, IntKey>>();
        }));

        // ACT.
        Func<Manager<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                         .GetRequiredService<Manager<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveManagerStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApiManagement<Model, IntKey>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey>>();
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ACT.
        Func<ManagerStore<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                              .GetRequiredService<ManagerStore<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApiManagement<Model, IntKey>(
                static options => options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApiManagement<Model, IntKey>(static options => options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApiManagement<Model, IntKey>(
                static options => options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
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
    private sealed class Store<TModel, TKey> : IApiStore<TModel, TKey>
#pragma warning restore CA1812
        where TModel : ApiModel<TKey>
        where TKey : IEquatable<TKey>
    {
        public Task<TModel> GetByKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<TKey> CreateAsync(TModel model, IApiOperationMapper mapper)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }
    }

#pragma warning disable CA1812
    private sealed class Manager<TModel, TKey>(IApiStore<TModel, TKey> store) : ApiManager<TModel, TKey>(store)
#pragma warning restore CA1812
        where TModel : ApiModel<TKey>
        where TKey : IEquatable<TKey>;

#pragma warning disable CA1812
    private sealed class ManagerStore<TModel, TKey> : ApiManager<TModel, TKey>
#pragma warning restore CA1812
        where TModel : ApiModel<TKey>
        where TKey : IEquatable<TKey>
    {
        public ManagerStore(IApiStore<TModel, TKey> store)
            : base(store)
        {
            if (store is not Store<TModel, TKey>)
            {
                throw new InvalidOperationException("The provided store isn't valid for this manager.");
            }
        }
    }

    [UsedImplicitly]
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

    [UsedImplicitly]
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

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new TypeRelay(typeof(IServiceCollection), typeof(ServiceCollection)));

        return fixture;
    });
}
