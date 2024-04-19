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
namespace Kwality.UVault.Grants.Auth0.QA.Filters;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Grants.Auth0.Operations.Filters;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class Auth0GrantFilterTests
{
    private readonly GrantFilter filter = new();

    [GrantManagement]
    [Fact(DisplayName = "Map to an invalid destination raises an exception.")]
    internal void Map_InvalidDestination_RaisesException()
    {
        // ACT.
        Action act = () => this.filter.Create<Expression<Func<string, bool>>>();

        // ASSERT.
        act.Should()
           .Throw<CreateException>()
           .WithMessage($"Invalid {nameof(IGrantFilter)}: Destination is NOT `{nameof(GetClientGrantsRequest)}`.");
    }

    [GrantManagement]
    [Fact(DisplayName = "Map succeeds.")]
    internal void Map_Succeeds()
    {
        // ACT.
        var result = this.filter.Create<GetClientGrantsRequest>();

        // ASSERT.
        result.Should()
              .BeEquivalentTo(new GetClientGrantsRequest());
    }

    private sealed class GrantFilter : Auth0GrantFilter
    {
        protected override GetClientGrantsRequest Map()
        {
            return new GetClientGrantsRequest();
        }
    }
}
