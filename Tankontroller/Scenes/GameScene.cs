using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Tankontroller.Controller;
using Tankontroller.GUI;
using Tankontroller.World;
using Tankontroller.World.Particles;
using Tankontroller.World.Gameplay;
using static Tankontroller.MapManager;

namespace Tankontroller.Scenes
{
    //-------------------------------------------------------------------------------------------------
    // GameScene
    //
    // This class is used to display the game scene. The game scene displays the tanks, bullets, tracks,
    // and walls of the game. The class contains a list of controllers, a world, a sprite batch, and a
    // list of tank positions and rotations. The class provides methods to draw the game scene, update
    // the game scene, and transition to the game over scene.
    //-------------------------------------------------------------------------------------------------
    public class GameScene : IScene
    {
        private static readonly SpriteFont SPRITE_FONT = Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");
        private static readonly Texture2D BACKGROUND_TEXTURE = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private static readonly Texture2D ERROR_BG_TEXTURE = Tankontroller.Instance().CM().Load<Texture2D>("background_err");
        private SoundEffectInstance mIntroMusicInstance = null;
        private SoundEffectInstance mTankMoveSound = null;

        IGame mGameInstance = Tankontroller.Instance();

        private const float SECONDS_BETWEEN_TRACKS_ADDED = 0.2f;
        private float mSecondsTillTracksAdded = SECONDS_BETWEEN_TRACKS_ADDED;

        private TheWorld mWorld;
        private List<Player> mTeams;

        Rectangle mBackgroundRectangle;

        private bool mControllersConnected = true;

        // Timer GUI
        private GameTimer mGameTimer;
        // configured match length in seconds (from DGS). If <= 0 timer is disabled.
        private double mGameLengthSeconds = 0.0;

        // Gameplay: Death Ring (shrinking safe zone)
        private DeathRing mDeathRing;

        public GameScene(List<Player> pPlayers, string mapFile)
        {
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);

            mIntroMusicInstance = mGameInstance.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_intro", false);
            mTankMoveSound = mGameInstance.GetSoundManager().GetLoopableSoundEffectInstance("Sounds/Tank_Tracks");

            mBackgroundRectangle = new Rectangle(0, 0, mGameInstance.GDM().GraphicsDevice.Viewport.Width, mGameInstance.GDM().GraphicsDevice.Viewport.Height);

            mTeams = pPlayers;

            mWorld = MapManager.LoadMapFromJson(mapFile);
            if (mWorld == null)
            {
                throw new Exception("Couldn't load map file: " + mapFile); //TODO Handle map load error
            }

            List<Tank> tanks = mWorld.GetTanksForPlayers(mTeams.Count);
            if (tanks == null || mWorld == null)
            {
                throw new Exception("Invalid number of players for map"); //TODO Handle map load error
            }

            // Calculate rectangle for the GUI of each player
            int textureWidth = mBackgroundRectangle.Width / 4;
            int spacePerPlayer = mBackgroundRectangle.Width / mTeams.Count;
            int textureHeight = mBackgroundRectangle.Height * 24 / 100;
            for (int i = 0; i < mTeams.Count; i++) // initialise players
            {
                mTeams[i].Controller.ResetJacks();
                mTeams[i].GamePreparation(tanks[i], new Rectangle((int)(i * spacePerPlayer + (spacePerPlayer - textureWidth) * 0.5f), 0, textureWidth, textureHeight));
            }

            Reset();
        }

        public override void Draw(float pSeconds)
        {
            Tankontroller.Instance().GDM().GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(BACKGROUND_TEXTURE, mBackgroundRectangle, Color.White);

            //Draws the GUI for each player
            foreach (Player player in mTeams)
            {
                if (player.GUI != null)
                {
                    player.GUI.Draw(spriteBatch, player.Tank.Health(), player.Tank.BulletType, player.Tank.GetState());
                }
            }

            // World draws play area, walls, tanks, bullets, and particle effects
            mWorld.Draw(spriteBatch);

            // Draw death ring overlay (if active) - clipped to play area
            if (mDeathRing != null)
            {
                spriteBatch.End();

                // Set up scissor rectangle to clip to play area
                Rectangle oldScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
                RasterizerState rasterizerState = new RasterizerState { ScissorTestEnable = true };
                spriteBatch.GraphicsDevice.ScissorRectangle = mWorld.PlayArea;

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, rasterizerState);
                mDeathRing.Draw(spriteBatch);
                spriteBatch.End();

                // Restore previous state
                spriteBatch.GraphicsDevice.ScissorRectangle = oldScissor;
                spriteBatch.Begin();
            }

