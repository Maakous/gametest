using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace prog_spel
    {
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D coin;
        Texture2D Boss;
        Texture2D model1;
        Texture2D Gubbe;
        Vector2 Gubbe_pos;
        Vector2 Gubbe_vel;
        Vector2 model1_pos;
        Vector2 model1_vel;
        Vector2 Boss_pos;
        Vector2 Boss_vel;
        List<Vector2> coin_pos_list = new List<Vector2>();
        Rectangle rec_Gubbe;
        Rectangle rec_coin;
        int coinsCollected = 0;
        int coinCount = 0;
        int health;
        bool hit = false;
        Random slump = new Random();
        private SpriteFont spriteFont;
        private int totalCoinsCollected = 0;
        private float model1Timer = 0f;
        private const float model1MovementDuration = 2f;
        bool isBossActive = false;
        bool isHitCooldown = false;
        float hitCooldownDuration = 1f; 
        float elapsedHitCooldownTime = 0f;
      
        bool menu = true;
        bool playing = false;
        
        int difficulty = 1;
        string mode;
        

        // healing behövs, lägg till ett hjärta som man kan hämta upp.

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = 1680; // Set the preferred width
            _graphics.PreferredBackBufferHeight = 980; // Set the preferred height
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        protected override void Initialize()
        {

            Gubbe_vel.X = 8f;
            Gubbe_vel.Y = 6f;

            model1_pos.X = 500;
            model1_pos.Y = 100;

            Gubbe_pos = GetRandomPositionWithinBounds(50); // Get random position for Gubbe
            model1_pos = GetRandomPositionAwayFromGubbe(Gubbe_pos, 300); // Get random position for model1 away from Gubbe
            SpawnCoins();
          
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            coin = Content.Load<Texture2D>("Bilder/coin (1)");
            Boss = Content.Load<Texture2D>("Bilder/Boss2");
            model1 = Content.Load<Texture2D>("Bilder/model1");
            Gubbe = Content.Load<Texture2D>("Bilder/Gubbe");
            spriteFont = Content.Load<SpriteFont>("Fonts/File");
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (menu == true && playing == false)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.Z))
                {
                    difficulty = 0;


                }
                if (keyboardState.IsKeyDown(Keys.X))
                {
                    difficulty = 1;


                }
                if (keyboardState.IsKeyDown(Keys.C))
                {
                    difficulty = 2;

                }
                if (difficulty == 0)
                {
                    health = 15;
                    Gubbe_vel.X = 8f;
                    Gubbe_vel.Y = 6f;
                }
                if (difficulty == 1)
                {
                    health = 10;
                    Gubbe_vel.X = 7f;
                    Gubbe_vel.Y = 5f;
                }
                if (difficulty == 2)
                {
                    health = 6;
                    Gubbe_vel.X = 5f;
                    Gubbe_vel.Y = 4f;
                }


            }
            if (menu == false && playing == true)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.W))
                    Gubbe_pos.Y -= Gubbe_vel.Y;
                if (keyboardState.IsKeyDown(Keys.S))
                    Gubbe_pos.Y += Gubbe_vel.Y;
                if (keyboardState.IsKeyDown(Keys.A))
                    Gubbe_pos.X -= Gubbe_vel.X;
                if (keyboardState.IsKeyDown(Keys.D))
                    Gubbe_pos.X += Gubbe_vel.X;

                if (Gubbe_pos.X > Window.ClientBounds.Width - (Gubbe.Width * 0.2f))
                    Gubbe_pos.X = Window.ClientBounds.Width - (Gubbe.Width * 0.2f);

                if (Gubbe_pos.X < 0)
                    Gubbe_pos.X = 0;

                if (Gubbe_pos.Y > Window.ClientBounds.Height - (Gubbe.Height * 0.2f))
                    Gubbe_pos.Y = Window.ClientBounds.Height - (Gubbe.Height * 0.2f);

                if (Gubbe_pos.Y < 0)
                    Gubbe_pos.Y = 0;

                if (totalCoinsCollected >= 30 && !isBossActive)
                {
                    isBossActive = true; // Activate the boss
                    model1_pos = new Vector2(-100, -100); // Move the model1 off-screen (you can adjust the position as needed)
                }
                rec_Gubbe = new Rectangle(Convert.ToInt32(Gubbe_pos.X), Convert.ToInt32(Gubbe_pos.Y), (int)(Gubbe.Width * 0.2f), (int)(Gubbe.Height * 0.2f));

                foreach (Vector2 cn in coin_pos_list.ToList())
                {
                    rec_coin = new Rectangle(Convert.ToInt32(cn.X), Convert.ToInt32(cn.Y), (int)(coin.Width * 0.8f), (int)(coin.Height * 0.8f));
                    hit = CheckCollision(rec_Gubbe, rec_coin);

                    if (hit == true)
                    {
                        coin_pos_list.Remove(cn);
                        coinsCollected++;
                        totalCoinsCollected++;
                        hit = false;
                    }
                }

                coinCount = coinsCollected;

                if (coinCount % 5 == 0 && coinsCollected > 0)
                {
                    SpawnCoins();
                    coinsCollected = 0;
                }
                model1_pos += model1_vel;

                float distanceToGubbe = Vector2.Distance(model1_pos, Gubbe_pos);

                // Check if model1 should follow Gubbe
                if (distanceToGubbe <= 250)
                {
                    // Calculate the direction vector towards Gubbe
                    Vector2 directionToGubbe = Gubbe_pos - model1_pos;
                    directionToGubbe.Normalize();

                    // Update the velocity of model1 to follow Gubbe
                    model1_vel = directionToGubbe * 2f;

                    // Reset the timer when model1 can see Gubbe
                    model1Timer = 0f;
                }
                else
                {
                    // Increment the timer when model1 cannot see Gubbe
                    model1Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Check if the timer has exceeded the movement duration
                    if (model1Timer >= model1MovementDuration)
                    {
                        // Generate random direction for model1
                        float directionX = (float)slump.NextDouble() - 0.5f; // Random value between -0.5 and 0.5
                        float directionY = (float)slump.NextDouble() - 0.5f; // Random value between -0.5 and 0.5
                        Vector2 direction = new Vector2(directionX, directionY);
                        direction.Normalize();

                        // Update the velocity of model1 to move randomly
                        model1_vel = direction * 2f;

                        // Reset the timer
                        model1Timer = 0f;
                    }
                }
                Rectangle rec_model1 = new Rectangle(Convert.ToInt32(model1_pos.X), Convert.ToInt32(model1_pos.Y), (int)(model1.Width * 0.2f), (int)(model1.Height * 0.2f));
                if (rec_Gubbe.Intersects(rec_model1))
                {
                    model1_pos.X = Window.ClientBounds.Width - model1.Width;
                    model1_pos.Y = slump.Next(0, Window.ClientBounds.Height - model1.Height);
                    health--;
                }


                if (model1_pos.X < 0)
                    model1_pos.X = 0;
                if (model1_pos.X > Window.ClientBounds.Width - (model1.Width * 0.2f))
                    model1_pos.X = Window.ClientBounds.Width - (model1.Width * 0.2f);
                if (model1_pos.Y < 0)
                    model1_pos.Y = 0;
                if (model1_pos.Y > Window.ClientBounds.Height - (model1.Height * 0.2f))
                    model1_pos.Y = Window.ClientBounds.Height - (model1.Height * 0.2f);
                // Update the position of model1 based on velocity
                model1_pos += model1_vel;

                if (model1_pos.X < 0 || model1_pos.X > Window.ClientBounds.Width - (model1.Width * 0.2f))
                {
                    // Reverse the X velocity to change direction instantly
                    model1_vel.X *= -1;
                }
                if (model1_pos.Y < 0 || model1_pos.Y > Window.ClientBounds.Height - (model1.Height * 0.2f))
                {
                    // Reverse the Y velocity to change direction instantly
                    model1_vel.Y *= -1;
                }
                if (Boss_pos.X < 0 || Boss_pos.X > Window.ClientBounds.Width - (Boss.Width))
                {

                    Boss_vel.X *= -1;
                }
                if (Boss_pos.Y < 0 || Boss_pos.Y > Window.ClientBounds.Height - (Boss.Height))
                {

                    Boss_vel.Y *= -1;
                }

                if (isBossActive)
                {
                    // Calculate the direction vector towards the player
                    Vector2 directionToPlayer = Gubbe_pos - Boss_pos;
                    directionToPlayer.Normalize();

                    Boss_vel = directionToPlayer * 0.5f;

                }
                if (isHitCooldown)
                {
                    // Update the elapsed cooldown time
                    elapsedHitCooldownTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Check if the cooldown duration has passed
                    if (elapsedHitCooldownTime >= hitCooldownDuration)
                    {
                        // Reset the cooldown
                        isHitCooldown = false;
                        elapsedHitCooldownTime = 0f;
                    }
                }
                // Check collision between Gubbe and Boss (if Boss is active)
                if (isBossActive && !isHitCooldown)
                {
                    Rectangle rec_Boss = new Rectangle((int)Boss_pos.X, (int)Boss_pos.Y, (int)((int)Boss.Width * 0.9f), (int)((int)Boss.Height * 0.9f));

                    if (rec_Gubbe.Intersects(rec_Boss))
                    {
                        health -= 5;
                        hit = true;
                        Boss_pos = GetRandomPositionWithinBounds(70);
                        isHitCooldown = true;

                    }
                }

                // Update the model1 position and timer
                model1Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;


                // Update the Boss position and timer (if Boss is active)
                if (isBossActive)
                {
                    Boss_pos.X += Boss_vel.X;
                    Boss_pos.Y += Boss_vel.Y;

                    if (Boss_pos.X <= 0 || Boss_pos.X >= _graphics.PreferredBackBufferWidth - Boss.Width)
                        Boss_vel.X *= -1;
                    if (Boss_pos.Y <= 0 || Boss_pos.Y >= _graphics.PreferredBackBufferHeight - Boss.Height)
                        Boss_vel.Y *= -1;
                }

                if (health <= 0)
                {
                    Exit();

                }
            }

          
            base.Update(gameTime); 
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (totalCoinsCollected >= 50)
            {
                _spriteBatch.Begin();
                menu = false;
                playing = false;
                string text = "Congratulations! You have Won!";
                Vector2 textPos = new Vector2(200, 40);
                _spriteBatch.DrawString(spriteFont, text, textPos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                string text1 = "To go back to menu press L!";
                Vector2 text1Pos = new Vector2(200, 100);
                _spriteBatch.DrawString(spriteFont, text1, text1Pos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.L))
                {
                    menu = true;
                    totalCoinsCollected = 0;
                    isBossActive = false;
                    
                }
                _spriteBatch.End();
            }
            

            if (menu == true && playing == false)
            {
                _spriteBatch.Begin();
                string meny = "Welcome! Press 'N' to start! Press 'ESC' to quit and press 'L' to go to menu!";
                Vector2 posmeny = new Vector2(200, 40);
                _spriteBatch.DrawString(spriteFont, meny, posmeny, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.N))
                {
                    menu = false;
                    playing = true;
                }
                if (difficulty == 0)
                {
                    mode = "Easy";   
                }
                if (difficulty == 1)
                {
                    mode = "Normal";
                }
                if (difficulty == 2)
                {
                    mode = "Hard";
                }
                string diff = "You are currently playing on " + mode + " difficulty";
                Vector2 diffPos = new Vector2(200, 100);
                _spriteBatch.DrawString(spriteFont, diff, diffPos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
               
                string changeD = "Press 'Z' to change Difficulty to Easy. Press 'X' to change to Normal. Press 'C' to change to Hard." ;
                Vector2 changePos = new Vector2(200, 160);
                _spriteBatch.DrawString(spriteFont, changeD, changePos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                _spriteBatch.End();
            }
            if (menu == false && playing == true)
            {
                _spriteBatch.Begin();
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.L))
                {
                    menu = true;
                    playing = false;
                }
                _spriteBatch.Draw(Gubbe, Gubbe_pos, null, Color.White, 0f, Vector2.Zero, 0.20f, SpriteEffects.None, 0f);

                foreach (Vector2 cn in coin_pos_list)
                {
                    Rectangle coinRect = new Rectangle(Convert.ToInt32(cn.X), Convert.ToInt32(cn.Y), (int)(coin.Width * 0.8f), (int)(coin.Height * 0.8f));
                    _spriteBatch.Draw(coin, coinRect, Color.White);
                }
                if (!isBossActive)
                {
                    _spriteBatch.Draw(model1, model1_pos, null, Color.White, 0f, Vector2.Zero, 0.2f, SpriteEffects.None, 0f);
                }
                if (isBossActive)
                {
                    _spriteBatch.Draw(Boss, Boss_pos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                string text = "Lives: " + health + "   Coins: " + totalCoinsCollected;
                Vector2 textPosition = new Vector2(10, 10);
                _spriteBatch.DrawString(spriteFont, text, textPosition, Color.White);
                if (health <= 0)
                {
                    string text1 = "YOU HAVE LOST!";
                    Vector2 textPosition1 = new Vector2(50, 50);
                    _spriteBatch.DrawString(spriteFont, text1, textPosition1, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                    playing = false;
                    menu = true;
                }
                _spriteBatch.End();


            }
            
            base.Draw(gameTime);
        }
 
            public bool CheckCollision(Rectangle gubbe, Rectangle coin)
            {
                return gubbe.Intersects(coin);
            }

            private void SpawnCoins()
            {
                coin_pos_list.Clear(); // Clear the list of existing coins

                for (int i = 0; i < 5; i++)
                {
                    // Generate a new coin position
                    Vector2 newCoinPos;
                    bool isOverlapping;

                    do
                    {
                        isOverlapping = false;

                        // Generate random position
                        newCoinPos.X = slump.Next(0, Window.ClientBounds.Width - 50);
                        newCoinPos.Y = slump.Next(0, Window.ClientBounds.Height - 50);

                        // Check if the new coin position is too close to any existing coins
                        foreach (Vector2 existingPos in coin_pos_list)
                        {
                            if (Vector2.Distance(newCoinPos, existingPos) < 160)
                            {
                                isOverlapping = true;
                                break;
                            }
                        }

                        // Check if the new coin position is too close to the Gubbe character
                        if (Vector2.Distance(newCoinPos, Gubbe_pos) < 50)
                        {
                            isOverlapping = true;
                        }
                        if (Vector2.Distance(newCoinPos, model1_pos) < 50)
                        {
                            isOverlapping = true;
                        }
                    }
                    while (isOverlapping);

                    // Add the non-overlapping coin position to the list
                    coin_pos_list.Add(newCoinPos);
                }
            }
            private Vector2 GetRandomPositionWithinBounds(int padding)
            {
                int minX = padding;
                int minY = padding;
                int maxX = Window.ClientBounds.Width - padding;
                int maxY = Window.ClientBounds.Height - padding;

                Vector2 position = new Vector2(slump.Next(minX, maxX), slump.Next(minY, maxY));
                return position;
            }

            private Vector2 GetRandomPositionAwayFromGubbe(Vector2 gubbePosition, float minDistance)
            {
                Vector2 position;
                float distance;

                do
                {
                    position = GetRandomPositionWithinBounds(50); // Get a randoms position
                    distance = Vector2.Distance(position, gubbePosition); // Calculate the distance between the position and Gubbe
                }
                while (distance < minDistance); // Repeat until the position is far enough from Gubbe

                return position;
            }

        }
    }
