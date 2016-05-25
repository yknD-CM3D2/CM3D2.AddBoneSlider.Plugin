using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using UnityEngine;
using UnityInjector.Attributes;
using System.Runtime.InteropServices;

namespace CM3D2.AddBoneSlider.Plugin
{
    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86")]
    [PluginName("CM3D2 AddBoneSlider"), PluginVersion("0.0.0.1")]

    //Debuginfo.Logの代わりにLoginfo.Logを使う
    static class Debuginfo
    {
        public static int settingLevel = 0;
        public static string premessage = "";
        //_messageLV 0：常に表示 1：公開デバッグモード用メッセージ 2：個人テスト用メッセージ
        public static void Log(string _message, int _messageLv = 2)
        {
            if (_messageLv <= settingLevel)
            {
                Debug.Log(premessage + _message);
            }

        }
    }

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

    //iniの内容
    public class SettingIni
    {
        public string ToggleKey = "f10";
        public string AnmOutputmode = "";
        public int WindowPositionX = -480;
        public int WindowPositionY = 40;
        public string PoseXmlDirectory = "";//Directory.GetCurrentDirectory() + @"\UnityInjector\Config";
        public string PoseImgDirectory = "";//Directory.GetCurrentDirectory() + @"\UnityInjector\Config\PoseImg";
        public string OutputAnmDirectory = @"C:\KISS\CM3D2\PhotoModeData\Mod\Motion";
        public string OutputJsonDirectory = "";
        public string OutputAnmSybarisDirectory = "";
        public int DebugLogLevel = 0;
        public int HandleLegacymode = 0;
    }

    public class AddBoneSlider : UnityInjector.PluginBase
    {

        #region Constants

        public const string PluginName = "AddBoneSlider";
        public const string Version = "0.0.1.1";

        private readonly string LogLabel = AddBoneSlider.PluginName + " : ";

        private readonly float TimePerInit = 1.00f;

        //private readonly int UIRootWidth = 1920; 
        //private readonly int UIRootHeight      = 1080; 
        private readonly int ScrollViewWidth = 550;
        private readonly int ScrollViewHeight = 860;

        private readonly int basedepth = 10;

        #endregion



        #region Variables

        private int sceneLevel;
        private bool xmlLoad = false;
        private bool visible = false;
        private bool bInitCompleted = false;
        private bool bLocked = false;

        private BoneParam mp;
        private Dictionary<string, Dictionary<string, float>> undoValue = new Dictionary<string, Dictionary<string, float>>();

        private Maid maid;


        //SeceneLevel5（複数メイド撮影）ではStockNoが、SeceneLevel27（公式撮影）ではMaidNoが設定される
        private int currentMaidNo;
        

        private GameObject goAMSPanel;

        private GameObject goScrollView;


        private GameObject[] goScrollViewTable = new GameObject[6];

        private GameObject goPSPanel;
        private GameObject goPSScrollView;
        private GameObject goPSScrollViewTable;
        private GameObject goPNamePanel;

        private UICamera uiCamara;
        private UIPanel uiAMSPanel;
        private UIPanel uiScrollPanel;
        private UIScrollView uiScrollView;
        private UIScrollBar uiScrollBar;
        private UITable[] uiTable = new UITable[6];

        private UIPanel uiPSPanel;
        private UIPanel uiPSScrollPanel;
        private UIScrollView uiPSScrollView;
        private UIScrollBar uiPSScrollBar;
        private UIPanel uiPNamePanel;
        private UITable uiPSTable;

        private UI2DSprite uiTextureCurrentMaid;
        private UILabel uiLabelCurrentMaid;

        private Font font;
        private Dictionary<string, Transform> trBoneUnit = new Dictionary<string, Transform>();
        private Dictionary<string, Dictionary<string, UILabel>> uiValueLable = new Dictionary<string, Dictionary<string, UILabel>>();
        public Dictionary<string, Transform> trBone = new Dictionary<string, Transform>();
        public Dictionary<string, Vector3> vPastBoneAngle = new Dictionary<string, Vector3>();
        private Dictionary<string, Transform> trPoseImgUnit = new Dictionary<string, Transform>();
        public Vector3 vPastBoneTrans;

        public string activeHandleName = "";
        AngleHandle posHandle;

        public string PoseXmlFileName;
        public string PoseTexDirectoryName;
        public string iniFileName;
        public string PoseName  ="";


        SettingIni settingIni;

        #endregion



        #region Nested classes

        //BoneParam.xml設定用
        private class BoneParam
        {
            private readonly string LogLabel = AddBoneSlider.PluginName + " : ";

            public readonly string DefMatchPattern = @"([-+]?[0-9]*\.?[0-9]+)";
            public readonly string XmlFileName = Directory.GetCurrentDirectory() + @"\UnityInjector\Config\BoneParam.xml";


            public string XmlFormat;
            public List<string> sBone = new List<string>();

            public Dictionary<string, bool> bEnabled = new Dictionary<string, bool>();
            public Dictionary<string, string> sDescription = new Dictionary<string, string>();
            public Dictionary<string, bool> bVisible = new Dictionary<string, bool>();
            public Dictionary<string, string> sCategory = new Dictionary<string, string>();
            public Dictionary<string, bool> bNosave = new Dictionary<string, bool>();
            public Dictionary<string, string> sPath = new Dictionary<string, string>();


            public Dictionary<string, string[]> sPropName = new Dictionary<string, string[]>();
            public Dictionary<string, Dictionary<string, float>> fValue = new Dictionary<string, Dictionary<string, float>>();

            public Dictionary<string, Dictionary<string, string>> sVAxis = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, float>> fVzero = new Dictionary<string, Dictionary<string, float>>();

            public Dictionary<string, Dictionary<string, float>> fVmin = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, float>> fVmax = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, float>> fVdef = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, string>> sVType = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, string>> sLabel = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, bool>> bVVisible = new Dictionary<string, Dictionary<string, bool>>();

            public int BoneCount { get { return sBone.Count; } }
            public int ValCount(string bone) { return sPropName[bone].Length; }

            //--------

            public BoneParam() { }

            public bool Init()
            {
                if (!loadBoneParamXML())
                {
                    Debug.LogError(LogLabel + "loadBoneParamXML() failed.");
                    return false;
                }

                return true;
            }


            public bool IsToggle(string bone)
            {
                return false;
                //return (sType[bone].Contains("toggle")) ? true : false;
            }

            public bool IsSlider(string bone)
            {
                return true;
                //return (sType[bone].Contains("slider")) ? true : false;
            }

            //--------


            private bool loadBoneParamXML()
            {


                if (!File.Exists(XmlFileName))
                {
                    Debug.LogError(LogLabel + "\"" + XmlFileName + "\" does not exist.");
                    return false;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(XmlFileName);

                XmlNode mods = doc.DocumentElement;
                XmlFormat = ((XmlElement)mods).GetAttribute("format");

                XmlNodeList modNodeS = mods.SelectNodes("/bones/bone");
                if (!(modNodeS.Count > 0))
                {
                    Debug.LogError(LogLabel + " \"" + XmlFileName + "\" has no <bone>elements.");
                    return false;
                }

                sBone.Clear();

                foreach (XmlNode modNode in modNodeS)
                {
                    string bone = ((XmlElement)modNode).GetAttribute("id");
                    if (bone != "" && !sBone.Contains(bone)) sBone.Add(bone);
                    else continue;


                    bool b = false;
                    bEnabled[bone] = false;
                    sDescription[bone] = ((XmlElement)modNode).GetAttribute("description");
                    bVisible[bone] = (Boolean.TryParse(((XmlElement)modNode).GetAttribute("visible"), out b)) ? b : true;
                    bNosave[bone] = (Boolean.TryParse(((XmlElement)modNode).GetAttribute("nosave"), out b)) ? b : false;
                    sCategory[bone] = ((XmlElement)modNode).GetAttribute("category");
                    sPath[bone] = ((XmlElement)modNode).GetAttribute("path");


                    if (!IsSlider(bone)) continue;

                    XmlNodeList valueNodeS = ((XmlElement)modNode).GetElementsByTagName("value");
                    if (!(valueNodeS.Count > 0)) continue;

                    sPropName[bone] = new string[valueNodeS.Count];
                    fValue[bone] = new Dictionary<string, float>();

                    sVAxis[bone] = new Dictionary<string, string>();
                    fVzero[bone] = new Dictionary<string, float>();

                    fVmin[bone] = new Dictionary<string, float>();
                    fVmax[bone] = new Dictionary<string, float>();
                    fVdef[bone] = new Dictionary<string, float>();
                    sVType[bone] = new Dictionary<string, string>();
                    sLabel[bone] = new Dictionary<string, string>();
                    bVVisible[bone] = new Dictionary<string, bool>();

                    // value属性
                    int j = 0;
                    foreach (XmlNode valueNode in valueNodeS)
                    {
                        float x = 0f;

                        string prop = ((XmlElement)valueNode).GetAttribute("prop_name");
                        if (prop != "" && Array.IndexOf(sPropName[bone], prop) < 0)
                        {
                            sPropName[bone][j] = prop;
                        }
                        else
                        {
                            sBone.Remove(bone);
                            break;
                        }

                        sVType[bone][prop] = ((XmlElement)valueNode).GetAttribute("type");
                        switch (sVType[bone][prop])
                        {
                            case "num": break;
                            case "scale": break;
                            case "int": break;
                            default: sVType[bone][prop] = "num"; break;
                        }

                        sVAxis[bone][prop] = ((XmlElement)valueNode).GetAttribute("axis");
                        switch (sVAxis[bone][prop])
                        {
                            case "posx": break;
                            case "posy": break;
                            case "posz": break;
                            case "rotx": break;
                            case "roty": break;
                            case "rotz": break;
                            default: break;
                        }

                        fVzero[bone][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("zero"), out x) ? x : 0f;
                        fVmin[bone][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("min"), out x) ? x : 0f;
                        fVmax[bone][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("max"), out x) ? x : 0f;
                        fVdef[bone][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("default"), out x) ? x : Single.NaN;
                        if (Single.IsNaN(fVdef[bone][prop]))
                        {
                            switch (sVType[bone][prop])
                            {
                                case "num": fVdef[bone][prop] = 0f; break;
                                case "scale": fVdef[bone][prop] = 1f; break;
                                case "int": fVdef[bone][prop] = 0f; break;
                                default: fVdef[bone][prop] = 0f; break;
                            }
                        }

                        fValue[bone][prop] = fVdef[bone][prop];

                        sLabel[bone][prop] = ((XmlElement)valueNode).GetAttribute("label");
                        bVVisible[bone][prop] = (Boolean.TryParse(((XmlElement)valueNode).GetAttribute("visible"), out b)) ? b : true;

                        j++;
                    }
                    if (j == 0) sBone.Remove(bone);
                }

                return true;
            }
        }

        //ハンドル君
        private class AngleHandle
        {
            private bool initComplete = false;

            private Maid maid = null;
            private Transform parentBone;

            private GameObject gameObject;

            private ControllOnMouse controllOnMouseX;
            private ControllOnMouse controllOnMouseY;
            private ControllOnMouse controllOnMouseZ;

            private int Legacymode;

            Texture2D m_texture_red;
            Texture2D m_texture_green;
            Texture2D m_texture_blue;
            Texture2D m_texture_red_2;
            Texture2D m_texture_green_2;
            Texture2D m_texture_blue_2;
            Texture2D m_texture_white;
            Texture2D m_texture_yellow;


            GameObject redring;
            GameObject bluering;
            GameObject greenring;

            GizmoRender gizmoRender;

            //ハンドルのドラッグ状態取得
            public bool controllDragged()
            {
                if (!initComplete)
                    return false;


                if (controllOnMouseX.DragFinished || controllOnMouseY.DragFinished || controllOnMouseZ.DragFinished)
                {
                    Visible = false;
                    controllOnMouseX.DragFinished = false;
                    controllOnMouseY.DragFinished = false;
                    controllOnMouseZ.DragFinished = false;

                    redring.renderer.material.mainTexture = m_texture_red_2;
                    greenring.renderer.material.mainTexture = m_texture_green_2;
                    bluering.renderer.material.mainTexture = m_texture_blue_2;

                    redring.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                    greenring.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                    bluering.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));



                    SetParentBone(parentBone);
                    Visible = true;

                }

                if (!controllOnMouseX.Dragged && !controllOnMouseY.Dragged && !controllOnMouseZ.Dragged)
                {
                    if (controllOnMouseX.mouseOver)
                    {
                        redring.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        redring.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        redring.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                        redring.renderer.material.mainTexture = m_texture_red_2;

                    }

                    if (controllOnMouseY.mouseOver)
                    {
                        greenring.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        greenring.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        greenring.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                        greenring.renderer.material.mainTexture = m_texture_green_2;

                    }

                    if (controllOnMouseZ.mouseOver)
                    {
                        bluering.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        bluering.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        bluering.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));
                        bluering.renderer.material.mainTexture = m_texture_blue_2;
                    }
                }

