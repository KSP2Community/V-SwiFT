using JetBrains.Annotations;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace VSwift.Selectables;

[PublicAPI]
public sealed class PredefinedDynamicNodeSelectable : BaseSelectable
{
    public JArray Nodes;
    public PartSelectable Selectable;
    public PredefinedDynamicNodeSelectable(JArray predefinedDynamicNodes, PartSelectable partSelectable)
    {
        Nodes = predefinedDynamicNodes;
        Selectable = partSelectable;
        Children = [];
        Classes = [];
        
        foreach (var node in predefinedDynamicNodes)
        {
            var obj = (JObject)node;
            Children.Add(new JTokenSelectable(Selectable.SetModified, obj, n => n["nodeID"].Value<string>(),
                "attach_node"));
            Classes.Add(obj["nodeID"]!.Value<string>());
        }
    }
    
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        foreach (var node in Nodes)
        {
            var obj = (JObject)node;
            if (obj["nodeID"]!.Value<string>() != @class) continue;
            classValue = DataValue.FromJToken(obj);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other) =>
        other is PredefinedDynamicNodeSelectable predefinedDynamicNodeSelectable &&
        predefinedDynamicNodeSelectable.Nodes == Nodes;

    public override IModifiable OpenModification() => new JTokenModifiable(Nodes, Selectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var engineModeData = new AttachNodeDefinition()
        {
            nodeID = elementType
        };
        var json = JObject.FromObject(engineModeData);
            Nodes.Add(json);
        var selectable =  new JTokenSelectable(Selectable.SetModified, json, mode => mode["nodeID"].Value<string>(),
            "attach_node");
        Children.Add(selectable);
        return selectable;
    }

    public override string Serialize() => Nodes.ToString(Formatting.Indented);

    public override DataValue GetValue() => DataValue.FromJToken(Nodes);

    public override List<ISelectable> Children { get; }
    public override string Name => nameof(PredefinedDynamicNodeSelectable);
    public override List<string> Classes { get; }
    public override string ElementType => Name;
}