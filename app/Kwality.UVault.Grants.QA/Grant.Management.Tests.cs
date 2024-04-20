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
namespace Kwality.UVault.Grants.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Extensions;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.QA.Internal.Factories;
using Kwality.UVault.Grants.Stores.Abstractions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class GrantManagementTests
{
    private const int pageSize = 100;

    private readonly GrantManager<Model, IntKey> manager
        = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefault_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>());

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManager_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseGrantManagement<Model, IntKey>(
                static options => { options.UseManager<Manager<Model, IntKey>>(); }, ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseGrantManagement<Model, IntKey>(
                static options => { options.UseManager<Manager<Model, IntKey>>(); }, ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseGrantManagement<Model, IntKey>(
                static options => { options.UseManager<Manager<Model, IntKey>>(); }, ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStore_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>(ServiceLifetime.Singleton);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>(ServiceLifetime.Scoped);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>(ServiceLifetime.Transient);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(GrantManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(GrantManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When NO manager configured, it can be resolved.")]
    internal void ResolveDefaultManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>());

        // ACT.
        Func<GrantManager<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                              .GetRequiredService<GrantManager<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [GrantManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveCustomManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
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
    [GrantManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveCustomManagerWithStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseGrantManagement<Model, IntKey>(static options =>
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

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        await this.manager.CreateAsync(model, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 10)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        await this.manager.CreateAsync(model, new CreateOperationMapper())
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

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new CreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 1)
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
              .BeEquivalentTo(modelOne);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, pageSize: Less than the total amount) succeeds.")]
    internal async Task GetAll_SecondPageWithLessElementsThanTotal_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new CreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(1, 1)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new CreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new CreateOperationMapper())
                  .ConfigureAwait(true);

        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Scopes))
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .ContainEquivalentOf(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ACT.
        await this.manager.CreateAsync(model, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        model.Scopes = new[] { "newScope", "newScope2" };

        await this.manager.UpdateAsync(key, model, new UpdateOperationMapper())
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ACT.
        Func<Task> act = () => this.manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update client grant: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        await this.manager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .BeEmpty();
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ACT.
        Func<Task> act = () => this.manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

#pragma warning disable CA1812
    private sealed class Store<TModel, TKey> : IGrantStore<TModel, TKey>
#pragma warning restore CA1812
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
        {
            throw new NotSupportedException();
        }

        public Task<TKey> CreateAsync(TModel model, IGrantOperationMapper mapper)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(TKey key, TModel model, IGrantOperationMapper mapper)
        {
            throw new NotSupportedException();
        }

        public Task DeleteByKeyAsync(TKey key)
        {
            throw new NotSupportedException();
        }
    }

#pragma warning disable CA1812
    private sealed class Manager<TModel, TKey>(IGrantStore<TModel, TKey> store) : GrantManager<TModel, TKey>(store)
#pragma warning restore CA1812
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>;

#pragma warning disable CA1812
    private sealed class ManagerStore<TModel, TKey>(IGrantStore<TModel, TKey> store) : GrantManager<TModel, TKey>(store)
#pragma warning restore CA1812
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>;

    [UsedImplicitly]
#pragma warning disable CA1812
    internal sealed class Model(IntKey key, IEnumerable<string> scopes) : GrantModel<IntKey>(key)
#pragma warning restore CA1812
    {
        public IEnumerable<string> Scopes { get; set; } = scopes;
    }

    private sealed class OperationFilter(IEnumerable<string> scopes) : IGrantFilter
    {
        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<KeyValuePair<IntKey, Model>, bool>))
            {
                throw new ReadException(
                    $"Invalid {nameof(IGrantFilter)}: Destination is NOT `{typeof(Func<KeyValuePair<IntKey, Model>, bool>).Name}`.");
            }

            return ((Func<KeyValuePair<IntKey, Model>, bool>)Filter)
                .UnsafeAs<Func<KeyValuePair<IntKey, Model>, bool>, TDestination>();

            // The filter which is filters out data in the store.
            bool Filter(KeyValuePair<IntKey, Model> kvp)
            {
                return Equals(kvp.Value.Scopes, scopes);
            }
        }
    }

    private sealed class CreateOperationMapper : IGrantOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IGrantOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    private sealed class UpdateOperationMapper : IGrantOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException(
                    $"Invalid {nameof(IGrantOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    internal sealed class Store : IGrantStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly Dictionary<IntKey, Model> collection = new();

        public Task<PagedResultSet<Model>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
        {
            IQueryable<KeyValuePair<IntKey, Model>> dataSet = this.collection.AsQueryable();

            if (filter != null)
            {
                dataSet = dataSet.AsEnumerable()
                                 .Where(filter.Create<Func<KeyValuePair<IntKey, Model>, bool>>())
                                 .AsQueryable();
            }

            IEnumerable<Model> grants = dataSet.Skip(pageIndex * pageSize)
                                               .Take(pageSize)
                                               .Select(static kvp => kvp.Value);

            var result = new PagedResultSet<Model>(grants, this.collection.Count > (pageIndex + 1) * pageSize);

            return Task.FromResult(result);
        }

        public Task<IntKey> CreateAsync(Model model, IGrantOperationMapper mapper)
        {
            this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

            return Task.FromResult(model.Key);
        }

        public async Task UpdateAsync(IntKey key, Model model, IGrantOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new UpdateException($"Custom: Failed to update client grant: `{key}`. Not found.");
            }

            this.collection.Remove(key);

            await this.CreateAsync(model, mapper)
                      .ConfigureAwait(true);
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
