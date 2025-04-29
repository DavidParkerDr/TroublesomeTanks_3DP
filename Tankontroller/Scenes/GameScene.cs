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
        private static readonly SpriteFont m_SpriteFont = Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");
        private static readonly Texture2D mBackgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private static readonly Texture2D m_ErrorBGTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_err");
        private SoundEffectInstance introMusicInstance = null;
        private SoundEffectInstance tankMoveSound = null;

        IGame mGameInstance = Tankontroller.Instance();

        private const float SECONDS_BETWEEN_TRACKS_ADDED = 0.2f;
        private float m_SecondsTillTracksAdded = SECONDS_BETWEEN_TRACKS_ADDED;

        private TheWorld m_World;
        private List<Player> m_Teams;

        Rectangle mBackgroundRectangle;

        private bool mControllersConnected = true;
        public GameScene(List<Player> pPlayers, string mapFile)
        {
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);

            introMusicInstance = mGameInstance.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_intro", false);
            tankMoveSound = mGameInstance.GetSoundManager().GetLoopableSoundEffectInstance("Sounds/Tank_Tracks");

            mBackgroundRectangle = new Rectangle(0, 0, mGameInstance.GDM().GraphicsDevice.Viewport.Width, mGameInstance.GDM().GraphicsDevice.Viewport.Height);

            m_Teams = pPlayers;

            m_World = MapManager.LoadMapFromJson(mapFile);
            if (m_World == null)
            {
                throw new Exception("Couldn't load map file: " + mapFile); //TODO Handle map load error
            }

            List<Tank> tanks = m_World.GetTanksForPlayers(m_Teams.Count);
            if (tanks == null || m_World == null)
            {
                throw new Exception("Invalid number of players for map"); //TODO Handle map load error
            }

            // Calculate rectangle for the GUI of each player
            int textureWidth = mBackgroundRectangle.Width / 4;
            int spacePerPlayer = mBackgroundRectangle.Width / m_Teams.Count;
            int textureHeight = mBackgroundRectangle.Height * 24 / 100;
            for (int i = 0; i < m_Teams.Count; i++) // initialise players
            {
                m_Teams[i].Controller.ResetJacks();
                m_Teams[i].GamePreparation(tanks[i], new Rectangle((int)(i * spacePerPlayer + (spacePerPlayer - textureWidth) * 0.5f), 0, textureWidth, textureHeight));
            }

            Reset();
        }

        public override void Draw(float pSeconds)
        {
            Tankontroller.Instance().GDM().GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);

            //Draws the GUI for each player
            foreach (Player player in m_Teams)
            {
                if (player.GUI != null)
                {
                    player.GUI.Draw(spriteBatch, player.Tank.Health(), player.Tank.mbulletType);
                }
            }

            // World draws play area, walls, tanks, bullets, and particle effects
            m_World.Draw(spriteBatch);

            if (!mControllersConnected)
            {
                Rectangle playArea = m_World.PlayArea;
                string message = "A controller has been disconnected.\r\nPlease reconnect it to continue.\r\nSearching for controller...";
                Vector2 centre = new Vector2(playArea.X + playArea.Width / 2, playArea.Y + playArea.Height / 2);
                Vector2 fontSize = m_SpriteFont.MeasureString(message);
                spriteBatch.Draw(m_ErrorBGTexture, playArea, Color.White);
                spriteBatch.DrawString(m_SpriteFont, message, new Vector2(centre.X - (fontSize.X / 2), centre.Y - (fontSize.Y / 2)), Color.Black);
            }

            spriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            if (introMusicInstance.State == SoundState.Stopped)
            {
                mGameInstance.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_loopable", true);
            }

            if (mControllersConnected) // Game should pause in the event of controller disconnect
            {
                //Updates each controller to check for inputs
                foreach (Player p in m_Teams)
                {
                    p.Controller.UpdateController();
                    // Check if controller is disconnected
                    mControllersConnected = mControllersConnected && p.Controller.IsConnected();
                }

                bool tankMoved = false;
                foreach (Player p in m_Teams)
                {
                    bool result = p.DoTankControls(pSeconds);
                    tankMoved = tankMoved | result;
                }

                //Checks for tank collisons between the play area and the walls
                m_World.Update(pSeconds);

                if (tankMoved)
                {
                    tankMoveSound.Play();
                }
                else
                {
                    tankMoveSound.Pause();
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
                    Tankontroller.Instance().SM().Transition(new GameOverScene(mBackgroundTexture, m_Teams, winner));
                }

                //Updates the track particles for each tank
                m_SecondsTillTracksAdded -= pSeconds;
                if (m_SecondsTillTracksAdded <= 0)
                {
                    m_SecondsTillTracksAdded += SECONDS_BETWEEN_TRACKS_ADDED;
                    TrackSystem trackSystem = TrackSystem.GetInstance();
                    foreach (Player p in m_Teams)
                    {
                        trackSystem.AddTrack(p.Tank.GetWorldPosition(), p.Tank.GetRotation(), p.Tank.Colour());
                    }
                }
            }
            else // At least one controller is disconnected
            {
                mGameInstance.GetControllerManager().DetectControllers();

                mControllersConnected = true;
                foreach (Player p in m_Teams) // Wait until all controllers are reconnected
                {
                    mControllersConnected = mControllersConnected && p.Controller.IsConnected();
                }
                if (mControllersConnected)
                {
                    foreach (Player p in m_Teams)
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
            foreach (Player p in m_Teams)
            {
                p.Reset();
            }
            ParticleManager.Instance().Reset();
            TrackSystem.GetInstance().Reset();
        }

        //Checks the health of all players and returns a list of tanks with more that 0 health
        private List<int> remainingTeams()
        {
            List<int> remaining = new List<int>();
            int index = 0;
            foreach (Player player in m_Teams)
            {
                if (player.Tank.Health() > 0)
                {
                    remaining.Add(index);
                }
                index++;
            }
            return remaining;
        }
    }
}