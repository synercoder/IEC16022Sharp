using System;
using System.Linq;
using Xunit;

namespace IEC16022Sharp.Tests
{
    public class ReedSolTests
    {
        [Fact]
        public void RsEncode_Produces_CorrectOuput()
        {
            // Arrange 
            byte[] inData = new byte[] { 142, 164, 186 };
            byte[] okData = new byte[] { 102, 88, 5, 25, 114 };
            var rs = new ReedSol();

            // Act
            rs.RsInitGf(0x12d);
            rs.RsInitCode(5, 1);
            rs.RsEncode(3, inData, out byte[] outData);

            // Assert
            Assert.Equal(okData.Length, outData.Length);
            Assert.True(okData.SequenceEqual(outData));
        }
    }

}
