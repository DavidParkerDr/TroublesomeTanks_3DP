using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TTMapEditor.GUI;
using TTMapEditor.Managers;
using TTMapEditor.Maps;
using TTMapEditor.Objects;
using Xunit;

namespace TTMapEditor.Tests
{
    public class MapBoundaryValidatorTests
    {
        [Fact]
        public void IsRectWithinPlayArea_ReturnsTrue_WhenRectIsFullyInsideOrOnEdge()
        {
            var validator = new MapBoundaryValidator(new Rectangle(0, 0, 100, 100));

            Assert.True(validator.IsRectWithinPlayArea(new Rectangle(0, 0, 10, 10)));
            Assert.True(validator.IsRectWithinPlayArea(new Rectangle(90, 90, 10, 10)));
        }

        [Fact]
        public void IsRectWithinPlayArea_ReturnsFalse_WhenRectIsOutside()
        {
            var validator = new MapBoundaryValidator(new Rectangle(0, 0, 100, 100));

            Assert.False(validator.IsRectWithinPlayArea(new Rectangle(-1, 0, 10, 10)));
            Assert.False(validator.IsRectWithinPlayArea(new Rectangle(95, 95, 10, 10)));
        }

        [Fact]
        public void IsWallWithinPlayArea_ReturnsExpected_ForUnrotatedAndRotatedWalls()
        {
            var validator = new MapBoundaryValidator(new Rectangle(0, 0, 100, 100));

            var insideWall = new RectWall(null, new Rectangle(40, 40, 20, 20), 0f);
            Assert.True(validator.IsWallWithinPlayArea(insideWall));

            var nearEdgeRotated = new RectWall(null, new Rectangle(90, 90, 20, 20), MathHelper.ToRadians(45f));
            Assert.False(validator.IsWallWithinPlayArea(nearEdgeRotated));
        }
    }

    public class ButtonAndButtonListTests
    {
        [Fact]
        public void PressButton_ReturnsTrueAndInvokesAction_WhenActionExists()
        {
            int called = 0;
            var button = new Button(null, null, new Rectangle(0, 0, 10, 10), Color.White, () => called++);

            bool result = button.PressButton();

            Assert.True(result);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SelectNextButton_WrapsAndUpdatesSelectedFlags()
        {
            var b1 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => { });
            var b2 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => { });
            var b3 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => { });
            b1.mSelected = true;

            var list = new ButtonList();
            list.AddButton(b1);
            list.AddButton(b2);
            list.AddButton(b3);

            list.SelectNextButton();
            Assert.False(b1.mSelected);
            Assert.True(b2.mSelected);

            list.SelectNextButton();
            Assert.False(b2.mSelected);
            Assert.True(b3.mSelected);

            list.SelectNextButton();
            Assert.False(b3.mSelected);
            Assert.True(b1.mSelected);
        }

        [Fact]
        public void SelectPreviousButton_WrapsBackToLastButton()
        {
            var b1 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => { });
            var b2 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => { });
            b1.mSelected = true;

            var list = new ButtonList();
            list.AddButton(b1);
            list.AddButton(b2);

            list.SelectPreviousButton();

            Assert.False(b1.mSelected);
            Assert.True(b2.mSelected);
        }

        [Fact]
        public void PressSelectedButton_PressesCurrentlySelectedButton()
        {
            int firstPressed = 0;
            int secondPressed = 0;

            var b1 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => firstPressed++);
            var b2 = new Button(null, null, new Rectangle(0, 0, 1, 1), Color.White, () => secondPressed++);
            b1.mSelected = true;

            var list = new ButtonList();
            list.AddButton(b1);
            list.AddButton(b2);

            list.SelectNextButton();
            list.PressSelectedButton();

            Assert.Equal(0, firstPressed);
            Assert.Equal(1, secondPressed);
        }
    }

    public class PickupActivationTests
    {
        [Fact]
        public void ToggleActivateDeactivate_ModifyPickupFlags()
        {
            var pickup = new Pickup(null, new Rectangle(0, 0, 10, 10));

            pickup.DeactivatePickupType(PickupType.EMP);
            Assert.False(pickup.GetActivatedPickups()[PickupType.EMP]);

            pickup.ActivatePickupType(PickupType.EMP);
            Assert.True(pickup.GetActivatedPickups()[PickupType.EMP]);

            pickup.TogglePickupType(PickupType.EMP);
            Assert.False(pickup.GetActivatedPickups()[PickupType.EMP]);
        }

        [Fact]
        public void SetActivatedPickups_ReplacesDictionaryReference()
        {
            var pickup = new Pickup(null, new Rectangle(0, 0, 10, 10));
            var map = new Dictionary<PickupType, bool>
            {
                { PickupType.HEALTH, false },
                { PickupType.EMP, true },
                { PickupType.MINE, false },
                { PickupType.BOUNCY_BULLET, true },
            };

            pickup.SetActivatedPickups(map);

            Assert.Same(map, pickup.GetActivatedPickups());
            Assert.False(pickup.GetActivatedPickups()[PickupType.HEALTH]);
        }
    }
}
