﻿// =====================================================================================================================
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
namespace Kwality.UVault.APIs.QA.Mappers;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Operations.Mappers;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

public sealed class ApiCreateOperationMapperTests
{
    private readonly CreateOperationMapper mapper = new();

    [ApiManagement]
    [AutoData]
    [Theory(DisplayName = "Map to an invalid destination raises an exception.")]
    internal void Map_InvalidDestination_RaisesException(ModelOne model)
    {
        // ACT.
        Action act = () => this.mapper.Create<ModelOne, ModelTwo>(model);

        // ASSERT.
        act.Should()
           .Throw<CreateException>()
           .WithMessage($"Invalid {nameof(IApiOperationMapper)}: Destination is NOT `{nameof(ModelOne)}`.");
    }

    [ApiManagement]
    [AutoData]
    [Theory(DisplayName = "Map succeeds.")]
    internal void Map_Succeeds(ModelOne model)
    {
        // ACT.
        ModelOne result = this.mapper.Create<ModelOne, ModelOne>(model);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(model);
    }

#pragma warning disable CA1812
    internal sealed class ModelOne
#pragma warning restore CA1812
    {
        [UsedImplicitly] public string? Name { get; set; }
    }

#pragma warning disable CA1812
    internal sealed class ModelTwo
#pragma warning restore CA1812
    {
        [UsedImplicitly] public string? Name { get; set; }
    }
}
