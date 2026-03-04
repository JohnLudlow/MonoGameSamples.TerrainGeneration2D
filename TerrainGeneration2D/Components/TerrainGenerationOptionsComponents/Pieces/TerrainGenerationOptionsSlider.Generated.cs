//Code for TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsSlider (Container)
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
namespace TerrainGeneration2D.Components.TerrainGenerationOptionsComponents.Pieces;
partial class TerrainGenerationOptionsSlider : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsSlider");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsSlider - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new TerrainGenerationOptionsSlider(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(TerrainGenerationOptionsSlider)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("TerrainGenerationOptionsComponents/Pieces/TerrainGenerationOptionsSlider", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime OptionLabel { get; protected set; }
    public Slider SliderInstance { get; protected set; }
    public TextRuntime ValueIndicator { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public string OptionName
    {
        get => OptionLabel.Text;
        set => OptionLabel.Text = value;
    }

    public float SliderInstanceSliderPercent
    {
        get => SliderInstance.SliderPercent;
        set => SliderInstance.SliderPercent = value;
    }


    public string OptionValue
    {
        get => ValueIndicator.Text;
        set => ValueIndicator.Text = value;
    }

    public TerrainGenerationOptionsSlider(InteractiveGue visual) : base(visual)
    {
    }
    public TerrainGenerationOptionsSlider()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        OptionLabel = this.Visual?.GetGraphicalUiElementByName("OptionLabel") as global::MonoGameGum.GueDeriving.TextRuntime;
        SliderInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Slider>(this.Visual,"SliderInstance");
        ValueIndicator = this.Visual?.GetGraphicalUiElementByName("ValueIndicator") as global::MonoGameGum.GueDeriving.TextRuntime;
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
