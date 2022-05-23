using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using KiraiMod.Core.UI;
using System;
using UnityEngine;

namespace KiraiMod.Adapter.UIMGUI
{
    [BepInPlugin("me.KiraiHooks.KiraiMod.Adapter.UIMGUI", "Adapter.UIMGUI", "0.1.0")]
    [BepInDependency(Core.UI.Plugin.GUID)]
    public class Plugin : BasePlugin
    {
        // under the assumption that OnGUI does not get called on inactive components,
        // in the future when the keybind manager is upgraded,
        // rather than checking the key state every frame,
        // toggle the monobehaviour instead

        public static ConfigEntry<Color32> BackgroundColor;
        public static ConfigEntry<Color32> ForegroundColor;

        private static readonly UnhollowerBaseLib.Il2CppReferenceArray<GUILayoutOption> options = new(0);
        private static Rect position = new(Screen.width / 2 - 78, Screen.height / 2 - 307, 156, 482);
        private static Vector2 scrollHighest = default;
        private static Vector2 scrollCurrent = default;

        private static UIGroup currentGroup;

        public override void Load()
        {
            TomlTypeConverter.AddConverter(typeof(Color32), new TypeConverter()
            {
                ConvertToObject = (str, type) => Color32Convert.FromString(str),
                ConvertToString = (obj, type) => Color32Convert.FromColor32((Color32)obj),
            });

            BackgroundColor = Config.Bind<Color32>("Visuals", "Background", new(0x56, 0x00, 0xA5, 0xFF), "What color should the background be?");
            ForegroundColor = Config.Bind<Color32>("Visuals", "Foreground", new(0xFF, 0xFF, 0xFF, 0xFF), "What color should the foreground be?");

            AddComponent<MonoHelper>();
        }

        private static void WindowSections(int _)
        {
            scrollHighest = GUILayout.BeginScrollView(scrollHighest, options);

            foreach (UIGroup group in UIManager.HighGroups)
                if (GUILayout.Button(group.name, options))
                    currentGroup = group;

            GUILayout.EndScrollView();

            GUI.DragWindow(new(0, 0, 156, 20));
        }

        private static void WindowElements(int _)
        {
            scrollCurrent = GUILayout.BeginScrollView(scrollCurrent, options);

            foreach (UIElement element in currentGroup.elements)
                if (element is UIElement<UIGroup> groupElem)
                {
                    if (GUILayout.Button(element.name, options))
                        currentGroup = groupElem.Bound._value;
                }
                else if (element is UIElement<bool> groupToggle)
                    groupToggle.Bound.Value = GUILayout.Toggle(groupToggle.Bound._value, element.name, options);
                else if (element is UIElement<float> groupSlider)
                {
                    GUILayout.Label(element.name, options);
                    float val = groupSlider.Bound._value;
                    groupSlider.Bound.Value = GUILayout.HorizontalSlider(val, val - 10, val + 10, options);
                }
                else if (element.GetType() == typeof(UIElement))
                {
                    if (GUILayout.Button(element.name, options))
                        element.Invoke();
                }
                else GUILayout.Label(element.name + " (Unsupported type)", options);

            GUILayout.EndScrollView(true);
        }

        private class MonoHelper : MonoBehaviour
        {
            public MonoHelper(IntPtr ptr) : base(ptr) { }

            public void OnGUI()
            {
                if (!Input.GetKey(KeyCode.Tab))
                    return;

                GUI.backgroundColor = BackgroundColor.Value;
                GUI.color = ForegroundColor.Value;

                position = GUI.Window(25, position, (GUI.WindowFunction)WindowSections, "Sections");
                if (currentGroup != null)
                    GUI.Window(30, new Rect(position.left + 158, position.top, 306, 482), (GUI.WindowFunction)WindowElements, "Elements");
            }
        }
    }
}
