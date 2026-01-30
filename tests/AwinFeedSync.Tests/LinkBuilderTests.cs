using AwinFeedSync.Core.Models;
using AwinFeedSync.Infrastructure.Configuration;
using AwinFeedSync.Infrastructure.LinkBuilders;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Tests;

public class LinkBuilderTests
{
    [Fact]
    public async Task ComputedLinkBuilder_GeneratesCorrectTrackingUrl()
    {
        var config = Options.Create(new AwinPublisherConfig
        {
            PublisherId = "12345"
        });
        var builder = new ComputedLinkBuilder(config, NullLogger<ComputedLinkBuilder>.Instance);
        
        var trackingUrl = await builder.BuildTrackingLinkAsync(
            advertiserId: 67890,
            destinationUrl: "https://example.com/product",
            clickRef: null
        );
        
        trackingUrl.Should().Contain("awin1.com/cread.php");
        trackingUrl.Should().Contain("awinmid=67890");
        trackingUrl.Should().Contain("awinaffid=12345");
        trackingUrl.Should().Contain("ued=https%3A%2F%2Fexample.com%2Fproduct");
    }
    
    [Fact]
    public async Task ComputedLinkBuilder_IncludesClickRef_WhenProvided()
    {
        var config = Options.Create(new AwinPublisherConfig { PublisherId = "12345" });
        var builder = new ComputedLinkBuilder(config, NullLogger<ComputedLinkBuilder>.Instance);
        
        var trackingUrl = await builder.BuildTrackingLinkAsync(
            advertiserId: 67890,
            destinationUrl: "https://example.com/product",
            clickRef: "test-ref-123"
        );
        
        trackingUrl.Should().Contain("clickref=test-ref-123");
    }
    
    [Fact]
    public void ComputedLinkBuilder_ReturnsCorrectSource()
    {
        var config = Options.Create(new AwinPublisherConfig { PublisherId = "12345" });
        var builder = new ComputedLinkBuilder(config, NullLogger<ComputedLinkBuilder>.Instance);
        
        builder.GetSource().Should().Be("computed");
    }
    
    [Fact]
    public async Task ComputedLinkBuilder_BatchGeneration_ReturnsAllLinks()
    {
        var config = Options.Create(new AwinPublisherConfig { PublisherId = "12345" });
        var builder = new ComputedLinkBuilder(config, NullLogger<ComputedLinkBuilder>.Instance);
        
        var urls = new List<string>
        {
            "https://example.com/product1",
            "https://example.com/product2",
            "https://example.com/product3"
        };
        
        var results = await builder.BuildTrackingLinksBatchAsync(67890, urls);
        
        results.Should().HaveCount(3);
        results.Keys.Should().BeEquivalentTo(urls);
        results.Values.Should().AllSatisfy(url => url.Should().Contain("awin1.com/cread.php"));
    }
}
