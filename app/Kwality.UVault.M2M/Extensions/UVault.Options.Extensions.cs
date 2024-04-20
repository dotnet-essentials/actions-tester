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
namespace Kwality.UVault.M2M.Extensions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Options;
using Kwality.UVault.M2M.Internal.Stores;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Options;
using Kwality.UVault.M2M.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class UVaultOptionsExtensions
{
    public static void UseApplicationManagement<TModel, TKey>(this UVaultOptions options)
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseApplicationManagement<TModel, TKey>(null);
    }

    public static void UseApplicationManagement<TModel, TKey>(this UVaultOptions options, ServiceLifetime storeLifetime)
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseApplicationManagement<TModel, TKey>(null, storeLifetime);
    }

    public static void UseApplicationManagement<TModel, TKey>(
        this UVaultOptions options, Action<ApplicationManagementOptions<TModel, TKey>>? applicationManagementOptions)
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        UseAndConfigureApplicationManagement(options, applicationManagementOptions);
    }

    public static void UseApplicationManagement<TModel, TKey>(
        this UVaultOptions options, Action<ApplicationManagementOptions<TModel, TKey>>? applicationManagementOptions,
        ServiceLifetime storeLifetime)
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        UseAndConfigureApplicationManagement(options, applicationManagementOptions, storeLifetime);
    }

    public static void UseApplicationTokenManagement<TToken, TModel, TKey>(
        this UVaultOptions options,
        Action<ApplicationTokenManagementOptions<TToken>>? applicationTokenManagementOptions)
        where TToken : TokenModel, new()
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        UseAndConfigureApplicationManagement<TModel, TKey>(options, null);
        UseAndConfigureApplicationTokenManagement<TToken, TKey>(options, applicationTokenManagementOptions);
    }

    private static void UseAndConfigureApplicationManagement<TModel, TKey>(
        UVaultOptions options, Action<ApplicationManagementOptions<TModel, TKey>>? applicationManagementOptions,
        ServiceLifetime storeLifetime = ServiceLifetime.Scoped)
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.Services.AddScoped<ApplicationManager<TModel, TKey>>();

        options.Services.Add(new ServiceDescriptor(typeof(IApplicationStore<TModel, TKey>),
            typeof(StaticStore<TModel, TKey>), storeLifetime));

        options.Services.AddScoped<IApplicationStore<TModel, TKey>, StaticStore<TModel, TKey>>();
        applicationManagementOptions?.Invoke(new ApplicationManagementOptions<TModel, TKey>(options.Services));
    }

    private static void UseAndConfigureApplicationTokenManagement<TToken, TKey>(
        UVaultOptions options, Action<ApplicationTokenManagementOptions<TToken>>? applicationTokenManagementOptions)
        where TToken : TokenModel, new()
        where TKey : IEquatable<TKey>
    {
        options.Services.AddScoped<ApplicationTokenManager<TToken>>();
        options.Services.AddScoped<IApplicationTokenStore<TToken>, StaticTokenStore<TToken>>();
        applicationTokenManagementOptions?.Invoke(new ApplicationTokenManagementOptions<TToken>(options.Services));
    }
}
