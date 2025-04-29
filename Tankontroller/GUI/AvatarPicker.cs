using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankontroller.Controller;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // AvatarPicker
    //
    // This is the picker on the player selection scene that the player navigates to select an avatar
    // and a colour.
    //-------------------------------------------------------------------------------------------------
    public class AvatarPicker
    {
        private List<Avatar> mAvatars;
        private List<Avatar> mColours;
        private List<Rectangle> mSelectionRectangles;

        private IController mController;
        private Avatar mCentreAvatar;
        private static List<int> mBlockedIndexList; // Keeps track of already selected colours across Avatar Pickers
        private int mSelectionIndex;
        private bool mAvatarSet;
        private bool mColourSet;

        private float mSelectionCoolDown;
        private float mJoinButtonFlashTimer;
        private bool mShowJoinButton;

        private Rectangle mBoundsRectangle;
        private Vector2 mCentre;
        private float mRadius;
        private float mAvatarRadius;

        // Textures and Bounds
        private Texture2D mCircle;
        private Texture2D mJoinButtonTexture;
        private Texture2D mBackButtonTexture;
        private Texture2D mRotateLeftTexture;
        private Texture2D mRotateRightTexture;
        private Texture2D mBackTextTexture;
        private Rectangle mAButtonRectangle;
        private Rectangle mBackButtonRectangle;
        private Rectangle mRotateLeftButton;
        private Rectangle mRotateRightButton;
        private Rectangle mBackTextRectangle;

        private List<Color> colours = new List<Color>
            {
                Color.Red,
                Color.Firebrick,
                Color.DarkOrange,
                Color.Gold,
                Color.Lime,
                Color.ForestGreen,
                Color.Blue,
                Color.DeepSkyBlue,
                Color.MediumPurple,
                Color.DeepPink,
            };

        public AvatarPicker(Rectangle pRectangle)
        {
            mBoundsRectangle = pRectangle;
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            mCircle = game.CM().Load<Texture2D>("circle");
            mJoinButtonTexture = game.CM().Load<Texture2D>("fire");
            mBackButtonTexture = game.CM().Load<Texture2D>("charge");
            mBackTextTexture = game.CM().Load<Texture2D>("back");
            mRotateLeftTexture = game.CM().Load<Texture2D>("turretLeft");
            mRotateRightTexture = game.CM().Load<Texture2D>("turretRight");
            PrepareButtons();

            mCentre = new Vector2(mBoundsRectangle.X + mBoundsRectangle.Width / 2, mBoundsRectangle.Y + mBoundsRectangle.Height / 2);
            mAvatarRadius = 70;
            mRadius = mBoundsRectangle.Height / 2 - mAvatarRadius;

            SetUpAvatarSelection();
            Reset();
        }

        public static void ResetBlockedIndexList()
        {
            mBlockedIndexList = new List<int>();
        }

        public void SetController(IController pController)
        {
            if (mController == null)
            {
                mController = pController;
                mSelectionIndex = 0;
                UpdateCentreAvatar();
            }
        }
        public IController GetController() { return mController; }
        public void RemoveController() { mController = null; }

        public Avatar GetAvatar() { return mCentreAvatar; }

        public bool Ready() { return HasController() && mAvatarSet && mColourSet; }
        public bool HasController() { return mController != null; }

        public void Reposition(Rectangle pRectangle)
        {
            mBoundsRectangle = pRectangle;
        }

        public void Reset()
        {
            mController = null;
            mShowJoinButton = true;
            mSelectionIndex = 0;
            mAvatarSet = false;
            mColourSet = false;
            mJoinButtonFlashTimer = 0;
            mSelectionCoolDown = 0;
            mSelectionRectangles = GetSelectionRectangles(mAvatars.Count);
            UpdateCentreAvatar();
        }

        private void UpdateCentreAvatar()
        {
            string name = mAvatarSet ? mCentreAvatar.GetName() : mAvatars[mSelectionIndex].GetName();

            Rectangle avatarRect = new Rectangle();
            float middleAvatarRadius = (HasController() && mColourSet) ? mBoundsRectangle.Height * 0.45f : 100;
            avatarRect.X = (int)(mCentre.X - middleAvatarRadius);
            avatarRect.Y = (int)(mCentre.Y - middleAvatarRadius);
            avatarRect.Width = avatarRect.Height = (int)(middleAvatarRadius * 2);

            Color color = mAvatarSet ? mColours[mSelectionIndex].GetColour() : Color.White;
            mCentreAvatar = new Avatar(name, avatarRect, color);
            mController?.SetColour(color);
        }

        private void PrepareButtons()
        {
            float widthRatio = (float)mJoinButtonTexture.Width / mJoinButtonTexture.Height;
            int height = 200;
            int width = (int)(height * widthRatio);
            mAButtonRectangle = new Rectangle(mBoundsRectangle.X + (mBoundsRectangle.Width - width) / 2, mBoundsRectangle.Y + (mBoundsRectangle.Height - height) / 2, width, height);

            height = 60;
            width = (int)(height * widthRatio);
            mBackButtonRectangle = new Rectangle(mBoundsRectangle.X + (mBoundsRectangle.Width / 16) - width, mBoundsRectangle.Y + (mBoundsRectangle.Height / 8) - height, width, height);

            height = 40;
            width = (int)(height * widthRatio);
            mBackTextRectangle = new Rectangle(mBoundsRectangle.X + ((mBoundsRectangle.Width / 16) + 30) - width, mBoundsRectangle.Y + (mBoundsRectangle.Height / 8) - height, width, height / 2);

            mRotateLeftButton = new Rectangle(mBoundsRectangle.X + ((mBoundsRectangle.Width / 10) * 3) - width + 20, mBoundsRectangle.Y + (mBoundsRectangle.Height / 8) - height, width, height);

            mRotateRightButton = new Rectangle(mBoundsRectangle.X + ((mBoundsRectangle.Width / 10) * 7) - width + 20, mBoundsRectangle.Y + (mBoundsRectangle.Height / 8) - height, width, height);
        }


        private List<Rectangle> GetSelectionRectangles(int pSelectionCount)
        {
            var output = new List<Rectangle>();
            float anglePerAvatar = (float)(Math.PI * 2 / pSelectionCount);
            float centreX = mCentre.X;
            float centreY = mCentre.Y;
            float topLeftAngle = (float)Math.PI * 5 / 4;
            float bottomRightAngle = (float)Math.PI / 4;
            for (int i = 0; i < pSelectionCount; i++)
            {
                float angle = i * anglePerAvatar;
                float x = centreX + (float)Math.Cos(angle) * mRadius;
                float y = centreY + (float)Math.Sin(angle) * mRadius;
                float topLeftX = x + (float)Math.Cos(topLeftAngle) * mAvatarRadius;
                float topLeftY = y + (float)Math.Sin(topLeftAngle) * mAvatarRadius;
                float bottomRightX = x + (float)Math.Cos(bottomRightAngle) * mAvatarRadius;
                float bottomRightY = y + (float)Math.Sin(bottomRightAngle) * mAvatarRadius;
                float width = bottomRightX - topLeftX;
                float height = bottomRightY - topLeftY;
                Rectangle rectangle = new Rectangle((int)topLeftX, (int)topLeftY, (int)width, (int)height);
                output.Add(rectangle);
            }
            return output;
        }

        private void SetUpAvatarSelection()
        {
            mAvatars = new List<Avatar>();
            List<string> avatarStrings = new List<string>
            {
                "engineer",
                "robo",
                "winterjack",
                "yeti"
            };

            var rectangles = GetSelectionRectangles(avatarStrings.Count);
            for (int i = 0; i < avatarStrings.Count; i++)
            {
                Avatar avatar = new Avatar(avatarStrings[i], rectangles[i], Color.White);
                mAvatars.Add(avatar);
            }
        }
        private void UpdateColorOptions(string pAvatarString)
        {
            int screenWidth = mBoundsRectangle.Width;
            int screenHeight = mBoundsRectangle.Height;
            mColours = new List<Avatar>();

            var rectangles = GetSelectionRectangles(colours.Count);
            for (int i = 0; i < colours.Count; i++)
            {
                Avatar avatar;
                if (mBlockedIndexList.Contains(i))
                {
                    avatar = new Avatar(pAvatarString, rectangles[i], Color.DimGray);
                }
                else
                {
                    avatar = new Avatar(pAvatarString, rectangles[i], colours[i]);
                }
                mColours.Add(avatar);
            }
        }

        public void DrawAvatars(SpriteBatch pSpriteBatch)
        {
            foreach (Avatar avatar in mAvatars)
            {
                avatar.Draw(pSpriteBatch, true, 0);
            }
        }
        public void DrawColours(SpriteBatch pSpriteBatch)
        {
            foreach (Avatar avatar in mColours)
            {
                avatar.Draw(pSpriteBatch, true, 0);
            }
        }
        public void DrawSelection(SpriteBatch pSpriteBatch)
        {
            if (mAvatarSet && mBlockedIndexList.Contains(mSelectionIndex))
            {
                pSpriteBatch.Draw(mCircle, mSelectionRectangles[mSelectionIndex], Color.Black);
            }
            else
            {
                pSpriteBatch.Draw(mCircle, mSelectionRectangles[mSelectionIndex], Color.White);
            }
        }
        public void DrawJoinButton(SpriteBatch pSpriteBatch)
        {
            if (mShowJoinButton)
            {
                pSpriteBatch.Draw(mJoinButtonTexture, mAButtonRectangle, Color.White);
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            if (HasController())
            {
                UpdateColorOptions(mCentreAvatar.GetName());
                mCentreAvatar.Draw(pSpriteBatch, true, 0);
                if (!mAvatarSet)
                {
                    DrawSelection(pSpriteBatch);
                    DrawAvatars(pSpriteBatch);
                    pSpriteBatch.Draw(mRotateLeftTexture, mRotateLeftButton, Color.White);
                    pSpriteBatch.Draw(mRotateRightTexture, mRotateRightButton, Color.White);
                }
                else if (!mColourSet)
                {
                    DrawSelection(pSpriteBatch);
                    DrawColours(pSpriteBatch);
                    pSpriteBatch.Draw(mRotateLeftTexture, mRotateLeftButton, Color.White);
                    pSpriteBatch.Draw(mRotateRightTexture, mRotateRightButton, Color.White);
                }
                pSpriteBatch.Draw(mBackButtonTexture, mBackButtonRectangle, Color.White);
                pSpriteBatch.Draw(mBackTextTexture, mBackTextRectangle, Color.White);
            }
            else
            {
                DrawJoinButton(pSpriteBatch);
            }
        }

        public void ChangeSelection(int pAmount)
        {
            if (HasController() && !Ready() && mSelectionCoolDown <= 0)
            {
                mSelectionIndex += pAmount;
                if (mSelectionIndex < 0)
                {
                    mSelectionIndex = mSelectionRectangles.Count + mSelectionIndex;
                }
                if (mSelectionIndex >= mAvatars.Count)
                {
                    mSelectionIndex = mSelectionIndex % mSelectionRectangles.Count;
                }
                UpdateCentreAvatar();
                mSelectionCoolDown = 0.2f;
            }
        }


        public void MakeSelection()
        {
            if (HasController())
            {
                if (!mAvatarSet)
                {
                    mAvatarSet = true;
                    mSelectionIndex = 0;
                    UpdateColorOptions(mCentreAvatar.GetName());
                    mSelectionRectangles = GetSelectionRectangles(mColours.Count);
                    UpdateCentreAvatar();
                }
                else if (!mColourSet)
                {
                    if (!mBlockedIndexList.Contains(mSelectionIndex))
                    {
                        mColourSet = true;
                        UpdateCentreAvatar();
                        mBlockedIndexList.Add(mSelectionIndex);
                    }
                }
            }
        }

        public void UndoSelection()
        {
            if (mColourSet)
            {
                mColourSet = false;
                mBlockedIndexList.Remove(mSelectionIndex);
                UpdateCentreAvatar();
            }
            else if (mAvatarSet)
            {
                mAvatarSet = false;
                mSelectionIndex = 0;
                mSelectionRectangles = GetSelectionRectangles(mAvatars.Count);
                UpdateCentreAvatar();
            }
            else
            {
                mController = null;
            }
        }

        public void Update(float pSeconds)
        {
            if (!mColourSet) { UpdateCentreAvatar(); }
            mJoinButtonFlashTimer -= pSeconds;
            mSelectionCoolDown -= pSeconds;
            if (mJoinButtonFlashTimer <= 0)
            {
                mJoinButtonFlashTimer = mShowJoinButton ? 0.25f : 1.0f;
                mShowJoinButton = !mShowJoinButton;
            }
        }
    }
}
