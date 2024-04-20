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
namespace Kwality.UVault.M2M.QA;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.Core.Extensions;
using Kwality.UVault.M2M.Extensions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.QA.Internal.Factories;
using Kwality.UVault.M2M.Stores.Abstractions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

public sealed class ApplicationTokenManagementTests
{
    private readonly ApplicationTokenManager<Model> manager = new ApplicationTokenManagerFactory().Create<Model>(
        static options =>
            options.UseStore<Store>());

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefault_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>());

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(ServiceLifetime.Singleton));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(ServiceLifetime.Scoped));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When NO manager is provided, the services are added.")]
    internal void UseDefaultAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(ServiceLifetime.Transient));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStore_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(static options =>
        {
            options.UseStore<Store>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsSingleton_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(static options =>
        {
            options.UseStore<Store>(ServiceLifetime.Singleton);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsScoped_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(static options =>
        {
            options.UseStore<Store>(ServiceLifetime.Scoped);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When a custom store is configured, the services are added.")]
    internal void UseCustomStoreAsTransient_AddsServices(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(static options =>
        {
            options.UseStore<Store>(ServiceLifetime.Transient);
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ApplicationTokenManager<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType ==
                                                    typeof(ApplicationTokenManager<Model>));

        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When NO manager configured, it can be resolved.")]
    internal void ResolveDefaultManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>());

        // ACT.
        Func<ApplicationTokenManager<Model>> act = () => services.BuildServiceProvider()
                                                                 .GetRequiredService<ApplicationTokenManager<Model>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When a custom store is configured, it can be resolved.")]
    internal void ResolveCustomStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationTokenManagement<Model>(static options =>
        {
            options.UseStore<Store>();
        }));

        // ACT.
        Func<IApplicationTokenStore<Model>> act = () => services.BuildServiceProvider()
                                                                .GetRequiredService<IApplicationTokenStore<Model>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoData]
    [M2MTokenManagement]
    [Theory(DisplayName = "Get access token succeeds.")]
    internal async Task GetToken_Succeeds(string clientId, string clientSecret, string audience, string grantType)
    {
        // ACT.
        Model result = await this.manager.GetAccessTokenAsync(clientId, clientSecret, audience, grantType)
                                 .ConfigureAwait(true);

        // ASSERT.
        result.Token.Should()
              .NotBeNullOrWhiteSpace();

        result.Scope.Should()
              .Be("read, write, update, delete");

        result.ExpiresIn.Should()
              .Be(86400);

        result.TokenType.Should()
              .Be("Bearer");
    }

    internal sealed class Model : TokenModel
    {
        public Model()
        {
        }

        public Model(string token, int expiresIn, string tokenType, string scope)
            : base(token, expiresIn, tokenType, scope)
        {
        }
    }

    
#pragma warning disable CA1812
    internal sealed class Store : IApplicationTokenStore<Model>
#pragma warning restore CA1812
    {
        public Task<Model> GetAccessTokenAsync(string clientId, string clientSecret, string audience, string grantType)
        {
            return Task.FromResult(new Model(Guid.NewGuid()
                                                 .ToString(), 86400, "Bearer", "read, write, update, delete"));
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