                return (controllOnMouseX.Dragged || controllOnMouseY.Dragged || controllOnMouseZ.Dragged);
            }

            private float handleScale = 1.0f;

            public Transform transform
            {
                get
                {
                    return (initComplete) ? this.gameObject.transform : null;
                }
            }
            public Vector3 Pos
            {
                get { return (initComplete) ? this.gameObject.transform.position : default(Vector3); }
                set { if (initComplete) this.gameObject.transform.position = value; }
            }
            public Quaternion Rot
            {
                get { return (initComplete) ? this.gameObject.transform.rotation : default(Quaternion); }
                set { if (initComplete) this.gameObject.transform.rotation = value; }
            }
            public bool Visible
            {
                get
                {
                    return (initComplete && this.gameObject != null) ? this.gameObject.activeSelf : default(bool);
                }
                set
                {
                    if (initComplete && this.gameObject != null) this.gameObject.SetActive(value);
                }
            }

            public AngleHandle(int _Legacymode ,Maid _maid = null)
            {
                this.Legacymode = _Legacymode;
                Init();

                if (_maid != null)
                {
                    SetMaid(_maid);
                }

                this.gameObject.SetActive(false);
            }

            public void Init()
            {
                this.gameObject = new GameObject();

                //SSでハンドル君を消すために
                //公式のハンドルを線の太さ0にして所持しとく
                //公式のハンドルが消えたらハンドル君も消す
                //ここまでやるなら公式のハンドル流用しろよとは思うけどなんとなく
                if (Legacymode == 0)
                {
                    gizmoRender = this.gameObject.AddComponent<GizmoRender>();
                    gizmoRender.Visible = true;
                    gizmoRender.offsetScale = 0;
                }

                Color alpha_red = Color.red;
                alpha_red.a = 0.5f;
                m_texture_red = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_red.height; y++)
                {
                    for (int x = 0; x < m_texture_red.width; x++)
                    {
                        m_texture_red.SetPixel(x, y, alpha_red);
                    }
                }
                m_texture_red.Apply();
                m_texture_red.name = "red";

                Color alpha_green = Color.green;
                alpha_green.a = 0.5f;
                m_texture_green = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_green.height; y++)
                {
                    for (int x = 0; x < m_texture_green.width; x++)
                    {
                        m_texture_green.SetPixel(x, y, alpha_green);
                    }
                }
                m_texture_green.Apply();
                m_texture_green.name = "green";


                Color alpha_blue = Color.blue;
                alpha_blue.a = 0.5f;
                m_texture_blue = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_blue.height; y++)
                {
                    for (int x = 0; x < m_texture_blue.width; x++)
                    {
                        m_texture_blue.SetPixel(x, y, alpha_blue);
                    }
                }
                m_texture_blue.Apply();
                m_texture_blue.name = "blue";

                Color alpha_red_2 = new Color(0, 1, 1, 0.5f);
                m_texture_red_2 = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_red_2.height; y++)
                {
                    for (int x = 0; x < m_texture_red.width; x++)
                    {
                        m_texture_red.SetPixel(x, y, alpha_red);
                    }

                }
                m_texture_red_2.Apply();
                m_texture_red_2.name = "red_2";

                Color alpha_green_2 = new Color(1, 0, 1, 0.5f);
                m_texture_green_2 = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_green_2.height; y++)
                {
                    for (int x = 0; x < m_texture_green.width; x++)
                    {
                        m_texture_green.SetPixel(x, y, alpha_green);
                    }
                }
                m_texture_green_2.Apply();
                m_texture_green_2.name = "green_2";


                Color alpha_blue_2 = new Color(1, 1, 0, 0.5f);
                m_texture_blue_2 = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_blue_2.height; y++)
                {
                    for (int x = 0; x < m_texture_blue.width; x++)
                    {
                        m_texture_blue.SetPixel(x, y, alpha_blue);
                    }
                }
                m_texture_blue_2.Apply();
                m_texture_blue_2.name = "blue_2";




                Color alpha_white = Color.white;
                alpha_white.a = 0.5f;
                m_texture_white = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_white.height; y++)
                {
                    for (int x = 0; x < m_texture_white.width; x++)
                    {
                        m_texture_white.SetPixel(x, y, alpha_white);
                    }
                }
                m_texture_white.Apply();
                m_texture_white.name = "white";

                Color alpha_yellow = Color.yellow;
                alpha_yellow.a = 0.3f;
                m_texture_yellow = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                for (int y = 0; y < m_texture_yellow.height; y++)
                {
                    for (int x = 0; x < m_texture_yellow.width; x++)
                    {
                        m_texture_yellow.SetPixel(x, y, alpha_yellow);
                    }
                }
                m_texture_yellow.Apply();
                m_texture_yellow.name = "yellow";

                GameObject boneCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                boneCenter.renderer.receiveShadows = false;
                boneCenter.renderer.castShadows = false;
                boneCenter.renderer.useLightProbes = false;
                boneCenter.renderer.material.mainTexture = m_texture_white;
                if (Legacymode == 0)
                {
                    boneCenter.renderer.material.shader = Shader.Find("Custom/GizmoShader");
                    boneCenter.renderer.material.SetInt("unity_GUIZTestMode", 6);
                }
                else
                {
                    boneCenter.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                }
                boneCenter.transform.localScale = new Vector3(0.125f, 0.125f, 0.125f);
                boneCenter.transform.localEulerAngles = new Vector3(0f, 0f, 0f);


                boneCenter.transform.parent = this.gameObject.transform;




                this.controllOnMouseZ = setHandleObject(m_texture_blue_2, new Vector3(0f, 0f, 0f), new Color(0, 0, 1, 0.5f));//Z
                this.controllOnMouseZ.wheelType = ControllOnMouse.WheelType.Angle;
                this.controllOnMouseZ.axisType = ControllOnMouse.AxisType.RZ;

                this.controllOnMouseX = setHandleObject(m_texture_red_2, new Vector3(90f, 0f, 0f), new Color(1, 0, 0, 0.5f));//X
                this.controllOnMouseX.wheelType = ControllOnMouse.WheelType.Angle;
                this.controllOnMouseX.axisType = ControllOnMouse.AxisType.RX;

                this.controllOnMouseY = setHandleObject(m_texture_green_2, new Vector3(0f, 0f, 90f), new Color(0, 1, 0, 0.5f));//Y
                this.controllOnMouseY.wheelType = ControllOnMouse.WheelType.Angle;
                this.controllOnMouseY.axisType = ControllOnMouse.AxisType.RY;


                GameObject blueZ = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                blueZ.renderer.receiveShadows = false;
                blueZ.renderer.castShadows = false;
                blueZ.renderer.useLightProbes = false;
                blueZ.renderer.material.mainTexture = m_texture_blue;
                if (Legacymode == 0)
                {
                    blueZ.renderer.material.shader = Shader.Find("Custom/GizmoShader");
                    blueZ.renderer.material.SetInt("unity_GUIZTestMode", 6);
                }
                else
                {
                    blueZ.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                }
                blueZ.transform.localScale = new Vector3(0.025f, 1f, 0.025f);
                blueZ.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                blueZ.transform.parent = this.gameObject.transform;

                GameObject redX = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                redX.renderer.receiveShadows = false;
                redX.renderer.castShadows = false;
                redX.renderer.useLightProbes = false;
                redX.renderer.material.mainTexture = m_texture_red;
                if (Legacymode == 0)
                {
                    redX.renderer.material.shader = Shader.Find("Custom/GizmoShader");
                    redX.renderer.material.SetInt("unity_GUIZTestMode", 6);
                }
                else
                {
                    redX.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                }
                redX.transform.localScale = new Vector3(0.025f, 1f, 0.025f);
                redX.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                redX.transform.parent = this.gameObject.transform;


                GameObject greenY = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                greenY.renderer.receiveShadows = false;
                greenY.renderer.castShadows = false;
                greenY.renderer.useLightProbes = false;
                greenY.renderer.material.mainTexture = m_texture_green;
                greenY.renderer.material.shader = Shader.Find("Custom/GizmoShader");
                greenY.renderer.material.SetInt("unity_GUIZTestMode", 6);
                if (Legacymode == 0)
                {
                    greenY.renderer.material.shader = Shader.Find("Custom/GizmoShader");
                    greenY.renderer.material.SetInt("unity_GUIZTestMode", 6);
                }
                else
                {
                    greenY.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                }
                greenY.transform.localScale = new Vector3(0.025f, 1f, 0.025f);
                greenY.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                greenY.transform.parent = this.gameObject.transform;


            }

            //ハンドル君のリング部分を作る
            private ControllOnMouse setHandleObject(Texture2D m_texture, Vector3 handleAngle, Color m_color)
            {
                GameObject Ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);


                Mesh mesh = Ring.GetComponent<MeshFilter>().mesh;

                //円筒プリミティブを縁を少し残して円柱の底面を抜いたメッシュに修正
                Vector3[] newMesh = new Vector3[92];
                Vector2[] newUV = new Vector2[92];

                for (int i = 0; i < 92; ++i)
                {

                    if (i >= 46)
                    {
                        newMesh[i] = newMesh[i - 46];
                        newMesh[i].x *= 0.95f;
                        newMesh[i].z *= 0.95f;

                        newUV[i] = newUV[i - 46];
                        newUV[i].y = 0.5f;

                    }
                    else if (i >= 40)
                    {
                        newMesh[i] = mesh.vertices[i + 2];
                        newUV[i] = mesh.uv[i + 2];
                    }
                    else
                    {

                        newMesh[i] = mesh.vertices[i];
                        newUV[i] = mesh.uv[i];
                    }

                }

                int[] newTri = new int[360];

                for (int i = 0; i < 120; ++i)
                {
                    if (mesh.triangles[i] > 40)
                        newTri[i] = mesh.triangles[i] - 2;
                    else
                        newTri[i] = mesh.triangles[i];


                }

                for (int i = 0; i < 20; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        if (newTri[6 * i + j] == 41)
                        {
                            newTri[6 * i + 122 - j] = 86;
                        }
                        else if (newTri[6 * i + j] == 43)
                        {
                            newTri[6 * i + 122 - j] = 90;
                        }
                        else if (newTri[6 * i + j] == 45)
                        {
                            newTri[6 * i + 122 - j] = 88;
                        }
                        else if (newTri[6 * i + j] >= 20 && newTri[6 * i + j] < 40)
                        {
                            newTri[6 * i + 122 - j] = newTri[6 * i + j] + 26;
                        }
                        else
                        {
                            newTri[6 * i + 122 - j] = newTri[6 * i + j];

                        }


                        if (newTri[6 * i + j + 3] == 41)
                        {
                            newTri[6 * i + 125 - j] = 86;
                        }
                        else if (newTri[6 * i + j + 3] == 43)
                        {
                            newTri[6 * i + 125 - j] = 90;
                        }
                        else if (newTri[6 * i + j + 3] == 45)
                        {
                            newTri[6 * i + 125 - j] = 88;
                        }
                        else if (newTri[6 * i + j + 3] >= 20 && newTri[6 * i + j + 3] < 40)
                        {
                            newTri[6 * i + 125 - j] = newTri[6 * i + j + 3] + 26;
                        }
                        else
                        {
                            newTri[6 * i + 125 - j] = newTri[6 * i + j + 3];

                        }

                        if (newTri[6 * i + j] == 40)
                        {
                            newTri[6 * i + 242 - j] = 87;
                        }
                        else if (newTri[6 * i + j] == 42)
                        {
                            newTri[6 * i + 242 - j] = 91;
                        }
                        else if (newTri[6 * i + j] == 44)
                        {
                            newTri[6 * i + 242 - j] = 89;
                        }
                        else if (newTri[6 * i + j] < 20)
                        {
                            newTri[6 * i + 242 - j] = newTri[6 * i + j] + 66;
                        }
                        else
                        {
                            newTri[6 * i + 242 - j] = newTri[6 * i + j];

                        }

                        if (newTri[6 * i + j + 3] == 40)
                        {
                            newTri[6 * i + 245 - j] = 87;
                        }
                        else if (newTri[6 * i + j + 3] == 42)
                        {
                            newTri[6 * i + 245 - j] = 91;
                        }
                        else if (newTri[6 * i + j + 3] == 44)
                        {
                            newTri[6 * i + 245 - j] = 89;
                        }
                        else if (newTri[6 * i + j + 3] < 20)
                        {
                            newTri[6 * i + 245 - j] = newTri[6 * i + j + 3] + 66;
                        }
                        else
                        {
                            newTri[6 * i + 245 - j] = newTri[6 * i + j + 3];

                        }

                    }

                }

                mesh.Clear();
                mesh.vertices = newMesh;
                mesh.uv = newUV;
                mesh.triangles = newTri;



                Ring.renderer.receiveShadows = false;
                Ring.renderer.castShadows = false;
                Ring.renderer.useLightProbes = false;
                Ring.renderer.material.mainTexture = m_texture;

                if (Legacymode == 0)
                {
                    Ring.renderer.material.shader = Shader.Find("Hidden/Transplant_Internal-Colored");
                    Ring.renderer.material.SetFloat("_ZTest", 6);
                    Ring.renderer.material.SetFloat("_Cull", 2);
                    Ring.renderer.material.SetFloat("_ZWrite", 0);

                }
                else
                {
                    Ring.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                }
                
                Ring.renderer.material.SetColor("_Color", m_color);

                Ring.transform.localScale = new Vector3(2f, 0.05f, 2f);
                Ring.transform.localEulerAngles = handleAngle;

                UnityEngine.Object.Destroy(Ring.GetComponent<Collider>());

                MeshCollider meshCollider = Ring.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = Ring.GetComponent<MeshFilter>().sharedMesh;

                Ring.name = m_texture.name + "ring";
                Ring.transform.parent = this.gameObject.transform;

                if (Ring.name == "red_2ring")
                {
                    redring = Ring;

                }
                else if (Ring.name == "green_2ring")
                {

                    greenring = Ring;

                }
                else if (Ring.name == "blue_2ring")
                {
                    bluering = Ring;

                }
                else
                {
                    Debug.LogError("ControllOnMouse: material name is invalid.");
                }

                return Ring.AddComponent<ControllOnMouse>();

            }

            //動かすメイド設定
            public void SetMaid(Maid _maid, Transform _parentBone = null)
            {
                if (_maid == this.maid)
                {
                    SetParentBone(this.parentBone);
                }
                else
                {
                    this.maid = _maid;
                    SetParentBone(_parentBone);

                }

            }

            //今動かしてるボーンを取得
            public Transform GetParentBone()
            {
                return (initComplete && this.gameObject != null) ? this.parentBone : null;
            }

            //動かすボーン設定
            public void SetParentBone(Transform _parentBone)
            {
                if (this.gameObject == null)
                {
                    Init();
                }
                if (_parentBone != null)
                {
                    this.parentBone = _parentBone;
                    this.gameObject.transform.parent = _parentBone;//FindParent(maid.transform, "AllOffset");
                    this.gameObject.transform.localPosition = Vector3.zero;//_parentBone.localPosition;//this.maid.gameObject.transform.localPosition;
                    this.gameObject.transform.localRotation = Quaternion.identity;//_parentBone.localRotation;//this.maid.gameObject.transform.localRotation;

                    initComplete = true;

                    int childBoneCount = 0;
                    handleScale = 0.0f;
                    for (int i = 0; i < parentBone.childCount; ++i)
                    {
                        Transform childBone = _parentBone.GetChild(i);
                        if (childBone.name.Contains("Bip") && !childBone.name.Contains("SCL"))
                        {
                            ++childBoneCount;
                            handleScale += childBone.localPosition.magnitude;
                        }
                    }
                    if (childBoneCount != 0)
                    {
                        handleScale /= (float)childBoneCount;
                    }
                    if (handleScale < 0.1) handleScale = 0.1f;
                    this.gameObject.transform.localScale = Vector3.one * handleScale;
                }
                else
                {
                    //nullが来たら非表示にしとく
                    this.gameObject.SetActive(false);
                    initComplete = false;
                }
            }


            public void Proc()
            {
                if (!initComplete) return;
                
                //検知用のGizmoRenderが消えたらハンドル君も消える
                if(gizmoRender.Visible != Visible)
                {
                    Visible = gizmoRender.Visible;
                }

            }

            public void setVisible(bool bVisible)
            {
                gizmoRender.Visible = bVisible;
                Visible = bVisible;
            }

            //どの軸がドラッグされてるのか判別してドラッグ回転を返す
            public Quaternion DeltaQuaternion()
            {
                if (!initComplete) return Quaternion.identity;
                if (controllOnMouseX.Dragged)
                {
                    return controllOnMouseX.dragQuaternion;
                }
                else if (controllOnMouseY.Dragged)
                {
                    return controllOnMouseY.dragQuaternion;
                }
                else if (controllOnMouseZ.Dragged)
                {
                    return controllOnMouseZ.dragQuaternion;
                }
                else
                {
                    return Quaternion.identity;
                }
            }


            public void Destroy()
            {
                if (this.gameObject)
                {
                    Debuginfo.Log("AngleHandle:Destroy!");
                    GameObject.Destroy(this.gameObject);

                }
                initComplete = false;
            }
        }


        private class ControllOnMouse : MonoBehaviour
        {
            public enum WheelType
            {
                Angle,
                Position
            }

            public enum AxisType
            {
                RX,
                RY,
                RZ
            }

            public bool mouseOver = false;

            private Vector3 screenPoint = Vector3.zero;


            public WheelType wheelType = WheelType.Angle;
            public AxisType axisType = AxisType.RX;
            public bool ShouldReset = false;


            public bool Dragged = false;
            public bool DragFinished = false;

            public Vector3 clickPointVector = Vector3.zero;
            public float oldAngle = 0f;

            public Quaternion dragQuaternion = Quaternion.identity;

            public void Destroy()
            {
            }

            public void Awake()
            {
            }

            public void OnMouseDown()
            {
                //カメラから見たオブジェクトの現在位置を画面位置座標に変換
                //screenPoint = Camera.main.WorldToScreenPoint(transform.position);

                //リングが直線状になるとき
                if (Math.Abs(Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) - 90f) < 10f)
                {
                    if ((Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) - 90f) < 0f)
                        screenPoint = Camera.main.WorldToScreenPoint(transform.position + 0.2f * transform.up);
                    else
                        screenPoint = Camera.main.WorldToScreenPoint(transform.position - 0.2f * transform.up);
                }
                else
                {
                    screenPoint = Camera.main.WorldToScreenPoint(transform.position);
                }


                clickPointVector = Input.mousePosition;
                clickPointVector.z = 0;
                clickPointVector -= screenPoint;

                oldAngle = 0.0f;
            }

            public void OnMouseDrag()
            {

                Vector3 dragPoint = Input.mousePosition;

                dragPoint.z = 0;

                dragPoint -= screenPoint;

                float dragAngle = Vector3.Angle(clickPointVector, dragPoint);


                if ((clickPointVector.x * dragPoint.y - clickPointVector.y * dragPoint.x) < 0)
                {
                    dragAngle = -dragAngle;

                }
                if (Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) < 90)
                {
                    dragAngle = -dragAngle;

                }
                if (axisType == AxisType.RY)
                {
                    dragAngle = -dragAngle;
                }



                float offsetAngle = dragAngle - oldAngle;


                switch (axisType)
                {
                    case AxisType.RY:

                        dragQuaternion = Quaternion.AngleAxis(offsetAngle, Vector3.right);

                        break;

                    case AxisType.RZ:

                        dragQuaternion = Quaternion.AngleAxis(offsetAngle, Vector3.up);

                        break;

                    case AxisType.RX:

                        dragQuaternion = Quaternion.AngleAxis(offsetAngle, Vector3.forward);

                        break;


                    default:
                        break;

                }


                oldAngle = dragAngle;
                Dragged = true;
            }
            public void OnMouseUp()
            {
                if (Dragged)
                {
                    Dragged = false;
                    DragFinished = true;


                }
            }

            public void OnMouseEnter()
            {
                mouseOver = true;
            }

            public void OnMouseOver()
            {
            }
            public void OnMouseExit()
            {
                mouseOver = false;
            }

            public void Update()
            {

                if (mouseOver)
                {
                    if (Input.GetMouseButton(2)) ShouldReset = true;
                }

            }

            public void OnGui()
            {
                if (DragFinished) DragFinished = false;
            }

        }

        #endregion



        #region MonoBehaviour methods

        public void OnLevelWasLoaded(int level)
        {
            if (level == 9)
            {
                font = GameObject.Find("SystemUI Root").GetComponentsInChildren<UILabel>()[0].trueTypeFont;
            }

            if (level != sceneLevel && ((sceneLevel == 5) || (sceneLevel == 27)))
            {
                finalize();
            }

            if ((level == 5) || (level == 27))
            {
                mp = new BoneParam();
                if (xmlLoad = mp.Init()) StartCoroutine(initCoroutine());
            }

            sceneLevel = level;
        }

        public void Update()
        {
            if (((sceneLevel == 5) || (sceneLevel == 27)) && bInitCompleted)
            {


                if (Input.GetKeyDown(settingIni.ToggleKey))
                {
                    if (maid != null && maid.Visible == true && visible == false)
                    {
                        currentMaidChange();
                    }
                    goAMSPanel.SetActive(visible = !visible);

                    posHandle.Visible = visible;

                    settingIni.WindowPositionX = (int)uiAMSPanel.transform.localPosition.x;
                    settingIni.WindowPositionY = (int)uiAMSPanel.transform.localPosition.y;


                }
                if (maid == null || maid.Visible == false)
                {

                    int stockNo = FindVisibleMaidStockNo(0, 1);
                    if (stockNo != -1)
                    {
                        currentMaidNo = stockNo;
                        currentMaidChange();
                    }
                    else
                    {

                        if (visible == true)
                        {
                            //Debuginfo.Log(LogLabel + "window destroy at photomode");

                            visible = false;
                            goAMSPanel.SetActive(false);
                            posHandle.Visible = false;
                        }

                    }
                }

                if (visible)
                {
                    if (bLocked == false)
                    {
                        //UIと一緒に消す用
                        if (settingIni.HandleLegacymode == 0)
                        {
                            posHandle.Proc();
                        }

                        //複数撮影SS対策用
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                            //Debuginfo.Log(LogLabel +"input S");
                            posHandle.Visible = false;
                        }

                        if (posHandle.Visible == true)
                        {
                            syncFromHandle();

                        }
                        syncSlider(false);
                    }

                    if (settingIni.WindowPositionX != (int)uiAMSPanel.transform.localPosition.x)
                        settingIni.WindowPositionX = (int)uiAMSPanel.transform.localPosition.x;
                    if (settingIni.WindowPositionY != (int)uiAMSPanel.transform.localPosition.y)
                        settingIni.WindowPositionY = (int)uiAMSPanel.transform.localPosition.y;
                }

            }
        }



        #endregion



        #region Callbacks

        public void OnClickHeaderButton()
        {
            try
            {
                string bone = getTag(UIButton.current, 1);
                bool b = false;

                if (mp.IsToggle(bone))
                {
                    b = !mp.bEnabled[bone];
                    mp.bEnabled[bone] = b;

                    // WIDESLIDER有効化/無効化に合わせて、依存項目UIを表示/非表示
                    if (bone == "WIDESLIDER") toggleActiveOnWideSlider();
                }

                if (mp.IsSlider(bone))
                {
                    if (!mp.IsToggle(bone)) b = !(UIButton.current.defaultColor.a == 1f);
                    setSliderVisible(bone, b);
                }

                if (sceneLevel == 5 && bone == "allpos")
                {
                    setParentAllOffset();
                }

                setButtonColor(UIButton.current, b);

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickToggleHeader() " + ex); return; }
        }

        public void OnClickPrevMaid()
        {
            Debuginfo.Log(LogLabel + "OnClickPrevMaid start ");
            int stockNo = FindVisibleMaidStockNo(this.currentMaidNo - 1, -1);
            Debuginfo.Log(LogLabel + "OnClickPrevMaid " + stockNo);
            
            if (stockNo != -1)
            {
                currentMaidNo = stockNo;
                currentMaidChange();
                Debuginfo.Log(LogLabel + "maid parent:" + maid.transform.parent.name);
            }
        }

        public void OnClickNextMaid()
        {
            int stockNo = FindVisibleMaidStockNo(this.currentMaidNo + 1, 1);
            if (stockNo != -1)
            {
                currentMaidNo = stockNo;
                currentMaidChange();
            }
        }
        public void OnClickPoseImg()
        {

            try
            {
                PoseTexDirectoryName = settingIni.PoseImgDirectory;
                string name = getTag(UIButton.current, 1);
                string text = PoseTexDirectoryName + @"\poseimg" + name + ".png";

                XmlDocument document = new XmlDocument();
                if (!File.Exists(PoseXmlFileName))
                {
                    Debug.LogError(LogLabel + ":XMLファイルが破損しています。");
                    return;
                }
                //Debuginfo.Log(LogLabel +"OnClickPoseImg() start");
                document.Load(PoseXmlFileName);
                XmlElement root = document.DocumentElement;


                XmlNode selectBones = root.SelectSingleNode("/poses/bones[@pose_id=" + name + "]");

                if (UICamera.currentTouchID == -1)
                {
                    //左クリックしたときの処理
                    //XMLからnameのボーンデータを読み込む

                    bLocked = true;
                    XmlNodeList boneNodeS = ((XmlElement)selectBones).GetElementsByTagName("bone");
                    if (!(boneNodeS.Count > 0)) return;

                    maid.body0.m_Bones.animation.Stop();
                    maid.body0.boHeadToCam = false;
                    maid.body0.boEyeToCam = false;

                    foreach (XmlNode boneNode in boneNodeS)
                    {
                        string bone = ((XmlElement)boneNode).GetAttribute("id");

                        XmlNodeList valueNodeS = ((XmlElement)boneNode).GetElementsByTagName("value");
                        if (!(valueNodeS.Count > 0)) continue;

                        foreach (XmlNode valueNode in valueNodeS)
                        {
                            float x = 0f;

                            string prop = ((XmlElement)valueNode).GetAttribute("prop_name");
                            float preValue = mp.fValue[bone][prop];
                            undoValue[bone][prop] = mp.fValue[bone][prop];
                            mp.sVAxis[bone][prop] = ((XmlElement)valueNode).GetAttribute("axis");
                            mp.fValue[bone][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("val"), out x) ? x : 0f;
                            uiValueLable[bone][prop].text = mp.fValue[bone][prop].ToString("F4");
                            uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop].ToString("F4");
                        }
                        foreach (Transform tr in trBoneUnit[bone])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);
                                slider.value = codecSliderValue(bone, prop);


                            }
                        }

                        rotateBone(bone);


                        vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                    }
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                    bLocked = false;

                }
                else if (UICamera.currentTouchID == -2)
                {
                    //右クリックしたときの処理
                    //XMLからnameのボーンデータを削除して
                    //UIも削除
                    root.RemoveChild(selectBones);
                    document.Save(PoseXmlFileName);

                    DestoryChild(goPSScrollViewTable, "Image:" + name);
                    uiPSTable.Reposition();
                    //サムネ画像も削除

                    File.Delete(text);
                }
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickPoseImg() " + ex); return; }
        }

        public void OnClickLoadPose()
        {
            //ポーズロード用パネルを開閉
            bool bToggle = !(UIButton.current.defaultColor.a == 1f);

            Color color = UIButton.current.defaultColor;
            UIButton.current.defaultColor = new Color(color.r, color.g, color.b, bToggle ? 1f : 0.8f);
            FindChild(UIButton.current.gameObject, "SelectCursor").SetActive(bToggle);
            goPSPanel.SetActive(bToggle);

        }

        public void OnClickSavePose()
        {
            if (UICamera.currentTouchID == -1)
            {
                //ボーンデータの保存処理
                XmlDocument document = new XmlDocument();


                if (!File.Exists(PoseXmlFileName))
                {
                    XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);  // XML宣言
                    document.AppendChild(declaration);
                    XmlElement Ini_root;
                    Ini_root = document.CreateElement("poses");  // ルート要素
                    Ini_root.SetAttribute("name", "bonepose.xml");
                    Ini_root.SetAttribute("description", "CM3D2.AddBoneSlider.Plugin の各bone値保存用XML");
                    document.AppendChild(Ini_root);
                    document.Save(PoseXmlFileName);
                }

                document.Load(PoseXmlFileName);

                PoseTexDirectoryName = settingIni.PoseImgDirectory;

                //Debuginfo.Log("PoseTexDirectoryName :" + PoseTexDirectoryName);
                string dateName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                XmlElement root = document.DocumentElement;


                XmlElement elementBones = document.CreateElement("bones");
                elementBones.SetAttribute("pose_id", dateName);  // 属性
                elementBones.SetAttribute("format", "1.00");
                root.AppendChild(elementBones);


                foreach (string bone in mp.sBone)
                {
                    if (mp.bNosave[bone] == true) continue;

                    XmlElement elementBone = document.CreateElement("bone");
                    elementBone.SetAttribute("id", bone);  // 属性
                    elementBones.AppendChild(elementBone);
                    {
                        if (bone == "Bip01")
                        {
                            XmlElement value_px = document.CreateElement("value");
                            value_px.SetAttribute("prop_name", bone + ".px");  // 属性
                            value_px.SetAttribute("axis", "posx");  // 属性
                            value_px.SetAttribute("val", mp.fValue[bone][bone + ".px"].ToString("F4"));  // 属性
                            elementBone.AppendChild(value_px);

                            XmlElement value_py = document.CreateElement("value");
                            value_py.SetAttribute("prop_name", bone + ".py");  // 属性
                            value_py.SetAttribute("axis", "posy");  // 属性
                            value_py.SetAttribute("val", mp.fValue[bone][bone + ".py"].ToString("F4"));  // 属性
                            elementBone.AppendChild(value_py);

                            XmlElement value_pz = document.CreateElement("value");
                            value_pz.SetAttribute("prop_name", bone + ".pz");  // 属性
                            value_pz.SetAttribute("axis", "posz");  // 属性
                            value_pz.SetAttribute("val", mp.fValue[bone][bone + ".pz"].ToString("F4"));  // 属性
                            elementBone.AppendChild(value_pz);
                        }

                        XmlElement value_x = document.CreateElement("value");
                        value_x.SetAttribute("prop_name", bone + ".x");  // 属性
                        value_x.SetAttribute("axis", "rotx");  // 属性
                        value_x.SetAttribute("val", mp.fValue[bone][bone + ".x"].ToString("F4"));  // 属性
                        elementBone.AppendChild(value_x);

                        XmlElement value_y = document.CreateElement("value");
                        value_y.SetAttribute("prop_name", bone + ".y");  // 属性
                        value_y.SetAttribute("axis", "roty");  // 属性
                        value_y.SetAttribute("val", mp.fValue[bone][bone + ".y"].ToString("F4"));  // 属性
                        elementBone.AppendChild(value_y);

                        XmlElement value_z = document.CreateElement("value");
                        value_z.SetAttribute("prop_name", bone + ".z");  // 属性
                        value_z.SetAttribute("axis", "rotz");  // 属性
                        value_z.SetAttribute("val", mp.fValue[bone][bone + ".z"].ToString("F4"));  // 属性
                        elementBone.AppendChild(value_z);

                    }
                }


                // ファイルに保存する
                document.Save(PoseXmlFileName);

                //ポーズ画像の保存処理
                if (!Directory.Exists(PoseTexDirectoryName))
                {
                    Directory.CreateDirectory(PoseTexDirectoryName);

                }

                string text = PoseTexDirectoryName + @"\poseimg" + dateName + ".png";
                ThumShot posethumshot = GameMain.Instance.ThumCamera.GetComponent<ThumShot>();

                //posethumshot.MoveTargetCard(maid);
                Transform transform = CMT.SearchObjName(maid.transform, "Bip01 HeadNub", true);
                if (transform != null)
                {
                    posethumshot.transform.position = transform.TransformPoint(transform.localPosition + new Vector3(0.84f, 2.25f, 0f));
                    posethumshot.transform.rotation = transform.rotation * Quaternion.Euler(90f, 0f, 90f);
                }
                else
                {
                    Debug.LogError(LogLabel + "：サムネイルを取ろうとしましたがメイドが居ません。");
                    return;
                }

                //ここの間で他のメイドがいたら消す処理を加える
                //メイドの表示状態を記録しておく
                //ここで消す
                bool otherMaid = true;
                int tempCurrentNo = this.currentMaidNo;
                List<int> visMaidNo = new List<int>();
                int stockNo = FindVisibleMaidStockNo(this.currentMaidNo + 1, 1);
                while (otherMaid)
                {

                    if (stockNo != -1)
                    {

                        if (this.currentMaidNo == stockNo)
                        {
                            otherMaid = false;
                            break;
                        }
                        else
                        {
                            Maid tempMaid;
                            if (sceneLevel == 27)
                            {
                                //公式撮影モード
                                tempMaid = GameMain.Instance.CharacterMgr.GetMaid(stockNo);
                            }
                            else
                            {
                                //複数撮影モード
                                tempMaid = GameMain.Instance.CharacterMgr.GetStockMaid(stockNo);
                            }

                            visMaidNo.Add(stockNo);
                            tempMaid.Visible = !tempMaid.Visible;
                            //GameMain.Instance.CharacterMgr.BanishmentMaid(maid);

                            stockNo = FindVisibleMaidStockNo(stockNo + 1, 1);
                        }
                    }
                    else
                    {
                        Debug.LogError(LogLabel + ":maid is Lost!");
                        break;
                    }
                }

                Camera poseshotthumCamera = posethumshot.gameObject.GetComponent<Camera>();
                poseshotthumCamera.fieldOfView = 50f;

                //撮影
                RenderTexture m_rtThumCard = new RenderTexture(100, 100, 24, RenderTextureFormat.ARGB32);
                m_rtThumCard.filterMode = FilterMode.Bilinear;
                m_rtThumCard.antiAliasing = 8;
                RenderTexture m_rtThumCard2 = new RenderTexture(100, 100, 0, RenderTextureFormat.ARGB32);

                Texture2D posetex = posethumshot.RenderThum(poseshotthumCamera, m_rtThumCard, m_rtThumCard2, new Size<int>(100, 100));
                byte[] bytes = posetex.EncodeToPNG();
                File.WriteAllBytes(text, bytes);

                //ここで消してたメイドを元に戻す

                foreach (int MaidNo in visMaidNo)
                {
                    Maid tempMaid;
                    if (sceneLevel == 27)
                    {
                        //公式撮影モード
                        tempMaid = GameMain.Instance.CharacterMgr.GetMaid(MaidNo);
                    }
                    else
                    {
                        //複数撮影モード
                        tempMaid = GameMain.Instance.CharacterMgr.GetStockMaid(MaidNo);
                    }
                    tempMaid.Visible = true;
                }
                
                this.currentMaidNo = tempCurrentNo;


                //作成した新しいポーズをロードできるようにする処理
                GameObject goPoseImgButton = SetCloneChild(goPSScrollViewTable, FindChild(goPSScrollViewTable, "ImageOriginal"), "Image:" + dateName);
                goPoseImgButton.AddComponent<UIDragScrollView>().scrollView = uiPSScrollView;

                UIButton uiPoseImgButton = goPoseImgButton.GetComponent<UIButton>();
                EventDelegate.Set(uiPoseImgButton.onClick, new EventDelegate.Callback(this.OnClickPoseImg));

                UISprite uiSpritePoseImgButton = goPoseImgButton.GetComponent<UISprite>();
                uiSpritePoseImgButton.type = UIBasicSprite.Type.Sliced;
                uiSpritePoseImgButton.SetDimensions(100, 100);

                UI2DSprite uiTexturePose = NGUITools.AddWidget<UI2DSprite>(goPoseImgButton);
                Sprite sprite2D = Sprite.Create(posetex, new Rect(0f, 0f, (float)posetex.width, (float)posetex.height), default(Vector2));
                uiTexturePose.sprite2D = sprite2D;
                uiTexturePose.depth = basedepth + 1;
                uiTexturePose.MakePixelPerfect();

                UILabel uiLabelPoseImg = FindChild(goPoseImgButton, "Name").GetComponent<UILabel>();
                NGUITools.Destroy(uiLabelPoseImg.gameObject);

                FindChild(goPoseImgButton, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                FindChild(goPoseImgButton, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goPoseImgButton);
                goPoseImgButton.SetActive(true);

                uiPSTable.Reposition();
            }
            //右クリックしたとき、anmファイル出力用のパネルを展開
            else if (UICamera.currentTouchID == -2)
            {
                goPNamePanel.SetActive(true);

                
            }
        }

        public void OnClickUndoAll()
        {
            try
            {

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickUndoAll() " + ex); return; }
        }

        public void OnClickHandleButton()
        {
            try
            {
                string bone = getTag(UIButton.current, 1);

                if (bone == "secret" || bone == "eye" || bone == "camera" || bone == "offset" || bone == "allpos")
                    return;

                if (activeHandleName != bone)
                {
                    if (activeHandleName != "")
                    {
                        visibleHandle(activeHandleName);
                        setButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);

                    }
                    posHandle.SetParentBone(trBone[bone]);
                }

                bool b = visibleHandle(bone);
                setButtonColor(UIButton.current, b);
                activeHandleName = b ? bone : "";

                posHandle.setVisible(b);

                //posHandle.Visible = b;


            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickHandleButton() " + ex); return; }
        }

        public void OnClickResetAll()
        {
            try
            {
                foreach (string bone in mp.sBone)
                {
                    if (mp.IsToggle(bone))
                    {
                        mp.bEnabled[bone] = false;
                        setButtonColor(bone, mp.bEnabled[bone]);
                    }

                    if (mp.IsSlider(bone))
                    {
                        resetSliderValue(bone);

                        if (mp.IsToggle(bone))
                        {
                            setSliderVisible(bone, mp.bEnabled[bone]);
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickResetAll() " + ex); return; }
        }

        public void OnClickResetButton()
        {
            resetSliderValue(getTag(UIButton.current, 1));
        }

        public void OnClickOutputOkANM()
        {
            Debuginfo.Log(LogLabel +FindChild(goPNamePanel, "NameStringValue").GetComponent<UIInput>().value);

            outputANMPose(FindChild(goPNamePanel, "NameStringValue").GetComponent<UIInput>().value);
            
            goPNamePanel.SetActive(false);

        }

        public void OnClickOutputCancelANM()
        {
         
            goPNamePanel.SetActive(false);
        }

        public void OnChangeSlider()
        {

            if (bLocked == true)
            {
                return;
            }
            try
            {
                bLocked = true;
                string bone = getTag(UIProgressBar.current, 1);
                string prop = getTag(UIProgressBar.current, 2);

                float value = codecSliderValue(bone, prop, UIProgressBar.current.value);
                string vType = mp.sVType[bone][prop];
                float vZero = mp.fVzero[bone][prop];

                undoValue[bone][prop] = mp.fValue[bone][prop];
                uiValueLable[bone][prop].text = value.ToString("F4");
                uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = value.ToString("F4");

                float preValue = mp.fValue[bone][prop];
                mp.fValue[bone][prop] = value;

                //Eye to camera をここで止める
                maid.body0.m_Bones.animation.Stop();
                maid.body0.boHeadToCam = false;
                maid.body0.boEyeToCam = false;
                //maid.body0.MuneYureL(0f);
                //maid.body0.MuneYureR(0f);


                rotateBone(bone, prop);

                //捻りボーンのオート調整
                //maid.body0.AutoTwist();

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
                {
                    vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                }



                if (bone == "Bip01")
                {
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                }

                bLocked = false;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnChangeSlider() " + ex); bLocked = false; return; }
        }

        public void OnSubmitSliderValueInput()
        {

            try
            {
                bLocked = true;
                string bone = getTag(UIInput.current, 1);
                string prop = getTag(UIInput.current, 2);
                UISlider slider = null;

                foreach (Transform t in UIInput.current.transform.parent.parent)
                {
                    if (getTag(t, 0) == "Slider") slider = t.GetComponent<UISlider>();
                }

                float value;
                if (Single.TryParse(UIInput.current.value, out value))
                {
                    float preValue = mp.fValue[bone][prop];
                    mp.fValue[bone][prop] = value;
                    slider.value = codecSliderValue(bone, prop);
                    UIInput.current.value = codecSliderValue(bone, prop, slider.value).ToString("F4");
                    uiValueLable[bone][prop].text = UIInput.current.value;


                }

                rotateBone(bone, prop);

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
                {
                    vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                }

                if (bone == "Bip01")
                {
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                }

                bLocked = false;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnSubmitSliderValueInput() " + ex); return; }

        }

        public void OnSubmitPoseName()
        {
            try
            {
                Debuginfo.Log(LogLabel + UIInput.current.value);

                outputANMPose(UIInput.current.value);

                goPNamePanel.SetActive(false);
                
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnSubmitPoseName() " + ex); return; }

        }

        public void OnClickBoneCategory()
        {
            //カテゴリーを選択
            GameObject goSystemUnit = FindChild(goAMSPanel, "System:Undo");
            GameObject goBoneCategory0 = FindChild(goSystemUnit, "BoneCategory0");
            GameObject goBoneCategory1 = FindChild(goSystemUnit, "BoneCategory1");
            GameObject goBoneCategory2 = FindChild(goSystemUnit, "BoneCategory2");
            GameObject goBoneCategory3 = FindChild(goSystemUnit, "BoneCategory3");
            GameObject goBoneCategory4 = FindChild(goSystemUnit, "BoneCategory4");
            GameObject goBoneCategory5 = FindChild(goSystemUnit, "BoneCategory5");


            UIButton uiButtonBoneCategory0 = goBoneCategory0.GetComponent<UIButton>();
            UIButton uiButtonBoneCategory1 = goBoneCategory1.GetComponent<UIButton>();
            UIButton uiButtonBoneCategory2 = goBoneCategory2.GetComponent<UIButton>();
            UIButton uiButtonBoneCategory3 = goBoneCategory3.GetComponent<UIButton>();
            UIButton uiButtonBoneCategory4 = goBoneCategory4.GetComponent<UIButton>();
            UIButton uiButtonBoneCategory5 = goBoneCategory5.GetComponent<UIButton>();

            Color color = UIButton.current.defaultColor;

            uiButtonBoneCategory0.defaultColor = new Color(color.r, color.g, color.b, 0.8f);
            uiButtonBoneCategory1.defaultColor = new Color(color.r, color.g, color.b, 0.8f);
            uiButtonBoneCategory2.defaultColor = new Color(color.r, color.g, color.b, 0.8f);
            uiButtonBoneCategory3.defaultColor = new Color(color.r, color.g, color.b, 0.8f);
            uiButtonBoneCategory4.defaultColor = new Color(color.r, color.g, color.b, 0.8f);
            uiButtonBoneCategory5.defaultColor = new Color(color.r, color.g, color.b, 0.8f);

            UIButton.current.defaultColor = new Color(color.r, color.g, color.b, 1f);


            FindChild(uiButtonBoneCategory0.gameObject, "SelectCursor").SetActive(false);
            FindChild(uiButtonBoneCategory1.gameObject, "SelectCursor").SetActive(false);
            FindChild(uiButtonBoneCategory2.gameObject, "SelectCursor").SetActive(false);
            FindChild(uiButtonBoneCategory3.gameObject, "SelectCursor").SetActive(false);
            FindChild(uiButtonBoneCategory4.gameObject, "SelectCursor").SetActive(false);
            FindChild(uiButtonBoneCategory5.gameObject, "SelectCursor").SetActive(false);

            FindChild(UIButton.current.gameObject, "SelectCursor").SetActive(true);



            foreach (GameObject go in goScrollViewTable)
            {
                go.SetActive(false);
            }
            if (UIButton.current == uiButtonBoneCategory0) goScrollViewTable[0].SetActive(true);
            else if (UIButton.current == uiButtonBoneCategory1) goScrollViewTable[1].SetActive(true);
            else if (UIButton.current == uiButtonBoneCategory2) goScrollViewTable[2].SetActive(true);
            else if (UIButton.current == uiButtonBoneCategory3) goScrollViewTable[3].SetActive(true);
            else if (UIButton.current == uiButtonBoneCategory4) goScrollViewTable[4].SetActive(true);
            else if (UIButton.current == uiButtonBoneCategory5) goScrollViewTable[5].SetActive(true);

            uiScrollView.OnScrollBar();
            uiScrollBar.value = 0f;
        }

        #endregion



        #region Private methods
        /*
        private IEnumerator waitTime(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
        }
        */

        private IEnumerator initCoroutine()
        {
            while (!(bInitCompleted = initialize())) yield return new WaitForSeconds(TimePerInit);
            Debuginfo.Log(LogLabel + "Initialization complete.");
        }

        private bool initialize()
        {
            try
            {
                Debuginfo.settingLevel = 0;

                Debuginfo.Log(LogLabel + "Initialization start.", 1);

                string currentDirectory;

                //ここでiniファイル読み込み処理
                //Sybaris環境かどうかチェック
                bool bExistSybaris;
                if (File.Exists(Directory.GetCurrentDirectory() + @"\opengl32.dll") && Directory.Exists(Directory.GetCurrentDirectory() + @"\Sybaris"))
                {
                    currentDirectory = Directory.GetCurrentDirectory() + @"\Sybaris\Plugins";
                    bExistSybaris = true;
 
                }
                else
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    bExistSybaris = false;

                }


                iniFileName = currentDirectory + @"\UnityInjector\Config\boneslider.ini";
                //iniファイルがない場合は作成
                if (!File.Exists(iniFileName))
                {
                    settingIni = new SettingIni();
                    settingIni.PoseXmlDirectory = currentDirectory + @"\UnityInjector\Config";
                    settingIni.PoseImgDirectory = currentDirectory + @"\UnityInjector\Config\PoseImg";
                    if(bExistSybaris)
                    {
                        settingIni.AnmOutputmode = "sybaris";
                        settingIni.OutputJsonDirectory = Directory.GetCurrentDirectory() + @"\Sybaris\Poses";
                        settingIni.OutputAnmSybarisDirectory = Directory.GetCurrentDirectory() + @"\Sybaris\GameData\Samples";
                    }
                    else
                    {
                        settingIni.AnmOutputmode = "photomode";
                        settingIni.OutputJsonDirectory = "none";
                        settingIni.OutputAnmSybarisDirectory = "none";
                    }


                }
                else
                {
                    //iniファイルがある場合は設定されてない項目を補完

                    settingIni = IniFileHelper.Read<SettingIni>("setting", iniFileName);
                    if (settingIni.PoseXmlDirectory == "")
                        settingIni.PoseXmlDirectory = currentDirectory + @"\UnityInjector\Config";
                    if (settingIni.PoseImgDirectory == "")
                        settingIni.PoseImgDirectory = currentDirectory + @"\UnityInjector\Config\PoseImg";

                    settingIni.AnmOutputmode = settingIni.AnmOutputmode.ToLower();
                    if (settingIni.AnmOutputmode != "both" && settingIni.AnmOutputmode != "photomode"&& settingIni.AnmOutputmode != "sybaris")
                    {
                        if(bExistSybaris)
                        {
                            settingIni.AnmOutputmode = "sybaris";
                        }
                        else
                        {
                            settingIni.AnmOutputmode = "photomode";
                        }
                    }

                    if (settingIni.OutputJsonDirectory == "")
                    {
                        if (bExistSybaris)
                        {
                            settingIni.OutputJsonDirectory = Directory.GetCurrentDirectory() + @"\Sybaris\Poses";
                        }
                        else
                        {
                            settingIni.OutputJsonDirectory = "none";
                        }
                    }

                    if (settingIni.OutputAnmSybarisDirectory == "")
                    {
                        if (bExistSybaris)
                        {
                            settingIni.OutputAnmSybarisDirectory = Directory.GetCurrentDirectory() + @"\Sybaris\GameData\Samples";
                        }
                        else
                        {
                            settingIni.OutputAnmSybarisDirectory = "none";
                        }
                    }


                    settingIni.ToggleKey = settingIni.ToggleKey.ToLower();





                }
                IniFileHelper.Write<SettingIni>("setting", settingIni, iniFileName);

                Debuginfo.settingLevel = settingIni.DebugLogLevel;

                //ここまで


                PoseXmlFileName = settingIni.PoseXmlDirectory + @"\bonepose.xml"; //Directory.GetCurrentDirectory() + @"\UnityInjector\Config\bonepose.xml";

                Debuginfo.Log(LogLabel + "PoseXmlFileName complete.", 1);

                //メイド情報取得

                //公式撮影モードでも複数メイドでもここの処理の結果は同じはずだからこのまま
                
                currentMaidNo = 0;
                maid = GameMain.Instance.CharacterMgr.GetMaid(currentMaidNo);

                //じゃまずいから複数メイド撮影のときだけcurrentMaidNoの値をここで同期しとく
                if(sceneLevel == 5)
                {
                    List<Maid> maidList = GameMain.Instance.CharacterMgr.GetStockMaidList();
                    for (int i = 0; i < maidList.Count; i++)
                    {
                        if (maidList[i] == maid)
                        {
                            currentMaidNo = i;
                        }
                    }
                }
                //if (maid == null) return false;

                Debuginfo.Log(LogLabel + "GetMaid complete.", 1);

                UIAtlas uiAtlasSceneEdit = FindAtlas("AtlasSceneEdit");
                UIAtlas uiAtlasDialog = FindAtlas("SystemDialog");

                GameObject goUIRoot = GameObject.Find("UI Root");
                GameObject cameraObject = GameObject.Find("/UI Root/Camera");
                Camera cameraComponent = cameraObject.GetComponent<Camera>();
                uiCamara = cameraObject.GetComponent<UICamera>();

                #region createSlider

                // スライダー作成
                GameObject goTestSliderUnit = new GameObject("TestSliderUnit");
                SetChild(goUIRoot, goTestSliderUnit);
                {
                    UISprite uiTestSliderUnitFrame = goTestSliderUnit.AddComponent<UISprite>();
                    uiTestSliderUnitFrame.atlas = uiAtlasSceneEdit;
                    uiTestSliderUnitFrame.spriteName = "cm3d2_edit_slidertitleframe";
                    uiTestSliderUnitFrame.type = UIBasicSprite.Type.Sliced;
                    uiTestSliderUnitFrame.SetDimensions(500, 50);

                    // スライダー作成
                    UISlider uiTestSlider = NGUITools.AddChild<UISlider>(goTestSliderUnit);
                    UISprite uiTestSliderRail = uiTestSlider.gameObject.AddComponent<UISprite>();
                    uiTestSliderRail.name = "Slider";
                    uiTestSliderRail.atlas = uiAtlasSceneEdit;
                    uiTestSliderRail.spriteName = "cm3d2_edit_slideberrail";
                    uiTestSliderRail.type = UIBasicSprite.Type.Sliced;
                    uiTestSliderRail.SetDimensions(250, 5);
                    uiTestSliderRail.depth = basedepth;

                    UIWidget uiTestSliderBar = NGUITools.AddChild<UIWidget>(uiTestSlider.gameObject);
                    uiTestSliderBar.name = "DummyBar";
                    uiTestSliderBar.width = uiTestSliderRail.width;

                    UISprite uiTestSliderThumb = NGUITools.AddChild<UISprite>(uiTestSlider.gameObject);
                    uiTestSliderThumb.name = "Thumb";
                    uiTestSliderThumb.depth = uiTestSliderRail.depth + 1;
                    uiTestSliderThumb.atlas = uiAtlasSceneEdit;
                    uiTestSliderThumb.spriteName = "cm3d2_edit_slidercursor";
                    uiTestSliderThumb.type = UIBasicSprite.Type.Sliced;
                    uiTestSliderThumb.SetDimensions(25, 25);


                    uiTestSliderThumb.gameObject.AddComponent<BoxCollider>();

                    uiTestSlider.backgroundWidget = uiTestSliderRail;
                    uiTestSlider.foregroundWidget = uiTestSliderBar;
                    uiTestSlider.thumb = uiTestSliderThumb.gameObject.transform;
                    uiTestSlider.value = 0.5f;
                    uiTestSlider.gameObject.AddComponent<BoxCollider>();
                    uiTestSlider.transform.localPosition = new Vector3(100f, 0f, 0f);

                    NGUITools.UpdateWidgetCollider(uiTestSlider.gameObject);
                    NGUITools.UpdateWidgetCollider(uiTestSliderThumb.gameObject);

                    // スライダーラベル作成
                    UILabel uiTestSliderLabel = NGUITools.AddChild<UILabel>(goTestSliderUnit);
                    uiTestSliderLabel.name = "Label";
                    uiTestSliderLabel.trueTypeFont = font;
                    uiTestSliderLabel.fontSize = 20;
                    uiTestSliderLabel.text = "テストスライダー";
                    uiTestSliderLabel.width = 110;
                    uiTestSliderLabel.overflowMethod = UILabel.Overflow.ShrinkContent;

                    uiTestSliderLabel.transform.localPosition = new Vector3(-190f, 0f, 0f);

                    // 値ラベル・インプット作成
                    UISprite uiTestSliderValueBase = NGUITools.AddChild<UISprite>(goTestSliderUnit);
                    uiTestSliderValueBase.name = "ValueBase";
                    uiTestSliderValueBase.atlas = uiAtlasSceneEdit;
                    uiTestSliderValueBase.spriteName = "cm3d2_edit_slidernumberframe";
                    uiTestSliderValueBase.type = UIBasicSprite.Type.Sliced;
                    uiTestSliderValueBase.SetDimensions(80, 35);
                    uiTestSliderValueBase.transform.localPosition = new Vector3(-90f, 0f, 0f);

                    UILabel uiTestSliderValueLabel = NGUITools.AddChild<UILabel>(uiTestSliderValueBase.gameObject);
                    uiTestSliderValueLabel.name = "Value";
                    uiTestSliderValueLabel.depth = uiTestSliderValueBase.depth + 1;
                    uiTestSliderValueLabel.width = uiTestSliderValueBase.width;
                    uiTestSliderValueLabel.trueTypeFont = font;
                    uiTestSliderValueLabel.fontSize = 20;
                    uiTestSliderValueLabel.text = "0.00";
                    uiTestSliderValueLabel.color = Color.black;

                    UIInput uiTestSliderValueInput = uiTestSliderValueLabel.gameObject.AddComponent<UIInput>();
                    uiTestSliderValueInput.label = uiTestSliderValueLabel;
                    uiTestSliderValueInput.onReturnKey = UIInput.OnReturnKey.Submit;
                    uiTestSliderValueInput.validation = UIInput.Validation.Float;
                    uiTestSliderValueInput.activeTextColor = Color.black;
                    uiTestSliderValueInput.caretColor = new Color(0.1f, 0.1f, 0.3f, 1f);
                    uiTestSliderValueInput.selectionColor = new Color(0.3f, 0.3f, 0.6f, 0.8f);
                    //EventDelegate.Add(uiTestSliderValueInput.onSubmit, new EventDelegate.Callback(this.OnSubmitSliderValueInput));

                    uiTestSliderValueInput.gameObject.AddComponent<BoxCollider>();
                    NGUITools.UpdateWidgetCollider(uiTestSliderValueInput.gameObject);
                }
                goTestSliderUnit.SetActive(false);

                Debuginfo.Log(LogLabel + "goTestSliderUnit complete.");

                #endregion


                // ボタンはgoProfileTabをコピー
                GameObject goProfileTabCopy = null;

                Transform tempProfilePanel = goUIRoot.transform.Find("ProfilePanel");
                if (tempProfilePanel != null)
                {
                    Debuginfo.Log(LogLabel + "EditMode");
                    goProfileTabCopy = UnityEngine.Object.Instantiate(FindChild(tempProfilePanel.Find("Comment").gameObject, "ProfileTab")) as GameObject;

                }
                else
                {
                    Debuginfo.Log(LogLabel + "PhotoWindowMode");


                    goProfileTabCopy = UnityEngine.Object.Instantiate(FindChild(goUIRoot.transform.Find("MainScreen").Find("PhotoWindowManager").Find("WindowVisibleBtnsParent").Find("WindowVisibleBtns").gameObject, "UIOFF")) as GameObject;


                    FindChild(goProfileTabCopy, "Text").name = "Name";
                    UISprite TabCopySprite = goProfileTabCopy.GetComponent<UISprite>();

                    UIAtlas uiAtlasCommon = FindAtlas("AtlasCommon");
                    //TabCopySprite.atlas = uiAtlasSceneEdit;
                    TabCopySprite.spriteName = "cm3d2_common_plate_white";
                    TabCopySprite.MakePixelPerfect();
                    TabCopySprite.depth = basedepth;

                    UIAtlas uiAtlasCommon2 = FindAtlas("AtlasCommon2");

                    UISprite CursorSprite = NGUITools.AddSprite(goProfileTabCopy, uiAtlasCommon2, "cm3d2_rentalmaid_selectcursor");
                    CursorSprite.depth = basedepth + 2;
                    goProfileTabCopy.transform.FindChild("Sprite").name = "SelectCursor";
                    Destroy(goProfileTabCopy.GetComponent<UIPlayAnimation>());

                    //Debuginfo.Log(LogLabel + " hover:"+goProfileTabCopy.GetComponent<UIButton>().);
                    //UIPlayAnimation

                }

                //丸いボタンは（どっかの）Cancelからコピー
                GameObject goCancelCopy = null;

                Transform tempOkCancel = goUIRoot.transform.Find("OkCancel");
                if (tempOkCancel != null)
                {
                    Debuginfo.Log(LogLabel + "EditMode2");
                    goCancelCopy = UnityEngine.Object.Instantiate(FindChild(tempOkCancel.gameObject, "Cancel")) as GameObject;
                }
                else
                {
                    Debuginfo.Log(LogLabel + "PhotoMode2");
                    goCancelCopy = UnityEngine.Object.Instantiate(FindChild(goUIRoot.transform.Find("MainScreen").Find("SaveAndLoadPanel").gameObject, "Cancel")) as GameObject;
                    
                }
               
                

                UILabel uiLabelTabCopy = FindChild(goProfileTabCopy, "Name").GetComponent<UILabel>();
                uiLabelTabCopy.text = "";

                

                //EventDelegate.Remove(goProfileTabCopy.GetComponent<UIButton>().onClick, new EventDelegate.Callback(ProfileMgr.Instance.ChangeCommentTab));
                EventDelegate.Remove(goProfileTabCopy.GetComponent<UIButton>().onClick, goProfileTabCopy.GetComponent<UIButton>().onClick.First());

                goProfileTabCopy.SetActive(false);
                goCancelCopy.SetActive(false);

                Debuginfo.Log(LogLabel + "SpriteSet complete.");

                #region createPanel

                Debuginfo.Log(LogLabel + " goProfileTabCopy complete.");


                // BoneSliderPanel作成
                // ここでパネル位置調整
                Vector3 originAMSPanel = new Vector3(settingIni.WindowPositionX, settingIni.WindowPositionY, 0f);
                int systemUnitHeight = 30;

                // 親Panel
                uiAMSPanel = NGUITools.AddChild<UIPanel>(goUIRoot);
                uiAMSPanel.name = "BoneSliderPanel";
                uiAMSPanel.transform.localPosition = originAMSPanel;

                goAMSPanel = uiAMSPanel.gameObject;

                // 背景
                UISprite uiBGSprite = NGUITools.AddChild<UISprite>(goAMSPanel);
                uiBGSprite.name = "BG";
                uiBGSprite.atlas = uiAtlasSceneEdit;
                uiBGSprite.spriteName = "cm3d2_edit_window_l";
                uiBGSprite.type = UIBasicSprite.Type.Sliced;
                uiBGSprite.SetDimensions(ScrollViewWidth, ScrollViewHeight);
                //////////////////////////////////////////////////////////////////////////////////
                // ScrollViewPanel
                uiScrollPanel = NGUITools.AddChild<UIPanel>(goAMSPanel);
                uiScrollPanel.name = "ScrollView";

                //uiScrollPanel.sortingOrder = uiAMSPanel.sortingOrder + 1;
                uiScrollPanel.depth = uiAMSPanel.depth + 1;

                uiScrollPanel.clipping = UIDrawCall.Clipping.SoftClip;
                uiScrollPanel.SetRect(0f, 0f, uiBGSprite.width, uiBGSprite.height - 130 - systemUnitHeight * 6);
                uiScrollPanel.transform.localPosition = new Vector3(-25f, -systemUnitHeight * 4, 0f);
                goScrollView = uiScrollPanel.gameObject;

                uiScrollView = goScrollView.AddComponent<UIScrollView>();
                uiScrollView.contentPivot = UIWidget.Pivot.Center;
                uiScrollView.movement = UIScrollView.Movement.Vertical;
                uiScrollView.scrollWheelFactor = 1.5f;



                uiBGSprite.gameObject.AddComponent<UIDragScrollView>().scrollView = uiScrollView;
                uiBGSprite.gameObject.AddComponent<BoxCollider>();
                NGUITools.UpdateWidgetCollider(uiBGSprite.gameObject);

                // ScrollBar
                uiScrollBar = NGUITools.AddChild<UIScrollBar>(goAMSPanel);
                uiScrollBar.value = 0f;

                uiScrollBar.gameObject.AddComponent<BoxCollider>();
                uiScrollBar.transform.localPosition = new Vector3(uiBGSprite.width / 2f - 10, -systemUnitHeight * 4, 0f);
                uiScrollBar.transform.localRotation *= Quaternion.Euler(0f, 0f, -90f);

                UIWidget uiScrollBarFore = NGUITools.AddChild<UIWidget>(uiScrollBar.gameObject);
                uiScrollBarFore.name = "DummyFore";
                uiScrollBarFore.height = 15;
                uiScrollBarFore.width = uiBGSprite.height;

                UISprite uiScrollBarThumb = NGUITools.AddChild<UISprite>(uiScrollBar.gameObject);
                uiScrollBarThumb.name = "Thumb";
                uiScrollBarThumb.depth = uiBGSprite.depth + 1;
                uiScrollBarThumb.atlas = uiAtlasSceneEdit;
                uiScrollBarThumb.spriteName = "cm3d2_edit_slidercursor";
                uiScrollBarThumb.type = UIBasicSprite.Type.Sliced;
                uiScrollBarThumb.SetDimensions(15, 15);
                uiScrollBarThumb.gameObject.AddComponent<BoxCollider>();

                uiScrollBar.foregroundWidget = uiScrollBarFore;
                uiScrollBar.thumb = uiScrollBarThumb.transform;

                NGUITools.UpdateWidgetCollider(uiScrollBarFore.gameObject);
                NGUITools.UpdateWidgetCollider(uiScrollBarThumb.gameObject);
                uiScrollView.verticalScrollBar = uiScrollBar;


                for (int i = 0; i < 6; ++i)
                {
                    uiTable[i] = NGUITools.AddChild<UITable>(goScrollView);
                    uiTable[i].pivot = UIWidget.Pivot.Center;
                    uiTable[i].columns = 1;
                    uiTable[i].padding = new Vector2(25f, 10f);
                    uiTable[i].hideInactive = true;
                    uiTable[i].keepWithinPanel = true;
                    uiTable[i].sorting = UITable.Sorting.Custom;
                    uiTable[i].onCustomSort = (Comparison<Transform>)this.sortGridByXMLOrder;
                    //uiTable[].onReposition    = this.OnRepositionTable;
                    goScrollViewTable[i] = uiTable[i].gameObject;
                    if (i > 0)
                    {
                        goScrollViewTable[i].SetActive(false);
                    }
                }


                //////////////////////////////////////////////////////////////////


                Debuginfo.Log(LogLabel + " goScrollViewTable complete.");


                // ドラッグ用タブ（タイトル部分）
                UISprite uiSpriteTitleTab = NGUITools.AddChild<UISprite>(goAMSPanel);
                uiSpriteTitleTab.name = "TitleTab";
                uiSpriteTitleTab.depth = uiBGSprite.depth - 1;
                uiSpriteTitleTab.atlas = uiAtlasDialog;
                uiSpriteTitleTab.spriteName = "cm3d2_dialog_frame";
                uiSpriteTitleTab.type = UIBasicSprite.Type.Sliced;
                uiSpriteTitleTab.SetDimensions(300, 80);
                uiSpriteTitleTab.autoResizeBoxCollider = true;


                UIDragObject uiDragObject = uiSpriteTitleTab.gameObject.AddComponent<UIDragObject>();
                uiDragObject.target = goAMSPanel.transform;
                uiDragObject.dragEffect = UIDragObject.DragEffect.None;
                uiSpriteTitleTab.gameObject.AddComponent<BoxCollider>().isTrigger = true;
                NGUITools.UpdateWidgetCollider(uiSpriteTitleTab.gameObject);


                uiSpriteTitleTab.transform.localPosition = new Vector3(uiBGSprite.width / 2f + 5f, (uiBGSprite.height - uiSpriteTitleTab.width) / 2f, 0f);
                uiSpriteTitleTab.transform.localRotation *= Quaternion.Euler(0f, 0f, -90f);


                UILabel uiLabelTitleTab = uiSpriteTitleTab.gameObject.AddComponent<UILabel>();
                uiLabelTitleTab.depth = uiSpriteTitleTab.depth + 1;
                uiLabelTitleTab.width = uiSpriteTitleTab.width;
                uiLabelTitleTab.color = Color.white;
                uiLabelTitleTab.trueTypeFont = font;
                uiLabelTitleTab.fontSize = 18;
                uiLabelTitleTab.text = "Bone Slider " + AddBoneSlider.Version;

                int conWidth = (int)(uiBGSprite.width - uiTable[0].padding.x * 2);
                int baseTop = (int)(uiBGSprite.height / 2f - 50);


                GameObject goSystemUnit = NGUITools.AddChild(goAMSPanel);
                goSystemUnit.name = ("System:Undo");
                ///////////////////////////////////////////////
                //ポーズロード用パネル作成
                uiPSPanel = NGUITools.AddChild<UIPanel>(goSystemUnit);
                uiPSPanel.name = "PoseSelectPanel";
                uiPSPanel.transform.localPosition = new Vector3(-ScrollViewWidth / 2f - 140f / 2f - 10f, 0f, 0f); ;
                goPSPanel = uiPSPanel.gameObject;


                UISprite uiPSBGSprite = NGUITools.AddChild<UISprite>(goPSPanel);
                uiPSBGSprite.name = "PSBG";
                uiPSBGSprite.atlas = uiAtlasSceneEdit;
                uiPSBGSprite.spriteName = "cm3d2_edit_window_l";
                uiPSBGSprite.type = UIBasicSprite.Type.Sliced;
                uiPSBGSprite.SetDimensions(140, ScrollViewHeight);


                uiPSScrollPanel = NGUITools.AddChild<UIPanel>(goPSPanel);
                uiPSScrollPanel.name = "ScrollView";

                //uiPSScrollPanel.sortingOrder = uiPSPanel.sortingOrder + 1;
                uiPSScrollPanel.depth = uiPSPanel.depth + 1;

                uiPSScrollPanel.clipping = UIDrawCall.Clipping.SoftClip;
                uiPSScrollPanel.SetRect(0f, 0f, uiPSBGSprite.width, uiPSBGSprite.height);
                uiPSScrollPanel.transform.localPosition = new Vector3(-23f, 0f, 0f);
                goPSScrollView = uiPSScrollPanel.gameObject;


                uiPSScrollView = goPSScrollView.AddComponent<UIScrollView>();
                uiPSScrollView.contentPivot = UIWidget.Pivot.Center;
                uiPSScrollView.movement = UIScrollView.Movement.Vertical;
                uiPSScrollView.scrollWheelFactor = 1.5f;

                uiPSBGSprite.gameObject.AddComponent<UIDragScrollView>().scrollView = uiPSScrollView;
                uiPSBGSprite.gameObject.AddComponent<BoxCollider>();

                NGUITools.UpdateWidgetCollider(uiPSBGSprite.gameObject);


                uiPSScrollBar = NGUITools.AddChild<UIScrollBar>(goPSPanel);
                uiPSScrollBar.value = 0f;
                uiPSScrollBar.gameObject.AddComponent<BoxCollider>();
                uiPSScrollBar.transform.localPosition = new Vector3(uiPSBGSprite.width / 2f - 10f, 0f, 0f);
                uiPSScrollBar.transform.localRotation *= Quaternion.Euler(0f, 0f, -90f);

                UIWidget uiPSScrollBarFore = NGUITools.AddChild<UIWidget>(uiPSScrollBar.gameObject);
                uiPSScrollBarFore.name = "DummyFore";
                uiPSScrollBarFore.height = 15;
                uiPSScrollBarFore.width = uiPSBGSprite.height;

                UISprite uiPSScrollBarThumb = NGUITools.AddChild<UISprite>(uiPSScrollBar.gameObject);
                uiPSScrollBarThumb.name = "Thumb";
                uiPSScrollBarThumb.depth = uiPSBGSprite.depth + 1;
                uiPSScrollBarThumb.atlas = uiAtlasSceneEdit;
                uiPSScrollBarThumb.spriteName = "cm3d2_edit_slidercursor";
                uiPSScrollBarThumb.type = UIBasicSprite.Type.Sliced;
                uiPSScrollBarThumb.SetDimensions(15, 15);
                uiPSScrollBarThumb.gameObject.AddComponent<BoxCollider>();

                uiPSScrollBar.foregroundWidget = uiPSScrollBarFore;
                uiPSScrollBar.thumb = uiPSScrollBarThumb.transform;

                NGUITools.UpdateWidgetCollider(uiPSScrollBarFore.gameObject);
                NGUITools.UpdateWidgetCollider(uiPSScrollBarThumb.gameObject);
                uiPSScrollView.verticalScrollBar = uiPSScrollBar;


                uiPSTable = NGUITools.AddChild<UITable>(goPSScrollView);
                uiPSTable.pivot = UIWidget.Pivot.Center;
                uiPSTable.columns = 1;
                uiPSTable.padding = new Vector2(25f, 10f);
                uiPSTable.hideInactive = true;
                uiPSTable.keepWithinPanel = true;
                uiPSTable.sorting = UITable.Sorting.Custom;
                goPSScrollViewTable = uiPSTable.gameObject;

                goPSPanel.SetActive(false);

                Debuginfo.Log(LogLabel + " goPSPanel complete.");

                //PoseImgボタンの作成処理

                GameObject goPoseImgButtonOriginal = SetCloneChild(goPSScrollViewTable, goProfileTabCopy, "ImageOriginal");
                goPoseImgButtonOriginal.SetActive(false);

                if (File.Exists(PoseXmlFileName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(PoseXmlFileName);
                    XmlNode poses = doc.DocumentElement;


                    XmlNodeList BonesNodeS = poses.SelectNodes("/poses/bones");

                    foreach (XmlNode bonesNode in BonesNodeS)
                    {
                        string name = ((XmlElement)bonesNode).GetAttribute("pose_id");

                        // button複製・追加

                        GameObject goPoseImgButton = SetCloneChild(goPSScrollViewTable, goPoseImgButtonOriginal, "Image:" + name);
                        goPoseImgButton.AddComponent<UIDragScrollView>().scrollView = uiPSScrollView;

                        UIButton uiPoseImgButton = goPoseImgButton.GetComponent<UIButton>();
                        EventDelegate.Set(uiPoseImgButton.onClick, new EventDelegate.Callback(this.OnClickPoseImg));

                        UISprite uiSpritePoseImgButton = goPoseImgButton.GetComponent<UISprite>();
                        uiSpritePoseImgButton.type = UIBasicSprite.Type.Sliced;
                        uiSpritePoseImgButton.SetDimensions(100, 100);



                        Texture2D thumPose = new Texture2D(2, 2);//
                        byte[] bytes = File.ReadAllBytes(settingIni.PoseImgDirectory + @"\PoseImg" + name + ".png");
                        thumPose.LoadImage(bytes);
                        if (thumPose != null)
                        {
                            UI2DSprite uiTexturePose = NGUITools.AddWidget<UI2DSprite>(goPoseImgButton);
                            Sprite sprite2D = Sprite.Create(thumPose, new Rect(0f, 0f, (float)thumPose.width, (float)thumPose.height), default(Vector2));
                            uiTexturePose.sprite2D = sprite2D;
                            uiTexturePose.depth = basedepth + 1;
                            uiTexturePose.MakePixelPerfect();

                        }
                        UILabel uiLabelPoseImg = FindChild(goPoseImgButton, "Name").GetComponent<UILabel>();
                        NGUITools.Destroy(uiLabelPoseImg.gameObject);
                        // 金枠Sprite

                        FindChild(goPoseImgButton, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                        FindChild(goPoseImgButton, "SelectCursor").SetActive(false);


                        NGUITools.UpdateWidgetCollider(goPoseImgButton);
                        goPoseImgButton.SetActive(true);
                    }
                }
                Debuginfo.Log(LogLabel + " goPoseImgButton complete.");
                ///////////////////////////////////////////////
                //PoseLoadボタン
                GameObject goLoadPose = SetCloneChild(goSystemUnit, goProfileTabCopy, "LoadPose");
                goLoadPose.transform.localPosition = new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight / 2f, 0f);

                UISprite uiSpriteLoadPose = goLoadPose.GetComponent<UISprite>();
                uiSpriteLoadPose.SetDimensions((int)(conWidth * 0.5f) - 2, systemUnitHeight);

                UILabel uiLabelLoadPose = FindChild(goLoadPose, "Name").GetComponent<UILabel>();

                uiLabelLoadPose.width = uiSpriteLoadPose.width - 10;
                uiLabelLoadPose.fontSize = 22;
                uiLabelLoadPose.spacingX = 0;
                uiLabelLoadPose.supportEncoding = true;
                uiLabelLoadPose.text = "[111111]LoadPose";

                UIButton uiButtonLoadPose = goLoadPose.GetComponent<UIButton>();
                uiButtonLoadPose.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonLoadPose.onClick, new EventDelegate.Callback(this.OnClickLoadPose));
                FindChild(goLoadPose, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.5f) - 2 - 4, systemUnitHeight - 4);
                FindChild(goLoadPose, "SelectCursor").SetActive(false);
                NGUITools.UpdateWidgetCollider(goLoadPose);
                goLoadPose.SetActive(true);

                Debuginfo.Log(LogLabel + " goLoadPose complete.");

                //PoseSaveボタン
                GameObject goSavePose = SetCloneChild(goSystemUnit, goLoadPose, "SavePose");
                goSavePose.transform.localPosition = new Vector3(conWidth * 0.25f - 4, baseTop - systemUnitHeight / 2f, 0f);

                UILabel uiLabelSavePose = FindChild(goSavePose, "Name").GetComponent<UILabel>();
                uiLabelSavePose.text = "[111111]SavePose";

                UIButton uiButtonSavePose = goSavePose.GetComponent<UIButton>();
                uiButtonSavePose.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonSavePose.onClick, new EventDelegate.Callback(this.OnClickSavePose));

                NGUITools.UpdateWidgetCollider(goSavePose);
                goSavePose.SetActive(true);

                Debuginfo.Log(LogLabel + " goSavePose complete.");

                ///////////////////////////////////////////////



                // 前メイド選択ボタン
                GameObject goPrevMaid = SetCloneChild(goSystemUnit, goProfileTabCopy, "PrevMaid");
                goPrevMaid.transform.localPosition = new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight * 5 / 2f - 2, 0f);


                UISprite uiSpritePrevMaid = goPrevMaid.GetComponent<UISprite>();
                uiSpritePrevMaid.SetDimensions((int)(conWidth * 0.25f) - 2, systemUnitHeight * 3);

                UILabel uiLabelPrevMaid = FindChild(goPrevMaid, "Name").GetComponent<UILabel>();

                uiLabelPrevMaid.width = uiSpritePrevMaid.width - 10;
                uiLabelPrevMaid.fontSize = 22;
                uiLabelPrevMaid.spacingX = 0;
                uiLabelPrevMaid.supportEncoding = true;
                uiLabelPrevMaid.text = "[111111]前のメイド";

                UIButton uiButtonPrevMaid = goPrevMaid.GetComponent<UIButton>();
                uiButtonPrevMaid.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonPrevMaid.onClick, new EventDelegate.Callback(this.OnClickPrevMaid));

                FindChild(goPrevMaid, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                FindChild(goPrevMaid, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goPrevMaid);
                goPrevMaid.SetActive(true);

                // メイド選択決定ボタン
                GameObject goCurrentMaid = SetCloneChild(goSystemUnit, goProfileTabCopy, "CurrentMaid");
                goCurrentMaid.transform.localPosition = new Vector3(-5f, baseTop - systemUnitHeight * 5 / 2f - 2f, 0f);


                UISprite uiSpriteCurrentMaid = goCurrentMaid.GetComponent<UISprite>();
                uiSpriteCurrentMaid.SetDimensions((int)(conWidth * 0.5f) - 6, systemUnitHeight * 3);
                uiSpriteCurrentMaid.depth = basedepth;


                Texture2D thumIcon;
                uiTextureCurrentMaid = NGUITools.AddWidget<UI2DSprite>(goCurrentMaid);
                uiTextureCurrentMaid.transform.localPosition = new Vector3(-systemUnitHeight * 2.5f, 0f, 0f);
                uiTextureCurrentMaid.depth = basedepth + 1;
                if (maid != null && maid.Visible == true)
                {
                    thumIcon = maid.GetThumIcon();

                    if (thumIcon != null)
                    {
                        Sprite sprite2D = Sprite.Create(thumIcon, new Rect(0f, 0f, (float)thumIcon.width, (float)thumIcon.height), default(Vector2));
                        uiTextureCurrentMaid.sprite2D = sprite2D;
                        uiTextureCurrentMaid.MakePixelPerfect();

                    }
                }
                uiLabelCurrentMaid = FindChild(goCurrentMaid, "Name").GetComponent<UILabel>();
                uiLabelCurrentMaid.width = uiSpriteCurrentMaid.width - 10;
                uiLabelCurrentMaid.fontSize = 18;
                uiLabelCurrentMaid.spacingX = 0;
                uiLabelCurrentMaid.supportEncoding = true;
                uiLabelCurrentMaid.maxLineCount = 0;
                uiLabelCurrentMaid.depth = basedepth + 2;
                if (maid != null && maid.Visible == true)
                {
                    uiLabelCurrentMaid.text = maid.Param.status.last_name + "\n" + maid.Param.status.first_name;
                }
                uiLabelCurrentMaid.height = 50;
                uiLabelCurrentMaid.alignment = NGUIText.Alignment.Left;
                uiLabelCurrentMaid.transform.localPosition = new Vector3(systemUnitHeight * 3, 0f, 0f);

                UIButton uiButtonCurrentMaid = goCurrentMaid.GetComponent<UIButton>();
                uiButtonCurrentMaid.defaultColor = new Color(1f, 1f, 1f, 1f);
                uiButtonCurrentMaid.enabled = false;
                FindChild(goCurrentMaid, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                FindChild(goCurrentMaid, "SelectCursor").SetActive(false);
                NGUITools.UpdateWidgetCollider(goPrevMaid);

                NGUITools.UpdateWidgetCollider(goCurrentMaid);
                goCurrentMaid.SetActive(true);

                Debuginfo.Log(LogLabel + " goCurrentMaid complete.");

                // 次メイド選択ボタン
                GameObject goNextMaid = SetCloneChild(goSystemUnit, goPrevMaid, "NextMaid");
                goNextMaid.transform.localPosition = new Vector3(conWidth * 0.375f - 4f, baseTop - systemUnitHeight * 5 / 2f - 2f, 0f);


                UILabel uiLabelNextMaid = FindChild(goNextMaid, "Name").GetComponent<UILabel>();
                uiLabelNextMaid.text = "[111111]次のメイド";

                UIButton uiButtonNextMaid = goNextMaid.GetComponent<UIButton>();
                uiButtonNextMaid.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonNextMaid.onClick, new EventDelegate.Callback(this.OnClickNextMaid));

                FindChild(goNextMaid, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                FindChild(goNextMaid, "SelectCursor").SetActive(false);
                NGUITools.UpdateWidgetCollider(goNextMaid);
                goNextMaid.SetActive(true);


                // Undoボタン
                GameObject goUndoAll = SetCloneChild(goSystemUnit, goProfileTabCopy, "UndoAll");
                goUndoAll.transform.localPosition = new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);

                UISprite uiSpriteUndoAll = goUndoAll.GetComponent<UISprite>();
                uiSpriteUndoAll.SetDimensions((int)(conWidth * 0.5f) - 2, systemUnitHeight);

                UILabel uiLabelUndoAll = FindChild(goUndoAll, "Name").GetComponent<UILabel>();
                uiLabelUndoAll.width = uiSpriteUndoAll.width - 10;
                uiLabelUndoAll.fontSize = 22;
                uiLabelUndoAll.spacingX = 0;
                uiLabelUndoAll.supportEncoding = true;
                uiLabelUndoAll.text = "[111111]*未使用*";

                UIButton uiButtonUndoAll = goUndoAll.GetComponent<UIButton>();
                uiButtonUndoAll.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonUndoAll.onClick, new EventDelegate.Callback(this.OnClickUndoAll));

                FindChild(goUndoAll, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                FindChild(goUndoAll, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goUndoAll);
                goUndoAll.SetActive(true);

                // Resetボタン
                GameObject goResetAll = SetCloneChild(goSystemUnit, goUndoAll, "ResetAll");
                goResetAll.transform.localPosition = new Vector3(conWidth * 0.25f - 4, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);

                UILabel uiLabelResetAll = FindChild(goResetAll, "Name").GetComponent<UILabel>();
                uiLabelResetAll.text = "[111111]ResetAll";

                UIButton uiButtonResetAll = goResetAll.GetComponent<UIButton>();
                uiButtonResetAll.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonResetAll.onClick, new EventDelegate.Callback(this.OnClickResetAll));

                NGUITools.UpdateWidgetCollider(goResetAll);
                goResetAll.SetActive(true);

                Debuginfo.Log(LogLabel + " goResetAll complete.");

                //ボーンカテゴリー切り替えボタン

                GameObject goBoneCategory0 = SetCloneChild(goSystemUnit, goLoadPose, "BoneCategory0");
                goBoneCategory0.transform.localPosition = new Vector3(-conWidth * 0.4167f - 7, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                UISprite uiSpriteBoneCategory0 = goBoneCategory0.GetComponent<UISprite>();
                uiSpriteBoneCategory0.SetDimensions((int)(conWidth * 0.1667f) - 5, systemUnitHeight);
                UILabel uiLabelBoneCategory0 = FindChild(goBoneCategory0, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory0.width = uiLabelBoneCategory0.width - 10;
                uiLabelBoneCategory0.fontSize = 20;
                uiLabelBoneCategory0.text = "[111111]その他";
                UIButton uiButtonBoneCategory0 = goBoneCategory0.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory0.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                FindChild(goBoneCategory0, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.1667f) - 10 - 4, systemUnitHeight - 4);
                NGUITools.UpdateWidgetCollider(goBoneCategory0);
                goBoneCategory0.SetActive(true);


                GameObject goBoneCategory1 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory1");
                goBoneCategory1.transform.localPosition = new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                UILabel uiLabelBoneCategory1 = FindChild(goBoneCategory1, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory1.text = "[111111]上半身";
                UIButton uiButtonBoneCategory1 = goBoneCategory1.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory1.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory1);
                goBoneCategory1.SetActive(true);

                GameObject goBoneCategory2 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory2");
                goBoneCategory2.transform.localPosition = new Vector3(-conWidth * 0.08333f - 5, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                UILabel uiLabelBoneCategory2 = FindChild(goBoneCategory2, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory2.text = "[111111]下半身";
                UIButton uiButtonBoneCategory2 = goBoneCategory2.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory2.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory2);
                goBoneCategory2.SetActive(true);

                GameObject goBoneCategory3 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory3");
                goBoneCategory3.transform.localPosition = new Vector3(conWidth * 0.08333f - 4, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                UILabel uiLabelBoneCategory3 = FindChild(goBoneCategory3, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory3.text = "[111111]足指";
                UIButton uiButtonBoneCategory3 = goBoneCategory1.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory3.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory3);
                goBoneCategory3.SetActive(true);

                GameObject goBoneCategory4 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory4");
                goBoneCategory4.transform.localPosition = new Vector3(conWidth * 0.25f - 3, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                UILabel uiLabelBoneCategory4 = FindChild(goBoneCategory4, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory4.text = "[111111]左手指";
                UIButton uiButtonBoneCategory4 = goBoneCategory4.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory4.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory4);
                goBoneCategory4.SetActive(true);

                GameObject goBoneCategory5 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory5");
                goBoneCategory5.transform.localPosition = new Vector3(conWidth * 0.4167f - 2, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                UILabel uiLabelBoneCategory5 = FindChild(goBoneCategory5, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory5.text = "[111111]右手指";
                UIButton uiButtonBoneCategory5 = goBoneCategory5.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory5.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory5);
                goBoneCategory5.SetActive(true);

                uiButtonBoneCategory0.defaultColor = new Color(uiButtonBoneCategory0.defaultColor.r, uiButtonBoneCategory0.defaultColor.g, uiButtonBoneCategory0.defaultColor.b, 1.0f);
                FindChild(uiButtonBoneCategory0.gameObject, "SelectCursor").SetActive(true);

                #endregion

                Debuginfo.Log(LogLabel + " goBoneCategory complete.");

                //メイドのボーン情報の取得
                if (maid != null && maid.Visible == true)
                {

                    getMaidBonetransform();
                    posHandle = new AngleHandle(settingIni.HandleLegacymode, maid);
                }
                else
                {
                    posHandle = new AngleHandle(settingIni.HandleLegacymode);
                }
                posHandle.Visible = false;

                Debuginfo.Log(LogLabel + " getMaidBonetransform complete.");

                #region addTableContents

                // BoneParamの設定に従ってボタン・スライダー追加
                for (int i = 0; i < mp.BoneCount; i++)
                {
                    string bone = mp.sBone[i];

                    if (!mp.bVisible[bone]) continue;

                    uiValueLable[bone] = new Dictionary<string, UILabel>();
                    string modeDesc = mp.sDescription[bone] + " (" + bone + ")";

                    // ModUnit：modタグ単位のまとめオブジェクト ScrollViewGridの子
                    GameObject goModUnit;
                    switch (mp.sCategory[bone])
                    {
                        case "Upper":
                            goModUnit = NGUITools.AddChild(goScrollViewTable[1]); break;
                        case "Lower":
                            goModUnit = NGUITools.AddChild(goScrollViewTable[2]); break;
                        case "Toe":
                            goModUnit = NGUITools.AddChild(goScrollViewTable[3]); break;
                        case "LeftFinger":
                            goModUnit = NGUITools.AddChild(goScrollViewTable[4]); break;
                        case "RightFinger":
                            goModUnit = NGUITools.AddChild(goScrollViewTable[5]); break;
                        default:
                            goModUnit = NGUITools.AddChild(goScrollViewTable[0]); break;
                    }

                    goModUnit.name = ("Unit:" + bone);
                    trBoneUnit[bone] = goModUnit.transform;

                    // プロフィールタブ複製・追加
                    GameObject goHeaderButton = SetCloneChild(goModUnit, goProfileTabCopy, "Header:" + bone);
                    goHeaderButton.SetActive(true);
                    goHeaderButton.AddComponent<UIDragScrollView>().scrollView = uiScrollView;
                    UIButton uiHeaderButton = goHeaderButton.GetComponent<UIButton>();
                    EventDelegate.Set(uiHeaderButton.onClick, new EventDelegate.Callback(this.OnClickHeaderButton));
                    setButtonColor(uiHeaderButton, mp.IsToggle(bone) ? mp.bEnabled[bone] : false);

                    NGUITools.UpdateWidgetCollider(goHeaderButton);

                    // 白地Sprite
                    UISprite uiSpriteHeaderButton = goHeaderButton.GetComponent<UISprite>();
                    uiSpriteHeaderButton.type = UIBasicSprite.Type.Sliced;
                    uiSpriteHeaderButton.SetDimensions(conWidth, 40);

                    UILabel uiLabelHeader = FindChild(goHeaderButton, "Name").GetComponent<UILabel>();
                    uiLabelHeader.width = uiSpriteHeaderButton.width - 20;
                    uiLabelHeader.height = 30;
                    uiLabelHeader.trueTypeFont = font;
                    uiLabelHeader.fontSize = 22;
                    uiLabelHeader.spacingX = 0;
                    uiLabelHeader.multiLine = false;
                    uiLabelHeader.overflowMethod = UILabel.Overflow.ClampContent;
                    uiLabelHeader.supportEncoding = true;
                    uiLabelHeader.text = "[000000]" + modeDesc + "[-]";
                    uiLabelHeader.gameObject.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                    // 金枠Sprite
                    UISprite uiSpriteHeaderCursor = FindChild(goHeaderButton, "SelectCursor").GetComponent<UISprite>();
                    uiSpriteHeaderCursor.gameObject.SetActive(mp.IsToggle(bone) ? mp.bEnabled[bone] : false);

                    NGUITools.UpdateWidgetCollider(goHeaderButton);

                    // スライダーならUndo/Resetボタンとスライダー追加
                    if (mp.IsSlider(bone))
                    {
                        uiSpriteHeaderButton.SetDimensions((int)(conWidth * 0.8f), 40);
                        uiLabelHeader.width = uiSpriteHeaderButton.width - 20;
                        uiHeaderButton.transform.localPosition = new Vector3(-conWidth * 0.1f, 0f, 0f);

                        NGUITools.UpdateWidgetCollider(goHeaderButton);

                        // Handleボタン
                        GameObject goHandle = SetCloneChild(goModUnit, goProfileTabCopy, "Handle:" + bone);
                        goHandle.transform.localPosition = new Vector3(conWidth * 0.4f + 2, 10.5f, 0f);
                        goHandle.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                        UISprite uiSpriteHandle = goHandle.GetComponent<UISprite>();
                        uiSpriteHandle.SetDimensions((int)(conWidth * 0.2f) - 2, 19);

                        UILabel uiLabelHandle = FindChild(goHandle, "Name").GetComponent<UILabel>();
                        uiLabelHandle.width = uiSpriteHandle.width - 10;
                        uiLabelHandle.fontSize = 14;
                        uiLabelHandle.spacingX = 0;
                        uiLabelHandle.supportEncoding = true;
                        uiLabelHandle.text = "[111111]Handle";

                        UIButton uiButtonHandle = goHandle.GetComponent<UIButton>();
                        uiButtonHandle.defaultColor = new Color(1f, 1f, 1f, 0.8f);

                        EventDelegate.Set(uiButtonHandle.onClick, new EventDelegate.Callback(this.OnClickHandleButton));
                        FindChild(goHandle, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.2f) - 12, 15);
                        FindChild(goHandle, "SelectCursor").SetActive(false);
                        FindChild(goHandle, "SelectCursor").name = "SelectCursorHandle";
                        NGUITools.UpdateWidgetCollider(goHandle);
                        goHandle.SetActive(true);

                        // Resetボタン
                        GameObject goReset = SetCloneChild(goModUnit, goProfileTabCopy, "Reset:" + bone);
                        goReset.AddComponent<UIDragScrollView>().scrollView = uiScrollView;
                        goReset.transform.localPosition = new Vector3(conWidth * 0.4f + 2, -10.5f, 0f);

                        UISprite uiSpriteReset = goReset.GetComponent<UISprite>();
                        uiSpriteReset.SetDimensions((int)(conWidth * 0.2f) - 2, 19);

                        UILabel uiLabelReset = FindChild(goReset, "Name").GetComponent<UILabel>();
                        uiLabelReset.width = uiSpriteReset.width - 10;
                        uiLabelReset.fontSize = 14;
                        uiLabelReset.spacingX = 0;
                        uiLabelReset.supportEncoding = true;
                        uiLabelReset.text = "[111111]Reset";

                        UIButton uiButtonReset = goReset.GetComponent<UIButton>();
                        uiButtonReset.defaultColor = new Color(1f, 1f, 1f, 0.8f);

                        EventDelegate.Set(uiButtonReset.onClick, new EventDelegate.Callback(this.OnClickResetButton));
                        FindChild(goReset, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                        FindChild(goReset, "SelectCursor").SetActive(false);
                        NGUITools.UpdateWidgetCollider(goReset);
                        goReset.SetActive(true);


                        for (int j = 0; j < mp.ValCount(bone); j++)
                        {
                            string prop = mp.sPropName[bone][j];

                            if (!mp.bVVisible[bone][prop]) continue;

                            float value = mp.fValue[bone][prop];
                            float vmin = mp.fVmin[bone][prop];
                            float vmax = mp.fVmax[bone][prop];
                            string label = mp.sLabel[bone][prop];
                            string vType = mp.sVType[bone][prop];

                            // スライダーをModUnitに追加
                            GameObject goSliderUnit = SetCloneChild(goModUnit, goTestSliderUnit, "SliderUnit");
                            goSliderUnit.transform.localPosition = new Vector3(0f, j * -70f - uiSpriteHeaderButton.height - 20f, 0f);
                            goSliderUnit.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                            // フレームサイズ
                            goSliderUnit.GetComponent<UISprite>().SetDimensions(conWidth, 50);

                            // スライダー設定
                            UISlider uiModSlider = FindChild(goSliderUnit, "Slider").GetComponent<UISlider>();
                            uiModSlider.name = "Slider:" + bone + ":" + prop;
                            uiModSlider.value = codecSliderValue(bone, prop);


                            if (vType == "int") uiModSlider.numberOfSteps = (int)(vmax - vmin + 1);
                            EventDelegate.Add(uiModSlider.onChange, new EventDelegate.Callback(this.OnChangeSlider));

                            // スライダーラベル設定
                            FindChild(goSliderUnit, "Label").GetComponent<UILabel>().text = label;
                            FindChild(goSliderUnit, "Label").AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                            // スライダー値ラベル参照取得
                            GameObject goValueLabel = FindChild(goSliderUnit, "Value");
                            goValueLabel.name = "Value:" + bone + ":" + prop;
                            uiValueLable[bone][prop] = goValueLabel.GetComponent<UILabel>();
                            uiValueLable[bone][prop].multiLine = false;
                            //goValueLabel.GetComponent<UIInput>().value = mp.fVdef[bone][prop].ToString("F4");
                            EventDelegate.Set(goValueLabel.GetComponent<UIInput>().onSubmit, this.OnSubmitSliderValueInput);

                            goSliderUnit.SetActive(false);
                        }
                    }

                }

                Debuginfo.Log(LogLabel + " goSliderUnit complete.");


                #endregion

                #region AddInputANMNamePanel

                ///////////////////////////////////////////////
                //ポーズ名入力用パネル作成
                uiPNamePanel = NGUITools.AddChild<UIPanel>(goSystemUnit);
                uiPNamePanel.name = "PoseSelectPanel";
                uiPNamePanel.transform.localPosition = new Vector3(0f, 250f, 0f);//-ScrollViewWidth / 2f - 140f / 2f - 10f, 0f, 0f); 


                uiPNamePanel.depth = basedepth + 2;
                goPNamePanel = uiPNamePanel.gameObject;


                UISprite uiPNameBGSprite = NGUITools.AddChild<UISprite>(goPNamePanel);
                uiPNameBGSprite.name = "PoseNameBG";
                uiPNameBGSprite.atlas = uiAtlasDialog;
                uiPNameBGSprite.spriteName = "cm3d2_dialog_frame";
                uiPNameBGSprite.type = UIBasicSprite.Type.Sliced;
                uiPNameBGSprite.SetDimensions(ScrollViewWidth, 300);
                uiPNameBGSprite.gameObject.AddComponent<BoxCollider>();
                NGUITools.UpdateWidgetCollider(uiPNameBGSprite.gameObject);


                // インプット作成
                GameObject goInputUnit = new GameObject("PoseNameInputUnit");
                SetChild(goPNamePanel, goInputUnit);

                UILabel uiPoseNameInput = NGUITools.AddChild<UILabel>(goPNamePanel);
                uiPoseNameInput.name = "PoseNameInputLabel";
                uiPoseNameInput.trueTypeFont = font;
                uiPoseNameInput.fontSize = 25;
                uiPoseNameInput.text = "ポーズ名";
                uiPoseNameInput.width = 110;
                uiPoseNameInput.color = Color.white;
                uiPoseNameInput.depth = basedepth + 2;
                uiPoseNameInput.overflowMethod = UILabel.Overflow.ShrinkContent;
                uiPoseNameInput.transform.localPosition = new Vector3(-150, 100f, 0f);

                UISprite uiPNameStringBase = NGUITools.AddChild<UISprite>(goInputUnit);
                uiPNameStringBase.name = "NameStringBase";
                uiPNameStringBase.atlas = uiAtlasSceneEdit;
                uiPNameStringBase.spriteName = "cm3d2_edit_slidernumberframe";
                uiPNameStringBase.type = UIBasicSprite.Type.Sliced;
                uiPNameStringBase.SetDimensions(ScrollViewWidth-100, 60);
                uiPNameStringBase.depth = basedepth + 2;
                uiPNameStringBase.transform.localPosition = new Vector3(0f, 50f, 0f);

                
                UILabel uiPNameStringLabel = NGUITools.AddChild<UILabel>(uiPNameStringBase.gameObject);
                uiPNameStringLabel.name = "NameStringValue";
                uiPNameStringLabel.depth = uiPNameStringBase.depth + 1;
                uiPNameStringLabel.width = uiPNameStringBase.width;
                uiPNameStringLabel.trueTypeFont = font;
                uiPNameStringLabel.fontSize = 35;
                uiPNameStringLabel.text = "";
                uiPNameStringLabel.color = Color.black;

                UIInput uiPNameStringInput =  uiPNameStringLabel.gameObject.AddComponent<UIInput>();
                uiPNameStringInput.label = uiPNameStringLabel;
                uiPNameStringInput.onReturnKey = UIInput.OnReturnKey.Submit;
                uiPNameStringInput.validation = UIInput.Validation.None;
                uiPNameStringInput.activeTextColor = Color.black;
                uiPNameStringInput.caretColor = new Color(0.1f, 0.1f, 0.3f, 1f);
                uiPNameStringInput.selectionColor = new Color(0.3f, 0.3f, 0.6f, 0.8f);
                EventDelegate.Set(uiPNameStringInput.onSubmit, this.OnSubmitPoseName);
                uiPNameStringInput.value = "";

                uiPNameStringInput.gameObject.AddComponent<BoxCollider>();
                NGUITools.UpdateWidgetCollider(uiPNameStringInput.gameObject);
                

                //出力OKボタン
                GameObject goOutputOkANM = SetCloneChild(goPNamePanel, goCancelCopy, "OutputOkANM");
                goOutputOkANM.transform.localPosition = new Vector3(-150f, -50f, 0f);

                goOutputOkANM.GetComponent<UISprite>().spriteName = "cm3d2_edit_okbutton";
                goOutputOkANM.GetComponent<UISprite>().MakePixelPerfect();
                goOutputOkANM.GetComponent<UISprite>().SetDimensions(100,100);
                Destroy(goOutputOkANM.GetComponent<UIPlayAnimation>());

                UIButton uiOutputOkANM = goOutputOkANM.GetComponent<UIButton>();
                uiOutputOkANM.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiOutputOkANM.onClick, new EventDelegate.Callback(this.OnClickOutputOkANM));

                NGUITools.UpdateWidgetCollider(goOutputOkANM);
                goOutputOkANM.SetActive(true);

                Debuginfo.Log(LogLabel + " goOutputOkANM complete.");

                //出力Cancelボタン
                GameObject goOutputCancelANM = SetCloneChild(goPNamePanel, goCancelCopy, "OutputCancelANM");
                goOutputCancelANM.transform.localPosition = new Vector3(150f, -50f, 0f);

                goOutputCancelANM.GetComponent<UISprite>().SetDimensions(100, 100);
                Destroy(goOutputCancelANM.GetComponent<UIPlayAnimation>());

                UIButton uiOutputCancelANM = goOutputCancelANM.GetComponent<UIButton>();
                uiOutputCancelANM.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiOutputCancelANM.onClick, new EventDelegate.Callback(this.OnClickOutputCancelANM));

                NGUITools.UpdateWidgetCollider(goOutputCancelANM);
                goOutputCancelANM.SetActive(true);

                Debuginfo.Log(LogLabel + " goOutputCancelANM complete.");

                goPNamePanel.SetActive(false);

                Debuginfo.Log(LogLabel + " goPNamePanel complete.");
                #endregion


                for (int i = 0; i < mp.BoneCount; i++)
                {
                    string bone = mp.sBone[i];

                    undoValue[bone] = new Dictionary<string, float>();

                    if (mp.IsSlider(bone))
                    {
                        for (int j = 0; j < mp.ValCount(bone); j++)
                        {
                            string prop = mp.sPropName[bone][j];
                            undoValue[bone][prop] = mp.fValue[bone][prop];
                        }
                    }
                }

                foreach (UITable ut in uiTable)
                {
                    ut.Reposition();
                }
                goAMSPanel.SetActive(false);



                Debuginfo.Log(LogLabel + " goAMSPanel complete.");


            }
            catch (Exception ex) { Debug.LogError(LogLabel + "initialize()" + ex); return false; }

            return true;
        }

        private void finalize()
        {


            Debuginfo.Log(LogLabel + "finalize...");


            IniFileHelper.Write<SettingIni>("setting", settingIni, iniFileName);

            posHandle.Destroy();

            bInitCompleted = false;
            visible = false;
            mp = null;

            maid = null;
            goAMSPanel = null;

            for (int i = 0; i < 6; ++i)
            {
                goScrollViewTable[i] = null;
            }

            goPSPanel = null;
            goPSScrollView = null;
            goPSScrollViewTable = null;


            uiValueLable.Clear();
        }

        //----

        public void toggleActiveOnWideSlider() { toggleActiveOnWideSlider(mp.bEnabled["WIDESLIDER"]); }
        public void toggleActiveOnWideSlider(bool b)
        {
            try
            {
                for (int i = 0; i < 6; ++i)
                {

                    foreach (Transform t in goScrollViewTable[i].transform)
                    {
                        string goType = getTag(t, 0);
                        string goBone = getTag(t, 1);

                        if (goType == "System") continue;

                        string s = (b ? "[000000]" : "[FF0000]WS必須 [-]") + mp.sDescription[goBone] + " (" + goBone + ")";
                        t.GetComponentsInChildren<UILabel>()[0].text = s;

                        UIButton uiButton = t.GetComponentsInChildren<UIButton>()[0];
                        uiButton.isEnabled = b;
                        if (!(b && mp.IsSlider(goBone))) setButtonColor(uiButton, b);

                        if (!b)
                        {
                            foreach (Transform tc in t)
                            {
                                string gocType = getTag(tc, 0);
                                if (gocType == "SliderUnit" || gocType == "Spacer") tc.gameObject.SetActive(b);
                            }
                        }
                    }
                    uiTable[i].repositionNow = true;
                }

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "toggleActiveOnWideSlider() " + ex); }
        }

        private bool visibleHandle(string bone)
        {
            try
            {
                GameObject goSelectCursor = FindChild(trBoneUnit[bone], "SelectCursorHandle").gameObject;
                bool b = goSelectCursor.activeSelf;
                goSelectCursor.SetActive(!b);
                return !b;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "visibleHandle() " + ex); return false; }
        }

        private void undoSliderValue(string bone)
        {
            /*
            try
            {
                bLocked = true;
                foreach (Transform tr in trModUnit[bone])
                {
                    if (tr.name == "SliderUnit")
                    {
                        UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                        string prop = getTag(slider, 2);

                        float preValue = mp.fValue[bone][prop];
                        mp.fValue[bone][prop] = undoValue[bone][prop];
                        slider.value = codecSliderValue(bone, prop);
                        uiValueLable[bone][prop].text = mp.fValue[bone][prop].ToString("F4");
                        uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop].ToString("F4");
                    }
                }

                rotateBone(bone);

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
                {
                    vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                }

                if (bone == "Bip01")
                {
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                }

                bLocked = false;

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "undoSliderValue() " + ex); bLocked = false; return; }
            */
        }

        private void resetSliderValue(string bone)
        {
            try
            {
                bLocked = true;

                foreach (Transform tr in trBoneUnit[bone])
                {
                    if (tr.name == "SliderUnit")
                    {
                        UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                        string prop = getTag(slider, 2);

                        float preValue = mp.fValue[bone][prop];
                        mp.fValue[bone][prop] = mp.fVdef[bone][prop];
                        slider.value = codecSliderValue(bone, prop);
                        uiValueLable[bone][prop].text = mp.fValue[bone][prop].ToString("F4");
                        uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop].ToString("F4");

                    }
                }

                rotateBone(bone);

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
                {
                    vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                }

                if (bone == "Bip01")
                {
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                }

                bLocked = false;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "resetSliderValue() " + ex); bLocked = false; return; }
        }


        private int sortGridByXMLOrder(Transform t1, Transform t2)
        {

            try
            {
                string type1 = t1.name.Split(':')[0];
                string type2 = t2.name.Split(':')[0];
                string bone1 = t1.name.Split(':')[1];
                string bone2 = t2.name.Split(':')[1];
                int n = mp.sBone.IndexOf(bone1);
                int m = mp.sBone.IndexOf(bone2);

                Dictionary<string, int> order = new Dictionary<string, int>()
                { {"System", -1}, {"Unit", 0}, {"Panel", 1}, {"Header", 2}, {"Slider", 3}, {"Spacer", 4} };

                if (n == m)
                {
                    if (type1 == "Slider" && type2 == "Slider")
                    {
                        int l = Array.IndexOf(mp.sPropName[bone1], t1.name.Split(':')[2]);
                        int k = Array.IndexOf(mp.sPropName[bone2], t2.name.Split(':')[2]);

                        return l - k;
                    }
                    else return order[type1] - order[type2];
                }
                else return n - m;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "sortGridByXMLOrder() " + ex); return 0; }

        }

        private void setSliderVisible(string bone, bool b)
        {
            foreach (Transform tc in trBoneUnit[bone])
            {
                string type = getTag(tc, 0);
                if (type == "SliderUnit" || type == "Spacer") tc.gameObject.SetActive(b);
            }

            foreach (UITable ut in uiTable)
            {
                ut.repositionNow = true;
            }
        }

        private void setButtonColor(string bone, bool b)
        {
            setButtonColor(FindChild(trBoneUnit[bone], "Header:" + bone).GetComponent<UIButton>(), b);
        }
        private void setButtonColor(UIButton button, bool b)
        {
            Color color = button.defaultColor;

            if (mp.IsToggle(getTag(button, 1)))
            {
                button.defaultColor = new Color(color.r, color.g, color.b, b ? 1f : 0.5f);
                FindChild(button.gameObject, "SelectCursor").SetActive(b);
            }
            else
            {
                button.defaultColor = new Color(color.r, color.g, color.b, b ? 1f : 0.75f);
            }
        }

        private void windowTweenFinished()
        {
            //Debuginfo.Log(LogLabel + "test");
            goScrollView.SetActive(true);
        }

        private string getTag(Component co, int n) { return getTag(co.gameObject, n); }
        private string getTag(GameObject go, int n)
        {
            return (go.name.Split(':') != null) ? go.name.Split(':')[n] : "";
        }

        private float codecSliderValue(string bone, string prop)
        {


            float value = mp.fValue[bone][prop];
            float vmin = mp.fVmin[bone][prop];
            float vmax = mp.fVmax[bone][prop];
            string vType = mp.sVType[bone][prop];

            if (value < vmin) value = vmin;
            if (value > vmax) value = vmax;


            return (value - vmin) / (vmax - vmin);
        }

        private float codecSliderValue(string bone, string prop, float value)
        {

            float vmin = mp.fVmin[bone][prop];
            float vmax = mp.fVmax[bone][prop];
            string vType = mp.sVType[bone][prop];


            if (value < vmin) value = vmin;
            if (value > vmax) value = vmax;


            return vmin + (vmax - vmin) * value;

        }


        //--------

        private int FindVisibleMaidStockNo(int startNo, int add)
        {


            if (sceneLevel == 27)
            {
                //公式撮影モード（メイド最大12人）
                if (add == 0)
                {
                    Maid maid = GameMain.Instance.CharacterMgr.GetMaid(startNo);
                    if (maid.Visible)
                        return startNo;
                    else
                        return -1;
                }

                add /= Math.Abs(add);


                int MaidCount = GameMain.Instance.CharacterMgr.GetMaidCount();
                int maidNo = startNo;
                for (int i = 0; i < MaidCount; i += Math.Abs(add))
                {
                    if (maidNo >= MaidCount)
                    {
                        maidNo = 0;
                    }
                    else if (maidNo < 0)
                    {
                        maidNo = MaidCount - 1;
                    }

                    Maid maid = GameMain.Instance.CharacterMgr.GetMaid(maidNo);
                    if (maid != null && maid.Visible)
                    {
                        return maidNo;
                    }

                    maidNo += add;
                }
                return -1;
            }
            else
            {
                //複数撮影モード(メイド最大13人以上対応)
                if (add == 0)
                {
                    Maid maid = GameMain.Instance.CharacterMgr.GetStockMaid(startNo);
                    if (maid.Visible)
                        return startNo;
                    else
                        return -1;
                }


                add /= Math.Abs(add);


                int MaidCount = GameMain.Instance.CharacterMgr.GetStockMaidCount();
                int maidNo = startNo;
                for (int i = 0; i < MaidCount; i += Math.Abs(add))
                {
                    if (maidNo >= MaidCount)
                    {
                        maidNo = 0;
                    }
                    else if (maidNo < 0)
                    {
                        maidNo = MaidCount - 1;
                    }

                    Maid maid = GameMain.Instance.CharacterMgr.GetStockMaid(maidNo);
                    if (maid != null && maid.Visible)
                    {
                        return maidNo;
                    }

                    maidNo += add;
                }
                return -1;
            }

        }


        private void currentMaidChange()
        {

            if (sceneLevel == 27)
            {
                //公式撮影モード
                maid = GameMain.Instance.CharacterMgr.GetMaid(currentMaidNo);
            }
            else
            {
                //複数撮影モード
                maid = GameMain.Instance.CharacterMgr.GetStockMaid(currentMaidNo);
            }



            if (maid != null && maid.Visible == true && maid.body0.m_Bones != null)
            {
                Texture2D thumIcon = maid.GetThumIcon();
                if (thumIcon != null)
                {

                    Sprite sprite2D = Sprite.Create(thumIcon, new Rect(0f, 0f, (float)thumIcon.width, (float)thumIcon.height), default(Vector2));

                    uiTextureCurrentMaid.sprite2D = sprite2D;

                    uiTextureCurrentMaid.MakePixelPerfect();
                }


                uiLabelCurrentMaid.text = maid.Param.status.last_name + "\n" + maid.Param.status.first_name;


                getMaidBonetransform();

                posHandle.SetMaid(maid);

                syncSlider(true);

                if (activeHandleName != "")
                {
                    visibleHandle(activeHandleName);
                    setButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);
                    activeHandleName = "";
                }

            }
            else
            {
                Debuginfo.Log(LogLabel + "currentMaidChange() exception");
            }

        }
        private void setParentAllOffset()
        {

            int stockNo = FindVisibleMaidStockNo(this.currentMaidNo + 1, 1);
            while(stockNo != this.currentMaidNo)
            { 
                Maid tempmaid = GameMain.Instance.CharacterMgr.GetStockMaid(stockNo);
                if (tempmaid.transform.parent != GameMain.Instance.CharacterMgr.GetMaid(0).transform.parent)
                {
                    Debuginfo.Log(LogLabel + "stockNo:" + stockNo + " parent change");
                    tempmaid.transform.parent = GameMain.Instance.CharacterMgr.GetMaid(0).transform.parent;
                }
                stockNo = FindVisibleMaidStockNo(stockNo + 1, 1);
            }
        }
       

        //メイドのボーン情報の取得
        private void getMaidBonetransform()
        {
            for (int i = 0; i < mp.BoneCount; i++)
            {
                string bone = mp.sBone[i];


                if (mp.IsSlider(bone))
                {
                    if (bone == "secret" || bone == "eye" || bone == "allpos" || bone == "offset" || bone == "camera") continue;


                    trBone[bone] = CMT.SearchObjName(maid.body0.m_Bones.transform, bone, true);

                    if (trBone[bone] == null) Debug.LogError(LogLabel + ":" + bone + "is null! ");
                    vPastBoneAngle[bone] = trBone[bone].localEulerAngles;

                }
            }
            vPastBoneTrans = trBone["Bip01"].localPosition;

            //Debuginfo.Log(LogLabel + ": Bip World to Local  :: \n" + trBone["Bip01"].worldToLocalMatrix);
            //Debuginfo.Log(LogLabel +": Bip Local to World  :: "+ trBone["Bip01"].localToWorldMatrix);
            //Debuginfo.Log(LogLabel +": parent World to Local  :: "+ trBone["Bip01"].parent.worldToLocalMatrix);
            //Debuginfo.Log(LogLabel + ": parent Local to World  :: \n" + trBone["Bip01"].parent.localToWorldMatrix);


            //BlenderのBoneScript作成の際、Bindposeの中身を見たときの記述
            //何かのために残しときます
            //Transform[] bones = new Transform[2];
            //Matrix4x4[] bindPoses = new Matrix4x4[2];
            //bones[0] = new GameObject("Lower").transform;
            //bones[1] = new GameObject("Upper").transform;
            //bones[0].parent = bones[1];
            //// Set the position relative to the parent
            //bones[0].localRotation = new Quaternion(0.5000003576278687f, -0.5000003576278687f, 0.49999964237213135f, 0.49999964237213135f);
            //bones[0].localPosition = new Vector3(-6.702811083947324e-10f, 0.8810071349143982f, -0.01933424361050129f);
            // The bind pose is bone's inverse transformation matrix
            // In this case the matrix we also make this matrix relative to the root
            // So that we can move the root game object around freely

            //Debuginfo.Log(LogLabel + ": BindPoses :: \n" + bones[0].worldToLocalMatrix /* bones[1].localToWorldMatrix */);
            //Debuginfo.Log(LogLabel + ": BindPoses :: \n" + bones[0].localToWorldMatrix /* bones[1].localToWorldMatrix */);
            //Debuginfo.Log(LogLabel + ": BindPoses :: \n" + bones[0].worldToLocalMatrix * trBone["Bip01"].parent.localToWorldMatrix);

        }

        //ボーン回転処理（プラグイン側の数値を本体側に反映）
        private void rotateBone(string boneID)
        {
            rotateBone(boneID, "null");
        }
        private void rotateBone(string bone, string prop)
        {

            if (bone == "Bip01")
            {
                trBone[bone].localPosition = new Vector3(0, 0, 0);
                trBone[bone].Translate(Quaternion.Inverse(trBone[bone].localRotation) * new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"],
                mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"], mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]), Space.Self);
            }

            if (bone == "allpos")
            {

                GameMain.Instance.CharacterMgr.SetCharaAllPos(new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"], mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"], mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]));
                GameMain.Instance.CharacterMgr.SetCharaAllRot(new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"], mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"], mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]));

             

            }
            else if (bone == "offset")
            {
                maid.SetPos(new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"], mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"], mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]));
                maid.SetRot(new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"], mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"], mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]));
            }
            else if (bone == "camera")
            {

                GameMain.Instance.MainCamera.SetTargetPos(new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"], mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"], mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]), false);

                GameMain.Instance.MainCamera.SetDistance(mp.fValue[bone][bone + ".d"] + mp.fVzero[bone][bone + ".d"], false);
                GameMain.Instance.MainCamera.camera.fieldOfView = mp.fValue[bone][bone + ".fov"] + mp.fVzero[bone][bone + ".fov"];

                GameMain.Instance.MainCamera.SetRotation(new Vector3(mp.fValue[bone][bone + ".rx"] + mp.fVzero[bone][bone + ".rx"], mp.fValue[bone][bone + ".ry"] + mp.fVzero[bone][bone + ".ry"], mp.fValue[bone][bone + ".rz"] + mp.fVzero[bone][bone + ".rz"]));


            }
            else if (bone == "eye")
            {

                maid.body0.quaDefEyeL.eulerAngles = new Vector3(
                maid.body0.quaDefEyeL.eulerAngles.x,
                mp.fValue["eye"]["eye.ly"] + mp.fVzero["eye"]["eye.ly"],
                mp.fValue["eye"]["eye.lz"] + mp.fVzero["eye"]["eye.lz"]);


                maid.body0.quaDefEyeR.eulerAngles = new Vector3(
                maid.body0.quaDefEyeR.eulerAngles.x,
                mp.fValue["eye"]["eye.ry"] + mp.fVzero["eye"]["eye.ry"],
                -mp.fValue["eye"]["eye.rz"] - mp.fVzero["eye"]["eye.rz"]);


            }
            else if (bone == "secret")
            {

                jiggleBone jbMuneL = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true).gameObject.GetComponent<jiggleBone>();
                jiggleBone jbMuneR = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true).gameObject.GetComponent<jiggleBone>();

                jbMuneL.MuneUpDown = mp.fValue["secret"]["secret.mlud"] + mp.fVzero["secret"]["secret.mlud"];
                jbMuneL.MuneYori = mp.fValue["secret"]["secret.mly"] + mp.fVzero["secret"]["secret.mly"];
                jbMuneR.MuneUpDown = mp.fValue["secret"]["secret.mrud"] + mp.fVzero["secret"]["secret.mrud"];
                jbMuneR.MuneYori = mp.fValue["secret"]["secret.mry"] + mp.fVzero["secret"]["secret.mry"];

            }

            else if (bone == "Bip01 L Hand" || bone == "Bip01 R Hand")
            {
                //苦肉の策
                trBone[bone].localRotation = Quaternion.identity;
                trBone[bone].localRotation *= Quaternion.AngleAxis((mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]), Vector3.forward);
                trBone[bone].localRotation *= Quaternion.AngleAxis((mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"]), Vector3.up);
                trBone[bone].localRotation *= Quaternion.AngleAxis((mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"]), Vector3.right);
            }
            else
            {
                trBone[bone].localRotation = Quaternion.identity;
                trBone[bone].localRotation *= Quaternion.AngleAxis((mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]), Vector3.forward);
                trBone[bone].localRotation *= Quaternion.AngleAxis((mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"]), Vector3.right);
                trBone[bone].localRotation *= Quaternion.AngleAxis((mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"]), Vector3.up);
            }
        }
        //ハンドルの値を本体とスライダーに反映
        private void syncFromHandle()
        {
            if (posHandle.controllDragged())
            {
                bLocked = true;

                maid.body0.m_Bones.animation.Stop();
                maid.body0.boHeadToCam = false;
                maid.body0.boEyeToCam = false;


                Transform parentBone = posHandle.GetParentBone();
                string bone = parentBone.name;


                float past_x = mp.fValue[bone][bone + ".x"];
                float past_y = mp.fValue[bone][bone + ".y"];
                float past_z = mp.fValue[bone][bone + ".z"];
                //先にボーンを回転させておく
                trBone[bone].rotation *= posHandle.DeltaQuaternion();
                //回転結果をプラグイン側の数値に反映
                calc_trBone2Param(bone);

                //スライダー限界値を超えてないかのチェック
                bool reRotate = false;

                if (mp.fValue[bone][bone + ".x"] > mp.fVmax[bone][bone + ".x"])
                {
                    if ((mp.fValue[bone][bone + ".x"] - 360f) <= mp.fVmax[bone][bone + ".x"] && (mp.fValue[bone][bone + ".x"] - 360f) >= mp.fVmin[bone][bone + ".x"])
                        mp.fValue[bone][bone + ".x"] -= 360f;
                    else
                    {
                        reRotate = true;
                    }
                }
                if (mp.fValue[bone][bone + ".y"] > mp.fVmax[bone][bone + ".y"])
                {
                    if ((mp.fValue[bone][bone + ".y"] - 360f) <= mp.fVmax[bone][bone + ".y"] && (mp.fValue[bone][bone + ".y"] - 360f) >= mp.fVmin[bone][bone + ".y"])
                        mp.fValue[bone][bone + ".y"] -= 360f;
                    else
                    {
                        reRotate = true;
                    }
                }

                if (mp.fValue[bone][bone + ".z"] > mp.fVmax[bone][bone + ".z"])
                {
                    if ((mp.fValue[bone][bone + ".z"] - 360f) <= mp.fVmax[bone][bone + ".z"] && (mp.fValue[bone][bone + ".z"] - 360f) >= mp.fVmin[bone][bone + ".z"])
                        mp.fValue[bone][bone + ".z"] -= 360f;
                    else
                    {
                        reRotate = true;
                    }
                }
                if (mp.fValue[bone][bone + ".x"] < mp.fVmin[bone][bone + ".x"])
                {
                    if ((mp.fValue[bone][bone + ".x"] + 360f) >= mp.fVmin[bone][bone + ".x"] && (mp.fValue[bone][bone + ".x"] + 360f) <= mp.fVmax[bone][bone + ".x"])
                        mp.fValue[bone][bone + ".x"] += 360f;
                    else
                    {
                        reRotate = true;
                    }
                }
                if (mp.fValue[bone][bone + ".y"] < mp.fVmin[bone][bone + ".y"])
                {
                    if ((mp.fValue[bone][bone + ".y"] + 360f) >= mp.fVmin[bone][bone + ".y"] && (mp.fValue[bone][bone + ".y"] + 360f) <= mp.fVmax[bone][bone + ".y"])
                        mp.fValue[bone][bone + ".y"] += 360f;
                    else
                    {
                        reRotate = true;
                    }
                }
                if (mp.fValue[bone][bone + ".z"] < mp.fVmin[bone][bone + ".z"])
                {
                    if ((mp.fValue[bone][bone + ".z"] + 360f) >= mp.fVmin[bone][bone + ".z"] && (mp.fValue[bone][bone + ".z"] + 360f) <= mp.fVmax[bone][bone + ".z"])
                        mp.fValue[bone][bone + ".z"] += 360f;
                    else
                    {
                        reRotate = true;
                    }
                }

                if (!reRotate)
                {
                    //限界値をこえてなかったらスライダーと入力ラベルに反映
                    foreach (Transform tr in trBoneUnit[bone])
                    {
                        if (tr.name == "SliderUnit")
                        {

                            UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                            string prop = getTag(slider, 2);





                            slider.value = codecSliderValue(bone, prop);
                            uiValueLable[bone][prop].text = mp.fValue[bone][prop].ToString("F4");
                            uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop].ToString("F4");


                        }
                    }
                    vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                }
                else
                {
                    //超えてたらボーン回転取り消し
                    trBone[bone].localEulerAngles = vPastBoneAngle[bone];

                    mp.fValue[bone][bone + ".x"] = past_x;
                    mp.fValue[bone][bone + ".y"] = past_y;
                    mp.fValue[bone][bone + ".z"] = past_z;



                }


                bLocked = false;
            }
        }
        //本体側の数値をスライダーとプラグイン側の数値に反映
        private void syncSlider(bool allSlider)
        {
            bLocked = true;
            foreach (string bone in mp.sBone)
            {
                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
                {
                    if (vPastBoneAngle[bone] != trBone[bone].localEulerAngles || vPastBoneTrans != trBone["Bip01"].localPosition || allSlider)
                    {
                        if (bone == "Bip01")
                        {

                            Vector3 tmpPosition = trBone[bone].localPosition;

                            mp.fValue[bone][bone + ".px"] = tmpPosition.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpPosition.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpPosition.z - mp.fVzero[bone][bone + ".pz"];
                        }

                        calc_trBone2Param(bone);
                        foreach (Transform tr in trBoneUnit[bone])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);

                                if ((mp.fValue[bone][prop]) > mp.fVmax[bone][prop] && (mp.fValue[bone][prop] - 360f) <= mp.fVmax[bone][prop] && (mp.fValue[bone][prop] - 360f) >= mp.fVmin[bone][prop])
                                {
                                    mp.fValue[bone][prop] -= 360f;
                                }
                                else if ((mp.fValue[bone][prop]) < mp.fVmin[bone][prop] && (mp.fValue[bone][prop] + 360f) >= mp.fVmin[bone][prop] && (mp.fValue[bone][prop] + 360f) <= mp.fVmax[bone][prop])
                                {
                                    mp.fValue[bone][prop] += 360f;
                                }

                                slider.value = codecSliderValue(bone, prop);
                                uiValueLable[bone][prop].text = mp.fValue[bone][prop].ToString("F4");
                                uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop].ToString("F4");


                            }
                        }
                        vPastBoneAngle[bone] = trBone[bone].localEulerAngles;

                    }
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                }
                else
                {
                    bool b_changed = false;

                    if (bone == "allpos")
                    {
                        Vector3 tmpAllPos;
                        Vector3 tmpAllRot;


                        tmpAllPos = GameMain.Instance.CharacterMgr.GetCharaAllPos();
                        tmpAllRot = GameMain.Instance.CharacterMgr.GetCharaAllRot();

                        if (
                            tmpAllPos.x != mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"] ||
                            tmpAllPos.y != mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"] ||
                            tmpAllPos.z != mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"] ||
                            tmpAllRot.x != mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"] ||
                            tmpAllRot.y != mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"] ||
                            tmpAllRot.z != mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]
                            )
                        {
                            mp.fValue[bone][bone + ".px"] = tmpAllPos.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpAllPos.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpAllPos.z - mp.fVzero[bone][bone + ".pz"];

                            mp.fValue[bone][bone + ".x"] = tmpAllRot.x - mp.fVzero[bone][bone + ".x"];
                            mp.fValue[bone][bone + ".y"] = tmpAllRot.y - mp.fVzero[bone][bone + ".y"];
                            mp.fValue[bone][bone + ".z"] = tmpAllRot.z - mp.fVzero[bone][bone + ".z"];

                            b_changed = true;
                        }


                    }
                    else if (bone == "offset")
                    {
                        Vector3 tmpMaidPos = maid.transform.localPosition;
                        Vector3 tmpMaidRot = maid.GetRot();

                        if (
                            tmpMaidPos.x != mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"] ||
                            tmpMaidPos.y != mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"] ||
                            tmpMaidPos.z != mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"] ||
                            tmpMaidRot.x != mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"] ||
                            tmpMaidRot.y != mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"] ||
                            tmpMaidRot.z != mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]
                            )
                        {

                            mp.fValue[bone][bone + ".px"] = tmpMaidPos.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpMaidPos.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpMaidPos.z - mp.fVzero[bone][bone + ".pz"];

                            mp.fValue[bone][bone + ".x"] = tmpMaidRot.x - mp.fVzero[bone][bone + ".x"];
                            mp.fValue[bone][bone + ".y"] = tmpMaidRot.y - mp.fVzero[bone][bone + ".y"];
                            mp.fValue[bone][bone + ".z"] = tmpMaidRot.z - mp.fVzero[bone][bone + ".z"];

                            b_changed = true;
                        }
                    }
                    else if (bone == "camera")
                    {


                        Vector3 CameraPos = GameMain.Instance.MainCamera.GetTargetPos();
                        Vector3 CameraRotation = GameMain.Instance.MainCamera.camera.transform.rotation.eulerAngles;

                        bool f1 = CameraPos.x != mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"];
                        bool f2 = CameraPos.y != mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"];
                        bool f3 = CameraPos.z != mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"];
                        bool f4 = GameMain.Instance.MainCamera.GetDistance() != mp.fValue[bone][bone + ".d"] + mp.fVzero[bone][bone + ".d"];
                        bool f5 = GameMain.Instance.MainCamera.camera.fieldOfView != mp.fValue[bone][bone + ".fov"] + mp.fVzero[bone][bone + ".fov"];



                        if (CameraRotation.x - mp.fVzero[bone][bone + ".rx"] > mp.fVmax[bone][bone + ".rx"])
                            CameraRotation.x -= 360;
                        else if (CameraRotation.x - mp.fVzero[bone][bone + ".rx"] < mp.fVmin[bone][bone + ".rx"])
                            CameraRotation.x += 360;

                        if (CameraRotation.y - mp.fVzero[bone][bone + ".ry"] > mp.fVmax[bone][bone + ".ry"])
                            CameraRotation.y -= 360;
                        else if (CameraRotation.y - mp.fVzero[bone][bone + ".ry"] < mp.fVmin[bone][bone + ".ry"])
                            CameraRotation.y += 360;

                        if (CameraRotation.z - mp.fVzero[bone][bone + ".rz"] > mp.fVmax[bone][bone + ".rz"])
                            CameraRotation.z -= 360;
                        else if (CameraRotation.z - mp.fVzero[bone][bone + ".rz"] < mp.fVmin[bone][bone + ".rz"])
                            CameraRotation.z += 360;




                        bool f8 = (System.Math.Floor(CameraRotation.x * 100) / 100) != (System.Math.Floor((mp.fValue[bone][bone + ".rx"] + mp.fVzero[bone][bone + ".rx"]) * 100) / 100);

                        bool fb = (System.Math.Floor(CameraRotation.y * 100) / 100) != (System.Math.Floor((mp.fValue[bone][bone + ".ry"] + mp.fVzero[bone][bone + ".ry"]) * 100) / 100);

                        bool fe = (System.Math.Floor(CameraRotation.z * 100) / 100) != (System.Math.Floor((mp.fValue[bone][bone + ".rz"] + mp.fVzero[bone][bone + ".rz"]) * 100) / 100);

                        if (f1 || f2 || f3 || f4 || f5 || f8 || fb || fe)
                        {
                            mp.fValue[bone][bone + ".px"] = CameraPos.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = CameraPos.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = CameraPos.z - mp.fVzero[bone][bone + ".pz"];

                            mp.fValue[bone][bone + ".d"] = GameMain.Instance.MainCamera.GetDistance() - mp.fVzero[bone][bone + ".d"];
                            mp.fValue[bone][bone + ".fov"] = GameMain.Instance.MainCamera.camera.fieldOfView - mp.fVzero[bone][bone + ".fov"];


                            mp.fValue[bone][bone + ".rx"] = CameraRotation.x - mp.fVzero[bone][bone + ".rx"];
                            mp.fValue[bone][bone + ".ry"] = CameraRotation.y - mp.fVzero[bone][bone + ".ry"];
                            mp.fValue[bone][bone + ".rz"] = CameraRotation.z - mp.fVzero[bone][bone + ".rz"];

                            b_changed = true;
                        }

                    }

                    else if (bone == "eye")
                    {


                        bool f1 = maid.body0.quaDefEyeL.eulerAngles.y == mp.fValue["eye"]["eye.ly"] + mp.fVzero["eye"]["eye.ly"];
                        bool f2 = maid.body0.quaDefEyeL.eulerAngles.y == mp.fValue["eye"]["eye.ly"] + mp.fVzero["eye"]["eye.ly"] - 360;

                        bool f3 = maid.body0.quaDefEyeL.eulerAngles.z == mp.fValue["eye"]["eye.lz"] + mp.fVzero["eye"]["eye.lz"];
                        bool f4 = maid.body0.quaDefEyeL.eulerAngles.z == mp.fValue["eye"]["eye.lz"] + mp.fVzero["eye"]["eye.lz"] + 360;

                        bool f5 = maid.body0.quaDefEyeR.eulerAngles.y == mp.fValue["eye"]["eye.ry"] + mp.fVzero["eye"]["eye.ry"];
                        bool f6 = maid.body0.quaDefEyeR.eulerAngles.y == mp.fValue["eye"]["eye.ry"] + mp.fVzero["eye"]["eye.ry"] - 360;

                        bool f7 = maid.body0.quaDefEyeR.eulerAngles.z == -mp.fValue["eye"]["eye.rz"] - mp.fVzero["eye"]["eye.rz"];
                        bool f8 = maid.body0.quaDefEyeR.eulerAngles.z == -mp.fValue["eye"]["eye.rz"] - mp.fVzero["eye"]["eye.rz"] - 360;


                        if (!(f1 || f2) || !(f3 || f4) || !(f5 || f6) || !(f7 || f8))
                        {

                            mp.fValue["eye"]["eye.ly"] = maid.body0.quaDefEyeL.eulerAngles.y - mp.fVzero["eye"]["eye.ly"];
                            mp.fValue["eye"]["eye.lz"] = maid.body0.quaDefEyeL.eulerAngles.z - mp.fVzero["eye"]["eye.lz"];
                            mp.fValue["eye"]["eye.ry"] = maid.body0.quaDefEyeR.eulerAngles.y - mp.fVzero["eye"]["eye.ry"];
                            mp.fValue["eye"]["eye.rz"] = -maid.body0.quaDefEyeR.eulerAngles.z - mp.fVzero["eye"]["eye.rz"];

                            if (mp.fValue["eye"]["eye.ly"] < -180)
                                mp.fValue["eye"]["eye.ly"] += 360;

                            if (mp.fValue["eye"]["eye.lz"] > 180)
                                mp.fValue["eye"]["eye.lz"] -= 360;


                            if (mp.fValue["eye"]["eye.ry"] < -180)
                                mp.fValue["eye"]["eye.ry"] += 360;

                            if (mp.fValue["eye"]["eye.rz"] > 180)
                                mp.fValue["eye"]["eye.rz"] -= 360;

                            b_changed = true;
                        }


                    }

                    else if (bone == "secret")
                    {
                        jiggleBone jbMuneL = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true).gameObject.GetComponent<jiggleBone>();
                        jiggleBone jbMuneR = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true).gameObject.GetComponent<jiggleBone>();

                        if (
                            jbMuneL.MuneUpDown != mp.fValue["secret"]["secret.mlud"] + mp.fVzero["secret"]["secret.mlud"] ||
                            jbMuneL.MuneYori != mp.fValue["secret"]["secret.mly"] + mp.fVzero["secret"]["secret.mly"] ||
                            jbMuneR.MuneUpDown != mp.fValue["secret"]["secret.mrud"] + mp.fVzero["secret"]["secret.mrud"] ||
                            jbMuneR.MuneYori != mp.fValue["secret"]["secret.mry"] + mp.fVzero["secret"]["secret.mry"]
                            )
                        {
                            mp.fValue["secret"]["secret.mlud"] = jbMuneL.MuneUpDown - mp.fVzero["secret"]["secret.mlud"];
                            mp.fValue["secret"]["secret.mly"] = jbMuneL.MuneYori - mp.fVzero["secret"]["secret.mly"];
                            mp.fValue["secret"]["secret.mrud"] = jbMuneR.MuneUpDown - mp.fVzero["secret"]["secret.mrud"];
                            mp.fValue["secret"]["secret.mry"] = jbMuneR.MuneYori - mp.fVzero["secret"]["secret.mry"];

                            b_changed = true;
                        }
                    }

                    if (b_changed)
                    {
                        foreach (Transform tr in trBoneUnit[bone])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);
                                slider.value = codecSliderValue(bone, prop);
                                uiValueLable[bone][prop].text = mp.fValue[bone][prop].ToString("F4");
                                uiValueLable[bone][prop].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop].ToString("F4");

                            }
                        }
                    }
                }

            }


            bLocked = false;

        }

        //ボーンの本体側の数値(クォータニオン)をプラグイン側の数値（オイラー角）に反映させる関数
        //回転順序を変えてるのでunity標準関数が使えなくて自力で変換させた結果がこれだよ！
        void calc_trBone2Param(string bone)
        {
            if (bone == "Bip01 L Hand" || bone == "Bip01 R Hand")
            {
                // m00:1-2y^2-2z^2 m01:2xy+2wz     m02:2xz-2wy
                // m10:2xy-2wz     m11:1-2x^2-2z^2 m12:2yz+2wx
                // m20:2xz+2wy     m21:2yz-2wx     m22:1-2x^2-2y^2
                // X->Y->Z
                // YZX
                float qx = trBone[bone].localRotation.x;
                float qy = trBone[bone].localRotation.y;
                float qz = trBone[bone].localRotation.z;
                float qw = trBone[bone].localRotation.w;

                float m10 = 2 * (qx * qy - qw * qz);
                if (m10 > 1.0f) m10 = 1.0f;
                if (m10 < -1.0f) m10 = -1.0f;

                float pastX = mp.fValue[bone][bone + ".x"];
                float pastY = mp.fValue[bone][bone + ".y"];
                float pastZ = mp.fValue[bone][bone + ".z"];
                mp.fValue[bone][bone + ".z"] = Mathf.Asin(-m10) * Mathf.Rad2Deg;
                if (System.Math.Floor(Mathf.Cos(mp.fValue[bone][bone + ".z"] * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
                {
                    float m00 = 1 - 2 * (qy * qy + qz * qz);
                    float m11 = 1 - 2 * (qx * qx + qz * qz);
                    float m12 = 2 * (qy * qz + qw * qx);
                    float m20 = 2 * (qx * qz + qw * qy);

                    if (m00 > 1.0f) m00 = 1.0f;
                    if (m00 < -1.0f) m00 = -1.0f;
                    if (m20 > 1.0f) m20 = 1.0f;
                    if (m20 < -1.0f) m20 = -1.0f;
                    if (m11 > 1.0f) m11 = 1.0f;
                    if (m11 < -1.0f) m11 = -1.0f;
                    if (m12 > 1.0f) m12 = 1.0f;
                    if (m12 < -1.0f) m12 = -1.0f;

                    mp.fValue[bone][bone + ".x"] = Mathf.Atan2(m12, m11) * Mathf.Rad2Deg;
                    float before = m20 / Mathf.Cos(mp.fValue[bone][bone + ".z"] * Mathf.Deg2Rad);
                    if (before > 1.0f) before = 1.0f;
                    if (before < -1.0f) before = -1.0f;
                    mp.fValue[bone][bone + ".y"] = Mathf.Asin(before) * Mathf.Rad2Deg;
                    if (m00 < 0)
                    {
                        mp.fValue[bone][bone + ".y"] = 180 - mp.fValue[bone][bone + ".y"];
                    }
                }
                if (System.Math.Floor(Mathf.Cos(mp.fValue[bone][bone + ".z"] * Mathf.Deg2Rad) * 10000) / 10000 == 0f || Double.IsNaN(mp.fValue[bone][bone + ".y"]))
                {
                    float m21 = 2 * (qy * qz - qw * qx);
                    float m22 = 1 - 2 * (qx * qx + qy * qy);

                    if (m21 > 1.0f) m21 = 1.0f;
                    if (m21 < -1.0f) m21 = -1.0f;
                    if (m22 > 1.0f) m22 = 1.0f;
                    if (m22 < -1.0f) m22 = -1.0f;

                    mp.fValue[bone][bone + ".y"] = 0f;
                    mp.fValue[bone][bone + ".z"] = (pastZ + mp.fVzero[bone][bone + ".z"] > 0) ? 90f : -90f;
                    mp.fValue[bone][bone + ".x"] = Mathf.Atan2(-m21, m22) * Mathf.Rad2Deg + mp.fValue[bone][bone + ".y"];
                }


                mp.fValue[bone][bone + ".z"] -= mp.fVzero[bone][bone + ".z"];
                mp.fValue[bone][bone + ".x"] -= mp.fVzero[bone][bone + ".x"];
                mp.fValue[bone][bone + ".y"] -= mp.fVzero[bone][bone + ".y"];


                if ((mp.fValue[bone][bone + ".x"] - pastX) > (mp.fValue[bone][bone + ".x"] - 180f - pastX) && (mp.fValue[bone][bone + ".y"] - pastY) > (mp.fValue[bone][bone + ".y"] - 180f - pastY) && (mp.fValue[bone][bone + ".z"] - pastZ) > (180f - mp.fValue[bone][bone + ".z"] - 2 * mp.fVzero[bone][bone + ".z"] - pastZ))
                {
                    mp.fValue[bone][bone + ".x"] -= 180f;
                    mp.fValue[bone][bone + ".y"] -= 180f;
                    mp.fValue[bone][bone + ".z"] = 180f - mp.fValue[bone][bone + ".z"] - 2 * mp.fVzero[bone][bone + ".z"];
                }


                //Debuginfo.Log(LogLabel +":trBone[bone].localEulerAngles:" + trBone[bone].localEulerAngles.ToString("F4"));
            }
            else
            {
                // m00:1-2y^2-2z^2 m01:2xy+2wz     m02:2xz-2wy
                // m10:2xy-2wz     m11:1-2x^2-2z^2 m12:2yz+2wx
                // m20:2xz+2wy     m21:2yz-2wx     m22:1-2x^2-2y^2
                // Y->X->Z
                float qx = trBone[bone].localRotation.x;
                float qy = trBone[bone].localRotation.y;
                float qz = trBone[bone].localRotation.z;
                float qw = trBone[bone].localRotation.w;

                float m02 = 2 * (qx * qz - qw * qy);
                float m10 = 2 * (qx * qy - qw * qz);
                float m11 = 1 - 2 * (qx * qx + qz * qz);
                float m12 = 2 * (qy * qz + qw * qx);
                float m20 = 2 * (qx * qz + qw * qy);
                float m21 = 2 * (qy * qz - qw * qx);
                float m22 = 1 - 2 * (qx * qx + qy * qy);

                float pastX = mp.fValue[bone][bone + ".x"];
                float pastY = mp.fValue[bone][bone + ".y"];
                float pastZ = mp.fValue[bone][bone + ".z"];

                if (m12 > 1.0f) m12 = 1.0f;
                if (m12 < -1.0f) m12 = -1.0f;
                if (m11 > 1.0f) m11 = 1.0f;
                if (m11 < -1.0f) m11 = -1.0f;
                if (m10 > 1.0f) m10 = 1.0f;
                if (m10 < -1.0f) m10 = -1.0f;
                if (m22 > 1.0f) m22 = 1.0f;
                if (m22 < -1.0f) m22 = -1.0f;

                mp.fValue[bone][bone + ".x"] = Mathf.Asin(m12) * Mathf.Rad2Deg;

                if (System.Math.Floor(Mathf.Cos(mp.fValue[bone][bone + ".x"] * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
                {

                    mp.fValue[bone][bone + ".z"] = Mathf.Atan2(-m10, m11) * Mathf.Rad2Deg;
                    float before = -m02 / Mathf.Cos(mp.fValue[bone][bone + ".x"] * Mathf.Deg2Rad);
                    if (before > 1.0f) before = 1.0f;
                    if (before < -1.0f) before = -1.0f;

                    mp.fValue[bone][bone + ".y"] = Mathf.Asin(before) * Mathf.Rad2Deg;
                    if (m22 < 0)
                    {
                        mp.fValue[bone][bone + ".y"] = 180 - mp.fValue[bone][bone + ".y"];
                    }

                }
                if (System.Math.Floor(Mathf.Cos(mp.fValue[bone][bone + ".x"] * Mathf.Deg2Rad) * 10000) / 10000 == 0f || Double.IsNaN(mp.fValue[bone][bone + ".y"]))
                {
                    float m00 = 1 - 2 * (qy * qy + qz * qz);
                    float m01 = 2 * (qx * qy + qw * qz);

                    if (m01 > 1.0f) m01 = 1.0f;
                    if (m01 < -1.0f) m01 = -1.0f;
                    if (m00 > 1.0f) m00 = 1.0f;
                    if (m00 < -1.0f) m00 = -1.0f;

                    mp.fValue[bone][bone + ".y"] = 0f;
                    mp.fValue[bone][bone + ".x"] = (pastX + mp.fVzero[bone][bone + ".x"] > 0) ? 90f : -90f;
                    mp.fValue[bone][bone + ".z"] = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg + mp.fValue[bone][bone + ".y"];

                }
                mp.fValue[bone][bone + ".z"] -= mp.fVzero[bone][bone + ".z"];
                mp.fValue[bone][bone + ".x"] -= mp.fVzero[bone][bone + ".x"];
                mp.fValue[bone][bone + ".y"] -= mp.fVzero[bone][bone + ".y"];

                if ((mp.fValue[bone][bone + ".z"] - pastZ) > (mp.fValue[bone][bone + ".z"] - 180f - pastZ) && (mp.fValue[bone][bone + ".y"] - pastY) > (mp.fValue[bone][bone + ".y"] - 180f - pastY) && (mp.fValue[bone][bone + ".x"] - pastX) > (180f - mp.fValue[bone][bone + ".x"] - 2 * mp.fVzero[bone][bone + ".x"] - pastX))
                {
                    mp.fValue[bone][bone + ".z"] -= 180f;
                    mp.fValue[bone][bone + ".y"] -= 180f;
                    mp.fValue[bone][bone + ".x"] = 180f - mp.fValue[bone][bone + ".x"] - 2 * mp.fVzero[bone][bone + ".x"];
                }
            }

        }
        private void outputANMPose(string poseName)
        {
            if(poseName == "")
            {
                poseName = "ポーズ"; 
            }
            //しばりすさんは半角英数字以外のファイル名も読み込んでくれるため
            //わかりやすくするためにファイル名とポーズ名を統一しようと思う
            //string dateName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            string masterName = poseName;// dateName;
            
            string posetextname = masterName;//"この項目は人間用なので適当に分かりやすい名前を書いて下さい(処理上は不要)";

            //公式撮影モード用フォルダに出力
            if (settingIni.AnmOutputmode == "both" || settingIni.AnmOutputmode == "photomode")
            {
                Debuginfo.Log(LogLabel + "outputANM for photomode");
                string anmFilePath = settingIni.OutputAnmDirectory + @"\" + masterName + ".anm";
                //同名のファイルがある場合は最後に(連番)をつける
                int fileno = 1;
                while( File.Exists(anmFilePath) )
                {
                    anmFilePath = settingIni.OutputAnmDirectory + @"\" + masterName + "(" + fileno + ").anm";
                    ++fileno;
                }

                // バイナリ形式でファイルに書き出し。
                outputANMFile(anmFilePath);
            }

            //しばりす用フォルダに出力
            if (settingIni.AnmOutputmode == "both" || settingIni.AnmOutputmode == "sybaris")
            {
                Debuginfo.Log(LogLabel + "outputANM for sybaris");
                string anmSybarisFilePath = settingIni.OutputAnmSybarisDirectory + @"\" + masterName + ".anm";
                string pngfilename = masterName + ".png";
                string texfilename = masterName + "_icon.tex";
                string text = settingIni.OutputAnmSybarisDirectory + @"\" + texfilename;

                string jsontext = settingIni.OutputJsonDirectory + @"\" + masterName + ".json";

                //同名のファイルがある場合は最後に(連番)をつける
                int fileno = 1;
                string mastercopy = masterName;
                while (File.Exists(anmSybarisFilePath)|| File.Exists(text) || File.Exists(jsontext))
                {

                    masterName = mastercopy + "("+ fileno +")";
                    posetextname = masterName;
                    pngfilename = masterName + ".png";
                    texfilename = masterName + "_icon.tex";
                    anmSybarisFilePath = settingIni.OutputAnmSybarisDirectory + @"\" + masterName + ".anm";
                    text = settingIni.OutputAnmSybarisDirectory + @"\" + texfilename;
                    jsontext = settingIni.OutputJsonDirectory + @"\" + masterName + ".json";
                    ++fileno;
                }


                // バイナリ形式でファイルに書き出し。
                outputANMFile(anmSybarisFilePath);


                //しばりす用アイコンtex作成処理
                if (!Directory.Exists(settingIni.OutputAnmSybarisDirectory))
                {
                    Directory.CreateDirectory(settingIni.OutputAnmSybarisDirectory);

                }
                if (!Directory.Exists(settingIni.OutputJsonDirectory))
                {
                    Directory.CreateDirectory(settingIni.OutputJsonDirectory);

                }

                setParentAllOffset();

                ThumShot posethumshot = GameMain.Instance.ThumCamera.GetComponent<ThumShot>();

                //posethumshot.MoveTargetCard(maid);
                Transform transform = CMT.SearchObjName(maid.transform, "Bip01", true);
                if (transform != null)
                {
                    posethumshot.transform.position = transform.position + 3.2f*transform.parent.forward;//transform.TransformPoint(transform.up * 3.5f); // + new Vector3(0.84f, 2.25f, 0f));
                    posethumshot.transform.rotation = transform.parent.rotation * Quaternion.Euler(0f, 180f, 0f);
                    Debuginfo.Log(LogLabel + "posethumshot.transform.position" + posethumshot.transform.position);
                    Debuginfo.Log(LogLabel + "transform.position" + transform.position);

                    Debuginfo.Log(LogLabel + "transform.parent.up" + transform.parent.up);
                    Debuginfo.Log(LogLabel + "transform.parent.forward" + transform.parent.forward);
                    Debuginfo.Log(LogLabel + "transform.parent.right" + transform.parent.right);

                    Debuginfo.Log(LogLabel + "posethumshot.transform.rotation" + posethumshot.transform.rotation.eulerAngles);
                    Debuginfo.Log(LogLabel + "transform.parent.rotation" + transform.parent.rotation.eulerAngles);
                }
                else
                {
                    Debug.LogError(LogLabel + "：サムネイルを取ろうとしましたがメイドが居ません。");
                    return;
                }

                Camera poseshotthumCamera = posethumshot.gameObject.GetComponent<Camera>();
                poseshotthumCamera.fieldOfView = 35f;
                //ここの間で他のメイドがいたら消す処理を加える

                //メイドの表示状態を記録しておく

                //ここで消す
                bool otherMaid = true;
                int tempCurrentNo = this.currentMaidNo;
                List<int> visMaidNo = new List<int>();
                int stockNo = FindVisibleMaidStockNo(this.currentMaidNo + 1, 1);
                while (otherMaid)
                {

                    if (stockNo != -1)
                    {

                        if (this.currentMaidNo == stockNo)
                        {
                            otherMaid = false;
                            break;
                        }
                        else
                        {
                            Maid tempMaid;
                            if (sceneLevel == 27)
                            {
                                //公式撮影モード
                                tempMaid = GameMain.Instance.CharacterMgr.GetMaid(stockNo);
                            }
                            else
                            {
                                //複数撮影モード
                                tempMaid = GameMain.Instance.CharacterMgr.GetStockMaid(stockNo);
                            }

                            visMaidNo.Add(stockNo);
                            tempMaid.Visible = !tempMaid.Visible;
                            //GameMain.Instance.CharacterMgr.BanishmentMaid(maid);

                            stockNo = FindVisibleMaidStockNo(stockNo + 1, 1);
                        }
                    }
                    else
                    {
                        Debug.LogError(LogLabel + ":maid is Lost!");
                        break;
                    }
                }

                //ここでメイドさんを裸＆丸坊主にさせる
                //加えてbodyのマスク状況も全て表示させる
                //その際、SAN値保護のため暗転処理を加えるかもしれない
                //※追記 一瞬だったので暗転いらんかった

                List<bool> visibleList = new List<bool>();

                List<string> slotList = new List<string>();
                slotList.Add("hairF");
                slotList.Add("hairR");
                slotList.Add("hairS");
                slotList.Add("hairT");
                slotList.Add("wear");
                slotList.Add("skirt");
                slotList.Add("onepiece");
                slotList.Add("mizugi");
                slotList.Add("panz");
                slotList.Add("bra");
                slotList.Add("stkg");
                slotList.Add("shoes");
                slotList.Add("headset");
                slotList.Add("glove");
                slotList.Add("accHead");
                slotList.Add("hairAho");
                //slotList.Add("accHana");
                //slotList.Add("accHa");
                slotList.Add("accKami_1_");
                slotList.Add("accMiMiR");
                slotList.Add("accKamiSubR");
                slotList.Add("accNipR");
                slotList.Add("HandItemR");
                slotList.Add("accKubi");
                slotList.Add("accKubiwa");
                //slotList.Add("accHeso");
                slotList.Add("accUde");
                slotList.Add("accAshi");
                slotList.Add("accSenaka");
                slotList.Add("accShippo");
                //slotList.Add("accAnl");
                //slotList.Add("accVag");
                slotList.Add("kubiwa");
                slotList.Add("megane");
                //slotList.Add("accXXX");
                slotList.Add("chikubi");
                slotList.Add("accHat");
                slotList.Add("kousoku_upper");
                slotList.Add("kousoku_lower");
                slotList.Add("accNipL");
                slotList.Add("accMiMiL");
                slotList.Add("accKamiSubL");
                slotList.Add("accKami_2_");
                slotList.Add("accKami_3_");
                slotList.Add("HandItemL");

                //↓の時用
                List<TBodySkin> existGoSlot = new List<TBodySkin>();

                //エディットモードかつメイドさんが2人以上いて対象が1人目じゃない場合
                //bool bVisBra = maid.body0.goSlot[i].AttachName
                if (sceneLevel == 5 )
                {

                    foreach (TBodySkin goSlotID in maid.body0.goSlot)
                    {
                        //複数メイドの2人目はアイテムスロットIDが一致しないらしいので
                        //中のカテゴリ名を調べて消すかどうか判断する
                        if (slotList.Contains(goSlotID.Category))
                        {
                            existGoSlot.Add(goSlotID);
                            visibleList.Add(goSlotID.boVisible);
                            goSlotID.boVisible = false;
                            goSlotID.Update();
                        }
                    }
                }
                else
                {
                    //それ以外の場合

                    foreach (string slotID in slotList)
                    {
                        visibleList.Add(maid.body0.goSlot[(int)TBody.hashSlotName[slotID]].boVisible);
                        maid.body0.goSlot[(int)TBody.hashSlotName[slotID]].boVisible = false;
                        maid.body0.goSlot[(int)TBody.hashSlotName[slotID]].Update();
                    }
                }
                //maid.body0.FixMaskFlag();
                maid.body0.FixVisibleFlag(false);

                //メイドさんの体型を撮影用に合わせる

                int tHeadX = maid.GetProp(MPN.HeadX).value;
                int tHeadY = maid.GetProp(MPN.HeadY).value;

                int tDouPer = maid.GetProp(MPN.DouPer).value;
                int tsintyou = maid.GetProp(MPN.sintyou).value;

                int tMuneL = maid.GetProp(MPN.MuneL).value;
                int tMuneTare = maid.GetProp(MPN.MuneTare).value;
                int tMuneUpDown = maid.GetProp(MPN.MuneUpDown).value;
                int tMuneYori = maid.GetProp(MPN.MuneYori).value;

                int twest = maid.GetProp(MPN.west).value;
                int tHara = maid.GetProp(MPN.Hara).value;
                int tkata = maid.GetProp(MPN.kata).value;
                int tUdeScl = maid.GetProp(MPN.UdeScl).value;
                int tArmL = maid.GetProp(MPN.ArmL).value;
                int tKubiScl = maid.GetProp(MPN.KubiScl).value;

                int tkoshi = maid.GetProp(MPN.koshi).value;
                int tRegMeet = maid.GetProp(MPN.RegMeet).value;
                int tRegFat = maid.GetProp(MPN.RegFat).value;

                maid.SetProp(MPN.HeadX,100);
                maid.SetProp(MPN.HeadY,100);

                maid.SetProp(MPN.DouPer,20);
                maid.SetProp(MPN.sintyou,20);

                maid.SetProp(MPN.MuneL,50);
                maid.SetProp(MPN.MuneTare,0);
                maid.SetProp(MPN.MuneUpDown,0);
                maid.SetProp(MPN.MuneYori,0);

                maid.SetProp(MPN.west,50);
                maid.SetProp(MPN.Hara,20);
                maid.SetProp(MPN.kata,0);
                maid.SetProp(MPN.UdeScl,50);
                maid.SetProp(MPN.ArmL,20);
                maid.SetProp(MPN.KubiScl,20);

                maid.SetProp(MPN.koshi,50);
                maid.SetProp(MPN.RegMeet,30);
                maid.SetProp(MPN.RegFat,30);
                maid.AllProcProp();

                //撮影までに脱衣が終わらない場合があるので何ミリ秒か待つ
                //waitTime(1.0f);

                //撮影

                RenderTexture m_rtThumCard = new RenderTexture(80, 80, 24, RenderTextureFormat.ARGB32);
                m_rtThumCard.filterMode = FilterMode.Bilinear;
                m_rtThumCard.antiAliasing = 8;
                RenderTexture m_rtThumCard2 = new RenderTexture(80, 80, 0, RenderTextureFormat.ARGB32);

                Texture2D posetex = posethumshot.RenderThum(poseshotthumCamera, m_rtThumCard, m_rtThumCard2, new Size<int>(80, 80));
                //できた画像は一旦全部α値そのままでグレーColor(128,128,128)にする
                //白背景と重ねる
                //やり方がよくわからないので直に計算する
                Color[] pixels = posetex.GetPixels();

                for (int i = 0; i < pixels.Length; ++i)
                {
                    if (pixels[i] != Color.clear)
                    {
                        pixels[i].r = 1 - 0.5f * pixels[i].a;
                        pixels[i].g = 1 - 0.5f * pixels[i].a;
                        pixels[i].b = 1 - 0.5f * pixels[i].a;
                        pixels[i].a = 1;
                    }
                    else
                    {
                        pixels[i] = Color.white;
                    }



                }
                posetex.SetPixels(pixels);

                Color clear1 = new Color(1, 1, 1, 239f / 255f);
                Color clear2 = new Color(1, 1, 1, 207f / 255f);
                Color clear3 = new Color(1, 1, 1, 128f / 255f);

                //角を削る
                posetex.SetPixel(0, 0, Color.clear);
                posetex.SetPixel(0, 1, Color.clear);
                posetex.SetPixel(1, 0, Color.clear);
                posetex.SetPixel(1, 1, clear2);
                posetex.SetPixel(2, 0, clear3);
                posetex.SetPixel(0, 2, clear3);
                posetex.SetPixel(3, 0, clear1);
                posetex.SetPixel(0, 3, clear1);

                posetex.SetPixel(79, 0, Color.clear);
                posetex.SetPixel(79, 1, Color.clear);
                posetex.SetPixel(78, 0, Color.clear);
                posetex.SetPixel(78, 1, clear2);
                posetex.SetPixel(77, 0, clear3);
                posetex.SetPixel(79, 2, clear3);
                posetex.SetPixel(76, 0, clear1);
                posetex.SetPixel(79, 3, clear1);

                posetex.SetPixel(0, 79, Color.clear);
                posetex.SetPixel(0, 78, Color.clear);
                posetex.SetPixel(1, 79, Color.clear);
                posetex.SetPixel(1, 78, clear2);
                posetex.SetPixel(2, 79, clear3);
                posetex.SetPixel(0, 77, clear3);
                posetex.SetPixel(3, 79, clear1);
                posetex.SetPixel(0, 76, clear1);

                posetex.SetPixel(79, 79, Color.clear);
                posetex.SetPixel(79, 78, Color.clear);
                posetex.SetPixel(78, 79, Color.clear);
                posetex.SetPixel(78, 78, clear2);
                posetex.SetPixel(77, 79, clear3);
                posetex.SetPixel(79, 77, clear3);
                posetex.SetPixel(76, 79, clear1);
                posetex.SetPixel(79, 76, clear1);

                posetex.Apply();

                byte[] bytes = posetex.EncodeToPNG();


                //撮影が終わったらメイドさんを元に戻す

                maid.SetProp(MPN.HeadX, tHeadX);
                maid.SetProp(MPN.HeadY, tHeadY);

                maid.SetProp(MPN.DouPer, tDouPer);
                maid.SetProp(MPN.sintyou, tsintyou);

                maid.SetProp(MPN.MuneL, tMuneL);
                maid.SetProp(MPN.MuneTare, tMuneTare);
                maid.SetProp(MPN.MuneUpDown, tMuneUpDown);
                maid.SetProp(MPN.MuneYori, tMuneYori);

                maid.SetProp(MPN.west, twest);
                maid.SetProp(MPN.Hara, tHara);
                maid.SetProp(MPN.kata, tkata);
                maid.SetProp(MPN.UdeScl, tUdeScl);
                maid.SetProp(MPN.ArmL, tArmL);
                maid.SetProp(MPN.KubiScl, tKubiScl);

                maid.SetProp(MPN.koshi, tkoshi);
                maid.SetProp(MPN.RegMeet, tRegMeet);
                maid.SetProp(MPN.RegFat, tRegFat);

                maid.AllProcProp();


                if (sceneLevel == 5 && maid != GameMain.Instance.CharacterMgr.GetMaid(0))//bVisivle.Count > 0)
                {
                    foreach (var exSlotVisiblePair in existGoSlot.Select((pairSlotIDList, index) => new { pairSlotIDList, index }))
                    {
                        TBodySkin exslotIDTBodySkin = exSlotVisiblePair.pairSlotIDList;
                        bool slotVisible = visibleList[exSlotVisiblePair.index];

                        exslotIDTBodySkin.boVisible = slotVisible;
                        exslotIDTBodySkin.Update();
                    }

                }
                else
                {
                    foreach (var slotVisiblePair in slotList.Select((pairSlotIDList, index) => new { pairSlotIDList, index }))
                    {
                        string slotID = slotVisiblePair.pairSlotIDList;
                        bool slotVisible = visibleList[slotVisiblePair.index];

                        maid.body0.goSlot[(int)TBody.hashSlotName[slotID]].boVisible = slotVisible;
                        maid.body0.goSlot[(int)TBody.hashSlotName[slotID]].Update();
                    }
                }
                //maid.body0.FixMaskFlag();
                maid.body0.FixVisibleFlag(false);

                //ここで消してたメイドを元に戻す

                foreach (int MaidNo in visMaidNo)
                {
                    Maid tempMaid;
                    if (sceneLevel == 27)
                    {
                        //公式撮影モード
                        tempMaid = GameMain.Instance.CharacterMgr.GetMaid(MaidNo);
                    }
                    else
                    {
                        //複数撮影モード
                        tempMaid = GameMain.Instance.CharacterMgr.GetStockMaid(MaidNo);
                    }
                    tempMaid.Visible = true;
                }



                this.currentMaidNo = tempCurrentNo;

                //pngの先頭にtexの情報を付加してファイル書き込み
                using (BinaryWriter w = new BinaryWriter(File.OpenWrite(text)))
                {

                    //string 9文字+"CM3D2_TEX"
                    w.Write(new byte[] { (byte)0x09, (byte)0x43, (byte)0x4D, (byte)0x33, (byte)0x44, (byte)0x32, (byte)0x5F, (byte)0x54, (byte)0x45, (byte)0x58 });
                    //int Version 1000
                    w.Write(new byte[] { (byte)0xE8, (byte)0x03, (byte)0x00, (byte)0x00 });
                    //w.Write((byte)bBonePath.Length);

                    string sTexpath = "assets/texture/texture/" + pngfilename;

                    //String texのパス＋ファイル名[サイズ可変]
                    byte[] bTexpath = System.Text.Encoding.UTF8.GetBytes(sTexpath);
                    if (sTexpath.Length < 128)
                    {
                        w.Write((byte)bTexpath.Length);
                    }
                    else
                    {
                        w.Write(new byte[] { (byte)(bTexpath.Length % 128 + 128), (byte)(bTexpath.Length / 128) });
                    }
                    w.Write(bTexpath);

                    //pngのサイズ
                    w.Write((int)bytes.Length);


                    w.Write(bytes);
                }


                //jsonファイルの生成

                //表情設定2が公式撮影だと「オリジナル」になるので
                //モーフの値を直に取ってきて判別する
                //int hohoLv = 0;
                //int namidaLv = 0;

                string strFace2 = "頬";
                if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["tear3"]] == 1)
                {
                    strFace2 += "３涙";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["tear2"]] == 1)
                {
                    strFace2 += "２涙";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["tear1"]] == 1)
                {
                    strFace2 += "１涙";
                }
                else
                {
                    strFace2 += "０涙";
                }

                if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["hohol"]] == 1)
                {
                    strFace2 += "３";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["hoho"]] == 1)
                {
                    strFace2 += "２";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["hohos"]] == 1)
                {
                    strFace2 += "１";
                }
                else
                {
                    strFace2 += "０";
                }

                if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["yodare"]] == 1)
                {
                    strFace2 += "よだれ";
                }


                Encoding UTF8BOM = Encoding.GetEncoding("UTF-8");

                StreamWriter writer = new StreamWriter(jsontext, true, UTF8BOM);
                writer.WriteLine("{");
                writer.WriteLine("    \"name\":\"" + posetextname + "\",");
                writer.WriteLine("    \"strFileName\": \"" + masterName + "\",");//anmファイル名前(拡張子無し)
                writer.WriteLine("    \"strIconFileName\": \"" + texfilename + "\",");
                writer.WriteLine("    \"bone\": \"Bip01 Pelvis\",");
                writer.WriteLine("    \"bAutoTwistShoulder\": true,");
                writer.WriteLine("    \"strFileName2\": \"" + maid.ActiveFace + "\",");
                writer.WriteLine("    \"strFileName3\": \"" + strFace2 + "\"");//表情その2
                writer.WriteLine("}");
                writer.Close();
            }
        }

        private void outputANMFile(string anmFilePath)
        {
            try {
                using (BinaryWriter w = new BinaryWriter(File.OpenWrite(anmFilePath)))
                {

                    //string 10文字+"CM3D2_ANIM"
                    w.Write(new byte[] { (byte)0x0A, (byte)0x43, (byte)0x4D, (byte)0x33, (byte)0x44, (byte)0x32, (byte)0x5F, (byte)0x41, (byte)0x4E, (byte)0x49, (byte)0x4D });
                    //int Version 100
                    w.Write(new byte[] { (byte)0xE8, (byte)0x03, (byte)0x00, (byte)0x00 });

                    for (int i = 0; i < mp.BoneCount; i++)
                    {
                        string bone = mp.sBone[i];

                        if (mp.sPath[bone] == "")
                        {
                            Debuginfo.Log(LogLabel + ":bone skip");
                            continue;
                        }

                        // char「01」[1byte]
                        w.Write((byte)0x01);

                        //String ボーンのパス＋ボーン名[サイズ可変]
                        byte[] bBonePath = System.Text.Encoding.UTF8.GetBytes(mp.sPath[bone]);
                        if (bBonePath.Length < 128)
                        {
                            w.Write((byte)bBonePath.Length);
                        }
                        else
                        {
                            w.Write(new byte[] { (byte)(bBonePath.Length % 128 + 128), (byte)(bBonePath.Length / 128) });
                        }
                        w.Write(bBonePath);

                        for (int j = 100; j < 104; j++)
                        {
                            //(Char)ボーンのローカル軸(LocaLRotation x,y,z,w LocalPosition X,Y,Z 100~106)[1byte]
                            w.Write((byte)j);
                            //(int)合計キーフレーム数 [4byte]
                            int keyframe = 2;
                            w.Write(keyframe);

                            //trBone[bone].localRotation.x
                            for (int k = 0; k < keyframe; k++)
                            {
                                //(float)キーフレームのタイミング(1フレーム＝1/60)[4byte]
                                w.Write((float)(2.0f * (float)k / ((float)keyframe - 1)));
                                //(float)各軸の数値[4byte]
                                switch (j)
                                {
                                    case 100:
                                        w.Write((float)trBone[bone].localRotation.x);
                                        break;
                                    case 101:
                                        w.Write((float)trBone[bone].localRotation.y);
                                        break;
                                    case 102:
                                        w.Write((float)trBone[bone].localRotation.z);
                                        break;
                                    case 103:
                                        w.Write((float)trBone[bone].localRotation.w);
                                        break;
                                    default:
                                        break;
                                }
                                //(float)inTangent(前の数値からの接線)[4byte]
                                w.Write((float)0);
                                //(float)outTangent(後の数値への接線)[4byte]
                                w.Write((float)0);
                            }
                        }
                    }


                    for (int i = 0; i < mp.BoneCount; i++)
                    {
                        string bone = mp.sBone[i];

                        if (mp.sPath[bone] == "")
                        {
                            Debuginfo.Log(LogLabel + ":bone skip");
                            continue;
                        }

                        // char「01」[1byte]
                        w.Write((byte)0x01);

                        //String ボーンのパス＋ボーン名[サイズ可変]
                        byte[] bBonePath = System.Text.Encoding.UTF8.GetBytes(mp.sPath[bone]);
                        if (bBonePath.Length < 128)
                        {
                            w.Write((byte)bBonePath.Length);
                        }
                        else
                        {
                            w.Write(new byte[] { (byte)(bBonePath.Length % 128 + 128), (byte)(bBonePath.Length / 128) });
                        }
                        w.Write(bBonePath);

                        for (int j = 104; j < 107; j++)
                        {
                            //(Char)ボーンのローカル軸(LocaLRotation x,y,z,w LocalPosition X,Y,Z 100~106)[1byte]
                            w.Write((byte)j);
                            //(int)合計キーフレーム数 [4byte]
                            int keyframe = 2;
                            w.Write(keyframe);

                            //trBone[bone].localRotation.x
                            for (int k = 0; k < keyframe; k++)
                            {
                                //(float)キーフレームのタイミング(1フレーム＝1/60)[4byte]
                                w.Write((float)(2.0f * (float)k / ((float)keyframe - 1)));
                                //(float)各軸の数値[4byte]
                                switch (j)
                                {
                                    case 104:
                                        w.Write((float)trBone[bone].localPosition.x);
                                        break;
                                    case 105:
                                        w.Write((float)trBone[bone].localPosition.y);
                                        break;
                                    case 106:
                                        w.Write((float)trBone[bone].localPosition.z);
                                        break;
                                    default:
                                        break;
                                }
                                //(float)inTangent(前の数値からの接線)[4byte]
                                w.Write((float)0);
                                //(float)outTangent(後の数値への接線)[4byte]
                                w.Write((float)0);
                            }
                        }
                    }


                    for (int i = 0; i < mp.BoneCount; i++)
                    {
                        string bone = mp.sBone[i];

                        if (mp.sPath[bone] == "")
                        {
                            Debuginfo.Log(LogLabel + ":bone skip");
                            continue;
                        }

                        // char「01」[1byte]
                        w.Write((byte)0x01);

                        //String ボーンのパス＋ボーン名[サイズ可変]
                        byte[] bBonePath = System.Text.Encoding.UTF8.GetBytes(mp.sPath[bone]);
                        if (bBonePath.Length < 128)
                        {
                            w.Write((byte)bBonePath.Length);
                        }
                        else
                        {
                            w.Write(new byte[] { (byte)(bBonePath.Length % 128 + 128), (byte)(bBonePath.Length / 128) });
                        }
                        w.Write(bBonePath);
                    }


                    // char「00」[1byte]
                    w.Write((byte)0x00);

                }
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "outputANMFile:"+ anmFilePath  + ":" + ex); return; }


        }

