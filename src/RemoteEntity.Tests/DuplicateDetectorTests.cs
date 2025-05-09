using NUnit.Framework;

namespace RemoteEntity.Tests;

[TestFixture]
public class DuplicateDetectorTests
{
    private class PocoDto
    {
        public string StringField { get; set; }
        public int IntField { get; set; }
    }

    private DuplicateDetector duplicateDetector = new();
    
    [Test]
    public void ShouldReturnFalseOnFirstSighting()
    {
        var dto1 = new PocoDto
        {
            StringField = "a",
            IntField = 2
        };
        Assert.That(duplicateDetector.IsDuplicate(dto1, "dto1"), Is.False);
    }
    
    [Test]
    public void ShouldReturnTrueOnSecondSighting()
    {
        var dto1 = new PocoDto
        {
            StringField = "a",
            IntField = 2
        };
        var dto2 = new PocoDto
        {
            StringField = "a",
            IntField = 2
        };
        var first = duplicateDetector.IsDuplicate(dto1, "dto1");
        var second = duplicateDetector.IsDuplicate(dto2, "dto1");
        Assert.That(second, Is.True);
    }
    
    [Test]
    public void ShouldReturnFalseOnChangedDto()
    {
        var dto1 = new PocoDto
        {
            StringField = "a",
            IntField = 2
        };
        var dto2 = new PocoDto
        {
            StringField = "a",
            IntField = 5
        };
        var first = duplicateDetector.IsDuplicate(dto1, "dto1");
        var second = duplicateDetector.IsDuplicate(dto2, "dto1");
        Assert.That(second, Is.False);
    }
    
}