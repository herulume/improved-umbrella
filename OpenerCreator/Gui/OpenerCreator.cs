using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal;
using ImGuiNET;
using OpenerCreator.Helpers;
using OpenerCreator.Managers;

namespace OpenerCreator.Gui;

public class OpenerCreator : IDisposable
{
    public bool Enabled;
    public List<uint> Actions;

    private Dictionary<uint, IDalamudTextureWrap> iconCache;
    private string search;
    private List<uint> filteredActions;

    const int iconSize = 32;

    public OpenerCreator()
    {
        Enabled = false;
        Actions = new();
        iconCache = new();
        search = "";
        filteredActions = ActionDictionary.Instance.ToIdList();
    }

    public void Dispose()
    {
        foreach (var v in iconCache)
            v.Value.Dispose();
    }

    public void Draw()
    {
        if (!Enabled)
            return;

        ImGui.SetNextWindowSizeConstraints(new Vector2(100, 100), new Vector2(4000, 2000));
        ImGui.Begin("Opener Creator", ref Enabled);

        var spacing = ImGui.GetStyle().ItemSpacing;
        var padding = ImGui.GetStyle().FramePadding;
        var icons_per_line = (int)Math.Floor((ImGui.GetContentRegionAvail().X - padding.X * 2.0 + spacing.X) / (iconSize + spacing.X));
        var lines = (float)Math.Max(Math.Ceiling(Actions.Count / (float)icons_per_line), 1);
        ImGui.BeginChildFrame(2426787, new Vector2(ImGui.GetContentRegionAvail().X, lines * (iconSize + spacing.Y) - spacing.Y + padding.Y * 2), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        int? delete = null;
        for (var i = 0; i < Actions.Count; i++)
        {
            if (i > 0)
            {
                ImGui.SameLine();
                if (ImGui.GetContentRegionAvail().X < iconSize)
                    ImGui.NewLine();
            }

            ImGui.Image(GetIcon(Actions[i]), new Vector2(iconSize, iconSize));
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(ActionDictionary.Instance.GetActionName(Actions[i]));
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                delete = i;
        }

        if (delete != null)
            Actions.RemoveAt(delete.Value);

        ImGui.Dummy(Vector2.Zero);
        ImGui.EndChildFrame();



        // ability filter
        ImGui.BeginChild("allactions");
        if (ImGui.InputText("Search", ref search, 64))
        {
            if (search.Length > 0)
                filteredActions = ActionDictionary.Instance.GetActionsByName(search);
            else
                filteredActions = ActionDictionary.Instance.ToIdList();
        }

        ImGui.Text($"{filteredActions.Count} Results");
        ImGui.SameLine();
        if (ImGui.Button("Save opener"))
        {
            OpenerManager.Instance.AddOrUpdate("live", Actions);
            global::OpenerCreator.OpenerCreator.ChatGui.Print(new XivChatEntry
            {
                Message = "Opener saved.",
                Type = XivChatType.Echo
            });
        }

        for (var i = 0; i < filteredActions.Count; i++)
        {
            var action = ActionDictionary.Instance.GetAction(filteredActions[i]);
            ImGui.Image(GetIcon(filteredActions[i]), new Vector2(iconSize, iconSize));
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                Actions.Add(filteredActions[i]);

            ImGui.SameLine();
            ImGui.Text(action.Name.ToString());
        }

        ImGui.EndChild();

        ImGui.End();
    }

    private nint GetIcon(uint id)
    {
        if (!iconCache.ContainsKey(id))
        {
            var icon = ActionDictionary.Instance.GetActionIcon(id).ToString("D6");
            var path = $"ui/icon/{icon[0]}{icon[1]}{icon[2]}000/{icon}_hr1.tex";
            // Dalamud.Logging.PluginLog.Log(path);
            var data = global::OpenerCreator.OpenerCreator.DataManager.GetFile<Lumina.Data.Files.TexFile>(path)!;
            var pixels = new byte[data.Header.Width * data.Header.Height * 4];
            for (var i = 0; i < data.Header.Width * data.Header.Height; i++)
            {
                pixels[i * 4 + 0] = data.ImageData[i * 4 + 2];
                pixels[i * 4 + 1] = data.ImageData[i * 4 + 1];
                pixels[i * 4 + 2] = data.ImageData[i * 4 + 0];
                pixels[i * 4 + 3] = data.ImageData[i * 4 + 3];
            }
            iconCache[id] = global::OpenerCreator.OpenerCreator.PluginInterface.UiBuilder.LoadImageRaw(pixels, data.Header.Width, data.Header.Height, 4);
        }

        return iconCache[id].ImGuiHandle;
    }
}