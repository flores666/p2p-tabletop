using System.Numerics;
using ImGuiNET;

namespace P2PVTT.Components;

public static class UsefulVars
{
    public static string SharedPopupName => "Popup";
    public static Vector2 Center => ImGui.GetMainViewport().GetCenter();
    public static float PredictedButtonHeight =>
        ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y * 2;
}
