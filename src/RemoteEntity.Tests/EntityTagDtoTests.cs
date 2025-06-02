using System.Threading.Tasks;
using NUnit.Framework;
using RemoteEntity.Tags;

namespace RemoteEntity.Tests;

[TestFixture]
public class EntityTagDtoTests
{
    [Test]
    public async Task ShouldMutateToDtoAndBack()
    {
        var tag = EntityTag.Create("some_tag", 45.55m);
        var asDto = EntityTagDto.FromEntityTag(tag);
        
        var tagFromDto = asDto.ToEntityTag();
        
        Assert.That(tagFromDto.TagName, Is.EqualTo(tag.TagName));
        Assert.That(tagFromDto.GetValueType(), Is.EqualTo(tag.GetValueType()));
        Assert.That(tagFromDto.GetTagValue<decimal>(), Is.EqualTo(tag.GetTagValue<decimal>()));
    }
    
    
}