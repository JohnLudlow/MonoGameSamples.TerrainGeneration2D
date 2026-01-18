/// <summary>
/// Entry point for the Content Builder project, 
/// which when executed will build content according to the "Content Collection Strategy" defined in the Builder class.
/// </summary>
/// <remarks>
/// Make sure to validate the directory paths in the "ContentBuilderParams" for your specific project.
/// For more details regarding the Content Builder, see the MonoGame documentation: <tbc.>
/// </remarks>

using MonoGame.Framework.Content.Pipeline.Builder;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Content.Builder;

internal sealed class TerrainGeneration2DContentBuilder : ContentBuilder
{
  public override IContentCollection GetContentCollection()
  {
    var contentCollection = new ContentCollection();

    // include everything in the folder
    contentCollection.Include<WildcardRule>("images/atlas.png");
    contentCollection.Include<WildcardRule>("images/background-pattern.png");
    contentCollection.IncludeCopy<WildcardRule>("images/atlas-definition.xml");
    contentCollection.IncludeCopy<WildcardRule>("images/tileset-definition.xml");

    contentCollection.Include<WildcardRule>("images/terrain-atlas.png");
    contentCollection.IncludeCopy<WildcardRule>("images/terrain-tileset-definition.xml");

    contentCollection.Include<WildcardRule>("audio/ui.wav");
    contentCollection.Include<WildcardRule>("audio/bounce.wav");
    contentCollection.Include<WildcardRule>("audio/collect.wav");
    contentCollection.Include<WildcardRule>("audio/theme.ogg");

    contentCollection.IncludeCopy<WildcardRule>("fonts/NotArial_0.png");
    contentCollection.IncludeCopy<WildcardRule>("fonts/NotArial.fnt");

    contentCollection.Include<WildcardRule>("effects/grayscaleEffect.fx");

    // By default, all content will be imported from the Assets folder using the default importer for their file type.
    // Please add any custom content collection rules here.    
    return contentCollection;
  }
}