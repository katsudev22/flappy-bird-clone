using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace flappy_bird_clone.FlappyBird;

public static class Globals
{
    public static float Time { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static ContentManager ContentManager { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static Point GameWindow { get; set; } = new(900, 900);
    public static Point VirtualWindow { get; set; } = new(144, 256);

    public static void Update(GameTime gameTime)
    {
        Time = (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}

public class InputManager {
    public static InputManager Instance { get; private set; }

    private KeyboardState _currentKeyboardState;
    private KeyboardState _previousKeyboardState;

    private InputManager() { }

    public static void Initialize() {
        if (Instance is null) Instance = new();
    }

    public static void Update() {
        if (Instance is null) return;

        Instance._previousKeyboardState = Instance._currentKeyboardState;
        Instance._currentKeyboardState = Keyboard.GetState();
    }

    public static bool IsKeyPressed(Keys key) {
        if (Instance is null) return false;
        return Instance._currentKeyboardState.IsKeyDown(key) &&
               Instance._previousKeyboardState.IsKeyUp(key);
    }
}

public class GameManager
{
    public Duck Duck;
    public PipeGenerator PipeGenerator;
    public int Score;
    public bool GameOver;
    
    public GameManager()
    {
        InputManager.Initialize();
        Reset();
    }
    
    public void Update()
    {
        InputManager.Update();
     
        if (GameOver && InputManager.IsKeyPressed(Keys.R)) 
            Reset();
        
        Duck.Update();
        PipeGenerator.Update();
        CheckScore();
        CheckCollision();
    }

    public void Draw()
    {
        Duck.Draw();
        PipeGenerator.Draw();
    }

    private void CheckScore()
    {
        if (GameOver) return;
        
        foreach (Pipe pipe in PipeGenerator.Pipes)
        {
            if (!pipe.Passed && !pipe.IsTop && Duck.Position.X > pipe.Position.X)
            {
                pipe.Passed = true;
                Score++;
                Console.WriteLine($"Pontos: {Score}");
            }
        }
    }

    private void CheckCollision()
    {
        if (GameOver) return;

        foreach (Pipe pipe in PipeGenerator.Pipes)
            if (Duck.Bounds.Intersects(pipe.Bounds))
            {
                Duck.IsDead = true;
                GameOver = true;
            }
    }

    private void Reset()
    {
        Duck = new(new(Globals.GameWindow.X / 2f - 45, Globals.GameWindow.Y / 2f));
        PipeGenerator = new();
        GameOver = false;
        Score = 0;
    }
}

public class Duck
{
    public Texture2D Texture;
    public Vector2 Position;
    public Rectangle Bounds;
    public float Gravity;
    public bool IsDead;
    private Animation Animation;
    private float Rotation;
    
    private int Width = 50;
    private int Height = 50;
    
    public Duck(Vector2 position)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("duck");
        Animation = new(Texture, 3, 0.1f);
        Position = position;
        IsDead = false;
        Rotation = 0;
    }

    public void Update()
    {
        Animation.Update();
        UpdateBounds();
        ApplyGravity();
        Jump();
    }

    public void Draw() =>
        Animation.Draw(Position, Rotation);

    private void UpdateBounds() =>
        Bounds = new((int)(Position.X - Texture.Width / 2f), (int)(Position.Y - Texture.Height / 2f), Texture.Width, Texture.Height);

    private void ApplyGravity()
    {
        Gravity += 800f * Globals.Time;
        float maxGravity = 1200f;
        
        if (Gravity >= maxGravity)
            Gravity = maxGravity;

        Rotation += 0.01f;
        Position.Y += Gravity * Globals.Time;
    }

    private void Jump()
    {
        if (!InputManager.IsKeyPressed(Keys.Space) || IsDead) return;
        Rotation = -0.01f;
        Gravity = -250f;
    }
}

public class Pipe
{
    public Texture2D Texture;
    public Vector2 Position;
    public Rectangle Bounds;
    public Vector2 Direction;
    public float Speed;
    public bool Passed;
    public bool IsTop;

    public Pipe(Vector2 position, bool isTop)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("pipe");
        Position = position;
        Direction = new(-1, 0);
        Speed = 200f;
        Passed = false;
        IsTop = isTop;
    }

    public void Update()
    {
        UpdateBounds();
        Position += Direction * Speed * Globals.Time;
    }

    public void Draw() =>
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White, 0, new(Texture.Width / 2f, Texture.Height / 2f), Vector2.One, IsTop ? SpriteEffects.FlipVertically : SpriteEffects.None, 1);   
        
    private void UpdateBounds() =>
        Bounds = new((int)(Position.X - Texture.Width / 2f), (int)(Position.Y - Texture.Height / 2f), Texture.Width, Texture.Height);
}

public class PipeGenerator
{
    public List<Pipe> Pipes;
    public float SpawnRate;
    public Random Random;
    
    public PipeGenerator()
    {
        Pipes = new();
        SpawnRate = 0f;
        Random = new();
    }

    public void Update()
    {
        SpawnRate += Globals.Time;
        SpawnPipe();
        
        foreach (Pipe pipe in Pipes)
            pipe.Update();
        
        Pipes.RemoveAll(p => p.Position.X < -200);
    }

    public void Draw()
    {
        foreach (Pipe pipe in Pipes)
            pipe.Draw();
    }

    private void SpawnPipe()
    {
        if (SpawnRate >= 2f)
        {
            int gapSize = 70;
            int pipeHeight = 160;
            
            int minCenterY = 400;
            int maxCenterY = Globals.GameWindow.Y - 400;
            int centerY = Random.Next(minCenterY, maxCenterY);
            
            Vector2 topPipePosition = new(Globals.GameWindow.Y + 100, centerY - gapSize / 2f - pipeHeight / 2f);
            Vector2 bottomPipePosition = new(topPipePosition.X, centerY + gapSize / 2f + pipeHeight / 2f);
            
            Pipe bottomPipe = new(bottomPipePosition, false);
            Pipe topPipe = new(topPipePosition, true);
            Pipes.Add(bottomPipe);
            Pipes.Add(topPipe);
            
            SpawnRate = 0f;
        }
    }
}

public class Animation
{
    private readonly Texture2D Texture;
    private readonly List<Rectangle> SourceRectangle = new();
    private readonly int Frames;
    private int Frame;
    private readonly float FrameTime;
    private float FrameTimeLeft;
    private bool Active = true;

    public Animation(Texture2D texture, int framesX, float frameTime)
    {
        Texture = texture;
        FrameTime = frameTime;
        FrameTimeLeft = FrameTime;
        Frames = framesX;

        int frameWidth = texture.Width / framesX;
        int frameHeight = texture.Height;
        
        for (int i = 0; i < Frames; i++)
            SourceRectangle.Add(new(i * frameWidth, 0, frameWidth, frameHeight));
    }

    public void Stop() => Active = false;

    public void Start() => Active = true;

    public void Reset()
    {
        Frame = 0;
        FrameTimeLeft = FrameTime;
    }

    public void Update()
    {
        if (!Active) return;

        FrameTimeLeft -= Globals.Time;

        if (FrameTimeLeft <= 0)
        {
            FrameTimeLeft += FrameTime;
            Frame = (Frame + 1) % Frames;
        }
    }

    public void Draw(Vector2 position, float rotation) =>
        Globals.SpriteBatch.Draw(Texture, position, SourceRectangle[Frame], Color.White, rotation, new(Texture.Width / 2f, Texture.Height / 2f), Vector2.One, SpriteEffects.None, 1);
}
