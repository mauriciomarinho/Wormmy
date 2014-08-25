#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Wormmy
{
	/// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

		Texture2D testImage;

		public SceneManager sceneManager;
		public Dictionary<string, string> configDict;
		public char separator = Path.DirectorySeparatorChar;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = false;		

			sceneManager = new SceneManager (this);

			string configPath = Environment.CurrentDirectory + separator + Content.RootDirectory + separator + "Config" + separator + "user.cfg";	
			configDict = Utils.ConfigParser.GetDictionary( Utils.ConfigParser.ReadFile (configPath) );

			IsMouseVisible = true;
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
//			graphics.PreferredBackBufferWidth = Convert.ToInt32 (configDict ["screenwidth"]);
//			graphics.PreferredBackBufferHeight = Convert.ToInt32 (configDict ["screenheight"]);
			graphics.IsFullScreen = Convert.ToBoolean (Convert.ToInt32( configDict ["fullscreen"]) );
			graphics.ApplyChanges ();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();				
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //TODO: use this.Content to load your game content here 
			testImage = Content.Load<Texture2D> ("Image/test_img_256");
			var IntroScene = new Intro ();

			sceneManager.CreateScene (IntroScene);
			sceneManager.ActivateScene (IntroScene);
			sceneManager.InitializeActive ();
			sceneManager.LoadActive ();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) )
			{
				Exit();
			}
            // TODO: Add your update logic here		
			sceneManager.UpdateActive ();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
		
            //TODO: Add your drawing code here
			sceneManager.DrawActive ();

            base.Draw(gameTime);
        }
    }
}

