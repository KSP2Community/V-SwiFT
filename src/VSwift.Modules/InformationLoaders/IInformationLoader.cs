using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.InformationLoaders;

public interface IInformationLoader
{
    public void LoadInformationInto(PartData partData, JToken storedInformation);
}