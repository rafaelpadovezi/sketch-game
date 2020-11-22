using Sketch.Business;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit
{
    public class GameTimerTests
    {
        [Fact]
        public async Task ShouldBeDoneAfterEndOfTimer()
        {
            var sut = new GameTimer(() => Task.Delay(10), 10);
            await Task.Delay(1000);

            Assert.True(sut.Done);
        }

        [Fact]
        public void ShouldBeNotDoneBeforeEndOfTimer()
        {
            var sut = new GameTimer(() => Task.Delay(1000), 1000);

            Assert.False(sut.Done);
        }

        [Fact]
        public void ShouldBeDoneAfterStop()
        {
            var sut = new GameTimer(() => Task.Delay(1000), 1000);
            sut.Stop();

            Assert.True(sut.Done);
        }
    }
}
