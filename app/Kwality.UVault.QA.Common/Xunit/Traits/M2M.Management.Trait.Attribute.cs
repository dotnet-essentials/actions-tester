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
namespace Kwality.UVault.QA.Common.Xunit.Traits;

using global::System.Diagnostics.CodeAnalysis;

using global::Xunit.Abstractions;
using global::Xunit.Sdk;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[TraitDiscoverer("Kwality.UVault.QA.Common.Xunit.Traits.M2MManagementFeatureDiscoverer", "Kwality.UVault.QA.Common")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class M2MManagementAttribute : Attribute, ITraitAttribute
{
    // NOTE: Intentionally left blank.
}


[SuppressMessage("ReSharper", "InconsistentNaming")]
#pragma warning disable CA1812
internal sealed class M2MManagementFeatureDiscoverer : ITraitDiscoverer
#pragma warning restore CA1812
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        yield return new KeyValuePair<string, string>("Feature", "M2M (Machine 2 Machine)");
    }
}
