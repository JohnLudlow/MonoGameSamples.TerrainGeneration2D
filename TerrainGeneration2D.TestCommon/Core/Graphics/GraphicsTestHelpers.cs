using System.Runtime.Serialization;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.TestCommon.Core.Graphics;

public static class GraphicsTestHelpers
{
  public static Tileset CreateMockTileset(int tileCount)
  {
    // Use a minimal fake Texture2D that does not require a real GraphicsDevice
    var fakeTexture = (Texture2D?)FormatterServices.GetUninitializedObject(typeof(Texture2D));
    var region = new TextureRegion(fakeTexture!, 0, 0, tileCount, 1);
    return new Tileset(region, 1, 1);
  }
}