#endregion



#region Utility methods


internal static Transform FindParent(Transform tr, string s) { return FindParent(tr.gameObject, s).transform; }
        internal static GameObject FindParent(GameObject go, string name)
        {
            if (go == null) return null;

            Transform _parent = go.transform.parent;
            while (_parent)
            {
                if (_parent.name == name) return _parent.gameObject;
                _parent = _parent.parent;
            }

            return null;
        }

        internal static Transform FindChild(Transform tr, string s) { return FindChild(tr.gameObject, s).transform; }
        internal static GameObject FindChild(GameObject go, string s)
        {
            if (go == null) return null;
            GameObject target = null;

            foreach (Transform tc in go.transform)
            {
                if (tc.gameObject.name == s) return tc.gameObject;
                target = FindChild(tc.gameObject, s);
                if (target) return target;
            }

            return null;
        }

        internal static Transform FindChildByTag(Transform tr, string s) { return FindChildByTag(tr.gameObject, s).transform; }
        internal static GameObject FindChildByTag(GameObject go, string s)
        {
            if (go == null) return null;
            GameObject target = null;

            foreach (Transform tc in go.transform)
            {
                if (tc.gameObject.name.Contains(s)) return tc.gameObject;
                target = FindChild(tc.gameObject, s);
                if (target) return target;
            }

            return null;
        }


        internal static void SetChild(GameObject parent, GameObject child)
        {
            child.layer = parent.layer;
            child.transform.parent = parent.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.rotation = Quaternion.identity;
        }

        internal static GameObject SetCloneChild(GameObject parent, GameObject orignal, string name)
        {
            GameObject clone = UnityEngine.Object.Instantiate(orignal) as GameObject;
            if (!clone) return null;

            clone.name = name;
            SetChild(parent, clone);

            return clone;
        }

        internal static void ReleaseChild(GameObject child)
        {
            child.transform.parent = null;
            child.SetActive(false);
        }

        internal static void DestoryChild(GameObject parent, string name)
        {
            GameObject child = FindChild(parent, name);
            if (child)
            {
                child.transform.parent = null;
                GameObject.Destroy(child);
            }
        }

        internal static UIAtlas FindAtlas(string s)
        {
            return ((new List<UIAtlas>(Resources.FindObjectsOfTypeAll<UIAtlas>())).FirstOrDefault(a => a.name == s));
        }

        internal static void WriteTrans(string s)
        {
            GameObject go = GameObject.Find(s);
            if (!go) return;

            WriteTrans(go.transform, 0, null);
        }
        internal static void WriteTrans(Transform t) { WriteTrans(t, 0, null); }
        internal static void WriteTrans(Transform t, int level, StreamWriter writer)
        {
            if (level == 0) writer = new StreamWriter(@".\" + t.name + @".txt", false);
            if (writer == null) return;

            string s = "";
            for (int i = 0; i < level; i++) s += "    ";
            writer.WriteLine(s + level + "," + t.name);
            foreach (Transform tc in t)
            {
                WriteTrans(tc, level + 1, writer);
            }

            if (level == 0) writer.Close();
        }

        internal static void WriteChildrenComponent(GameObject go)
        {
            WriteComponent(go);

            foreach (Transform tc in go.transform)
            {
                WriteChildrenComponent(tc.gameObject);
            }
        }

        internal static void WriteComponent(GameObject go)
        {
            Component[] compos = go.GetComponents<Component>();
            foreach (Component c in compos) { Debuginfo.Log(go.name + ":" + c.GetType().Name); }
        }

        #endregion
    }
}

