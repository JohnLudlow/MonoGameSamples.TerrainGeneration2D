//Code for TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsPanel (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
using TerrainGeneration2D.Components.Controls;
using TerrainGeneration2D.Components.TerrainGenerationOptionsComponents.Pieces;
namespace TerrainGeneration2D.Components.TerrainGenerationOptionsComponents.Pieces;
partial class TerrainGenerationOptionsPanel : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsPanel");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsPanel - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new TerrainGenerationOptionsPanel(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(TerrainGenerationOptionsPanel)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsPanel", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TerrainGroupGenerationOptionsPanel OceanGroupGenerationOptionsPanel { get; protected set; }
    public TerrainGroupGenerationOptionsPanel BeachGroupGenerationOptionsPanel { get; protected set; }
    public TerrainGroupGenerationOptionsPanel PlainsGroupGenerationOptionsPanel { get; protected set; }
    public TerrainGroupGenerationOptionsPanel ForestGroupGenerationOptionsPanel { get; protected set; }
    public TerrainGroupGenerationOptionsPanel SnowGroupGenerationOptionsPanel { get; protected set; }
    public TerrainGroupGenerationOptionsPanel MountainGroupGenerationOptionsPanel1 { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public TerrainGenerationOptionsPanel(InteractiveGue visual) : base(visual)
    {
    }
    public TerrainGenerationOptionsPanel()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        OceanGroupGenerationOptionsPanel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGroupGenerationOptionsPanel>(this.Visual,"OceanGroupGenerationOptionsPanel");
        BeachGroupGenerationOptionsPanel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGroupGenerationOptionsPanel>(this.Visual,"BeachGroupGenerationOptionsPanel");
        PlainsGroupGenerationOptionsPanel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGroupGenerationOptionsPanel>(this.Visual,"PlainsGroupGenerationOptionsPanel");
        ForestGroupGenerationOptionsPanel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGroupGenerationOptionsPanel>(this.Visual,"ForestGroupGenerationOptionsPanel");
        SnowGroupGenerationOptionsPanel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGroupGenerationOptionsPanel>(this.Visual,"SnowGroupGenerationOptionsPanel");
        MountainGroupGenerationOptionsPanel1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGroupGenerationOptionsPanel>(this.Visual,"MountainGroupGenerationOptionsPanel1");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
