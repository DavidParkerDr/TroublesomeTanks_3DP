using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using TTMapEditor.Managers;
using TTMapEditor.Objects;
using TTMapEditor.Saving;
using Xunit;


namespace TTMapEditor.Tests
{
        internal class TestSceneObject : SceneObject
        {
            public TestSceneObject(int x, int y, int w = 10, int h = 10) : base(null, new Rectangle(x, y, w, h))
            {

            }
        }

        public class DraggableTemplateTests
        {
            [Fact]
            public void BeginDrag_sets_offset_an_original_rect_and_flag()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);

                tpl.BeginDrag(new Vector2(110, 60));

                Assert.True(tpl.mIsDragging);
                Assert.Equal(new Vector2(10, 10), tpl.mDragOffset);
                Assert.Equal(new Rectangle(100, 50, 10, 10), tpl.mOriginalRect);
            }

            [Fact]
            public void Update_moves_template_while_dragging()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);
                tpl.BeginDrag(new Vector2(110, 60));
                tpl.Update(new Vector2(150, 100));
                Assert.Equal(new Rectangle(140, 90, 10, 10), obj.mRectangle);
            }

            [Fact]
            public void EndDrag_returns_final_and_resets_when_requested()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);
                tpl.BeginDrag(new Vector2(110, 60));
                tpl.Update(new Vector2(150, 120));
                var final = tpl.EndDrag(true);

                Assert.Equal(new Rectangle(140, 110, 10, 10), final);
                Assert.Equal(new Rectangle(100, 50, 10, 10), obj.mRectangle);
                Assert.False(tpl.mIsDragging);
            }

            [Fact]
            public void Reset_restores_original_and_clears_state()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);
                tpl.BeginDrag(new Vector2(110, 60));
                tpl.Update(new Vector2(150, 120));
                tpl.Reset();

                Assert.Equal(new Rectangle(100, 50, 10, 10), obj.mRectangle);
                Assert.False(tpl.mIsDragging);
                Assert.Equal(Vector2.Zero, tpl.mDragOffset);
            }
        }

        public class SceneObjectTests
        {
            [Fact]
            public void UpdatePosition_updates_rectangle_and_outline()
            {
                var obj = new TestSceneObject(10, 20, 30, 40);
                obj.UpdatePosition(50, 60);

                Assert.Equal(new Rectangle(50, 60, 30, 40), obj.mRectangle);
                // OutlinePad = 2 => outline = rect expanded by 2 on each side
                var outlineField = typeof(SceneObject).GetField("mOutlineRectangle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(outlineField);
                var outline = (Rectangle)outlineField.GetValue(obj);
                Assert.Equal(new Rectangle(48, 58, 34, 44), outline);
            }

            [Fact]
            public void SetRectangle_updates_outline_correctly()
            {
                var obj = new TestSceneObject(0, 0, 10, 10);
                obj.SetRectangle(new Rectangle(5, 5, 2, 2));
                Assert.Equal(new Rectangle(5, 5, 2, 2), obj.mRectangle);
                // expected outline: X-2,Y-2, W+4,H+4 => (3,3,6,6)
                var outlineField = typeof(SceneObject).GetField("mOutlineRectangle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(outlineField);
                var outline = (Rectangle)outlineField.GetValue(obj);
                Assert.Equal(new Rectangle(3, 3, 6, 6), outline);
            }

            [Fact]
            public void IsPointWithin_returns_expected()
            {
                var obj = new TestSceneObject(10, 10, 20, 20);
                Assert.True(obj.IsPointWithin(new Vector2(15, 15)));
                Assert.False(obj.IsPointWithin(new Vector2(100, 100)));
            }

            [Fact]
            public void Selection_helpers_toggle_and_report_state()
            {
                var obj = new TestSceneObject(0, 0, 1, 1);
                Assert.False(obj.GetIsSelected());
                obj.ToggleSelected();
                Assert.True(obj.GetIsSelected());
                obj.SetSelected(false);
                Assert.False(obj.GetIsSelected());
            }
        }

        public class RectWallTests
        {
            [Fact]
            public void ScaleHeight_increases_height_and_ceil_and_minimum_1()
            {
                var wall = new RectWall(null, new Rectangle(0, 0, 10, 10));
                wall.ScaleHeight(1.5f); // 10 * 1.5 = 15
                Assert.Equal(15, wall.mRectangle.Height);

                var small = new RectWall(null, new Rectangle(0, 0, 5, 1));
                small.ScaleHeight(0.0001f); // should never go below 1
                Assert.True(small.mRectangle.Height >= 1);
            }

            [Fact]
            public void ScaleWidth_increases_width_and_ceil_and_minimum_1()
            {
                var wall = new RectWall(null, new Rectangle(0, 0, 10, 10));
                wall.ScaleWidth(1.6f); // 10 * 1.6 = 16 -> ceil 16
                Assert.Equal(16, wall.mRectangle.Width);

                var small = new RectWall(null, new Rectangle(0, 0, 1, 5));
                small.ScaleWidth(0.0001f);
                Assert.True(small.mRectangle.Width >= 1);
            }

        [Fact]
            public void Rotate_adds_delta_to_rotation()
            {
                var wall = new RectWall(null, new Rectangle(0, 0, 10, 10));

                wall.Rotate((float)System.Math.PI / 2f);
                Assert.Equal((float)System.Math.PI / 2f, wall.mRotation);

                wall.Rotate((float)System.Math.PI / 2f);
                Assert.Equal((float)System.Math.PI, wall.mRotation);
        }
    }

        public class TankTests
        {
            [Fact]
            public void Constructor_sets_initial_rotation_and_rectangle()
            {
                var rect = new Rectangle(5, 6, 12, 14);
                var tank = new Tank(null, rect);

                Assert.Equal(0f, tank.mRotation);
                Assert.Equal(rect, tank.mRectangle);
            }

            [Fact]
            public void Rotate_adds_delta_to_rotation()
            {
                var tank = new Tank(null, new Rectangle(0, 0, 10, 10));
                tank.Rotate((float)System.Math.PI / 2f);
                Assert.Equal((float)System.Math.PI / 2f, tank.mRotation);
                tank.Rotate((float)System.Math.PI / 2f);
                Assert.Equal((float)System.Math.PI, tank.mRotation);
            }
        }

        public class PickupTests
        {
            [Fact]
            public void Constructor_sets_rectangle()
            {
                var rect = new Rectangle(1, 2, 3, 4);
                var pick = new Pickup(null, rect);
                Assert.Equal(rect, pick.mRectangle);
            }
        }

    public class FileNamerTests
    {
        [Theory]
        [InlineData(Keys.A, true, 'A')]
        [InlineData(Keys.A, false, 'a')]
        [InlineData(Keys.Z, true, 'Z')]
        [InlineData(Keys.Z, false, 'z')]
        public void KeyToChar_Letters_RespectCapsLock(Keys pKey, bool pCapsLock, char pExpected)
        {
            char? result = InvokeKeyToChar(pKey, pCapsLock);

            Assert.True(result.HasValue);
            Assert.Equal(pExpected, result.Value);
        }

        [Theory]
        [InlineData(Keys.D0, '0')]
        [InlineData(Keys.D5, '5')]
        [InlineData(Keys.D9, '9')]
        public void KeyToChar_TopRowDigits_ReturnDigit(Keys pKey, char pExpected)
        {
            char? result = InvokeKeyToChar(pKey, false);

            Assert.True(result.HasValue);
            Assert.Equal(pExpected, result.Value);
        }

        [Theory]
        [InlineData(Keys.NumPad0, '0')]
        [InlineData(Keys.NumPad3, '3')]
        [InlineData(Keys.NumPad9, '9')]
        public void KeyToChar_NumpadDigits_ReturnDigit(Keys pKey, char pExpected)
        {
            char? result = InvokeKeyToChar(pKey, false);

            Assert.True(result.HasValue);
            Assert.Equal(pExpected, result.Value);
        }

        [Theory]
        [InlineData(Keys.Space)]
        [InlineData(Keys.Back)]
        [InlineData(Keys.Enter)]
        [InlineData(Keys.Left)]
        [InlineData(Keys.Escape)]
        public void KeyToChar_NonTextKeys_ReturnNull(Keys pKey)
        {
            char? result = InvokeKeyToChar(pKey, false);

            Assert.False(result.HasValue);
        }

        [Theory]
        [InlineData(Keys.A, true)]
        [InlineData(Keys.Z, true)]
        [InlineData(Keys.D0, true)]
        [InlineData(Keys.D9, true)]
        [InlineData(Keys.Space, true)]
        [InlineData(Keys.Back, true)]
        [InlineData(Keys.NumPad0, false)] // currently not whitelisted in IsValidKey
        [InlineData(Keys.NumPad9, false)]
        [InlineData(Keys.Left, false)]
        [InlineData(Keys.Right, false)]
        [InlineData(Keys.LeftControl, false)]
        [InlineData(Keys.Escape, false)]
        public void IsValidKey_ReturnsExpected(Keys pKey, bool pExpected)
        {
            bool result = InvokeIsValidKey(pKey);

            Assert.Equal(pExpected, result);
        }

        // Because IsValidKey and KeyToChar are private, we need reflection to call them.
        // If you’re happy to make them internal, you can skip reflection and call them directly.
        private static bool InvokeIsValidKey(Keys pKey)
        {
            var method = typeof(FileNamer).GetMethod(
                "IsValidKey",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (bool)method.Invoke(null, new object[] { pKey })!;
        }

        private static char? InvokeKeyToChar(Keys pKey, bool pCapsLock)
        {
            var method = typeof(FileNamer).GetMethod(
                "KeyToChar",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (char?)method.Invoke(null, new object[] { pKey, pCapsLock });
        }
    }

    public class MapEditingMapServiceTests : IDisposable
    {
        private readonly string mTempRoot;

        public MapEditingMapServiceTests()
        {
            mTempRoot = Path.Combine(Path.GetTempPath(), "MapEditingMapServiceTests_" + Guid.NewGuid());
            Directory.CreateDirectory(mTempRoot);
        }

        [Fact]
        public void ResolveMapPath_EmptyString_UsesRootMapJson()
        {
            var service = new MapEditingMapService(mTempRoot);

            string path = InvokeResolveMapPath(service, string.Empty);

            Assert.Equal(Path.Combine(mTempRoot, "map.json"), path);
            Assert.True(Directory.Exists(mTempRoot));
        }

        [Fact]
        public void ResolveMapPath_BareName_CreatesFolderWithMapJson()
        {
            var service = new MapEditingMapService(mTempRoot);

            string path = InvokeResolveMapPath(service, "New Map");

            string expectedDir = Path.Combine(mTempRoot, "New Map");
            string expectedFile = Path.Combine(expectedDir, "map.json");

            Assert.Equal(Path.GetFullPath(expectedFile), path);
            Assert.True(Directory.Exists(expectedDir));
        }

        [Fact]
        public void ResolveNewMapPath_BareName_ReturnsMapJsonInRootIfNoExtension()
        {
            var service = new MapEditingMapService(mTempRoot);

            string path = InvokeResolveNewMapPath(service, "New Map");

            // Current implementation: if no extension, it treats as directory and appends "map.json"
            string expectedDir = Path.Combine(mTempRoot, "New Map");
            string expectedFile = Path.Combine(expectedDir, "map.json");

            Assert.Equal(Path.GetFullPath(expectedFile), path);
        }

        private static string InvokeResolveMapPath(MapEditingMapService pService, string pValue)
        {
            var method = typeof(MapEditingMapService).GetMethod("ResolveMapPath",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (string)method.Invoke(pService, new object[] { pValue })!;
        }

        private static string InvokeResolveNewMapPath(MapEditingMapService pService, string pValue)
        {
            var method = typeof(MapEditingMapService).GetMethod("ResolveNewMapPath",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (string)method.Invoke(pService, new object[] { pValue })!;
        }

        public void Dispose()
        {
            if (Directory.Exists(mTempRoot))
            {
                try
                {
                    Directory.Delete(mTempRoot, true);
                }
                catch
                {
                    // ignore cleanup failures
                }
            }
        }
    }
}