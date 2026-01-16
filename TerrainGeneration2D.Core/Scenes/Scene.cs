using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Scenes;

public abstract class Scene : IDisposable
{
  protected ContentManager Content { get; }
  public bool IsDisposed { get; private set; }

  protected Scene()
  {
    Content = new ContentManager(GameCore.Content.ServiceProvider)
    {
      RootDirectory = GameCore.Content.RootDirectory
    };
  }

  ~Scene() => Dispose(false);

  public virtual void Initialize()
  {
    LoadContent();
  }

  public virtual void LoadContent()
  {

  }

  public virtual void Update(GameTime gameTime)
  {

  }

  public virtual void Draw(GameTime gameTime)
  {

  }

  public virtual void UnloadContent()
  {
    Content.Unload();
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (IsDisposed)
    {
      return;
    }

    if (disposing)
    {
      UnloadContent();
      Content?.Dispose();
    }

    IsDisposed = true;
  }
}