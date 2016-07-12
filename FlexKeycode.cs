using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.AddBoneSlider.Plugin
{
    //キー入力でキー名がややこしいやつの対策
    public static class FlexKeycode
    {
        static Dictionary<string, KeyCode> dicKey = new Dictionary<string, KeyCode>()
        {
            {"f1",KeyCode.F1},
            {"f2",KeyCode.F2},
            {"f3",KeyCode.F3},
            {"f4",KeyCode.F4},
            {"f5",KeyCode.F5},
            {"f6",KeyCode.F6},
            {"f7",KeyCode.F7},
            {"f8",KeyCode.F8},
            {"f9",KeyCode.F9},
            {"f10",KeyCode.F10},
            {"f11",KeyCode.F11},
            {"f12",KeyCode.F12},
            {"capslock",KeyCode.CapsLock},
            {"caps lock",KeyCode.CapsLock},
            {"backspace",KeyCode.Backspace },
            {"back space",KeyCode.Backspace },
            {"↓",KeyCode.DownArrow },
            {"down",KeyCode.DownArrow },
            {"downarrow",KeyCode.DownArrow },
            {"down arrow",KeyCode.DownArrow },
            {"↑",KeyCode.UpArrow },
            {"up",KeyCode.DownArrow },
            {"uparrow",KeyCode.DownArrow },
            {"up arrow",KeyCode.DownArrow },
            {"←",KeyCode.LeftArrow },
            {"left",KeyCode.LeftArrow },
            {"leftarrow",KeyCode.LeftArrow },
            {"left arrow",KeyCode.LeftArrow },
            {"→",KeyCode.RightArrow },
            {"right",KeyCode.RightArrow },
            {"rightarrow",KeyCode.RightArrow },
            {"right arrow",KeyCode.RightArrow },
            {"alt",KeyCode.LeftAlt },
            {"leftalt",KeyCode.LeftAlt },
            {"left alt",KeyCode.LeftAlt },
            {"rightalt",KeyCode.RightAlt },
            {"right alt",KeyCode.RightAlt },
            {"shift",KeyCode.LeftShift },
            {"leftshift",KeyCode.LeftShift },
            {"left shift",KeyCode.LeftShift },
            {"rightshift",KeyCode.RightShift },
            {"right shift",KeyCode.RightShift },
            {"control",KeyCode.LeftControl },
            {"leftcontrol",KeyCode.LeftControl },
            {"left control",KeyCode.LeftControl },
            {"rightcontrol",KeyCode.RightControl },
            {"right control",KeyCode.RightControl },
            {"ctrl",KeyCode.LeftControl },
            {"left ctrl",KeyCode.LeftControl },
            {"rightctrl",KeyCode.RightControl },
            {"right ctrl",KeyCode.RightControl },
            {"numlock", KeyCode.Numlock },
            {"num lock", KeyCode.Numlock },
            {"pageup", KeyCode.PageUp },
            {"page up", KeyCode.PageUp },
            {"pagedown", KeyCode.PageDown },
            {"page down", KeyCode.PageDown },
            {"escape",KeyCode.Escape},
            {"esc",KeyCode.Escape}
        };

        public static bool GetKeyDown(string key)
        {
            return dicKey.ContainsKey(key) ? Input.GetKeyDown(dicKey[key]) : Input.GetKeyDown(key);
        }
        public static bool GetKeyUp(string key)
        {
            return dicKey.ContainsKey(key) ? Input.GetKeyUp(dicKey[key]) : Input.GetKeyUp(key);
        }
        public static bool GetKey(string key)
        {
            return dicKey.ContainsKey(key) ? Input.GetKey(dicKey[key]) : Input.GetKey(key);
        }
    }
}
