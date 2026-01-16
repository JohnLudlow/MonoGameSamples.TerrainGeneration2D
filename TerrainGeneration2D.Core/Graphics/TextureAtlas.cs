using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class TextureAtlas
{
  private readonly Dictionary<string, TextureRegion> _regions;
  private readonly Dictionary<string, Animation> _animations;
  public Texture2D Texture { get; set; }


  public TextureAtlas(Texture2D texture)
  {
    Texture = texture;
    _regions = [];
    _animations = [];
  }

  /// <summary>
  /// Creates a new region and adds it to this texture atlas.
  /// </summary>
  /// <param name="name">The name to give the texture region.</param>
  /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
  /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
  /// <param name="width">The width, in pixels, of the region.</param>
  /// <param name="height">The height, in pixels, of the region.</param>
  public void AddRegion(string name, int x, int y, int width, int height)
  {
    var region = new TextureRegion(Texture, x, y, width, height);
    _regions.Add(name, region);
  }

  /// <summary>
  /// Gets the region from this texture atlas with the specified name.
  /// </summary>
  /// <param name="name">The name of the region to retrieve.</param>
  /// <returns>The TextureRegion with the specified name.</returns>
  public TextureRegion GetRegion(string name) => _regions[name];

  /// <summary>
  /// Removes the region from this texture atlas with the specified name.
  /// </summary>
  /// <param name="name">The name of the region to remove.</param>
  /// <returns></returns>
  public bool RemoveRegion(string name) => _regions.Remove(name);

  /// <summary>
  /// Adds the given animation to this texture atlas with the specified name.
  /// </summary>
  /// <param name="animationName">The name of the animation to add.</param>
  /// <param name="animation">The animation to add.</param>
  public void AddAnimation(string animationName, Animation animation)
  {
    _animations.Add(animationName, animation);
  }

  /// <summary>
  /// Gets the animation from this texture atlas with the specified name.
  /// </summary>
  /// <param name="animationName">The name of the animation to retrieve.</param>
  /// <returns>The animation with the specified name.</returns>
  public Animation GetAnimation(string animationName)
  {
    return _animations[animationName];
  }

  /// <summary>
  /// Removes the animation with the specified name from this texture atlas.
  /// </summary>
  /// <param name="animationName">The name of the animation to remove.</param>
  /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
  public bool RemoveAnimation(string animationName)
  {
    return _animations.Remove(animationName);
  }

  /// <summary>
  /// Removes all regions from this texture atlas.
  /// </summary>
  public void Clear()
  {
    _regions.Clear();
    _animations.Clear();
  }

  /// <summary>
  /// Creates a new texture atlas based on a texture atlas xml configuration file.
  /// </summary>
  /// <param name="content">The content manager used to load the texture for the atlas.</param>
  /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
  /// <returns>The texture atlas created by this method.</returns>
  public static TextureAtlas FromFile(ContentManager content, string fileName)
  {
    System.ArgumentNullException.ThrowIfNull(content);
    System.ArgumentException.ThrowIfNullOrEmpty(fileName);

    var filePath = Path.Combine(content.RootDirectory, fileName);

    using var stream = TitleContainer.OpenStream(filePath);
    using var reader = XmlReader.Create(stream);

    var doc = XDocument.Load(reader);
    var root = doc.Root ?? throw new XmlException("Texture atlas xml file is missing root element.");

    // The <Texture> element contains the content path for the Texture2D to load.
    // So we will retrieve that value then use the content manager to load the texture.
    var texturePath = root.Element("Texture")!.Value;
    var atlas = new TextureAtlas(
      content.Load<Texture2D>(texturePath)
    );

    LoadTextureRegions(root, atlas);
    LoadAnimations(root, atlas);

    return atlas;
  }

  private static void LoadTextureRegions(XElement root, TextureAtlas atlas)
  {
    // The <Regions> element contains individual <Region> elements, each one describing
    // a different texture region within the atlas.  
    //
    // Example:
    // <Regions>
    //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
    //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
    // </Regions>
    //
    // So we retrieve all of the <Region> elements then loop through each one
    // and generate a new TextureRegion instance from it and add it to this atlas.
    var regions = root.Element("Regions")?.Elements("Region");

    if (regions != null)
    {
      foreach (var region in regions)
      {
        var name = region.Attribute("name")?.Value;
        var x = int.Parse(region.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture);
        var y = int.Parse(region.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture);
        var width = int.Parse(region.Attribute("width")?.Value ?? "0", CultureInfo.InvariantCulture);
        var height = int.Parse(region.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(name))
        {
          atlas.AddRegion(name, x, y, width, height);
        }
      }
    }
  }

  public static void LoadAnimations(XElement root, TextureAtlas atlas)
  {
    // The <Animations> element contains individual <Animation> elements, each one describing
    // a different animation within the atlas.
    //
    // Example:
    // <Animations>
    //      <Animation name="animation" delay="100">
    //          <Frame region="spriteOne" />
    //          <Frame region="spriteTwo" />
    //      </Animation>
    // </Animations>
    //
    // So we retrieve all of the <Animation> elements then loop through each one
    // and generate a new Animation instance from it and add it to this atlas.
    var animationElements = root?.Element("Animations")?.Elements("Animation");

    if (animationElements != null)
    {
      foreach (var animationElement in animationElements)
      {
        var name = animationElement.Attribute("name")?.Value;
        var delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0", CultureInfo.InvariantCulture);
        var delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

        var frames = new List<TextureRegion>();

        var frameElements = animationElement.Elements("Frame");

        if (frameElements != null)
        {
          foreach (var frameElement in frameElements)
          {
            var regionName = frameElement?.Attribute("region")?.Value ?? throw new XmlException("Frame element is missing required region attribute.");
            var region = atlas?.GetRegion(regionName) ?? throw new KeyNotFoundException($"Texture region '{regionName}' not found in atlas.");
            frames.Add(region);
          }
        }

        var animation = new Animation(frames, delay);
        atlas?.AddAnimation(name ?? throw new XmlException("Animation element is missing required name attribute."), animation);
      }
    }
  }

  public Sprite CreateSprite(string regionName)
  {
    var region = GetRegion(regionName);
    return new Sprite(region);
  }

  public AnimatedSprite CreateAnimatedSprite(string animationName)
  {
    var animation = GetAnimation(animationName);
    return new AnimatedSprite(animation.Frames[0], animation);
  }
}