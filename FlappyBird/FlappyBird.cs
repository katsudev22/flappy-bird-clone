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
    public static Point GameWindow { get; set; } = new(600, 900);

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
    
    public GameManager()
    {
        InputManager.Initialize();
        Duck = new(new(Globals.GameWindow.X / 2, 0));
    }
    
    public void Update()
    {
        InputManager.Update();
        Duck.Update();
    }

    public void Draw()
    {
        Duck.Draw();
    }
}

public class Duck
{
    public Texture2D Texture;
    public Vector2 Position;
    public Rectangle Bounds;
    public float Gravity;
    
    private int Width = 50;
    private int Height = 50;
    
    public Duck(Vector2 position)
    {
        Texture = new(Globals.GraphicsDevice, 1, 1);
        Texture.SetData([ Color.YellowGreen ]);
        Position = position;
    }

    public void Update()
    {
        UpdateBounds();
        ApplyGravity();
        Jump();
    }

    public void Draw() =>
        Globals.SpriteBatch.Draw(Texture, Position, Bounds, Color.White, 0f, new(Width / 2, Height / 2), 1f, SpriteEffects.None, 0f);

    private void UpdateBounds() =>
        Bounds = new((int)Position.X, (int)Position.Y, Width, Height);

    private void ApplyGravity()
    {
        Gravity += 800f * Globals.Time;
        float maxGravity = 1200f;
        
        if (Gravity >= maxGravity)
            Gravity = maxGravity;
        
        Position.Y += Gravity * Globals.Time;
    }

    private void Jump()
    {
        if (!InputManager.IsKeyPressed(Keys.Space)) return;
        Gravity = -450f;
    }
    
}
