using flappy_bird_clone.FlappyBird;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace flappy_bird_clone;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GameManager _gameManager;

    private Matrix _screenScaleMatrix;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = Globals.GameWindow.X;
        _graphics.PreferredBackBufferHeight = Globals.GameWindow.Y;
        _graphics.ApplyChanges();

        UpdateMatrixAndViewport();
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Globals.GraphicsDevice = GraphicsDevice;
        Globals.SpriteBatch = new SpriteBatch(Globals.GraphicsDevice);
        Globals.ContentManager = Content;

        _gameManager = new();
    }

    protected override void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);
        _gameManager.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Globals.GraphicsDevice.Clear(Color.Black);
        Globals.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _screenScaleMatrix);
        _gameManager.Draw();
        Globals.SpriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void UpdateMatrixAndViewport()
    {
        float zoom = 4f;

        Vector2 center = new Vector2(Globals.GameWindow.X / 2f, Globals.GameWindow.Y / 2f);

        _screenScaleMatrix = 
            Matrix.CreateTranslation(-center.X, -center.Y, 0) * 
            Matrix.CreateScale(zoom, zoom, 1f) * 
            Matrix.CreateTranslation(center.X, center.Y, 0);
    }
}
