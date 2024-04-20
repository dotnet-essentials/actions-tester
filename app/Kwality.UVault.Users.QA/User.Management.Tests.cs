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
namespace Kwality.UVault.Users.QA;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.Xunit.Traits;
using Kwality.UVault.Users.Extensions;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.QA.Internal.Factories;
using Kwality.UVault.Users.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

public sealed class UserManagementTests
{
    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefault_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>());

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManager_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseUserManagement<Model, IntKey>(
                static options => { options.UseManager<Manager<Model, IntKey>>(); }, ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseUserManagement<Model, IntKey>(
                static options => { options.UseManager<Manager<Model, IntKey>>(); }, ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, the services are added.")]
    internal void UseCustomManagerAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseUserManagement<Model, IntKey>(
                static options => { options.UseManager<Manager<Model, IntKey>>(); }, ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Manager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStore_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>(ServiceLifetime.Singleton);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>(ServiceLifetime.Scoped);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
        {
            options.UseStore<Store<Model, IntKey>>(ServiceLifetime.Transient);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom data store is configured, the services are added.")]
    internal void UseCustomDataStore_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Model>(static options =>
        {
            options.UseDataStore<DataStore<Model, IntKey>, Model>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(DataStore<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom data store is configured, the services are added.")]
    internal void UseCustomDataStoreAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Model>(static options =>
        {
            options.UseDataStore<DataStore<Model, IntKey>, Model>(ServiceLifetime.Singleton);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(DataStore<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom data store is configured, the services are added.")]
    internal void UseCustomDataStoreAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Model>(static options =>
        {
            options.UseDataStore<DataStore<Model, IntKey>, Model>(ServiceLifetime.Scoped);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(DataStore<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom data store is configured, the services are added.")]
    internal void UseCustomDataStoreAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
        {
            options.UseDataStore<DataStore<Model, IntKey>, Model>(ServiceLifetime.Transient);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(UserManager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(UserManager<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(DataStore<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When NO manager configured, it can be resolved.")]
    internal void ResolveDefaultManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>());

        // ACT.
        Func<UserManager<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                             .GetRequiredService<UserManager<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveCustomManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
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
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveCustomManagerWithStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey>(static options =>
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
    [UserManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns NO users when NO users are found.")]
    internal async Task GetByEmail_UnknownEmail_ReturnsEmptyCollection(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        IEnumerable<Model> result = await manager.GetByEmailAsync("email")
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEmpty();
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_SingleMatch_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        foreach (Model model in models)
        {
            await manager.CreateAsync(model, new CreateOperationMapper())
                         .ConfigureAwait(true);
        }

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(new[] { expected });
    }

    [FixedEmail]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_MultipleMatches_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        foreach (Model model in models)
        {
            await manager.CreateAsync(model, new CreateOperationMapper())
                         .ConfigureAwait(true);
        }

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(models);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create raises an exception when another user with the same key already exist.")]
    internal async Task Create_KeyExists_RaisesException(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        Func<Task<IntKey>> act = () => manager.CreateAsync(model, new CreateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<CreateException>()
                 .WithMessage($"Custom: Failed to create user: `{model.Key}`. Duplicate key.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        model.Email = "kwality.uvault@github.com";

        await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

#pragma warning disable CA1812
    private sealed class Store<TModel, TKey> : IUserStore<TModel, TKey>
#pragma warning restore CA1812
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
    {
        public Task<TModel> GetByKeyAsync(TKey key)
        {
            throw new NotSupportedException();
        }

        public Task<IEnumerable<TModel>> GetByEmailAsync(string email)
        {
            throw new NotSupportedException();
        }

        public Task<TKey> CreateAsync(TModel model, IUserOperationMapper mapper)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(TKey key, TModel model, IUserOperationMapper mapper)
        {
            throw new NotSupportedException();
        }

        public Task DeleteByKeyAsync(TKey key)
        {
            throw new NotSupportedException();
        }
    }

#pragma warning disable CA1812
    private sealed class DataStore<TData, TKey> : IUserDataStore<TData, TKey>
        where TData : class
        where TKey : IEquatable<TKey>
#pragma warning restore CA1812
    {
        public Task CreateAsync(TKey key, TData data)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(TKey key, TData data)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(TKey key)
        {
            throw new NotSupportedException();
        }
    }

#pragma warning disable CA1812
    private sealed class Manager<TModel, TKey>(IUserStore<TModel, TKey> store) : UserManager<TModel, TKey>(store)
#pragma warning restore CA1812
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>;

#pragma warning disable CA1812
    private sealed class ManagerStore<TModel, TKey>(IUserStore<TModel, TKey> store) : UserManager<TModel, TKey>(store)
#pragma warning restore CA1812
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>;

    internal sealed class Model(IntKey key, string email) : UserModel<IntKey>(key, email);

    private sealed class CreateOperationMapper : IUserOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IUserOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    private sealed class UpdateOperationMapper : IUserOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException(
                    $"Invalid {nameof(IUserOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

#pragma warning disable CA1812
    internal sealed class Store : IUserStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly Dictionary<IntKey, Model> collection = new();

        public Task<Model> GetByKeyAsync(IntKey key)
        {
            if (!this.collection.TryGetValue(key, out Model? value))
            {
                throw new ReadException($"Custom: Failed to read user: `{key}`. Not found.");
            }

            return Task.FromResult(value);
        }

        public Task<IEnumerable<Model>> GetByEmailAsync(string email)
        {
            return Task.FromResult(this
                                   .collection.Where(user => user.Value.Email.Equals(email, StringComparison.Ordinal))
                                   .Select(static user => user.Value));
        }

        public Task<IntKey> CreateAsync(Model model, IUserOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(model.Key))
            {
                this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

                return Task.FromResult(model.Key);
            }

            throw new CreateException($"Custom: Failed to create user: `{model.Key}`. Duplicate key.");
        }

        public async Task UpdateAsync(IntKey key, Model model, IUserOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new UpdateException($"Custom: Failed to update user: `{key}`. Not found.");
            }

            this.collection.Remove(key);

            await this.CreateAsync(model, mapper)
                      .ConfigureAwait(false);
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

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class FixedEmailAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        var email = $"{fixture.Create<string>()}@acme.com";
        fixture.Customizations.Add(new FixedEmailSpecimenBuilder(email));

        return fixture;
    })
    {
        private sealed class FixedEmailSpecimenBuilder(string email) : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(Model))
                {
                    return new Model(context.Create<IntKey>(), email);
                }

                return new NoSpecimen();
            }
        }
    }
}
