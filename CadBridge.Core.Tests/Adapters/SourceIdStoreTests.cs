using System.Security.Cryptography;
using System.Text;
using CadBridge.Revit.Adapters;

namespace CadBridge.Core.Tests.Adapters;

public sealed class SourceIdStoreTests
{
    [Fact]
    public void ToGuid_ParsesCanonicalGuid()
    {
        const string sourceId = "5F7C4B3E-2D1A-4F8E-9A6C-7B5D3E1F2A4C";
        Assert.Equal(Guid.Parse(sourceId), SourceIdStore.ToGuid(sourceId));
    }

    [Fact]
    public void ToGuid_AcceptsBracedAndCompactForms()
    {
        var expected = Guid.Parse("5F7C4B3E-2D1A-4F8E-9A6C-7B5D3E1F2A4C");
        Assert.Equal(expected, SourceIdStore.ToGuid("{5F7C4B3E-2D1A-4F8E-9A6C-7B5D3E1F2A4C}"));
        Assert.Equal(expected, SourceIdStore.ToGuid("5f7c4b3e2d1a4f8e9a6c7b5d3e1f2a4c"));
    }

    [Fact]
    public void ToGuid_GuidInput_IsParsedNotHashed()
    {
        const string sourceId = "5F7C4B3E-2D1A-4F8E-9A6C-7B5D3E1F2A4C";
        Assert.NotEqual(Md5Guid(sourceId), SourceIdStore.ToGuid(sourceId));
    }

    [Fact]
    public void ToGuid_NonGuidInput_IsMd5Hashed()
    {
        Assert.Equal(Md5Guid("room-101"), SourceIdStore.ToGuid("room-101"));
    }

    [Fact]
    public void ToGuid_NonGuidInput_IsDeterministic()
    {
        Assert.Equal(SourceIdStore.ToGuid("room-101"), SourceIdStore.ToGuid("room-101"));
    }

    [Fact]
    public void ToGuid_DifferentNonGuidInputs_Differ()
    {
        Assert.NotEqual(SourceIdStore.ToGuid("room-101"), SourceIdStore.ToGuid("room-102"));
    }

    [Fact]
    public void ToGuid_EmptyInput_IsMd5Hashed()
    {
        Assert.Equal(Md5Guid(string.Empty), SourceIdStore.ToGuid(string.Empty));
    }

    [Fact]
    public void SchemaContract_IsStable_ForExistingRevitModels()
    {
        // ломка этих значений = потеря связи с уже записанными помещениями
        Assert.Equal(Guid.Parse("5F7C4B3E-2D1A-4F8E-9A6C-7B5D3E1F2A4C"), SourceIdStore.SchemaId);
        Assert.Equal("SourceId", SourceIdStore.FieldName);
        Assert.Equal("CadArea", SourceIdStore.FieldNameCadArea);
    }

    private static Guid Md5Guid(string value)
    {
        using var md5 = MD5.Create();
        return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }
}
