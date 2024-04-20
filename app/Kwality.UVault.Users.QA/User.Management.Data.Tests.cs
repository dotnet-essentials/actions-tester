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

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.Xunit.Traits;
using Kwality.UVault.Users.Extensions;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

public sealed class UserManagementDataTests
{
    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, it's registered.")]
    internal void UseManager_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
        {
            options.UseManager<Manager<Model, IntKey, Data>, Data>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey, Data>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager (with a custom store) is configured, it's registered.")]
    internal void UseManagerStore_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey, Data>, Data>();
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor =>
                    descriptor.ServiceType == typeof(ManagerStore<Model, IntKey, Data>) &&
                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager (with a custom data store) is configured, it's registered.")]
    internal void UseManagerDataStore_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey, Data>, Data>();
            options.UseStore<Store<Model, IntKey>>();
            options.UseDataStore<DataStore<Data, IntKey>, Data>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor =>
                    descriptor.ServiceType == typeof(ManagerStore<Model, IntKey, Data>) &&
                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Data, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(DataStore<Data, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
        {
            options.UseManager<Manager<Model, IntKey, Data>, Data>();
            options.UseDataStore<DataStore<Data, IntKey>, Data>();
        }));

        // ACT.
        Func<Manager<Model, IntKey, Data>> act = () => services.BuildServiceProvider()
                                                               .GetRequiredService<Manager<Model, IntKey, Data>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveManagerStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey, Data>, Data>();
            options.UseStore<Store<Model, IntKey>>();
            options.UseDataStore<DataStore<Data, IntKey>, Data>();
        }));

        // ACT.
        Func<ManagerStore<Model, IntKey, Data>> act = () => services.BuildServiceProvider()
                                                                    .GetRequiredService<
                                                                        ManagerStore<Model, IntKey, Data>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseUserManagement<Model, IntKey, Data>(static options =>
                options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseUserManagement<Model, IntKey, Data>(static options =>
                options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseUserManagement<Model, IntKey, Data>(static options =>
                options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When the data store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseDataStoreAsSingleton_RegisterStoreAsSingleton(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
            options.UseDataStore<DataStore<Data, IntKey>, Data>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Data, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(DataStore<Data, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When the data store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseDataStoreAsScoped_RegisterStoreAsScoped(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
            options.UseDataStore<DataStore<Data, IntKey>, Data>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Data, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(DataStore<Data, IntKey>));
    }

    [AutoDomainData]
    [UserManagement]
    [Theory(DisplayName = "When the data store is configured as a `Transient` one, it behaves as such.")]
    internal void UseDataStoreAsTransient_RegisterStoreAsTransient(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseUserManagement<Model, IntKey, Data>(static options =>
            options.UseDataStore<DataStore<Data, IntKey>, Data>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserDataStore<Data, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(DataStore<Data, IntKey>));
    }

#pragma warning disable CA1812
    private sealed class Store<TModel, TKey> : IUserStore<TModel, TKey>
#pragma warning restore CA1812
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
    {
        public Task<TModel> GetByKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TModel>> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<TKey> CreateAsync(TModel model, IUserOperationMapper mapper)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TKey key, TModel model, IUserOperationMapper mapper)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }
    }

#pragma warning disable CA1812
    private sealed class DataStore<TData, TKey> : IUserDataStore<TData, TKey>
        where TData : class
        where TKey : IEquatable<TKey>
#pragma warning restore CA1812
    {
        public Task CreateAsync(TKey key, TData data, IUserDataOperationMapper mapper)
        {
            throw new NotImplementedException();
        }
    }

#pragma warning disable CA1812
    private sealed class Manager<TModel, TKey, TData>(
        IUserStore<TModel, TKey> store,
        IUserDataStore<TData, TKey> dataStore) : UserManager<TModel, TKey, TData>(store, dataStore)
#pragma warning restore CA1812
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
        where TData : class;

#pragma warning disable CA1812
    private sealed class ManagerStore<TModel, TKey, TData> : UserManager<TModel, TKey, TData>
#pragma warning restore CA1812
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
        where TData : class
    {
        public ManagerStore(IUserStore<TModel, TKey> store, IUserDataStore<TData, TKey> dataStore)
            : base(store, dataStore)
        {
            if (store is not Store<TModel, TKey>)
            {
                throw new InvalidOperationException("The provided store isn't valid for this manager.");
            }
        }
    }

    [UsedImplicitly]
    internal sealed class Model(IntKey key, string email) : UserModel<IntKey>(key, email);

    [UsedImplicitly]
#pragma warning disable CA1812
    internal sealed class Data;
#pragma warning restore CA1812

    [UsedImplicitly]
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
