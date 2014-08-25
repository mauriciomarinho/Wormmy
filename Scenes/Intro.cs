using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Wormmy
{
	public class Intro : Scene
	{
		public bool Active { get; set; }
		public bool Visible { get; set; }

		private VideoPlayer videoPlayer;
		private Video introVideo;

		Texture2D videoTexture;

		public Intro ()
		{
			videoPlayer = new VideoPlayer ();
		}


		public override void Initialize ()
		{
			Manager.Game.Window.Title = "Wormmy - Intro";
		}

		public override void Load (ContentManager CM)
		{
			introVideo = CM.Load<Video> ("Video/intro_naif");
		}

		public override void Update ()
		{
			videoPlayer.Play (introVideo);
		}

		public override void Draw (SpriteBatch spriteBatch)
		{
			videoTexture = videoPlayer.GetTexture ();
			Rectangle screen = new Rectangle (0, 0, this.Manager.Game.graphics.PreferredBackBufferWidth, this.Manager.Game.graphics.PreferredBackBufferHeight);

			spriteBatch.Begin ();
			spriteBatch.Draw (videoTexture, screen, Color.White);
			spriteBatch.End ();
		}
	}
}