            if (!mControllersConnected)
            {
                Rectangle playArea = mWorld.PlayArea;
                string message = "A controller has been disconnected.\r\nPlease reconnect it to continue.\r\nSearching for controller...";
                Vector2 centre = new Vector2(playArea.X + playArea.Width / 2, playArea.Y + playArea.Height / 2);
                Vector2 fontSize = SPRITE_FONT.MeasureString(message);
                spriteBatch.Draw(ERROR_BG_TEXTURE, playArea, Color.White);
                spriteBatch.DrawString(SPRITE_FONT, message, new Vector2(centre.X - (fontSize.X / 2), centre.Y - (fontSize.Y / 2)), Color.Black);
            }

            // Draw timer (top center). We compute remaining time from the configured match length and
            // the timer's total elapsed time (your GameTimer is a count-up).
            if (mGameTimer != null && mGameLengthSeconds > 0.0)
            {
                TimeSpan elapsed = mGameTimer.GetTotalTime();
                double remainingSeconds = mGameLengthSeconds - elapsed.TotalSeconds;
                if (remainingSeconds < 0) remainingSeconds = 0;
                TimeSpan remaining = TimeSpan.FromSeconds(remainingSeconds);
                string timeText = string.Format("{0:00}:{1:00}", remaining.Minutes, remaining.Seconds);
                Vector2 size = SPRITE_FONT.MeasureString(timeText);
                float x = mBackgroundRectangle.Width / 2f - size.X / 2f;
                float y = 10f;
                spriteBatch.DrawString(SPRITE_FONT, timeText, new Vector2(x, y), Color.White);
            }

            spriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            if (mIntroMusicInstance.State == SoundState.Stopped)
            {
                mGameInstance.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_loopable", true);
            }

