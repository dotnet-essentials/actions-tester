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
namespace Kwality.UVault.M2M.Options;

using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

public sealed class ApplicationTokenManagementOptions<TToken>
    where TToken : TokenModel
{
    internal ApplicationTokenManagementOptions(IServiceCollection serviceCollection)
    {
        this.ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }

    public void UseStore<TStore>()
        where TStore : class, IApplicationTokenStore<TToken>
    {
        this.ServiceCollection.AddScoped<IApplicationTokenStore<TToken>, TStore>();
    }

    public void UseStore<TStore>(ServiceLifetime serviceLifetime)
        where TStore : class, IApplicationTokenStore<TToken>
    {
        this.ServiceCollection.Add(new ServiceDescriptor(typeof(IApplicationTokenStore<TToken>), typeof(TStore),
            serviceLifetime));
    }
}
