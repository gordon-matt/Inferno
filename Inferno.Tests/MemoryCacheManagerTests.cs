using Xunit;
using Inferno.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Tests;

public class MemoryCacheManagerTests
{
    private static MemoryCacheManager Create()
    {
        var services = new ServiceCollection().AddMemoryCache().BuildServiceProvider();
        return new MemoryCacheManager(services);
    }

    [Fact]
    public void RemoveByPattern_MatchingKeys_RemovesOnlyThose()
    {
        using var cache = Create();
        cache.Set("Repository_MenuItem_1_False", new object(), 10);
        cache.Set("Repository_MenuItem_2_True", new object(), 10);
        cache.Set("Repository_Other_1", new object(), 10);

        cache.RemoveByPattern("Repository_MenuItem_.*");

        Assert.False(cache.IsSet("Repository_MenuItem_1_False"));
        Assert.False(cache.IsSet("Repository_MenuItem_2_True"));
        Assert.True(cache.IsSet("Repository_Other_1"));
    }

    [Fact]
    public void Set_SameKeyTwice_StillRemovedByPattern()
    {
        using var cache = Create();
        cache.Set("Repository_A_1", "first", 10);
        cache.Set("Repository_A_1", "second", 10);

        cache.RemoveByPattern("Repository_A_.*");

        Assert.False(cache.IsSet("Repository_A_1"));
    }

    [Fact]
    public void Clear_RemovesEverything()
    {
        using var cache = Create();
        cache.Set("a", new object(), 10);
        cache.Set("b", new object(), 10);

        cache.Clear();

        Assert.False(cache.IsSet("a"));
        Assert.False(cache.IsSet("b"));
    }
}