            if (mControllersConnected) // Game should pause in the event of controller disconnect
            {
                //Updates each controller to check for inputs
                foreach (Player p in mTeams)
                {
                    p.Controller.UpdateController();
                    // Check if controller is disconnected
                    mControllersConnected = mControllersConnected && p.Controller.IsConnected();
                }

                bool tankMoved = false;
                foreach (Player p in mTeams)
                {
                    bool result = p.DoTankControls(pSeconds);
                    tankMoved = tankMoved | result;
                }

                //Checks for tank collisons between the play area and the walls
                mWorld.Update(pSeconds);

                if (tankMoved)
                {
                    mTankMoveSound.Play();
                }
                else
                {
                    mTankMoveSound.Pause();
                }

                //If there is only on player remaining, the GameOverScene is transitioned to
                List<int> remainingTeamsList = remainingTeams();
                if (remainingTeamsList.Count <= 1)
                {
                    int winner = -1;
                    if (remainingTeamsList.Count == 1)
                    {
                        winner = remainingTeamsList[0];
                    }
                    Tankontroller.Instance().SM().Transition(new GameOverScene(BACKGROUND_TEXTURE, mTeams, winner));
                }

                //Updates the track particles for each tank
                mSecondsTillTracksAdded -= pSeconds;
                if (mSecondsTillTracksAdded <= 0)
                {
                    mSecondsTillTracksAdded += SECONDS_BETWEEN_TRACKS_ADDED;
                    TrackSystem trackSystem = TrackSystem.GetInstance();
                    foreach (Player p in mTeams)
                    {
                        if(p.Tank.GetState() == TankStates.DESTROYED)
                        {
                            continue;
                        }
                        trackSystem.AddTrack(p.Tank.Transform.Position, p.Tank.Transform.Rotation, p.Tank.Colour());
                    }
                }

                // Update timer: GameTimer uses Update(GameTime) and counts up.
                if (mGameTimer != null && mGameLengthSeconds > 0.0)
                {
                    // Use a small GameTime wrapper to call the existing Update(GameTime) API
                    var gt = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(pSeconds));
                    mGameTimer.Update(gt);

                    // DeathRing: compute remaining seconds and pass tank list
                    float remainingSecondsForRing = (float)Math.Max(0.0, mGameLengthSeconds - mGameTimer.GetTotalTime().TotalSeconds);
                    List<Tank> tanksList = mTeams.Select(p => p.Tank).ToList();
                    mDeathRing?.Update(pSeconds, remainingSecondsForRing, tanksList);

                    // If elapsed >= configured length, end match
                    if (mGameTimer.GetTotalTime().TotalSeconds >= mGameLengthSeconds)
                    {
                        int winner = DetermineWinnerByHealth();
                        IGame game = Tankontroller.Instance();
                        game.GetControllerManager().SetAllTheLEDsWhite();
                        game.SM().Transition(new GameOverScene(BACKGROUND_TEXTURE, mTeams, winner));
                        return;
                    }
                }
                else
                {
                    // If there's no configured timer the ring won't activate, but still keep updating if desired:
                    // build tanks list and update with 'infinite' remaining time so it stays inactive.
                    List<Tank> tanksListNoTimer = mTeams.Select(p => p.Tank).ToList();
                    mDeathRing?.Update(pSeconds, float.MaxValue, tanksListNoTimer);
                }
            }
            else // At least one controller is disconnected
            {
                mGameInstance.GetControllerManager().DetectControllers();

                mControllersConnected = true;
                foreach (Player p in mTeams) // Wait until all controllers are reconnected
                {
                    mControllersConnected = mControllersConnected && p.Controller.IsConnected();
                }
                if (mControllersConnected)
                {
                    foreach (Player p in mTeams)
                    {
                        p.Controller.SetColour(p.Tank.Colour());
                    }
                }
            }
        }
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                mGameInstance.GetControllerManager().SetAllTheLEDsWhite();
                mGameInstance.SM().Transition(null);
            }
        }

        private void Reset()
        {
            foreach (Player p in mTeams)
            {
                p.Reset();
            }
            ParticleManager.Instance().Reset();
            TrackSystem.GetInstance().Reset();

            // Initialize your existing GameTimer and start it if GAME_LENGTH is configured.
            float configuredLength = DGS.Instance.GetFloat("GAME_LENGTH");
            mGameLengthSeconds = configuredLength;
            if (configuredLength > 0f)
            {
                mGameTimer = new GameTimer();
                mGameTimer.Reset();
                mGameTimer.Start();
            }
            else
            {
                mGameTimer = null; // timer disabled
            }

            // Create DeathRing instance (uses world play area to compute center/start radius)
            if (mWorld != null)
            {
                mDeathRing = new DeathRing(mWorld.PlayArea);
            }
            else
            {
                mDeathRing = null;
            }
        }

        //Checks the health of all players and returns a list of tanks with more that 0 health
        private List<int> remainingTeams()
        {
            List<int> remaining = new List<int>();
            int index = 0;
            foreach (Player player in mTeams)
            {
                if (player.Tank.Health() > 0)
                {
                    remaining.Add(index);
                }
                index++;
            }
            return remaining;
        }

        // <summary>
        // Determine winner by highest health. Returns -1 for tie or no winner.
        // </summary>
        private int DetermineWinnerByHealth()
        {
            int bestIndex = -1;
            int bestHealth = -1;
            bool tie = false;
            for (int i = 0; i < mTeams.Count; i++)
            {
                int health = mTeams[i].Tank.Health();
                if (health > bestHealth)
                {
                    bestHealth = health;
                    bestIndex = i;
                    tie = false;
                }
                else if (health == bestHealth)
                {
                    tie = true;
                }
            }
            return (bestIndex == -1 || tie) ? -1 : bestIndex;
        }
    }
}