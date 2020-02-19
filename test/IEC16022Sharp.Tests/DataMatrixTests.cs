using Xunit;

namespace IEC16022Sharp.Tests
{
    public class DataMatrixTests
    {
        [Fact]
        public void LargeSize_Generates_Error()
        {
            // Arrange
            var largeString = new string('x', 4000);

            // Act
            // Assert
            Assert.Throws<DataMatrixException>(() =>
            {
                var dm = new DataMatrix(largeString);
            });
        }
    }
}
