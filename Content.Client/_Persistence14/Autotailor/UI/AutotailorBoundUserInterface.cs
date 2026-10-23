using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Persistence14.Autotailor.UI;

[UsedImplicitly]
public sealed class AutotailorBoundUserInterface : BoundUserInterface
{
    public AutotailorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
}
