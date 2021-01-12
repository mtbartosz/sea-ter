﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System;

namespace Shmup
{

    class BigFish : Sprite
    {
        float currentSpeed = 100f;
        float maxSpeed = 1000;
        float acceleration = 500f;
        public bool dead = false;
        public BigFish(Texture2D newTxr, Vector2 newPos, float newMaxSpeed = 1000f) : base(newTxr, newPos)
        {
            maxSpeed = newMaxSpeed;



        }
        public override void Update(GameTime gameTime, Point screenSize)
        {
            currentSpeed += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            currentSpeed = Math.Min(currentSpeed, maxSpeed);
            spritePos.X -= currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (spritePos.X < -spriteTexture.Width) dead = true;
        }
    }


}    
