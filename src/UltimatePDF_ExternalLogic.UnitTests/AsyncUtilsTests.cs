using System;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class AsyncUtilsTests {

        [Fact]
        public void StartAndWait_Void_CompletesSuccessfully() {
            // Arrange
            var ran = false;

            // Act
            AsyncUtils.StartAndWait(() => { ran = true; return Task.CompletedTask; });

            // Assert
            Assert.True(ran);
        }

        [Fact]
        public void StartAndWait_Void_PropagatesException() {
            // Arrange
            static Task Throw() => Task.FromException(new InvalidOperationException("boom"));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => AsyncUtils.StartAndWait(Throw));
        }
    }
}
