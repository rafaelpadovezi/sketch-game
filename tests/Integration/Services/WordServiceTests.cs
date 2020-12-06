using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Sketch.Models;
using Sketch.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tests.Support;
using Xunit;

namespace Tests.Integration.Services
{
    public class WordServiceTests : TestingCaseFixture<TestingStartUp>
    {
        [Fact]
        public async Task ShouldGetRandomWord()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SketchDbContext>();
            await context.AddRangeAsync(
                new Word { Content = "hyena", GameRoomType = GameRoomType.Animals },
                new Word { Content = "seal", GameRoomType = GameRoomType.Animals },
                new Word { Content = "penguin", GameRoomType = GameRoomType.Animals },
                new Word { Content = "spring", GameRoomType = GameRoomType.General });
            await context.SaveChangesAsync();
            var sut = new WordService(context);

            var word = await sut.PickWord(GameRoomType.Animals);

            Assert.NotNull(word);
            Assert.Equal(GameRoomType.Animals, word.GameRoomType);
        }
    }
}
