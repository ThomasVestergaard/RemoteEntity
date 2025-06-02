using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RemoteEntity.Tags;

namespace RemoteEntity.Tests;

[TestFixture]
public class EntityTagTests
{
    [Test]
    public async Task ShouldReturnBoolValue()
    {
        var tag = EntityTag.Create("bool_tag", true);
        var tag_value = tag.GetTagValue<bool>();
        Assert.That(tag_value, Is.True);
    } 
    
    [Test]
    public void ShouldThrowIfWrongTypeRequested()
    {
        var tag = EntityTag.Create("int_tag", 123);
        Assert.Throws<InvalidCastException>(() => tag.GetTagValue<string>());
    }

    [Test]
    public void ShouldReturnCorrectTypeInfo()
    {
        var tag = EntityTag.Create("double_tag", 3.14);
        Assert.That(tag.GetValueType(), Is.EqualTo(typeof(double)));
    }

    [Test]
    public void ShouldPreserveTagName()
    {
        var tag = EntityTag.Create("my_tag", 42);
        Assert.That(tag.TagName, Is.EqualTo("my_tag"));
    }

    [Test]
    public void ShouldSupportNullableType_whenNull()
    {
        int? value = null;
        var tag = EntityTag.Create("nullable_tag", value);
        var tagValue = tag.GetTagValue<int?>();
        Assert.That(tagValue, Is.Null);
    }

    [Test]
    public void ShouldSupportNullableType_hasValue()
    {
        int? value = 50;
        var tag = EntityTag.Create("nullable_tag", value);
        var tagValue = tag.GetTagValue<int?>();
        Assert.That(tagValue.Value, Is.EqualTo(50));
    }
    
}