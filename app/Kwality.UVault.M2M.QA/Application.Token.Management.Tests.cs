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

using JetBrains.Annotations;

using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Keys;
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
    private readonly ApplicationTokenManager<Model> manager
        = new ApplicationTokenManagerFactory().Create<Model, ApplicationModel<IntKey>, IntKey>(static options =>
            options.UseStore<Store>());

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApplicationTokenManagement<Model, ApplicationModel<IntKey>, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApplicationTokenManagement<Model, ApplicationModel<IntKey>, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MTokenManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApplicationTokenManagement<Model, ApplicationModel<IntKey>, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApplicationTokenStore<Model>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
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

    [UsedImplicitly]
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
