using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace CM3D2.AddBoneSlider.Plugin
{
    //ini設定用
    static class IniFileHelper
    {
        [DllImport("KERNEL32.DLL")]
        public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        public static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        public static extern uint WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        public static T Read<T>(string section, string filepath)
        {
            T ret = (T)Activator.CreateInstance(typeof(T));

            foreach (var n in typeof(T).GetFields())
            {
                if (n.FieldType == typeof(int))
                {
                    n.SetValue(ret, (int)GetPrivateProfileInt(section, n.Name, 0, Path.GetFullPath(filepath)));
                }
                else if (n.FieldType == typeof(uint))
                {
                    n.SetValue(ret, GetPrivateProfileInt(section, n.Name, 0, Path.GetFullPath(filepath)));
                }
                else
                {
                    var sb = new StringBuilder(1024);
                    GetPrivateProfileString(section, n.Name, "", sb, (uint)sb.Capacity, Path.GetFullPath(filepath));
                    n.SetValue(ret, sb.ToString());
                }
            };

            return ret;
        }

        public static void Write<T>(string secion, T data, string filepath)
        {
            foreach (var n in typeof(T).GetFields())
            {
                WritePrivateProfileString(secion, n.Name, n.GetValue(data).ToString(), Path.GetFullPath(filepath));
            };
        }

    }
    //todo0.0.1.5-02
    //Fキー以外での起動
    //iniの内容
    public class SettingIni
    {
        public string ToggleKey = "f10";
        public string AnmOutputmode = "";
        public int WindowPositionX = -480;
        public int WindowPositionY = 40;
        public int UndoCount = 10;
        public string PoseXmlDirectory = "";//Directory.GetCurrentDirectory() + @"\UnityInjector\Config";
        public string PoseImgDirectory = "";//Directory.GetCurrentDirectory() + @"\UnityInjector\Config\PoseImg";
        public string OutputAnmDirectory = "";
        public string OutputJsonDirectory = "";
        public string OutputAnmSybarisDirectory = "";
        public int DebugLogLevel = 0;
        //public int HandleLegacymode = 0;
        public int VRmodeEnable = 0;
        public string ShaderDirectry = "";

    }
}
