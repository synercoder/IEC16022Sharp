using Xunit;

namespace IEC16022Sharp.Tests
{
    public class HexPbmTests
    {
        [Fact]
        public void HexPbm_IsCorrect()
        {
            // Arrange
            int width = 32;
            int height = 8;
            var msg = "44";
            var expectedResult = "AAAAAAAAD99B8EC38676A62A9D07B257C73AD2608A7F8791D8F0\nDC38FFFFFFFF";

            // Act
            var dm = new DataMatrix(msg, width, height);

            // Assert
            Assert.Equal(expectedResult, dm.HexPbm);
        }
    }
}
