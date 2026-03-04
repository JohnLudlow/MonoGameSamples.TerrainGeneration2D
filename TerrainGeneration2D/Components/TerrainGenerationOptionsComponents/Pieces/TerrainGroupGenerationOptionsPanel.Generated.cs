//Code for TerrainGenerationOptionsComponents/Pieces/TerrainGroupGenerationOptionsPanel (Container)
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
partial class TerrainGroupGenerationOptionsPanel : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("TerrainGenerationOptionsComponents/Pieces/TerrainGroupGenerationOptionsPanel");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named TerrainGenerationOptionsComponents/Pieces/TerrainGroupGenerationOptionsPanel - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new TerrainGroupGenerationOptionsPanel(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(TerrainGroupGenerationOptionsPanel)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("TerrainGenerationOptionsComponents/Pieces/TerrainGroupGenerationOptionsPanel", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime GroupName { get; protected set; }
    public TerrainGenerationOptionsSlider MinGroupSizeXSlider { get; protected set; }
    public TerrainGenerationOptionsSlider MaxGroupSizeXSlider { get; protected set; }
    public TerrainGenerationOptionsSlider MinGroupSizeYSlider { get; protected set; }
    public TerrainGenerationOptionsSlider MaxGroupSizeYSlider { get; protected set; }
    public TerrainGenerationOptionsSlider MinGroupElevationSlider { get; protected set; }
    public TerrainGenerationOptionsSlider MaxGroupElevationSlider { get; protected set; }
    public TextRuntime NoiseProvider { get; protected set; }
    public TerrainGenerationOptionsSlider NoiseThresholdSlider { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public string TerrainGroupName
    {
        get => GroupName.Text;
        set => GroupName.Text = value;
    }

    public float MaxGroupElevationSliderSliderInstanceSliderPercent
    {
        get => MaxGroupElevationSlider.SliderInstanceSliderPercent;
        set => MaxGroupElevationSlider.SliderInstanceSliderPercent = value;
    }

    public float MaxGroupSizeXSliderSliderInstanceSliderPercent
    {
        get => MaxGroupSizeXSlider.SliderInstanceSliderPercent;
        set => MaxGroupSizeXSlider.SliderInstanceSliderPercent = value;
    }

    public float MaxGroupSizeYSliderSliderInstanceSliderPercent
    {
        get => MaxGroupSizeYSlider.SliderInstanceSliderPercent;
        set => MaxGroupSizeYSlider.SliderInstanceSliderPercent = value;
    }

    public float MinGroupElevationSliderSliderInstanceSliderPercent
    {
        get => MinGroupElevationSlider.SliderInstanceSliderPercent;
        set => MinGroupElevationSlider.SliderInstanceSliderPercent = value;
    }

    public float MinGroupSizeXSliderSliderInstanceSliderPercent
    {
        get => MinGroupSizeXSlider.SliderInstanceSliderPercent;
        set => MinGroupSizeXSlider.SliderInstanceSliderPercent = value;
    }

    public float MinGroupSizeYSliderSliderInstanceSliderPercent
    {
        get => MinGroupSizeYSlider.SliderInstanceSliderPercent;
        set => MinGroupSizeYSlider.SliderInstanceSliderPercent = value;
    }

    public string NoiseProviderText
    {
        get => NoiseProvider.Text;
        set => NoiseProvider.Text = value;
    }

    public float NoiseThresholdSliderSliderInstanceSliderPercent
    {
        get => NoiseThresholdSlider.SliderInstanceSliderPercent;
        set => NoiseThresholdSlider.SliderInstanceSliderPercent = value;
    }

    public TerrainGroupGenerationOptionsPanel(InteractiveGue visual) : base(visual)
    {
    }
    public TerrainGroupGenerationOptionsPanel()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        GroupName = this.Visual?.GetGraphicalUiElementByName("GroupName") as global::MonoGameGum.GueDeriving.TextRuntime;
        MinGroupSizeXSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"MinGroupSizeXSlider");
        MaxGroupSizeXSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"MaxGroupSizeXSlider");
        MinGroupSizeYSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"MinGroupSizeYSlider");
        MaxGroupSizeYSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"MaxGroupSizeYSlider");
        MinGroupElevationSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"MinGroupElevationSlider");
        MaxGroupElevationSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"MaxGroupElevationSlider");
        NoiseProvider = this.Visual?.GetGraphicalUiElementByName("NoiseProvider") as global::MonoGameGum.GueDeriving.TextRuntime;
        NoiseThresholdSlider = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<TerrainGenerationOptionsSlider>(this.Visual,"NoiseThresholdSlider");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
