
using Microsoft.Xna.Framework;
using Tankontroller.Utilities;

namespace TankontrollerTests
{
    public class ConvertUtilitiesTests
    {
        [Fact]
        public void ToRectangle_WithFourCorners_ReturnsCorrectAABB()
        {
            var topLeft = new Vector2(0f, 0f);
            var topRight = new Vector2(2f, 0f);
            var bottomLeft = new Vector2(0f, 3f);
            var bottomRight = new Vector2(2f, 3f);

            var rect = ConvertUtilities.ToRectangle(topLeft, topRight, bottomLeft, bottomRight);

            Assert.Equal(0, rect.Left);
            Assert.Equal(0, rect.Top);
            Assert.Equal(2, rect.Width);
            Assert.Equal(3, rect.Height);
        }

        [Fact]
        public void ToRectangle_ArrayOverload_ThrowsWhenNotFour()
        {
            var corners = new Vector2[3] { Vector2.Zero, Vector2.UnitX, Vector2.UnitY };
            Assert.Throws<ArgumentException>(() => ConvertUtilities.ToRectangle(corners));
        }
    }
}
