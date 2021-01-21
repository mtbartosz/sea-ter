using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;



namespace Shmup
{
	public class Game1 : Game                                                 //defining main class and naming all assets used in game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		Texture2D piranhaTxr, fishTxr, backgroundTxr, particleTxr, bigfishTxr;
		Point screenSize = new Point(1920, 1080);
		float spawnCooldown = 2;
		float playTime = 0;
		
		Sprite backgroundSprite;
		PlayerSprite playerSprite;
		List<FishSprite> fishList = new List<FishSprite>();
		List<FishSprite> bigFishList = new List<FishSprite>();
		List<ParticleSprite> particleList = new List<ParticleSprite>();
		SpriteFont uiFont, bigfont;
		SoundEffect chardeadSnd, fishdeadSnd, backgroundSnd, sharkSnd;


		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);                        // defining path to content folder         
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()                                   // initializing game component	
		{
			_graphics.PreferredBackBufferWidth = screenSize.X;
			_graphics.PreferredBackBufferHeight = screenSize.Y;
			_graphics.ApplyChanges();

			base.Initialize();
		}

		protected override void LoadContent()                                   // loading content - fonts textures and sounds
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			piranhaTxr = Content.Load<Texture2D>("piranha");
			fishTxr = Content.Load<Texture2D>("cutefish");
			backgroundTxr = Content.Load<Texture2D>("seabackground");
			particleTxr = Content.Load<Texture2D>("bubble");
			uiFont = Content.Load<SpriteFont>("UIFont");
			bigfont = Content.Load<SpriteFont>("bigfont");
			sharkSnd = Content.Load<SoundEffect>("sharkattack");
			chardeadSnd = Content.Load<SoundEffect>("chardead");
			fishdeadSnd = Content.Load<SoundEffect>("fishdead");
			backgroundSnd = Content.Load<SoundEffect>("waterloop");
			bigfishTxr = Content.Load<Texture2D>("shark");



			backgroundSprite = new Sprite(backgroundTxr, new Vector2());
			playerSprite = new PlayerSprite(piranhaTxr, new Vector2(screenSize.X/6, screenSize.Y/2));     //defining position of background and player sprite
			
			var instance = backgroundSnd.CreateInstance();                                                // playing background sound in loop
			instance.IsLooped = true;
			instance.Play();


		}

		protected override void Update(GameTime gameTime)
		{
			Random rng = new Random();
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))  // determining what key escapes game
				Exit(); 
			if (spawnCooldown > 0)
			{
				spawnCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

			}
			else if (playerSprite.playerLives > 0)    
			{
				if (fishList.Count < Math.Max(1, (60 - playTime) / 12))   // defining amount of edible fish on-screen at a time
				{
					fishList.Add(new FishSprite(
						fishTxr,
						new Vector2(screenSize.X, rng.Next(0, screenSize.Y - fishTxr.Height)),
						 (Math.Min(playTime, 120f) / 120f) * 20000f + 200f
						));
				}

				if (bigFishList.Count == 0)
				{
					bigFishList.Add(new FishSprite(
						bigfishTxr,
						new Vector2(screenSize.X, rng.Next(0, screenSize.Y - bigfishTxr.Height)),      // defining random spawn of fish on y axis
						 (Math.Min(playTime, 120f) / 120f) * 20000f + 200f                             // defining speed of spawned fish depending on a play time
						));
				}
				spawnCooldown = (float)(rng.NextDouble() + 0.5);

			}

			if (playerSprite.playerLives > 0)
			{
				playerSprite.Update(gameTime, screenSize);
				playTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
			}

			foreach (FishSprite fish in fishList)                   // defining what happens when colliding  with small edible fish
            {
				fish.Update(gameTime, screenSize);
                if (playerSprite.playerLives > 0 && playerSprite.IsColliding(fish))
                {
					for (int i = 0; i < 16; i++)
						particleList.Add(new ParticleSprite(particleTxr,
							new Vector2(
										fish.spritePos.X + (fish.spriteTexture.Width / 2) - (particleTxr.Width / 2),
										fish.spritePos.Y + (fish.spriteTexture.Height / 2) - (particleTxr.Height / 2)
										)
										)); 
					fish.dead = true;
					playerSprite.playerLives++;
					fishdeadSnd.Play();
                }
            }

			foreach (FishSprite fish in bigFishList)                // defining what happens when colliding with shark
			{
				fish.Update(gameTime, screenSize);
				if (playerSprite.playerLives > 0 && playerSprite.IsColliding(fish))
				{
					for (int i = 0; i < 16; i++)
					{
						particleList.Add(new ParticleSprite(particleTxr,
							  new Vector2(
										 playerSprite.spritePos.X + (piranhaTxr.Width / 2) - (particleTxr.Width / 2),
										playerSprite.spritePos.Y + (piranhaTxr.Height / 2) - (particleTxr.Height / 2)
										  )
										  ));
						particleList[particleList.Count - 1]._particolor = Color.Red;
					} 
					fish.dead = true;
					playerSprite.playerLives-=5;
					sharkSnd.Play();
					if (playerSprite.playerLives <= 0)
					{
						for (int i = 0; i < 16; i++)
							particleList.Add(new ParticleSprite(particleTxr,
								new Vector2(
											playerSprite.spritePos.X + (piranhaTxr.Width / 2) - (particleTxr.Width / 2),
											playerSprite.spritePos.Y + (piranhaTxr.Height / 2) - (particleTxr.Height / 2)
											)
											));
					}
					if(playerSprite.playerLives <= 0)
                    {
						chardeadSnd.Play();
                    }

				}
				
			}

			foreach (ParticleSprite particle in particleList) particle.Update(gameTime, screenSize);
		
			fishList.RemoveAll(fish => fish.dead);                             // removing sprites from screen when they are dead after collision
			bigFishList.RemoveAll(fish => fish.dead);
			particleList.RemoveAll(particle => particle.currentLife <= 0);
			
			base.Update(gameTime);

			Debug.WriteLine(fishList.Count);
			
		}




		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			backgroundSprite.Draw(_spriteBatch);
			if(playerSprite.playerLives > 0) playerSprite.Draw(_spriteBatch);

            foreach (FishSprite fish in fishList) fish.Draw(_spriteBatch);
			foreach (FishSprite fish in bigFishList) fish.Draw(_spriteBatch);
			foreach (ParticleSprite particle in particleList) particle.Draw(_spriteBatch);

			_spriteBatch.DrawString(                             // writing basic information about state of the player's lives and gameplay time - which is also score
				uiFont,
				"Lives : " + playerSprite.playerLives, 
				new Vector2(14, 14),
				Color.Black
				);
			_spriteBatch.DrawString(                              // writing same thing again in different position with different colour to make it more distinctive
				uiFont,
				"Lives : " + playerSprite.playerLives,
				new Vector2(10, 10),
				Color.GhostWhite
				);
			_spriteBatch.DrawString(
				uiFont,
				"Score : " + Math.Round(playTime),
				new Vector2(14, 44),
				Color.Black
				);
			_spriteBatch.DrawString(
				uiFont,
				"Score : " + Math.Round(playTime),
				new Vector2(10, 40),
				Color.GhostWhite
				);


			if (playerSprite.playerLives <= 0)                         // displaying text at the end of the game 
            {
				Vector2 textSize = bigfont.MeasureString(" GAME OVER");
				
				_spriteBatch.DrawString
					(bigfont,
					" GAME OVER",
					new Vector2((screenSize.X / 2) - (textSize.X / 2), (screenSize.Y / 3) - (textSize.Y / 3)),
					Color.White
					);
				_spriteBatch.DrawString
					(bigfont,
					"YOUR SCORE:",
					new Vector2((screenSize.X / 2) - (textSize.X / 2), (screenSize.Y / 2) - (textSize.Y / 2)),
					Color.White
					);
				_spriteBatch.DrawString
					(bigfont,
					" " + Math.Round(playTime),
					new Vector2(900, 600),
					Color.Red
					);
			}
			

			_spriteBatch.End();

			base.Draw(gameTime);
			
		}
	}
}
