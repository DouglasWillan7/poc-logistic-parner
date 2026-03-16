using FluentAssertions;
using LogisticsPartnerHub.Infrastructure.BackgroundJobs;

namespace LogisticsPartnerHub.Tests.BackgroundJobs;

public class InMemoryRetryQueueTests
{
    private readonly InMemoryRetryQueue _sut = new();

    [Fact]
    public async Task EnqueueAndDequeue_ShouldReturnItem()
    {
        var id = Guid.NewGuid();
        await _sut.EnqueueAsync(id);

        var item = await _sut.DequeueAsync();

        item.Should().NotBeNull();
        item!.ServiceOrderId.Should().Be(id);
    }

    [Fact]
    public async Task Dequeue_ShouldReturnNull_WhenEmpty()
    {
        var item = await _sut.DequeueAsync();
        item.Should().BeNull();
    }

    [Fact]
    public async Task GetCount_ShouldReturnCorrectCount()
    {
        await _sut.EnqueueAsync(Guid.NewGuid());
        await _sut.EnqueueAsync(Guid.NewGuid());

        var count = await _sut.GetCountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task Queue_ShouldBeFIFO()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        await _sut.EnqueueAsync(id1);
        await _sut.EnqueueAsync(id2);

        var first = await _sut.DequeueAsync();
        var second = await _sut.DequeueAsync();

        first!.ServiceOrderId.Should().Be(id1);
        second!.ServiceOrderId.Should().Be(id2);
    }
}
