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
//using System.Runtime.InteropServices;

namespace CM3D2.AddBoneSlider.Plugin
{

    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86"), PluginFilter("CM3D2OHx64"),PluginFilter("CM3D2OHx86")]
    [PluginName("CM3D2 AddBoneSlider"), PluginVersion("0.0.1.4")]


    //Debuginfo.Logの代わりにLoginfo.Logを使う
    static class Debuginfo
    {
        public static int settingLevel = 0;
        public static string premessage = "";
        //_messageLV 0：常に表示 1：公開デバッグモード用メッセージ 2：個人テスト用メッセージ
        internal static void Log(string _message, int _messageLv = 2)
        {
            if (_messageLv <= settingLevel)
            {
                Debug.Log(premessage + _message);
            }

        }
    }
    

    public class AddBoneSlider : UnityInjector.PluginBase
    {

        #region Constants


        public const string PluginName = "AddBoneSlider";
        public const string Version = "0.0.1.5dev1";

        private readonly string LogLabel = AddBoneSlider.PluginName + " : ";


        private readonly int iSceneEdit = 5; //メイン版エディットモード
        private readonly int iScenePhoto = 27; //メイン版公式撮影モード
        private readonly int iSceneEditCBL = 4; //CBL版エディットモード
        private readonly int iScenePhotoCBL = 21; //CBL版公式撮影モード

        private readonly float TimePerInit = 1.00f;

        //private readonly int IKCalc = 3;

        //private readonly float clickCheckOffsetInit = 40f;


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
        private bool bCBLMode = false;

        private BoneParam mp;

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
        private Dictionary<string, Dictionary<string, UISlider>> uiSlider = new Dictionary<string, Dictionary<string, UISlider>>();


        public Dictionary<string, Transform> trBone = new Dictionary<string, Transform>();
        public Dictionary<string, Quaternion> vPastBoneAngle = new Dictionary<string, Quaternion>();
        private Dictionary<string, Transform> trPoseImgUnit = new Dictionary<string, Transform>();

        public Vector3 vPastBoneTrans;


        //copy用
        private Dictionary<string, Quaternion> vCopyBoneAngle = new Dictionary<string, Quaternion>();
        private Vector3 vCopyBoneTrans;


        //Undo履歴
        private UndoList undoList;
        private string UndofuncName;
        //その他カテのスライダー初期化カウント
        private Dictionary<string, int> iOtherSliderInit;
        private bool bUndoLock = false;

        //ボーンカテゴリー管理用
        private string sCurrentCategory = "Default";

        public string activeHandleName = "";
        HandleKun posHandle;

        public string PoseXmlFileName;
        public string PoseTexDirectoryName;
        public string iniFileName;
        public string PoseName  ="";

        private IKManage ikManage;

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

                if(XmlFormat != "1.01" )
                {
                    Debug.LogError(LogLabel + " \"" + XmlFileName + "\" is old Version format.");
                    return false;
                }

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

        

        #endregion



        #region MonoBehaviour methods

        public void OnLevelWasLoaded(int level)
        {



            if (level == 9)
            {
                font = GameObject.Find("SystemUI Root").GetComponentsInChildren<UILabel>()[0].trueTypeFont;
                setInifile();

                //CBL版か通常版かこのタイミングで判断させる
                if (File.Exists(Directory.GetCurrentDirectory() + @"\CM3D2OH.exe"))
                {
                    bCBLMode = true;

                }
                else
                {
                    bCBLMode = false;

                }
            }

            //VR版は当面の間非サポートに決定
            //VR版はUnity5らしい＆PluginFilterが機能してないので
            //ここでフィルタリング
            //一応iniで起動できるようにもしておく

            if (UnityEngine.Application.unityVersion.Split('.')[0] == "4" || settingIni.VRmodeEnable != 0 )
            {
                //SceneLevel == 4(CBL版エディットモード)  と　SceneLevel21 == 21（CBL版公式撮影モード） も追加
                if (level != sceneLevel && ((sceneLevel == getEditModeSceneNo()) || (sceneLevel == getPhotoModeSceneNo())))
                {
                    finalize();
                }

                if ((level == getEditModeSceneNo()) || (level == getPhotoModeSceneNo()))
                {
                    mp = new BoneParam();
                    if (xmlLoad = mp.Init()) StartCoroutine(initCoroutine());

                }

                sceneLevel = level;
            }
        }

        public void Update()
        {
            if (((sceneLevel == getEditModeSceneNo()) || (sceneLevel == getPhotoModeSceneNo())) && bInitCompleted)
            {
                //Input.GetKeyDownの代わりに
                //FlexKeycode.GetKeyDownを使う
                if (FlexKeycode.GetKeyDown(settingIni.ToggleKey))
                {
                    if (maid != null && maid.Visible == true && visible == false)
                    {
                        currentMaidChange();
                    }
                    goAMSPanel.SetActive(visible = !visible);

                    //posHandle.Visible = visible;

                    posHandle.setVisible(visible);
                    
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
                            posHandle.setVisible(false);
                        }
                    }
                }

                if (visible)
                {
                    if (bLocked == false)
                    {
                        //UIと一緒に消す用
                        //if (settingIni.HandleLegacymode == 0)
                        //{
                            posHandle.Proc();
                        //}
                        //複数撮影SS対策用
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                            //Debuginfo.Log(LogLabel +"input S");
                            posHandle.Visible = false;
                        }

                        //ハンドル君がIKモードの時、右クリックして
                        //クリック位置から最も近い_IK_ボーンを探す
                        if(posHandle.Visible == true && posHandle.IKmode!= HandleKun.IKMODE.None )
                        {
                            ikManage.updateFunc(posHandle,trBone);
                        }

                        if (posHandle.Visible == true)
                        {
                            //syncFromHandle();

                            if (posHandle.IKTargetVisible == true)
                            {
                                //Undoチェック
                                //UndofuncName = "IKTargetClicked";


                                ikTargetClicked();
                            }

                            if(posHandle.IKmode == HandleKun.IKMODE.None)
                            {
                                syncFromHandle();
                            }
                            else
                            {
                                string tempUndofuncName = ikManage.inversekinematicHandle(undoList.Add, getCurrentIKBone, setCurrentIKBone, 
                                    getCurrentIKBone(currentMaidNo, (int)posHandle.IKmode), this.setIKButtonActiveforUndo, this.getIKButtonActiveforUndo, 
                                    posHandle, maid, currentMaidNo,UndofuncName);

                                if (tempUndofuncName != UndofuncName)
                                {
                                    setIKButtonActive(true, (int)posHandle.IKmode);
                                    UndofuncName = tempUndofuncName;
                                }
                            }
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

        //IK機能用
        public void LateUpdate()
        {
            if (maid != null && maid.Visible == true && ikManage !=null  )
            {
                ikManage.lateupdateFunc(posHandle);

                syncFromIK();


                //ハンドル君がIKボーンにアタッチした状態で
                //そのIKボーンをもつメイドさんがいなくなると
                //ハンドル君もいっしょに死ぬので
                //ハンドル君が死んでないかチェック
                if (posHandle.checkAlive() == false)
                {
                    Debuginfo.Log(LogLabel + "ハンドル君にリザします");
                    posHandle.Init(true);
                    rebootHandle();
                }

                //ハンドル君がIKボーンにアタッチした状態で
                //そのIKボーンをもつメイドさんが消えると
                //ハンドル君もいっしょに消えるので
                //ハンドル君が消えてないかチェック
                if (posHandle.checkBanishment() == false)
                {
                    Debuginfo.Log(LogLabel + "ハンドル君のバニシュを解除します");
                    rebootHandle();
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

                if ((sceneLevel == getEditModeSceneNo()) && bone == "allpos")
                {
                    setParentAllOffset();
                }
                setUnitButtonColor(UIButton.current, b);
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

                    bUndoLock = true;
                    //Undoチェック
                    UndofuncName = "PoseLoad:" + maid.name;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                    //Undo履歴に加える
                    //undoList.Add(new UndoBoneAll(trBone));
                    undoList.Add(Undo.createUndoBoneAll(trBone));

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
                        //    undoValue[bone][prop] = mp.fValue[bone][prop];
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


                        vPastBoneAngle[bone] = trBone[bone].localRotation;
                    }
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                    bLocked = false;
                    bUndoLock = false;

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
            FindChildByTag(UIButton.current.gameObject, "SelectCursor").SetActive(bToggle);
            goPSPanel.SetActive(bToggle);

            if(bToggle == true)
                uiPSTable.Reposition();
        }

        public void OnClickSavePose()
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

                        tempMaid = GetMaid(stockNo);
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

                tempMaid = GetMaid(MaidNo);
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
        public void OnClickOutputPose()
        {
            //anmファイル出力用のパネルを展開
            goPNamePanel.SetActive(true);
        }


        public void OnClickIKLeftLeg()
        {
            try
            {
                if (UICamera.currentTouchID == -1)
                {
                    //カーソルの状態で判別
                    //bool bToggle = !(UIButton.current.defaultColor.a == 1f);
                    bool bToggle = !FindChildByTag(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {

                        //Eye to camera をここで止める
                        maid.body0.m_Bones.animation.Stop();
                        maid.body0.boHeadToCam = false;
                        maid.body0.boEyeToCam = false;

                        //以下、IK用変数初期化処理
                        Transform[] boneList = { trBone["Bip01 L Thigh"], trBone["Bip01 L Calf"], trBone["Bip01 L Foot"] };
                        ikManage.addList(trBone["Bip01"], boneList, maid, currentMaidNo,0);

                        /*
                        if (!IKLeftLeg.ContainsKey(currentMaidNo))
                        {
                            //Debuginfo.Log("init IKLeftLeg");
                            IKCONSTRAINED ikTempLeftLeg = new IKCONSTRAINED();
                            float[,] constrait =
                                {
                                {
                                    mp.fVmin["Bip01 L Foot"]["Bip01 L Foot.x"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.x"],
                                    mp.fVmax["Bip01 L Foot"]["Bip01 L Foot.x"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.x"],
                                    mp.fVmin["Bip01 L Foot"]["Bip01 L Foot.y"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.y"],
                                    mp.fVmax["Bip01 L Foot"]["Bip01 L Foot.y"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.y"],
                                    mp.fVmin["Bip01 L Foot"]["Bip01 L Foot.z"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.z"],
                                    mp.fVmax["Bip01 L Foot"]["Bip01 L Foot.z"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.z"]
                                },
                                {
                                    mp.fVmin["Bip01 L Calf"]["Bip01 L Calf.x"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.x"],
                                    mp.fVmax["Bip01 L Calf"]["Bip01 L Calf.x"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.x"],
                                    mp.fVmin["Bip01 L Calf"]["Bip01 L Calf.y"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.y"],
                                    mp.fVmax["Bip01 L Calf"]["Bip01 L Calf.y"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.y"],
                                    mp.fVmin["Bip01 L Calf"]["Bip01 L Calf.z"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.z"],
                                    mp.fVmax["Bip01 L Calf"]["Bip01 L Calf.z"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.z"]
                                },
                                {
                                    mp.fVmin["Bip01 L Thigh"]["Bip01 L Thigh.x"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.x"],
                                    mp.fVmax["Bip01 L Thigh"]["Bip01 L Thigh.x"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.x"],
                                    mp.fVmin["Bip01 L Thigh"]["Bip01 L Thigh.y"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.y"],
                                    mp.fVmax["Bip01 L Thigh"]["Bip01 L Thigh.y"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.y"],
                                    mp.fVmin["Bip01 L Thigh"]["Bip01 L Thigh.z"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.z"],
                                    mp.fVmax["Bip01 L Thigh"]["Bip01 L Thigh.z"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.z"]
                                }
                            };
                            ikTempLeftLeg.Init(trBone["Bip01 L Thigh"], trBone["Bip01 L Calf"], trBone["Bip01 L Foot"], maid.body0, constrait);
                            IKLeftLeg.Add(currentMaidNo, ikTempLeftLeg);
                        }


                        //IK対象ボーンリストが設定されていなければ初期化
                        if (!trIKLeftLegBones.ContainsKey(currentMaidNo))
                        {
                            //Debuginfo.Log("init trIKLeftLegBones");
                            Transform[] boneList = { trBone["Bip01 L Thigh"], trBone["Bip01 L Calf"], trBone["Bip01 L Foot"] };
                            trIKLeftLegBones.Add(currentMaidNo, boneList);
                        }

                        //IKアタッチ状態が設定されていなければ一時表示[None]で初期化設定
                        if (!bIKAttachLeftLeg.ContainsKey(currentMaidNo))
                        {
                            //Debuginfo.Log("init bIKAttachLeftLeg");
                            bIKAttachLeftLeg.Add(currentMaidNo, false);
                        }

                        //IKターゲットが生成されてなければ生成
                        if (!goIKLeftLegTarget.ContainsKey(currentMaidNo))
                        {
                            //Debuginfo.Log("init goIKLeftLegTarget");
                            GameObject tempIKLeftLegTarget = new GameObject();
                            tempIKLeftLegTarget.transform.parent = trBone["Bip01"];
                            goIKLeftLegTarget.Add(currentMaidNo, tempIKLeftLegTarget);
                            //念のため
                            bIKAttachLeftLeg[currentMaidNo] = false;
                        }
                        */

                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.LeftLeg;
                        //if (!bIKAttachLeftLeg.ContainsKey(currentMaidNo) || !bIKAttachLeftLeg[currentMaidNo])
                        if(!ikManage.checkIKAttach(currentMaidNo,0) )
                        {
                            //Debuginfo.Log("init bIKAttachLeftLeg");
                            //posHandle.SetParentBone(trBone["Bip01"]);

                            posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                            //posHandle.Rot = Quaternion.Euler(-90, 0, 90) ;
                            //posHandle.Scale = 0.2f;
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTransform(currentMaidNo,0));
                        }

                        //IK用ハンドル君表示
                        posHandle.setVisible(true);

                        //今表示されているメイドさんの_IK_ボーン情報をコレクションに収納
                        settrTargetIKBones();

                    }
                    else
                    {
                        //ターゲット操作用ハンドル君非表示

                        posHandle.setVisible(false);
                    }

                    if (ikManage.checkParentName(currentMaidNo, 0))
                    {
                        posHandle.IKTargetAttachedColor(true);
                    }
                    else
                    {
                        posHandle.IKTargetAttachedColor(false);
                    }

                    //IKボタン類の表示状態を処理

                    setIKButtonCursorActive(bToggle);

                }
                else if (UICamera.currentTouchID == -2)
                {   //右クリックでIK解除
                    
                    ikManage.detachIK(undoList.Add ,this.setIKButtonActiveforUndo,getIKButtonActiveforUndo(), trBone["Bip01"], currentMaidNo, 0,UndofuncName);

                    if (posHandle.IKmode ==HandleKun.IKMODE.LeftLeg)
                    {
                        posHandle.SetParentBone(trBone["Bip01 L Foot"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }
                    
                    setIKButtonCursorActive(false);
                    setIKButtonActive(false, 0);

                    posHandle.IKTargetAttachedColor(false);
                }
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKLeftLeg() " + ex); return; }
        }
        public void OnClickIKRightLeg()
        {
            try
            {
                if (UICamera.currentTouchID == -1)
                {
                    //カーソルの状態で判別
                    bool bToggle = !FindChildByTag(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {

                        //Eye to camera をここで止める
                        maid.body0.m_Bones.animation.Stop();
                        maid.body0.boHeadToCam = false;
                        maid.body0.boEyeToCam = false;

                        //以下、IK用変数初期化処理
                        Transform[] boneList = { trBone["Bip01 R Thigh"], trBone["Bip01 R Calf"], trBone["Bip01 R Foot"] };
                        ikManage.addList(trBone["Bip01"], boneList, maid, currentMaidNo, 1);


                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.RightLeg;
                        //if (!bIKAttachRightLeg.ContainsKey(currentMaidNo) || !bIKAttachRightLeg[currentMaidNo])
                        if (!ikManage.checkIKAttach(currentMaidNo, 1))
                        {
                            posHandle.SetMaid(maid, trBone["Bip01 R Foot"]);
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTransform(currentMaidNo, 1));
                        }

                        //IK用ハンドル君表示
                        posHandle.setVisible(true);

                        //今表示されているメイドさんの_IK_ボーン情報をコレクションに収納
                        settrTargetIKBones();

                    }
                    else
                    {
                        //IK用ハンドル君非表示表示
                        posHandle.setVisible(false);
                    }

                    //IKボタン類の表示状態を一括処理

                    setIKButtonCursorActive(bToggle);

                    if (ikManage.checkParentName(currentMaidNo, 1))
                    {
                        posHandle.IKTargetAttachedColor(true);
                    }
                    else
                    {
                        posHandle.IKTargetAttachedColor(false);
                    }
                }
                else if (UICamera.currentTouchID == -2)
                {  //右クリックでIK解除
                    
                    ikManage.detachIK(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), trBone["Bip01"], currentMaidNo, 1,UndofuncName);

                    if (posHandle.IKmode == HandleKun.IKMODE.RightLeg)
                    {
                        posHandle.SetParentBone(trBone["Bip01 R Foot"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }

                    setIKButtonCursorActive(false);
                    setIKButtonActive(false, 1);

                    posHandle.IKTargetAttachedColor(false);
                }

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKRightLeg() " + ex); return; }
        }
        public void OnClickIKLeftArm()
        {
            try
            {
                if (UICamera.currentTouchID == -1)
                {
                    //カーソルの状態で判別
                    bool bToggle = !FindChildByTag(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {

                        //Eye to camera をここで止める
                        maid.body0.m_Bones.animation.Stop();
                        maid.body0.boHeadToCam = false;
                        maid.body0.boEyeToCam = false;

                        //本体側で腕IKが設定されていれば解除
                        if (maid.body0.tgtHandL != null || maid.body0.tgtHandL_AttachName != string.Empty)
                        {
                            maid.IKTargetToBone("左手", null, "無し", Vector3.zero);
                        }


                        //以下、IK用変数初期化処理

                        Transform[] boneList = { trBone["Bip01 L UpperArm"], trBone["Bip01 L Forearm"], trBone["Bip01 L Hand"] };
                        ikManage.addList(trBone["Bip01"], boneList, maid, currentMaidNo, 2);


                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.LeftArm;
                        //if (!bIKAttachLeftArm.ContainsKey(currentMaidNo) || !bIKAttachLeftArm[currentMaidNo])
                        if (!ikManage.checkIKAttach(currentMaidNo, 2))
                        {
                            posHandle.SetMaid(maid, trBone["Bip01 L Hand"]);
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTransform(currentMaidNo, 2));
                        }

                        //IK用ハンドル君表示
                        posHandle.setVisible(true);

                        //今表示されているメイドさんの_IK_ボーン情報をコレクションに収納
                        settrTargetIKBones();

                    }
                    else
                    {
                        //IK用ハンドル君非表示表示
                        posHandle.setVisible(false);
                    }

                    //IKボタン類の表示状態を一括処理

                    setIKButtonCursorActive(bToggle);

                    if (ikManage.checkParentName(currentMaidNo, 2))
                    {
                        posHandle.IKTargetAttachedColor(true);
                    }
                    else
                    {
                        posHandle.IKTargetAttachedColor(false);
                    }
                }
                else if (UICamera.currentTouchID == -2)
                {  //右クリックでIK解除
                   
                    ikManage.detachIK(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), trBone["Bip01"], currentMaidNo, 2, UndofuncName);

                    if (posHandle.IKmode == HandleKun.IKMODE.LeftArm)
                    {
                        posHandle.SetParentBone(trBone["Bip01 L Hand"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }

                    setIKButtonCursorActive(false);
                    setIKButtonActive(false, 2);

                    posHandle.IKTargetAttachedColor(false);
                }

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKLefttArm() " + ex); return; }
        }
        public void OnClickIKRightArm()
        {
            try
            {
                if (UICamera.currentTouchID == -1)
                {
                    //カーソルの状態で判別
                    bool bToggle = !FindChildByTag(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {

                        //Eye to camera をここで止める
                        maid.body0.m_Bones.animation.Stop();
                        maid.body0.boHeadToCam = false;
                        maid.body0.boEyeToCam = false;

                        //本体側で腕IKが設定されていれば解除

                        if (maid.body0.tgtHandR != null || maid.body0.tgtHandR_AttachName != string.Empty)
                        {
                            maid.IKTargetToBone("右手", null, "無し", Vector3.zero);
                        }

                        //以下、IK用変数初期化処理

                        Transform[] boneList = { trBone["Bip01 R UpperArm"], trBone["Bip01 R Forearm"], trBone["Bip01 R Hand"] };
                        ikManage.addList(trBone["Bip01"], boneList, maid, currentMaidNo, 3);

                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.RightArm;
                        //if (!bIKAttachRightArm.ContainsKey(currentMaidNo) || !bIKAttachRightArm[currentMaidNo])
                        if (!ikManage.checkIKAttach(currentMaidNo, 3))
                        {
                            posHandle.SetMaid(maid, trBone["Bip01 R Hand"]);
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTransform(currentMaidNo, 3));
                        }

                        //IK用ハンドル君表示
                        posHandle.setVisible(true);

                        //今表示されているメイドさんの_IK_ボーン情報をコレクションに収納
                        settrTargetIKBones();

                    }
                    else
                    {
                        //IK用ハンドル君非表示
                        posHandle.setVisible(false);
                    }

                    //IKボタン類の表示状態を一括処理

                    setIKButtonCursorActive(bToggle);

                    if (ikManage.checkParentName(currentMaidNo, 3))
                    {
                        posHandle.IKTargetAttachedColor(true);
                    }
                    else
                    {
                        posHandle.IKTargetAttachedColor(false);
                    }

                }
                else if (UICamera.currentTouchID == -2)
                {  //右クリックでIK解除
                    
                    ikManage.detachIK(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), trBone["Bip01"], currentMaidNo, 3, UndofuncName);

                    if (posHandle.IKmode == HandleKun.IKMODE.RightArm)
                    {
                        posHandle.SetParentBone(trBone["Bip01 R Hand"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }

                    setIKButtonCursorActive(false);
                    setIKButtonActive(false, 3);

                    posHandle.IKTargetAttachedColor(false);
                }

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKRightArm() " + ex); return; }
        }
        //IK全解除
        public void OnClickIKDetach()
        {
            try
            {

                //ikManage.setUndoDetachAllSet(undoList.Add, this.setIKButtonActiveforUndo, this.getIKButtonActiveforUndo(), posHandle, maid, currentMaidNo, UndofuncName);

                UndofuncName = ikManage.detachIKAll(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), trBone["Bip01"], currentMaidNo, UndofuncName);

                

                //IKを解除

                //ハンドル君の位置を初期化
                posHandle.SetParentBone(trBone["Bip01"]);

                //現在のメイドのIK情報をコレクションリストから全削除
                //ikManage.removeAttachMaidList(currentMaidNo);
                //ikManage.attachIKMaidNo.Remove(currentMaidNo);

                //IK用ハンドル君非表示
                posHandle.setVisible(false);

                //IKボタンをオフらせる
                setIKButtonCursorActive(false);
                setIKButtonActive();

                posHandle.IKTargetAttachedColor(false);  
                                
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKDetach() " + ex); return; }

        }
        //全メイドさんのIK全解除
        public void OnClickIKDetachAll()
        {
            try
            {

                //ikManage.setUndoDetachAllMaidAllSet(undoList.Add, this.setIKButtonActiveforUndo, this.getIKButtonActiveforUndo(), UndofuncName);


                //ハンドル君の位置を初期化
                posHandle.SetParentBone(trBone["Bip01"]);


                //全メイドのIK情報をコレクションリストから全削除
                UndofuncName = ikManage.detachAll(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), UndofuncName);

                //IK用ハンドル君非表示
                posHandle.setVisible(false);
                posHandle.IKTargetAttachedColor(false);


                //IKボタンを全部オフらせる
                setIKButtonCursorActive(false);
                setIKButtonActive();



            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKDetachAll() " + ex); return; }
        }

        public void OnClickHandleButton()
        {
            try
            {
                posHandle.IKmode = HandleKun.IKMODE.None;

                string bone = getTag(UIButton.current, 1);


                if ((sceneLevel == getEditModeSceneNo()) && bone == "allpos")
                {
                    setParentAllOffset();
                }

                //IKハンドルモードからの切り替え処理も書く

                //if (UICamera.currentTouchID == -1)
                {
                    if (bone == "secret" || bone == "eye" || bone == "camera" || bone == "light" || bone == "gravity")//|| bone == "offset" || bone == "allpos")
                        return;

                    if (activeHandleName != bone)
                    {
                        if (activeHandleName != "")
                        {
                            imvisibleBothHandle(activeHandleName);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "RotHandle:" + activeHandleName).GetComponent<UIButton>(), false);

                            if (activeHandleName == "Bip01" || activeHandleName == "allpos" || activeHandleName == "offset")
                            {
                                setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "PosHandle:" + activeHandleName).GetComponent<UIButton>(), false);

                            }
                        }

                        if(bone == "allpos")
                            posHandle.SetParentBone(maid.gameObject.transform.parent);
                        else if(bone == "offset")
                            posHandle.SetParentBone(maid.gameObject.transform);
                        else
                            posHandle.SetParentBone(trBone[bone]);
                    }

                    if (posHandle.bHandlePositionMode == true)
                    {
                        posHandle.ChangeHandleModePosition(false);

                        if (posHandle.Visible == true && activeHandleName == bone)
                        {
                            setUnitButtonColor(UIButton.current, true);
                            GameObject goSelectCursorRot = FindChild(trBoneUnit[bone], "SelectCursorRotHandle:" + bone).gameObject;
                            goSelectCursorRot.SetActive(true);
                            if (bone == "allpos" || bone == "offset" || bone == "Bip01")
                            {
                                setUnitButtonColor(FindChildByTag(trBoneUnit[bone], "PosHandle:" + bone).GetComponent<UIButton>(), false);
                                GameObject goSelectCursorPos = FindChild(trBoneUnit[bone], "SelectCursorPosHandle:" + bone).gameObject;
                                goSelectCursorPos.SetActive(false);
                            }
                            return;
                        }
                    }
                    
                    //posHandle.Visible = b;
                }

                bool b = visibleHandle(bone,false);
                setUnitButtonColor(UIButton.current, b);
                //setUnitButtonColor(UIButton.current, b);
                if (bone == "allpos" || bone == "offset" || bone =="Bip01")
                {
                    setUnitButtonColor(FindChildByTag(trBoneUnit[bone], "PosHandle:" + bone).GetComponent<UIButton>(), false);
                }

                activeHandleName = b ? bone : "";

                posHandle.IKTargetAttachedColor(false);
                posHandle.setVisible(b);
                

                //IKボタンのカーソルだけをオフらせる
                //setIKButtonActive(true, true);
                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);


            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickHandleButton() " + ex); return; }
        }

        public void OnClickPosHandleButton()
        {
            try
            {
                posHandle.IKmode = HandleKun.IKMODE.None;

                string bone = getTag(UIButton.current, 1);

                if (sceneLevel == getEditModeSceneNo() && bone == "allpos")
                {
                    setParentAllOffset();
                }
                

                {
                    
                    if (bone != "Bip01" && bone != "allpos" && bone != "offset")
                        return;


                    if (activeHandleName != bone)
                    {
                        if (activeHandleName != "")
                        {

                            imvisibleBothHandle(activeHandleName);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "RotHandle:" + activeHandleName).GetComponent<UIButton>(), false);
                            

                            if (activeHandleName == "Bip01" || activeHandleName == "allpos" || activeHandleName == "offset")
                            {
                                setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "PosHandle:" + activeHandleName).GetComponent<UIButton>(), false);

                            }
                        }
                        

                        if (bone == "allpos")
                            posHandle.SetParentBone(maid.gameObject.transform.parent);
                        else if (bone == "offset")
                            posHandle.SetParentBone(maid.gameObject.transform);
                        else
                            posHandle.SetParentBone(trBone[bone]);


                    }

                    if (posHandle.bHandlePositionMode == false)
                    {
                        posHandle.ChangeHandleModePosition(true);
                        if (posHandle.Visible == true && activeHandleName == bone)
                        {
                            setUnitButtonColor(UIButton.current, true);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[bone], "RotHandle:" + bone).GetComponent<UIButton>(), false);
                            GameObject goSelectCursorPos = FindChild(trBoneUnit[bone], "SelectCursorPosHandle:" + bone).gameObject;
                            goSelectCursorPos.SetActive(true);
                            GameObject goSelectCursorRot = FindChild(trBoneUnit[bone], "SelectCursorRotHandle:" + bone).gameObject;
                            goSelectCursorRot.SetActive(false);

                            return;
                        }
                    }
                }

                bool b = visibleHandle(bone,true);
                setUnitButtonColor(UIButton.current, b);

                activeHandleName = b ? bone : "";

                posHandle.IKTargetAttachedColor(false);
                posHandle.setVisible(b);

                //IKボタンのカーソルだけをオフらせる
                //setIKButtonActive(true, true);
                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);


            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickPosHandleButton() " + ex); return; }
        }

        public void OnClickResetAll()
        {
            try
            {
                bUndoLock = true;
                //Undoチェック
                UndofuncName = "ResetAll:" + maid.name;
                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);



                jiggleBone jbMuneR = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true).gameObject.GetComponent<jiggleBone>();
                jiggleBone jbMuneL = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true).gameObject.GetComponent<jiggleBone>();



                //Undo履歴に加える                
                undoList.Add(Undo.createUndoAll(maid,
                    trBone.ToDictionary(n => n.Value, n => n.Value.localRotation), (trBone.Where(n => n.Value.name == "Bip01").First().Value.localPosition),
                    GameMain.Instance.CharacterMgr.GetCharaAllPos(), GameMain.Instance.CharacterMgr.GetCharaAllRot(),
                    maid.transform.localPosition, maid.GetRot(), maid.body0.quaDefEyeR, maid.body0.quaDefEyeL,
                    jbMuneR, jbMuneR.MuneUpDown, jbMuneR.MuneYori, jbMuneL, jbMuneL.MuneUpDown, jbMuneL.MuneYori
                    ));

                foreach (string bone in mp.sBone)
                {
                    if (mp.IsToggle(bone))
                    {
                        mp.bEnabled[bone] = false;
                        setUnitButtonColor(bone, mp.bEnabled[bone]);
                    }

                    if (mp.IsSlider(bone) && bone != "camera" )
                    {
                        resetSliderValue(bone);

                        if (mp.IsToggle(bone))
                        {
                            setSliderVisible(bone, mp.bEnabled[bone]);
                        }
                    }
                }
                bUndoLock = false;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickResetAll() " + ex); return; }
        }

        public void OnClickResetButton()
        {
            bUndoLock = true;

            string bone = getTag(UIButton.current, 1);

            //Undo履歴に加える
            switch(bone)
            {
                case "Bip01":
                    //Undoチェック
                    UndofuncName = "Reset:" + bone + ":" + maid.name;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                    undoList.Add(Undo.createUndoBoneBoth(trBone[bone], trBone[bone].localRotation, trBone[bone].localPosition));

                    break;


                case "allpos":
                    //Undoチェック
                    UndofuncName = "Reset:" + bone;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                    undoList.Add(Undo.createUndoAllposBoth(GameMain.Instance.CharacterMgr.GetCharaAllPos(), GameMain.Instance.CharacterMgr.GetCharaAllRot()));


                    break;

                case "offset":
                    //Undoチェック
                    UndofuncName = "Reset:" + bone;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                    undoList.Add(Undo.createUndoOffsetBoth(maid, maid.transform.localPosition,maid.GetRot()));

                    break;

                case "eye":
                    //Undoチェック
                    UndofuncName = "Reset:" + bone;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                    undoList.Add(Undo.createUndoEyeBoth(maid, maid.body0.quaDefEyeR, maid.body0.quaDefEyeL));

                    break;

                case "secret":

                    //Undoチェック
                    UndofuncName = "Reset:" + bone;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                    jiggleBone jbMuneR = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true).gameObject.GetComponent<jiggleBone>();
                    jiggleBone jbMuneL = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true).gameObject.GetComponent<jiggleBone>();

                    undoList.Add(Undo.createUndoSecretBoth(jbMuneR, jbMuneR.MuneUpDown, jbMuneR.MuneYori, jbMuneL, jbMuneL.MuneUpDown, jbMuneL.MuneYori));


                    break;

                case "camera":
                case "light":
                case "gravity":
                    
                    break;

                default:
                    //Undoチェック
                    UndofuncName = "Reset:" + bone + ":" + maid.name;
                    //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                    undoList.Add(Undo.createUndoBone(trBone[bone], trBone[bone].localRotation));

                    break;
            }


            resetSliderValue(bone);

            bUndoLock = false;
        }

        public void OnClickMirrorAll()
        {
            bUndoLock = true;

            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;

            //Undoチェック
            UndofuncName = "MirrorAll:" + maid.name;
            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

            //Undo履歴に加える                
            undoList.Add(Undo.createUndoBone(trBone));

            foreach (var bone in trBone)
            {
                if(bone.Key.Contains("R"))
                    continue;

                mirrorBone(bone.Key);
            }

            bUndoLock = false;
        }

        public void OnClickCopyAll()
        {

            foreach (var tr in trBone)
            {
                if (vCopyBoneAngle.ContainsKey(tr.Key))
                {
                    vCopyBoneAngle[tr.Key] = tr.Value.localRotation;
                }
                else
                {
                    vCopyBoneAngle.Add(tr.Key, tr.Value.localRotation);
                }

                if (tr.Key == "Bip01")
                {
                    vCopyBoneTrans = tr.Value.localPosition ;
                }
            }
            
        }
        public void OnClickPasteAll()
        {
            bUndoLock = true;
            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;

            //Undoチェック
            UndofuncName = "PasteAll:" + maid.name;
            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

            //Undo履歴に加える
            undoList.Add(Undo.createUndoBoneAll(trBone));



            foreach (var tr in trBone)
            {
                if (vCopyBoneAngle.ContainsKey(tr.Key))
                {
                    tr.Value.localRotation = vCopyBoneAngle[tr.Key];
                    if (tr.Key == "Bip01")
                    {
                        tr.Value.localPosition = vCopyBoneTrans;
                    }
                    //スライダーと入力値にも反映
                    calc_trBone2Param(tr.Key);
                    SyncTransform2SliderLabel(tr.Key);

                }
            }
            bUndoLock = false;

        }
        public void OnClickFlushAll()
        {
            vCopyBoneAngle.Clear();
            vCopyBoneTrans = Vector3.zero;

        }

        public void OnClickUndo()
        {
            UndofuncName = "Undo";



            undoList.doUndo();

            //ハンドル君がIKモードで表示されてたら消す
            if(posHandle.Visible == true && posHandle.IKmode != HandleKun.IKMODE.None)
            {
                posHandle.setVisible(false);
                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);
            }
        }

        public void OnClickRedo()
        {
            UndofuncName = "Redo";



            undoList.doRedo();

            //ハンドル君がIKモードで表示されてたら消す
            if (posHandle.Visible == true && posHandle.IKmode != HandleKun.IKMODE.None)
            {
                posHandle.setVisible(false);
                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);
            }

        }

        public void OnClickCopyCategoryButton()
        {
            if (sCurrentCategory == "Default")
                return;

            var trCategory = trBone.Where(m => mp.sCategory[m.Key] == sCurrentCategory);
            
            foreach (var tr in trCategory)
            {
                if (vCopyBoneAngle.ContainsKey(tr.Key))
                {
                    vCopyBoneAngle[tr.Key] = tr.Value.localRotation;
                }
                else
                {
                    vCopyBoneAngle.Add(tr.Key, tr.Value.localRotation);
                }

                if (tr.Key == "Bip01")
                {
                    vCopyBoneTrans = tr.Value.localPosition;
                }
            }

        }

        public void OnClickPasteCategoryButton()
        {
            if (sCurrentCategory == "Default")
                return;

            bUndoLock = true;
            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;


            //Undoチェック
            UndofuncName = "PasteCategory:" + maid.name;
            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

            var trCategory = trBone.Where(m => mp.sCategory[m.Key] == sCurrentCategory);

            //Undo履歴に加える
            if(sCurrentCategory == "Lower")
                undoList.Add(Undo.createUndoBoneAll(trCategory));
            else
                undoList.Add(Undo.createUndoBone(trCategory));


            foreach (var tr in trCategory)
            {
                if (vCopyBoneAngle.ContainsKey(tr.Key))
                {
                    tr.Value.localRotation = vCopyBoneAngle[tr.Key];
                    if (tr.Key == "Bip01")
                    {
                        tr.Value.localPosition = vCopyBoneTrans;
                    }
                    //スライダーと入力値にも反映
                    calc_trBone2Param(tr.Key);
                    SyncTransform2SliderLabel(tr.Key);

                }
            }
            bUndoLock = false;

        }

        public void OnClickFlushCategoryButton()
        {
            if (sCurrentCategory == "Default")
                return;
            
            var trCategory = trBone.Where(m => mp.sCategory[m.Key] == sCurrentCategory);


            foreach (var tr in trCategory)
            {
                //if (vCopyBoneAngle.ContainsKey(tr.Key))
                {
                    vCopyBoneAngle.Remove(tr.Key);

                    if (tr.Key == "Bip01")
                    {
                        vCopyBoneTrans = Vector3.zero;
                    }

                }
            }

        }

        public void OnClickResetCategoryButton()
        {
            if (sCurrentCategory == "Default")
                return;

            bUndoLock = true;
            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;

            //Undoチェック
            UndofuncName = "ResetCategory:" + maid.name;
            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

            var trCategory = trBone.Where(m => mp.sCategory[m.Key] == sCurrentCategory);

            //Undo履歴に加える
            if (sCurrentCategory == "Lower")
                undoList.Add(Undo.createUndoBoneAll(trCategory));
            else
                undoList.Add(Undo.createUndoBone(trCategory));


            foreach (var tr in trCategory)
            {
                resetSliderValue(tr.Key);
            }

            bUndoLock = false;
        }

        public void OnClickMirrorCategoryButton()
        {
            if (sCurrentCategory == "Default")
                return;

            bUndoLock = true;

            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;

            //Undoチェック
            UndofuncName = "MirrorCategory:" + maid.name;
            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

            var trCategory = trBone.Where(m => mp.sCategory[m.Key] == sCurrentCategory);

            //Undo履歴に加える                
            undoList.Add(Undo.createUndoBone(trCategory));

            foreach (var bone in trCategory)
            {
                if (bone.Key.Contains("R") && sCurrentCategory != "RightFinger")
                    continue;

                mirrorBone(bone.Key);
            }
            bUndoLock = false;
        }

        public void OnClickCopyButton()
        {
            string bone = getTag(UIButton.current, 1);

            //ボーンのみ対応
            if (!bone.Contains("Bip01") && !bone.Contains("_IK_"))
                return;

            if (vCopyBoneAngle.ContainsKey(bone))
            {
                vCopyBoneAngle[bone] = trBone[bone].localRotation;
            }
            else
            {
                vCopyBoneAngle.Add(bone,trBone[bone].localRotation);

            }

            if (bone == "Bip01")
            {
                vCopyBoneTrans = trBone[bone].localPosition ;
            }

            //copyType = CopyType.Single;
            

        }

        public void OnClickPasteButton()
        {
            string bone = getTag(UIButton.current, 1);

            //ボーンのみ対応
            if (!bone.Contains("Bip01") && !bone.Contains("_IK_"))
                return;

            bUndoLock = true;
            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;

            //Undoチェック
            
            UndofuncName = "PasteSingle:" + bone + ":" + maid.name;
            //Debuginfo.Log(LogLabel + "Undo::" +UndofuncName);
            //Undo履歴に加える
            if (bone != "Bip01")
            {
                undoList.Add(Undo.createUndoBone(trBone[bone],trBone[bone].localRotation));
            }
            else
            {
                undoList.Add(Undo.createUndoBoneBoth(trBone[bone],trBone[bone].localRotation,trBone[bone].localPosition));
            }

            if (vCopyBoneAngle.ContainsKey(bone))
            {
                trBone[bone].localRotation = vCopyBoneAngle[bone];
                if (bone == "Bip01")
                {
                    trBone[bone].localPosition = vCopyBoneTrans;
                }
                //スライダーと入力値にも反映
                calc_trBone2Param(bone);
                SyncTransform2SliderLabel(bone);

            }
            bUndoLock = false;

        }

        public void OnClickFlushButton()
        {
            string bone = getTag(UIButton.current, 1);

            //ボーンのみ対応
            if (!bone.Contains("Bip01") && !bone.Contains("_IK_"))
                return;

            //if (vCopyBoneAngle.ContainsKey(bone))
            {
                vCopyBoneAngle.Remove(bone);
            }

            if (bone == "Bip01")
            {
                vCopyBoneTrans = Vector3.zero; ;
            }
            /*
            Debuginfo.Log("シェーダー実験");

            string _shaderDir = settingIni.ShaderDirectry;
            if (!File.Exists(_shaderDir + @"\CM3D2_Toony_Lighted_Trans_Extra.Shader"))
            {
                Debug.LogError(AddBoneSlider.PluginName + " : " + _shaderDir + @"\CM3D2_Toony_Lighted_Trans_Extra.Shader is not exist.");
            }

            StreamReader sr = new StreamReader(_shaderDir + @"\CM3D2_Toony_Lighted_Trans_Extra.Shader");
            string shader = sr.ReadToEnd();
            sr.Close();
            Material mMatrix = new Material(shader);
            Transform[] componentsInChildren = maid.body0.goSlot[0].obj.transform.GetComponentsInChildren<Transform>(true);
            Material mOriginal = null;
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Transform transform = componentsInChildren[i];
                Renderer component = transform.GetComponent<Renderer>();
                if (component != null && component.material != null)
                {
                    if (1 < component.materials.Length)
                    {
                        mOriginal = component.materials[1];
                    }
                }
            }
            
            Shader originalShader = mOriginal.shader;
            mOriginal.shader = mMatrix.shader;
            Debuginfo.Log("シェーダー実験");
            */
        }

        public void OnClickMirrorButton()
        {
            string bone = getTag(UIButton.current, 1);

            //ボーンのみ対応
            if (!bone.Contains("Bip01") && !bone.Contains("_IK_"))
                return;

            bUndoLock = true;
            //Eye to camera をここで止める
            maid.body0.m_Bones.animation.Stop();
            maid.body0.boHeadToCam = false;
            maid.body0.boEyeToCam = false;

            //Undoチェック
            UndofuncName = "MirrorSingle:" + bone + ":" + maid.name;
            //Debuginfo.Log(LogLabel +"Undo::" + UndofuncName);

            if (bone.Contains("L") || bone.Contains("R"))
            {
                string mir_bone = bone.Contains("L") ? bone.Replace("L", "R") : bone.Replace("R", "L");

                //Undo履歴に加える                
                undoList.Add(Undo.createUndoBone(new Dictionary<Transform, Quaternion>()
                {
                    {trBone[bone],trBone[bone].localRotation },
                    {trBone[mir_bone],trBone[mir_bone].localRotation }
                }));
            }
            else
            {
                //Undo履歴に加える                
                undoList.Add(Undo.createUndoBone(trBone[bone],trBone[bone].localRotation ));
            }
            mirrorBone(bone);

            bUndoLock = false;
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


                //スライダー初期化時に呼ばれた時はUndoに登録しない
                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera" && bone != "gravity" && bone != "light")
                {
                    if(vPastBoneAngle[bone] != trBone[bone].localRotation)
                        checkUndo("slider", bone, prop);


                    if(bone == "Bip01" && vPastBoneTrans != trBone[bone].localPosition)
                        checkUndo("slider", bone, prop);
                }
                else
                {
                    if (iOtherSliderInit[bone] == 0)
                        checkUndo("slider", bone, prop);
                    else
                        --iOtherSliderInit[bone];
                }
                

                //       undoValue[bone][prop] = mp.fValue[bone][prop];
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

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera" && bone != "gravity" && bone != "light")
                {
                    vPastBoneAngle[bone] = trBone[bone].localRotation;
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
                
                checkUndo("input", bone, prop);

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

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera" && bone != "light" && bone != "gravity")
                {
                    vPastBoneAngle[bone] = trBone[bone].localRotation;
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

            GameObject goBoneCategory0 = FindChild(goAMSPanel, "BoneCategory0");
            GameObject goBoneCategory1 = FindChild(goAMSPanel, "BoneCategory1");
            GameObject goBoneCategory2 = FindChild(goAMSPanel, "BoneCategory2");
            GameObject goBoneCategory3 = FindChild(goAMSPanel, "BoneCategory3");
            GameObject goBoneCategory4 = FindChild(goAMSPanel, "BoneCategory4");
            GameObject goBoneCategory5 = FindChild(goAMSPanel, "BoneCategory5");

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


            FindChild(uiButtonBoneCategory0.gameObject, "SelectCursorBoneCategory0").SetActive(false);
            FindChild(uiButtonBoneCategory1.gameObject, "SelectCursorBoneCategory1").SetActive(false);
            FindChild(uiButtonBoneCategory2.gameObject, "SelectCursorBoneCategory2").SetActive(false);
            FindChild(uiButtonBoneCategory3.gameObject, "SelectCursorBoneCategory3").SetActive(false);
            FindChild(uiButtonBoneCategory4.gameObject, "SelectCursorBoneCategory4").SetActive(false);
            FindChild(uiButtonBoneCategory5.gameObject, "SelectCursorBoneCategory5").SetActive(false);

            FindChildByTag(UIButton.current.gameObject, "SelectCursor").SetActive(true);



            foreach (GameObject go in goScrollViewTable)
            {
                go.SetActive(false);
            }

            if (UIButton.current == uiButtonBoneCategory0)
            {
                goScrollViewTable[0].SetActive(true);
                sCurrentCategory = "Default";
            }
            else if (UIButton.current == uiButtonBoneCategory1)
            {
                goScrollViewTable[1].SetActive(true);
                sCurrentCategory = "Upper";
            }
            else if (UIButton.current == uiButtonBoneCategory2)
            {
                goScrollViewTable[2].SetActive(true);
                sCurrentCategory = "Lower";
            }
            else if (UIButton.current == uiButtonBoneCategory3)
            {
                goScrollViewTable[3].SetActive(true);
                sCurrentCategory = "Toe";
            }
            else if (UIButton.current == uiButtonBoneCategory4)
            {
                goScrollViewTable[4].SetActive(true);
                sCurrentCategory = "LeftFinger";
            }
            else if (UIButton.current == uiButtonBoneCategory5)
            {
                goScrollViewTable[5].SetActive(true);
                sCurrentCategory = "RightFinger";
            }

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

                setInifile();

                Debuginfo.settingLevel = settingIni.DebugLogLevel;

                //ここまで


                PoseXmlFileName = settingIni.PoseXmlDirectory + @"\bonepose.xml"; //Directory.GetCurrentDirectory() + @"\UnityInjector\Config\bonepose.xml";

                Debuginfo.Log(LogLabel + "PoseXmlFileName complete.", 1);

                //メイド情報取得

                //公式撮影モードでも複数メイドでもここの処理の結果は同じはずだからこのまま
                
                currentMaidNo = 0;
                maid = GameMain.Instance.CharacterMgr.GetMaid(currentMaidNo);

                //じゃまずいから複数メイド撮影のときだけcurrentMaidNoの値をここで同期しとく
                if(sceneLevel == getEditModeSceneNo())
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

                if(goProfileTabCopy.GetComponent<UIButton>().onClick.Count > 0)
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

                //ボタン類を追加したときはここを調整
                uiScrollPanel.clipping = UIDrawCall.Clipping.SoftClip;
                uiScrollPanel.SetRect(0f, 0f, uiBGSprite.width, uiBGSprite.height - 130 - systemUnitHeight * 9.5f);
                uiScrollPanel.transform.localPosition = new Vector3(-25f, -systemUnitHeight * 5.5f, 0f);
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
                uiScrollBar.transform.localPosition = new Vector3(uiBGSprite.width / 2f - 10, -systemUnitHeight * 4.5f, 0f);
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
                    goScrollViewTable[i].name = "ScrollViewTable:" + i;
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


                    //サムネポーズ紛失した時用
                    List<XmlNode> deleteNodes = new List<XmlNode>();

                    foreach (XmlNode bonesNode in BonesNodeS)
                    {
                        
                        string name = ((XmlElement)bonesNode).GetAttribute("pose_id");

                        //画像がなかった場合はとばす&ポーズ消去
                        if (!File.Exists(settingIni.PoseImgDirectory + @"\PoseImg" + name + ".png"))
                        {
                            deleteNodes.Add(bonesNode);
                            continue;
                        }

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

                    //画像がなかったポーズのノード消去
                    foreach(XmlNode deleteNode in deleteNodes)
                    {
                        doc.DocumentElement.RemoveChild(deleteNode);
                    }

                }
                Debuginfo.Log(LogLabel + " goPoseImgButton complete.");
                ///////////////////////////////////////////////
                //PoseLoadボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goProfileTabCopy, "LoadPose"),
                    new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight / 2f, 0f), (int)(conWidth * 0.5f) - 2, systemUnitHeight,22, "[111111]LoadPose",true, this.OnClickLoadPose);

                //PoseSaveボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goProfileTabCopy, "SavePose"),
                    new Vector3(conWidth * 0.125f - 5, baseTop - systemUnitHeight / 2f, 0f), (int)(conWidth * 0.25f) - 4, systemUnitHeight, 22,  "[111111]SavePose", true, this.OnClickSavePose);

                //OutputPoseボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goProfileTabCopy, "OutputPose"),
                    new Vector3(conWidth * 0.375f - 4, baseTop - systemUnitHeight / 2f, 0f), (int)(conWidth * 0.25f) - 4, systemUnitHeight, 22, "[111111]OutputPose", true, this.OnClickOutputPose);


                Debuginfo.Log(LogLabel + " goSavePose complete.");

                ///////////////////////////////////////////////

                // 前メイド選択ボタン
                GameObject goPrevMaid = SetCloneChild(goSystemUnit, goProfileTabCopy, "PrevMaid");
                generateUnitButton(goPrevMaid,
                    new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight * 2.5f - 2, 0f), (int)(conWidth * 0.25f) - 4, systemUnitHeight * 3, 22, "[111111]前のメイド", true, this.OnClickPrevMaid);


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

                NGUITools.UpdateWidgetCollider(goCurrentMaid);
                goCurrentMaid.SetActive(true);

                Debuginfo.Log(LogLabel + " goCurrentMaid complete.");

                // 次メイド選択ボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goPrevMaid, "NextMaid"),
                     new Vector3(conWidth * 0.375f - 4, baseTop - systemUnitHeight * 2.5f - 2, 0f), "[111111]次のメイド",true , this.OnClickNextMaid);

                Debuginfo.Log(LogLabel + " goNextMaid complete.");
                ///////////////////////////////////////////////

                // Undoボタン
                GameObject goUndo = SetCloneChild(goSystemUnit, goProfileTabCopy, "Undo");
                generateUnitButton(goUndo,
                    new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight * 4.5f - 6f, 0f), (int)(conWidth * 0.25f) - 4, systemUnitHeight, 22, "[111111]Undo", true, this.OnClickUndo);
                
                // Redoボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "Redo"),
                    new Vector3(-conWidth * 0.125f - 5, baseTop - systemUnitHeight * 4.5f - 6f, 0f), "[111111]Redo", true, this.OnClickRedo);
                
                // MirrorAllボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "MirrorAll"),
                    new Vector3(conWidth * 0.125f - 5, baseTop - systemUnitHeight * 4.5f - 6f, 0f), "[111111]MirrorAll", true, this.OnClickMirrorAll);
                
                // ResetAllボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "ResetAll"),
                    new Vector3(conWidth * 0.375f - 4, baseTop - systemUnitHeight * 4.5f - 6f, 0f), "[111111]ResetAll", true, this.OnClickResetAll);

                ///////////////////////////////////////////////

                // 全IK解除ボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "IKDetachAll"),
                    new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight * 5.5f - 10f, 0f), "[111111]全解除", true, this.OnClickIKDetachAll);

                // IKアタッチ解除ボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "IKDetach"),
                    new Vector3(-conWidth * 0.125f - 5, baseTop - systemUnitHeight * 5.5f - 10f, 0f), "[111111]IK解除", true, this.OnClickIKDetach);

                //CopyAllボタン
                GameObject goCopyAll = SetCloneChild(goSystemUnit, goProfileTabCopy, "goCopyAll");
                generateUnitButton(goCopyAll,
                    new Vector3(conWidth * 0.08333f - 5, baseTop - systemUnitHeight * 5.5f - 10f, 0f), (int)(conWidth * 0.1667f) - 5, systemUnitHeight, 17, "[111111]AllCopy", true, this.OnClickCopyAll);

                //PasteAllボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goCopyAll, "goPasteAll"),
                    new Vector3(conWidth * 0.25f - 4, baseTop - systemUnitHeight * 5.5f - 10f, 0f), "[111111]AllPaste", true, this.OnClickPasteAll);

                //FlushAllボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goCopyAll, "goFlushAll"),
                    new Vector3(conWidth * 0.4167f - 3, baseTop - systemUnitHeight * 5.5f - 10f, 0f), "[111111]AllFlush", true, this.OnClickFlushAll);


                ///////////////////////////////////////////////

                // 左足IKボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "IKLeftLeg"),
                    new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight * 6.5f - 14f, 0f), "[111111]左足IK", true, this.OnClickIKLeftLeg);

                // 右足IKボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "IKRightLeg"),
                    new Vector3(-conWidth * 0.125f - 5, baseTop - systemUnitHeight * 6.5f - 14f, 0f), "[111111]右足IK", true, this.OnClickIKRightLeg);
                
                // 左手IKボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "IKLeftArm"),
                    new Vector3(conWidth * 0.125f - 5, baseTop - systemUnitHeight * 6.5f - 14f, 0f), "[111111]左手IK", true, this.OnClickIKLeftArm);

                // 右手IKボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goUndo, "IKRightArm"),
                    new Vector3(conWidth * 0.375f - 4, baseTop - systemUnitHeight * 6.5f - 14f, 0f), "[111111]右手IK", true, this.OnClickIKRightArm);

                
                ///////////////////////////////////////////////

                //ボーンカテゴリー切り替えボタン
                GameObject goBoneCategory0 = SetCloneChild(goSystemUnit, goProfileTabCopy, "BoneCategory0");
                generateUnitButton(goBoneCategory0,
                    new Vector3(-conWidth * 0.4167f - 7, baseTop - systemUnitHeight * 7.5f - 18f, 0f), (int)(conWidth * 0.1667f) - 5, systemUnitHeight, 20, "[111111]その他", true, this.OnClickBoneCategory);

                generateUnitButton(SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory1"),
                    new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight * 7.5f - 18f, 0f), "[111111]上半身", true, this.OnClickBoneCategory);

                generateUnitButton(SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory2"),
                    new Vector3(-conWidth * 0.08333f - 4, baseTop - systemUnitHeight * 7.5f - 18f, 0f), "[111111]下半身", true, this.OnClickBoneCategory);

                generateUnitButton(SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory3"),
                    new Vector3(conWidth * 0.08333f - 5, baseTop - systemUnitHeight * 7.5f - 18f, 0f), "[111111]足指", true, this.OnClickBoneCategory);
                
                generateUnitButton(SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory4"),
                    new Vector3(conWidth * 0.25f - 4, baseTop - systemUnitHeight * 7.5f - 18f, 0f), "[111111]左手指", true, this.OnClickBoneCategory);

                generateUnitButton(SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory5"),
                    new Vector3(conWidth * 0.4167f - 3, baseTop - systemUnitHeight * 7.5f - 18f, 0f), "[111111]右手指", true, this.OnClickBoneCategory);

                UIButton uiButtonBoneCategory0 = goBoneCategory0.GetComponent<UIButton>();
                uiButtonBoneCategory0.defaultColor = new Color(uiButtonBoneCategory0.defaultColor.r, uiButtonBoneCategory0.defaultColor.g, uiButtonBoneCategory0.defaultColor.b, 1.0f);
                FindChild(uiButtonBoneCategory0.gameObject, "SelectCursorBoneCategory0").SetActive(true);

                ///////////////////////////////////////////////

                float CategoryFuncBase = baseTop - systemUnitHeight * 8.5f - 30f;

                // MirrorCategoryボタン
                GameObject goMirrorCategory = SetCloneChild(goSystemUnit, goProfileTabCopy, "MirrorCategory");
                generateUnitButton(goMirrorCategory,
                    new Vector3(-conWidth * 0.25f - 6, 11f + CategoryFuncBase, 0f), (int)(conWidth * 0.5f) - 2, 19, 14, "[111111]CategoryMirror", true, this.OnClickMirrorCategoryButton);

                // ResetCategoryボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goMirrorCategory, "ResetCategory"),
                    new Vector3(conWidth * 0.25f - 4, 11f + CategoryFuncBase, 0f), "[111111]CategoryReset", true, this.OnClickResetCategoryButton);

                // CopyCategoryボタン
                GameObject goCopyCategory = SetCloneChild(goSystemUnit, goProfileTabCopy, "CopyCategory");
                generateUnitButton(goCopyCategory,
                    new Vector3(-conWidth * 0.3333f -6 , -11f + CategoryFuncBase, 0f), (int)(conWidth * 0.3333f) - 2, 19, 14, "[111111]CategoryCopy", true, this.OnClickCopyCategoryButton);
                
                // PasteCategoryボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goCopyCategory, "PasteCategory"),
                    new Vector3(-5, -11f + CategoryFuncBase, 0f), "[111111]CategoryPaste", true, this.OnClickPasteCategoryButton);

                // FlushCategoryボタン
                generateUnitButton(SetCloneChild(goSystemUnit, goCopyCategory, "FlushCategory"),
                    new Vector3(conWidth * 0.3333f - 4, -11f + CategoryFuncBase, 0f), "[111111]CategoryFlush", true, this.OnClickFlushCategoryButton);




                #endregion

                Debuginfo.Log(LogLabel + " goBoneCategory complete.");

                //メイドのボーン情報の取得
                if (maid != null && maid.Visible == true)
                {

                    getMaidBonetransform();
                    //posHandle = new HandleKun(settingIni.HandleLegacymode, FindAtlas("SystemDialog"), maid);
                    posHandle = new HandleKun(settingIni.ShaderDirectry, FindAtlas("SystemDialog"), maid);

                }
                else
                {
                    //posHandle = new HandleKun(settingIni.HandleLegacymode, FindAtlas("SystemDialog"));
                    posHandle = new HandleKun(settingIni.ShaderDirectry, FindAtlas("SystemDialog"));

                }
                posHandle.setVisible(false);

                Debuginfo.Log(LogLabel + " getMaidBonetransform complete.");

                #region addTableContents


                // BoneParamの設定に従ってボタン・スライダー追加
                for (int i = 0; i < mp.BoneCount; i++)
                {
                    string bone = mp.sBone[i];

                    if (!mp.bVisible[bone]) continue;

                    uiValueLable[bone] = new Dictionary<string, UILabel>();
                    uiSlider[bone] = new Dictionary<string, UISlider>();
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
                    setUnitButtonColor(uiHeaderButton, mp.IsToggle(bone) ? mp.bEnabled[bone] : false);

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

                    // 各ユニット
                    //スライダーなら Handle/Resetボタンとスライダー追加
                    if (mp.IsSlider(bone))
                    {
                        uiSpriteHeaderButton.SetDimensions((int)(conWidth * 0.8f), 40);
                        uiLabelHeader.width = uiSpriteHeaderButton.width - 20;
                        uiHeaderButton.transform.localPosition = new Vector3(-conWidth * 0.1f, 0f, 0f);

                        NGUITools.UpdateWidgetCollider(goHeaderButton);

                        if (bone == "camera" || bone == "eye" || bone == "secret" || bone == "light" || bone == "gravity")
                        {

                        }
                        else if (bone == "Bip01" || bone == "allpos" || bone == "offset")
                        {
                            // Handleボタン
                            generateUnitButton(SetCloneChild(goModUnit, goProfileTabCopy, "RotHandle:" + bone),
                                new Vector3(conWidth * 0.4f + 2, 10.5f, 0f), (int)(conWidth * 0.2f) - 2, 19, 14, "[111111]RotHandle", true, this.OnClickHandleButton);

                            // PosHandleボタン
                            generateUnitButton(SetCloneChild(goModUnit, goProfileTabCopy, "PosHandle:" + bone),
                                new Vector3(conWidth * 0.4f + 2, -10.5f, 0f), (int)(conWidth * 0.2f) - 2, 19, 14, "[111111]PosHandle", true, this.OnClickPosHandleButton);
                        }
                        else
                        {
                            // Handleボタン
                            generateUnitButton(SetCloneChild(goModUnit, goProfileTabCopy, "RotHandle:" + bone),
                                new Vector3(conWidth * 0.4f + 2, 0, 0f), (int)(conWidth * 0.2f) - 2, 40, 22, "[111111]Handle", true, this.OnClickHandleButton);

                        }

                        //各パネルの下にボタン追加

                        float PanelFuncBase = -44f;

                        // Mirrorボタン
                        GameObject goMirror = SetCloneChild(goModUnit, goProfileTabCopy, "Mirror:" + bone);
                        generateUnitButton(goMirror,
                            new Vector3(-conWidth * 0.25f - 1f, 11f + PanelFuncBase, 0f), (int)(conWidth * 0.5f) - 2, 19, 14, "[111111]Mirror", false, this.OnClickMirrorButton);

                        // Resetボタン
                        generateUnitButton(SetCloneChild(goModUnit, goMirror, "Reset:" + bone),
                            new Vector3(conWidth * 0.25f + 1f, 11f + PanelFuncBase, 0f), "[111111]Reset", false, this.OnClickResetButton);
                        
                        // Copyボタン
                        GameObject goCopy = SetCloneChild(goModUnit, goProfileTabCopy, "Copy:" + bone);
                        generateUnitButton(goCopy,
                            new Vector3(-conWidth * 0.3333f -1, -11f + PanelFuncBase, 0f), (int)(conWidth * 0.3333f) - 2, 19, 14,"[111111]Copy", false, this.OnClickCopyButton);

                        // Pasteボタン
                        generateUnitButton(SetCloneChild(goModUnit, goCopy, "Paste:" + bone),
                        　  new Vector3(                     0f, -11f + PanelFuncBase, 0f), "[111111]Paste", false, this.OnClickPasteButton);

                        // Flushボタン
                        generateUnitButton(SetCloneChild(goModUnit, goCopy, "Flush:" + bone),
                            new Vector3( conWidth * 0.3333f +1, -11f + PanelFuncBase, 0f), "[111111]Flush", false, this.OnClickFlushButton);


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
                            goSliderUnit.transform.localPosition = new Vector3(0f, j * -70f - uiSpriteHeaderButton.height - 20f - 40f, 0f);
                            goSliderUnit.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                            // フレームサイズ
                            goSliderUnit.GetComponent<UISprite>().SetDimensions(conWidth, 50);

                            // スライダー設定
                            uiSlider[bone][prop] = FindChild(goSliderUnit, "Slider").GetComponent<UISlider>();
                            uiSlider[bone][prop].name = "Slider:" + bone + ":" + prop;
                            uiSlider[bone][prop].value = codecSliderValue(bone, prop);


                            if (vType == "int") uiSlider[bone][prop].numberOfSteps = (int)(vmax - vmin + 1);
                            EventDelegate.Add(uiSlider[bone][prop].onChange, new EventDelegate.Callback(this.OnChangeSlider));

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

                 //   undoValue[bone] = new Dictionary<string, float>();

                    if (mp.IsSlider(bone))
                    {
                        for (int j = 0; j < mp.ValCount(bone); j++)
                        {
                            string prop = mp.sPropName[bone][j];
                        //    undoValue[bone][prop] = mp.fValue[bone][prop];
                        }
                    }
                }

                foreach (UITable ut in uiTable)
                {
                    ut.Reposition();
                }
                goAMSPanel.SetActive(false);
                
                Debuginfo.Log(LogLabel + " goAMSPanel complete.");

                float[,] _constraitLeftLeg =
                    {
                        {
                            mp.fVmin["Bip01 L Foot"]["Bip01 L Foot.x"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.x"],
                            mp.fVmax["Bip01 L Foot"]["Bip01 L Foot.x"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.x"],
                            mp.fVmin["Bip01 L Foot"]["Bip01 L Foot.y"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.y"],
                            mp.fVmax["Bip01 L Foot"]["Bip01 L Foot.y"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.y"],
                            mp.fVmin["Bip01 L Foot"]["Bip01 L Foot.z"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.z"],
                            mp.fVmax["Bip01 L Foot"]["Bip01 L Foot.z"] + mp.fVzero["Bip01 L Foot"]["Bip01 L Foot.z"]
                        },
                        {
                            mp.fVmin["Bip01 L Calf"]["Bip01 L Calf.x"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.x"],
                            mp.fVmax["Bip01 L Calf"]["Bip01 L Calf.x"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.x"],
                            mp.fVmin["Bip01 L Calf"]["Bip01 L Calf.y"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.y"],
                            mp.fVmax["Bip01 L Calf"]["Bip01 L Calf.y"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.y"],
                            mp.fVmin["Bip01 L Calf"]["Bip01 L Calf.z"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.z"],
                            mp.fVmax["Bip01 L Calf"]["Bip01 L Calf.z"] + mp.fVzero["Bip01 L Calf"]["Bip01 L Calf.z"]
                        },
                        {
                            mp.fVmin["Bip01 L Thigh"]["Bip01 L Thigh.x"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.x"],
                            mp.fVmax["Bip01 L Thigh"]["Bip01 L Thigh.x"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.x"],
                            mp.fVmin["Bip01 L Thigh"]["Bip01 L Thigh.y"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.y"],
                            mp.fVmax["Bip01 L Thigh"]["Bip01 L Thigh.y"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.y"],
                            mp.fVmin["Bip01 L Thigh"]["Bip01 L Thigh.z"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.z"],
                            mp.fVmax["Bip01 L Thigh"]["Bip01 L Thigh.z"] + mp.fVzero["Bip01 L Thigh"]["Bip01 L Thigh.z"]
                        }
                     };

                float[,] _constraitRightLeg =
                    {
                        {
                            mp.fVmin["Bip01 R Foot"]["Bip01 R Foot.x"] + mp.fVzero["Bip01 R Foot"]["Bip01 R Foot.x"],
                            mp.fVmax["Bip01 R Foot"]["Bip01 R Foot.x"] + mp.fVzero["Bip01 R Foot"]["Bip01 R Foot.x"],
                            mp.fVmin["Bip01 R Foot"]["Bip01 R Foot.y"] + mp.fVzero["Bip01 R Foot"]["Bip01 R Foot.y"],
                            mp.fVmax["Bip01 R Foot"]["Bip01 R Foot.y"] + mp.fVzero["Bip01 R Foot"]["Bip01 R Foot.y"],
                            mp.fVmin["Bip01 R Foot"]["Bip01 R Foot.z"] + mp.fVzero["Bip01 R Foot"]["Bip01 R Foot.z"],
                            mp.fVmax["Bip01 R Foot"]["Bip01 R Foot.z"] + mp.fVzero["Bip01 R Foot"]["Bip01 R Foot.z"]
                        },
                        {
                            mp.fVmin["Bip01 R Calf"]["Bip01 R Calf.x"] + mp.fVzero["Bip01 R Calf"]["Bip01 R Calf.x"],
                            mp.fVmax["Bip01 R Calf"]["Bip01 R Calf.x"] + mp.fVzero["Bip01 R Calf"]["Bip01 R Calf.x"],
                            mp.fVmin["Bip01 R Calf"]["Bip01 R Calf.y"] + mp.fVzero["Bip01 R Calf"]["Bip01 R Calf.y"],
                            mp.fVmax["Bip01 R Calf"]["Bip01 R Calf.y"] + mp.fVzero["Bip01 R Calf"]["Bip01 R Calf.y"],
                            mp.fVmin["Bip01 R Calf"]["Bip01 R Calf.z"] + mp.fVzero["Bip01 R Calf"]["Bip01 R Calf.z"],
                            mp.fVmax["Bip01 R Calf"]["Bip01 R Calf.z"] + mp.fVzero["Bip01 R Calf"]["Bip01 R Calf.z"]
                       },
                       {
                            mp.fVmin["Bip01 R Thigh"]["Bip01 R Thigh.x"] + mp.fVzero["Bip01 R Thigh"]["Bip01 R Thigh.x"],
                            mp.fVmax["Bip01 R Thigh"]["Bip01 R Thigh.x"] + mp.fVzero["Bip01 R Thigh"]["Bip01 R Thigh.x"],
                            mp.fVmin["Bip01 R Thigh"]["Bip01 R Thigh.y"] + mp.fVzero["Bip01 R Thigh"]["Bip01 R Thigh.y"],
                            mp.fVmax["Bip01 R Thigh"]["Bip01 R Thigh.y"] + mp.fVzero["Bip01 R Thigh"]["Bip01 R Thigh.y"],
                            mp.fVmin["Bip01 R Thigh"]["Bip01 R Thigh.z"] + mp.fVzero["Bip01 R Thigh"]["Bip01 R Thigh.z"],
                            mp.fVmax["Bip01 R Thigh"]["Bip01 R Thigh.z"] + mp.fVzero["Bip01 R Thigh"]["Bip01 R Thigh.z"]
                       }
                   };

                float[,] _constraitLeftArm =
                    {
                        {
                            mp.fVmin["Bip01 L Hand"]["Bip01 L Hand.x"] + mp.fVzero["Bip01 L Hand"]["Bip01 L Hand.x"],
                            mp.fVmax["Bip01 L Hand"]["Bip01 L Hand.x"] + mp.fVzero["Bip01 L Hand"]["Bip01 L Hand.x"],
                            mp.fVmin["Bip01 L Hand"]["Bip01 L Hand.y"] + mp.fVzero["Bip01 L Hand"]["Bip01 L Hand.y"],
                            mp.fVmax["Bip01 L Hand"]["Bip01 L Hand.y"] + mp.fVzero["Bip01 L Hand"]["Bip01 L Hand.y"],
                            mp.fVmin["Bip01 L Hand"]["Bip01 L Hand.z"] + mp.fVzero["Bip01 L Hand"]["Bip01 L Hand.z"],
                            mp.fVmax["Bip01 L Hand"]["Bip01 L Hand.z"] + mp.fVzero["Bip01 L Hand"]["Bip01 L Hand.z"]
                        },
                        {
                            mp.fVmin["Bip01 L Forearm"]["Bip01 L Forearm.x"] + mp.fVzero["Bip01 L Forearm"]["Bip01 L Forearm.x"],
                            mp.fVmax["Bip01 L Forearm"]["Bip01 L Forearm.x"] + mp.fVzero["Bip01 L Forearm"]["Bip01 L Forearm.x"],
                            mp.fVmin["Bip01 L Forearm"]["Bip01 L Forearm.y"] + mp.fVzero["Bip01 L Forearm"]["Bip01 L Forearm.y"],
                            mp.fVmax["Bip01 L Forearm"]["Bip01 L Forearm.y"] + mp.fVzero["Bip01 L Forearm"]["Bip01 L Forearm.y"],
                            mp.fVmin["Bip01 L Forearm"]["Bip01 L Forearm.z"] + mp.fVzero["Bip01 L Forearm"]["Bip01 L Forearm.z"],
                            mp.fVmax["Bip01 L Forearm"]["Bip01 L Forearm.z"] + mp.fVzero["Bip01 L Forearm"]["Bip01 L Forearm.z"]
                         },
                         {
                            mp.fVmin["Bip01 L UpperArm"]["Bip01 L UpperArm.x"] + mp.fVzero["Bip01 L UpperArm"]["Bip01 L UpperArm.x"],
                            mp.fVmax["Bip01 L UpperArm"]["Bip01 L UpperArm.x"] + mp.fVzero["Bip01 L UpperArm"]["Bip01 L UpperArm.x"],
                            mp.fVmin["Bip01 L UpperArm"]["Bip01 L UpperArm.y"] + mp.fVzero["Bip01 L UpperArm"]["Bip01 L UpperArm.y"],
                            mp.fVmax["Bip01 L UpperArm"]["Bip01 L UpperArm.y"] + mp.fVzero["Bip01 L UpperArm"]["Bip01 L UpperArm.y"],
                            mp.fVmin["Bip01 L UpperArm"]["Bip01 L UpperArm.z"] + mp.fVzero["Bip01 L UpperArm"]["Bip01 L UpperArm.z"],
                            mp.fVmax["Bip01 L UpperArm"]["Bip01 L UpperArm.z"] + mp.fVzero["Bip01 L UpperArm"]["Bip01 L UpperArm.z"]
                         }
                     };

                float[,] _constraitRightArm =
                    {
                        {
                            mp.fVmin["Bip01 R Hand"]["Bip01 R Hand.x"] + mp.fVzero["Bip01 R Hand"]["Bip01 R Hand.x"],
                            mp.fVmax["Bip01 R Hand"]["Bip01 R Hand.x"] + mp.fVzero["Bip01 R Hand"]["Bip01 R Hand.x"],
                            mp.fVmin["Bip01 R Hand"]["Bip01 R Hand.y"] + mp.fVzero["Bip01 R Hand"]["Bip01 R Hand.y"],
                            mp.fVmax["Bip01 R Hand"]["Bip01 R Hand.y"] + mp.fVzero["Bip01 R Hand"]["Bip01 R Hand.y"],
                            mp.fVmin["Bip01 R Hand"]["Bip01 R Hand.z"] + mp.fVzero["Bip01 R Hand"]["Bip01 R Hand.z"],
                            mp.fVmax["Bip01 R Hand"]["Bip01 R Hand.z"] + mp.fVzero["Bip01 R Hand"]["Bip01 R Hand.z"]
                        },
                        {
                            mp.fVmin["Bip01 R Forearm"]["Bip01 R Forearm.x"] + mp.fVzero["Bip01 R Forearm"]["Bip01 R Forearm.x"],
                            mp.fVmax["Bip01 R Forearm"]["Bip01 R Forearm.x"] + mp.fVzero["Bip01 R Forearm"]["Bip01 R Forearm.x"],
                            mp.fVmin["Bip01 R Forearm"]["Bip01 R Forearm.y"] + mp.fVzero["Bip01 R Forearm"]["Bip01 R Forearm.y"],
                            mp.fVmax["Bip01 R Forearm"]["Bip01 R Forearm.y"] + mp.fVzero["Bip01 R Forearm"]["Bip01 R Forearm.y"],
                            mp.fVmin["Bip01 R Forearm"]["Bip01 R Forearm.z"] + mp.fVzero["Bip01 R Forearm"]["Bip01 R Forearm.z"],
                            mp.fVmax["Bip01 R Forearm"]["Bip01 R Forearm.z"] + mp.fVzero["Bip01 R Forearm"]["Bip01 R Forearm.z"]
                        },
                        {
                            mp.fVmin["Bip01 R UpperArm"]["Bip01 R UpperArm.x"] + mp.fVzero["Bip01 R UpperArm"]["Bip01 R UpperArm.x"],
                            mp.fVmax["Bip01 R UpperArm"]["Bip01 R UpperArm.x"] + mp.fVzero["Bip01 R UpperArm"]["Bip01 R UpperArm.x"],
                            mp.fVmin["Bip01 R UpperArm"]["Bip01 R UpperArm.y"] + mp.fVzero["Bip01 R UpperArm"]["Bip01 R UpperArm.y"],
                            mp.fVmax["Bip01 R UpperArm"]["Bip01 R UpperArm.y"] + mp.fVzero["Bip01 R UpperArm"]["Bip01 R UpperArm.y"],
                            mp.fVmin["Bip01 R UpperArm"]["Bip01 R UpperArm.z"] + mp.fVzero["Bip01 R UpperArm"]["Bip01 R UpperArm.z"],
                            mp.fVmax["Bip01 R UpperArm"]["Bip01 R UpperArm.z"] + mp.fVzero["Bip01 R UpperArm"]["Bip01 R UpperArm.z"]
                        }
                    };

                ikManage = new IKManage(_constraitLeftLeg, _constraitRightLeg,_constraitLeftArm,_constraitRightArm,this.GetMaid);

                undoList = new UndoList(settingIni.UndoCount);

                iOtherSliderInit = new Dictionary<string, int>()
                {
                    {"allpos",6 },
                    {"offset",6 },
                    {"eye",4 },
                    {"secret",4 },
                    {"camera",8 },
                    {"light",8 },
                    {"gravity",3 }
                };
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


            ikManage.Destroy();


            ikManage = null;
            posHandle = null;

            uiValueLable.Clear();
            uiSlider.Clear();
        }

        private void setInifile()
        {

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
                settingIni.ShaderDirectry = currentDirectory + @"\UnityInjector\Config\CustomShader";
                if (bExistSybaris)
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

                //CBL版をサポートに加えたのでフォルダ決め打ちしないでここで参照＆設定
                settingIni.OutputAnmDirectory = Directory.GetCurrentDirectory() + @"\PhotoModeData\Mod\Motion";

            }
            else
            {
                //iniファイルがある場合は設定されてない項目を補完

                settingIni = IniFileHelper.Read<SettingIni>("setting", iniFileName);
                if (settingIni.PoseXmlDirectory == "")
                    settingIni.PoseXmlDirectory = currentDirectory + @"\UnityInjector\Config";
                if (settingIni.PoseImgDirectory == "")
                    settingIni.PoseImgDirectory = currentDirectory + @"\UnityInjector\Config\PoseImg";
                if (settingIni.ShaderDirectry == "")
                    settingIni.ShaderDirectry = currentDirectory + @"\UnityInjector\Config\CustomShader";

                settingIni.AnmOutputmode = settingIni.AnmOutputmode.ToLower();
                if (settingIni.AnmOutputmode != "both" && settingIni.AnmOutputmode != "photomode" && settingIni.AnmOutputmode != "sybaris")
                {
                    if (bExistSybaris)
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

                if (settingIni.UndoCount == 0)
                    settingIni.UndoCount = 10;

            }
            IniFileHelper.Write<SettingIni>("setting", settingIni, iniFileName);

        }

        //----

        public int getPhotoModeSceneNo()
        {
            return (bCBLMode ? iScenePhotoCBL : iScenePhoto);
        }

        public int getEditModeSceneNo()
        {
            return (bCBLMode ? iSceneEditCBL : iSceneEdit);
        }

        public void rebootHandle()
        {
            if (FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
            }
            else if (FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 R Foot"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
            }
            else if (FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 L Arm"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
            }
            else if (FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 R Arm"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);
            }
            else
            {
                posHandle.SetMaid(maid,trBone["Bip01"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);
            }
        }

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
                        if (!(b && mp.IsSlider(goBone))) setUnitButtonColor(uiButton, b);

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

        //いなくなったメイドさんのIKアタッチ情報の要素をコレクションリストから削除
        private void removeAttachMaidList(int removeNo)
        {
            ikManage.removeAttachMaidList(removeNo);
        }

        private bool visibleHandle(string bone,bool isPosHandle)
        {
            try
            {
                //GameObject goSelectCursor = FindChildByTag(trBoneUnit[bone], "SelectCursorRotHandle").gameObject;
                GameObject goSelectCursor = isPosHandle? FindChild(UIButton.current.gameObject, "SelectCursorPosHandle:" + bone).gameObject : FindChild(UIButton.current.gameObject, "SelectCursorRotHandle:" + bone).gameObject;
                bool b = goSelectCursor.activeSelf;
                goSelectCursor.SetActive(!b);
                return !b;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "visibleHandle() " + ex); return false; }
        }
        private void imvisibleBothHandle(string bone)
        {
            try
            {
                Transform tr = FindChild(trBoneUnit[bone], "SelectCursorRotHandle:" + bone);
                GameObject goSelectCursorAng = tr.gameObject;
                goSelectCursorAng.SetActive(false);
                if (bone == "Bip01" || bone == "offset" || bone == "allpos")
                {
                    GameObject goSelectCursorPos = FindChild(trBoneUnit[bone], "SelectCursorPosHandle:" + bone).gameObject;
                    goSelectCursorPos.SetActive(false);
                }
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "imvisibleBothHandle() " + ex);  }
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

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera" && bone != "light" && bone != "gravity")
                {
                    vPastBoneAngle[bone] = trBone[bone].localRotation;
                }

                if (bone == "Bip01")
                {
                    vPastBoneTrans = trBone["Bip01"].localPosition;
                }

                bLocked = false;
            }
            catch (Exception ex) { Debug.LogError(LogLabel + "resetSliderValue() " + ex); bLocked = false; return; }
        }

        //IKボタンの表示一括処理

        //IKボタンの挙動について
        //IKが設定されていない場合(0)：灰色、カーソル無し
        //  この状態でクリック：(1)に移行
        //  この状態で他をクリック：変化なし
        //IKが設定されていて現在操作中の場合(1)：白、カーソルあり
        //  この状態でクリック：(2)に移行
        //  この状態で他をクリック：(2)に移行
        //IKが設定されていれ現在操作中でない場合(2)：白、カーソル無し
        //　この状態でクリック：(1)に移行
        //  この状態で他をクリック：変化なし
        //
        //結論 
        //カーソル無しの場合　→ボタン白、カーソル有りにする
        //カーソル有りの場合　→ボタン白、カーソル無しにする

        private void setIKButtonActive(bool _enableCursor, bool _allikdisable = false ,bool _allmaiddisable = false)
        {
            //ハンドルボタンが表示されてたら消す
            if (activeHandleName != "")
            {
                imvisibleBothHandle(activeHandleName);
                setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "RotHandle:" + activeHandleName).GetComponent<UIButton>(), false);
                if(activeHandleName == "Bip01" || activeHandleName == "offset" || activeHandleName == "allpos" )
                {
                    setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "PosHandle:" + activeHandleName).GetComponent<UIButton>(), false);

                }

                activeHandleName = "";
            }
            
            
            UIButton uiButton0 = FindChild(goAMSPanel, "IKDetach").GetComponent<UIButton>();
            UIButton uiButton1 = FindChild(goAMSPanel, "IKDetachAll").GetComponent<UIButton>();

            UIButton uiButton2 = FindChild(goAMSPanel, "IKLeftLeg").GetComponent<UIButton>();
            UIButton uiButton3 = FindChild(goAMSPanel, "IKRightLeg").GetComponent<UIButton>();
            UIButton uiButton4 = FindChild(goAMSPanel, "IKLeftArm").GetComponent<UIButton>();
            UIButton uiButton5 = FindChild(goAMSPanel, "IKRightArm").GetComponent<UIButton>();


            //Color color = UIButton.current.defaultColor;
            Color color = uiButton0.defaultColor;

            if (!_allikdisable)
            { 
                //押されたボタンと解除関係のボタンを白にする。カーソルの状態は問わない
                color.a = 1.0f;

                uiButton0.defaultColor = color;
                uiButton1.defaultColor = color;


                UIButton.current.defaultColor = color;// new Color(color.r, color.g, color.b, _enableColor ? 1f : 0.8f);


            }
            else
            {
                //IK解除のときはIK解除以外のボタンを灰色にする
                //全IK解除とIK解除をどうするかは引数で指定
                color.a = 0.8f;

                uiButton0.defaultColor = new Color(color.r, color.g, color.b, !_enableCursor ? 0.8f : 1.0f);
                uiButton1.defaultColor = new Color(color.r, color.g, color.b, _allmaiddisable ? 0.8f : 1.0f);

                uiButton2.defaultColor = color;
                uiButton3.defaultColor = color;
                uiButton4.defaultColor = color;
                uiButton5.defaultColor = color;
            }

            FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);

            //カーソルは今操作中のIKボタンのものだけ表示
            if (!_allikdisable)
            {
                FindChildByTag(UIButton.current.gameObject, "SelectCursor").SetActive(_enableCursor);
            }


            //ついでにIKボーンアタッチ用オブジェクトも消す
            posHandle.IKTargetVisible = false;
        }

        private void setIKButtonCursorActive(bool _enableCursor)
        {
            //ハンドルボタンが表示されてたら消す
            if (activeHandleName != "")
            {
                imvisibleBothHandle(activeHandleName);
                setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "RotHandle:" + activeHandleName).GetComponent<UIButton>(), false);
                if (activeHandleName == "Bip01" || activeHandleName == "offset" || activeHandleName == "allpos")
                {
                    setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "PosHandle:" + activeHandleName).GetComponent<UIButton>(), false);

                }

                activeHandleName = "";
            }
            
            FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);

            //カーソルは今操作中のIKボタンのものだけ表示

            FindChildByTag(UIButton.current.gameObject, "SelectCursor").SetActive(_enableCursor);



            //ついでにIKボーンアタッチ用オブジェクトも消す
            posHandle.IKTargetVisible = false;
        }

        private void setIKButtonActive(bool _active = false,int No = 99)
        {
            UIButton[] uiButton = { FindChild(goAMSPanel, "IKLeftLeg").GetComponent<UIButton>(),
            FindChild(goAMSPanel, "IKRightLeg").GetComponent<UIButton>(),
            FindChild(goAMSPanel, "IKLeftArm").GetComponent<UIButton>(),
            FindChild(goAMSPanel, "IKRightArm").GetComponent<UIButton>() };

            Color def_Color; 

            if (No <= 3)
            {
                def_Color = uiButton[No].defaultColor;
                uiButton[No].defaultColor = new Color(def_Color.r, def_Color.g, def_Color.b, _active ? 1.0f : 0.8f);
            }
            else
            {
                def_Color = uiButton[0].defaultColor;
                Array.ForEach(uiButton, n => 
                {
                    n.defaultColor = new Color(def_Color.r, def_Color.g, def_Color.b, 0.8f);
                } );
            }


            if(uiButton.All(n => n.defaultColor.a == 0.8f) )
            {
                FindChild(goAMSPanel, "IKDetach").GetComponent<UIButton>().defaultColor = new Color(def_Color.r, def_Color.g, def_Color.b,  0.8f);
                FindChild(goAMSPanel, "IKDetachAll").GetComponent<UIButton>().defaultColor = new Color(def_Color.r, def_Color.g, def_Color.b, (ikManage.attachIKMaidNo.Count != 0) ? 1.0f : 0.8f);
            }


        }

        private bool[] setIKButtonActiveforUndo(bool[] _boolList)
        {

            UIButton uiButton0 = FindChild(goAMSPanel, "IKDetach").GetComponent<UIButton>();
            UIButton uiButton1 = FindChild(goAMSPanel, "IKDetachAll").GetComponent<UIButton>();

            UIButton uiButton2 = FindChild(goAMSPanel, "IKLeftLeg").GetComponent<UIButton>();
            UIButton uiButton3 = FindChild(goAMSPanel, "IKRightLeg").GetComponent<UIButton>();
            UIButton uiButton4 = FindChild(goAMSPanel, "IKLeftArm").GetComponent<UIButton>();
            UIButton uiButton5 = FindChild(goAMSPanel, "IKRightArm").GetComponent<UIButton>();

            bool[] currentBoolList = 
            {
                uiButton2.defaultColor.a == 1.0f,
                uiButton3.defaultColor.a == 1.0f,
                uiButton4.defaultColor.a == 1.0f,
                uiButton5.defaultColor.a == 1.0f
            }; 
            
            Color color = uiButton0.defaultColor;
                    
            uiButton0.defaultColor = new Color(color.r, color.g, color.b, (_boolList[0] || _boolList[1] || _boolList[2] || _boolList[3]) ? 1.0f : 0.8f);
            uiButton1.defaultColor = new Color(color.r, color.g, color.b, (ikManage.attachIKMaidNo.Count != 0) ? 1.0f : 0.8f);

            uiButton2.defaultColor = new Color(color.r, color.g, color.b, _boolList[0] ? 1.0f : 0.8f);
            uiButton3.defaultColor = new Color(color.r, color.g, color.b, _boolList[1] ? 1.0f : 0.8f);
            uiButton4.defaultColor = new Color(color.r, color.g, color.b, _boolList[2] ? 1.0f : 0.8f);
            uiButton5.defaultColor = new Color(color.r, color.g, color.b, _boolList[3] ? 1.0f : 0.8f);


            FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);

            return currentBoolList;

        }

        private bool[] getIKButtonActiveforUndo()
        {
            UIButton uiButton2 = FindChild(goAMSPanel, "IKLeftLeg").GetComponent<UIButton>();
            UIButton uiButton3 = FindChild(goAMSPanel, "IKRightLeg").GetComponent<UIButton>();
            UIButton uiButton4 = FindChild(goAMSPanel, "IKLeftArm").GetComponent<UIButton>();
            UIButton uiButton5 = FindChild(goAMSPanel, "IKRightArm").GetComponent<UIButton>();

            bool[] currentBoolList =
            {
                uiButton2.defaultColor.a == 1.0f,
                uiButton3.defaultColor.a == 1.0f,
                uiButton4.defaultColor.a == 1.0f,
                uiButton5.defaultColor.a == 1.0f
            };

            return currentBoolList;
        }

        //現在のメイドのIKアタッチ情報に合わせてIKボタンの状態を再設定
        private void resetIKButtonActive()
        {

            UIButton uiButton0 = FindChild(goAMSPanel, "IKDetach").GetComponent<UIButton>();
            UIButton uiButton1 = FindChild(goAMSPanel, "IKDetachAll").GetComponent<UIButton>();

            UIButton uiButton2 = FindChild(goAMSPanel, "IKLeftLeg").GetComponent<UIButton>();
            UIButton uiButton3 = FindChild(goAMSPanel, "IKRightLeg").GetComponent<UIButton>();
            UIButton uiButton4 = FindChild(goAMSPanel, "IKLeftArm").GetComponent<UIButton>();
            UIButton uiButton5 = FindChild(goAMSPanel, "IKRightArm").GetComponent<UIButton>();

            Color colorActive = uiButton0.defaultColor;
            colorActive.a = 1.0f;
            Color colorOff = new Color(colorActive.r, colorActive.g, colorActive.b, 0.8f);


            //IKアタッチ状態を調べてそれに合わせてボタンの色を変更

            bool b2 = ikManage.checkIKAttach(currentMaidNo, 1);//(bIKAttachLeftLeg.ContainsKey(currentMaidNo) && bIKAttachLeftLeg[currentMaidNo]);
            bool b3 = ikManage.checkIKAttach(currentMaidNo, 2);//(bIKAttachRightLeg.ContainsKey(currentMaidNo) && bIKAttachRightLeg[currentMaidNo]);
            bool b4 = ikManage.checkIKAttach(currentMaidNo, 3);//(bIKAttachLeftArm.ContainsKey(currentMaidNo) && bIKAttachLeftArm[currentMaidNo]);
            bool b5 = ikManage.checkIKAttach(currentMaidNo, 4);//(bIKAttachRightArm.ContainsKey(currentMaidNo) && bIKAttachRightArm[currentMaidNo]);


            uiButton2.defaultColor =  b2 ? colorActive : colorOff;
            uiButton3.defaultColor =  b3 ? colorActive : colorOff;
            uiButton4.defaultColor =  b4 ? colorActive : colorOff;
            uiButton5.defaultColor =  b5 ? colorActive : colorOff;

            uiButton0.defaultColor = ( b2||b3||b4||b5) ? colorActive : colorOff;
            uiButton1.defaultColor = (ikManage.attachIKMaidNoCount() != 0)? colorActive : colorOff;


            

            //カーソル全非表示
            FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursorIKLeftLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursorIKRightLeg").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursorIKLeftArm").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursorIKRightArm").SetActive(false);


            //ついでにIKボーンアタッチ用オブジェクトも消す
            posHandle.IKTargetVisible = false;
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
                if (type == "SliderUnit" || type == "Spacer" || type == "Copy" || type == "Paste" || 
                    type == "Mirror" || type == "Reset" || type == "Flush")
                    tc.gameObject.SetActive(b);
            }

            foreach (UITable ut in uiTable)
            {
                ut.repositionNow = true;
            }
        }

        //汎用ボタン色変え
        private void setButtonColor(UIButton button, bool _enableColor)
        {
            Color color = button.defaultColor;

            button.defaultColor = new Color(color.r, color.g, color.b, _enableColor ? 1f : 0.8f);

        }

        private void setUnitButtonColor(string bone, bool b)
        {
            setUnitButtonColor(FindChild(trBoneUnit[bone], "Header:" + bone).GetComponent<UIButton>(), b);
        }
        private void setUnitButtonColor(UIButton button, bool b)
        {
            Color color = button.defaultColor;

            if (mp.IsToggle(getTag(button, 1)))
            {
                button.defaultColor = new Color(color.r, color.g, color.b, b ? 1f : 0.5f);
                FindChildByTag(button.gameObject, "SelectCursor").SetActive(b);
            }
            else
            {
                button.defaultColor = new Color(color.r, color.g, color.b, b ? 1f : 0.75f);
            }
        }

        private void generateUnitButton(GameObject goButton,Vector3 _localPosition,int dimensionW,int dimensionH,int fontsize,string _text,bool _active, Action _callback)
        {
            // 違うサイズのボタン作成（正確にはコピー作成）
            
            goButton.transform.localPosition = _localPosition;
            goButton.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

            UISprite uiSpriteHandle = goButton.GetComponent<UISprite>();
            uiSpriteHandle.SetDimensions(dimensionW, dimensionH);

            UILabel uiLabel = FindChild(goButton, "Name").GetComponent<UILabel>();
            uiLabel.width = uiSpriteHandle.width - 10;
            uiLabel.fontSize = fontsize;
            uiLabel.spacingX = 0;
            uiLabel.supportEncoding = true;
            uiLabel.text = _text;

            UIButton uiButton = goButton.GetComponent<UIButton>();
            uiButton.defaultColor = new Color(1f, 1f, 1f, 0.8f);

            EventDelegate.Set(uiButton.onClick, new EventDelegate.Callback(_callback));
            FindChild(goButton, "SelectCursor").GetComponent<UISprite>().SetDimensions(dimensionW - 4, dimensionH - 4);
            FindChild(goButton, "SelectCursor").SetActive(false);
            FindChild(goButton, "SelectCursor").name = "SelectCursor" + goButton.name;
            NGUITools.UpdateWidgetCollider(goButton);
            goButton.SetActive(_active);
        }
        private void generateUnitButton(GameObject goButton, Vector3 _localPosition, string _text, bool _active, Action _callback)
        {
            // 同じサイズのボタン作成（正確にはコピー作成）

            goButton.transform.localPosition = _localPosition;

            UILabel uiLabel = FindChild(goButton, "Name").GetComponent<UILabel>();
            uiLabel.text = _text;

            UIButton uiButton = goButton.GetComponent<UIButton>();
            uiButton.defaultColor = new Color(1f, 1f, 1f, 0.8f);

            EventDelegate.Set(uiButton.onClick, new EventDelegate.Callback(_callback));

            FindChildByTag(goButton, "SelectCursor").name = "SelectCursor" + goButton.name;

            NGUITools.UpdateWidgetCollider(goButton);
            goButton.SetActive(_active);
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
            if (add == 0)
            {
                Maid maid = GetMaid(startNo);
                if (maid.Visible)
                    return startNo;
                else
                    return -1;
            }

            add /= Math.Abs(add);


            int MaidCount = GetMaidCount();
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

                Maid maid = GetMaid(maidNo);
                if (maid != null && maid.Visible)
                {
                    return maidNo;
                }

                maidNo += add;
            }
            return -1;
            

        }

        //公式撮影と複数撮影でメイドさんやメイドさんの数の取得方法が違うので
        //ここで差異を吸収する
        private Maid GetMaid(int _No)
        {
            if(sceneLevel == getPhotoModeSceneNo())
                return GameMain.Instance.CharacterMgr.GetMaid(_No);
            else
                return GameMain.Instance.CharacterMgr.GetStockMaid(_No);
        }

        private int GetMaidCount()
        {
            if(sceneLevel == getPhotoModeSceneNo())
                return GameMain.Instance.CharacterMgr.GetMaidCount();
            else
                return GameMain.Instance.CharacterMgr.GetStockMaidCount();
        }

        private void currentMaidChange()
        {

            maid = GetMaid(currentMaidNo);

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

                
                syncSlider(true,true);


                if (activeHandleName != "")
                {
                    imvisibleBothHandle(activeHandleName);
                    setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "RotHandle:" + activeHandleName).GetComponent<UIButton>(), false);

                    if(activeHandleName == "Bip01"|| activeHandleName == "offset"|| activeHandleName == "allpos")
                        setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "PosHandle:" + activeHandleName).GetComponent<UIButton>(), false);
                    activeHandleName = "";
                }

                //いったんIKボタンをオフらせる
                //setIKButtonActive(false, true, attachIKMaidNo.Count == 0);
                //現在のメイドのIKアタッチ状態を調べてボタンの色を変える
                posHandle.IKTargetAttachedColor(false);
                resetIKButtonActive();
            }
            else
            {
                Debuginfo.Log(LogLabel + "currentMaidChange() exception");
            }

        }

        private void settrTargetIKBones()
        {
            if (!ikManage.trTargetIKBoneContainsNo(currentMaidNo))
            {
                Transform[] tempTransformList =
                {
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_handL", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_handR", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_footL", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_footR", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_hohoL", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_hohoR", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_muneL", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_muneR", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_hara", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_hipL", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_hipR", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_anal", true),
                        CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_vagina", true),
                    };

                ikManage.trTargetIKBoneAdd(currentMaidNo, tempTransformList);
            }

            int stockNo = FindVisibleMaidStockNo(this.currentMaidNo + 1, 1);
            while (stockNo != this.currentMaidNo)
            {
                if(!ikManage.trTargetIKBoneContainsNo(stockNo))
                {
                    Maid tempmaid = GetMaid(stockNo);
                    //Debuginfo.Log("IKBones");
                    Transform[] tempTransformList = 
                    {
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_handL", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_handR", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_footL", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_footR", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_hohoL", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_hohoR", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_muneL", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_muneR", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_hipL", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_hipR", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_anal", true),
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "_IK_vagina", true),
                    };

                    ikManage.trTargetIKBoneAdd(stockNo, tempTransformList);
                }
                                
                stockNo = FindVisibleMaidStockNo(stockNo + 1, 1);
            }
        }

        //
        public Quaternion[] getCurrentIKBone(int _maidNo, int No)
        {
            if(_maidNo == currentMaidNo)
            {
                switch (No)
                {
                    case 0:
                        Quaternion[] tempTransformList0 = { trBone["Bip01 L Thigh"].localRotation, trBone["Bip01 L Calf"].localRotation, trBone["Bip01 L Foot"].localRotation };
                        return tempTransformList0;
                    case 1:
                        Quaternion[] tempTransformList1 = { trBone["Bip01 R Thigh"].localRotation, trBone["Bip01 R Calf"].localRotation, trBone["Bip01 R Foot"].localRotation };
                        return tempTransformList1;
                    case 2:
                        Quaternion[] tempTransformList2 = { trBone["Bip01 L UpperArm"].localRotation, trBone["Bip01 L Forearm"].localRotation, trBone["Bip01 L Hand"].localRotation };
                        return tempTransformList2;
                    case 3:
                        Quaternion[] tempTransformList3 = { trBone["Bip01 R UpperArm"].localRotation, trBone["Bip01 R Forearm"].localRotation, trBone["Bip01 R Hand"].localRotation };
                        return tempTransformList3;
                    default:
                        return null;
                }
            }
            else
            {
                Maid tempmaid = GetMaid(_maidNo);
                if (tempmaid == null || tempmaid.Visible != true)
                    return null;

                switch (No)
                {
                    case 0:
                        Quaternion[] tempTransformList0 =
                        {
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Thigh", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Calf", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Foot", true).localRotation
                        };
                        return tempTransformList0;
                    case 1:
                        Quaternion[] tempTransformList1 =
                        {
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Thigh", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Calf", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Foot", true).localRotation
                        };
                        return tempTransformList1;
                    case 2:
                        Quaternion[] tempTransformList2 =
                        {
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L UpperArm", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Forearm", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Hand", true).localRotation
                        };
                        return tempTransformList2;
                    case 3:
                        Quaternion[] tempTransformList3 =
                        {
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R UpperArm", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Forearm", true).localRotation,
                            CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Hand", true).localRotation
                        };
                        return tempTransformList3;
                    default:
                        return null;
                }
            }
        }

        public void setCurrentIKBone(int _maidNo, int No,Quaternion[] _bones)
        {
            if (_maidNo == currentMaidNo)
            {
                switch (No)
                {
                    case 0:
                        trBone["Bip01 L Thigh"].localRotation = _bones[0];
                        trBone["Bip01 L Calf"].localRotation = _bones[1];
                        trBone["Bip01 L Foot"].localRotation = _bones[2];
                        SyncTransform2SliderLabel("Bip01 L Thigh");
                        SyncTransform2SliderLabel("Bip01 L Calf");
                        SyncTransform2SliderLabel("Bip01 L Foot");
                        break;
                    case 1:
                        trBone["Bip01 R Thigh"].localRotation = _bones[0];
                        trBone["Bip01 R Calf"].localRotation = _bones[1];
                        trBone["Bip01 R Foot"].localRotation = _bones[2];
                        SyncTransform2SliderLabel("Bip01 R Thigh");
                        SyncTransform2SliderLabel("Bip01 R Calf");
                        SyncTransform2SliderLabel("Bip01 R Foot");
                        break;
                    case 2:
                        trBone["Bip01 L UpperArm"].localRotation = _bones[0];
                        trBone["Bip01 L Forearm"].localRotation = _bones[1];
                        trBone["Bip01 L Hand"].localRotation = _bones[2];
                        SyncTransform2SliderLabel("Bip01 L UpperArm");
                        SyncTransform2SliderLabel("Bip01 L Forearm");
                        SyncTransform2SliderLabel("Bip01 L Hand");
                        break;
                    case 3:
                        trBone["Bip01 R UpperArm"].localRotation = _bones[0];
                        trBone["Bip01 R Forearm"].localRotation = _bones[1];
                        trBone["Bip01 R Hand"].localRotation = _bones[2];
                        SyncTransform2SliderLabel("Bip01 R UpperArm");
                        SyncTransform2SliderLabel("Bip01 R Forearm");
                        SyncTransform2SliderLabel("Bip01 R Hand");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Maid tempmaid = GetMaid(_maidNo);
                if (tempmaid == null || tempmaid.Visible != true)
                    return;
                switch (No)
                {
                    case 0:
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Thigh", true).localRotation = _bones[0];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Calf", true).localRotation = _bones[1];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Foot", true).localRotation = _bones[2];
                        break;
                    case 1:
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Thigh", true).localRotation = _bones[0];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Calf", true).localRotation = _bones[1];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Foot", true).localRotation = _bones[2];
                        break;
                    case 2:
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L UpperArm", true).localRotation = _bones[0];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Forearm", true).localRotation = _bones[1];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 L Hand", true).localRotation = _bones[2];
                        break;
                    case 3:

                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R UpperArm", true).localRotation = _bones[0];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Forearm", true).localRotation = _bones[1];
                        CMT.SearchObjName(tempmaid.body0.m_Bones.transform, "Bip01 R Hand", true).localRotation = _bones[2];
                        break;
                    default:
                        break;
                }
            }
        }


        //この関数は複数撮影対策なのでメイドさん取得関数はそのままでおｋ
        private void setParentAllOffset()
        {

            if (maid.transform.parent != GameMain.Instance.CharacterMgr.GetMaid(0).transform.parent)
            {
                Debuginfo.Log(LogLabel + "stockNo:" + currentMaidNo + " parent change");
                maid.transform.parent = GameMain.Instance.CharacterMgr.GetMaid(0).transform.parent;
            }

            int stockNo = FindVisibleMaidStockNo(this.currentMaidNo + 1, 1);
            while(stockNo != this.currentMaidNo)
            { 
                Maid tempmaid = GameMain.Instance.CharacterMgr.GetMaid(stockNo);
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
                    if (bone == "secret" || bone == "eye" || bone == "allpos" || bone == "offset" || bone == "camera" || bone == "light" || bone == "gravity") continue;


                    trBone[bone] = CMT.SearchObjName(maid.body0.m_Bones.transform, bone, true);

                    if (trBone[bone] == null) Debug.LogError(LogLabel + ":" + bone + "is null! ");
                    vPastBoneAngle[bone] = trBone[bone].localRotation;

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

        private void checkUndo(string _Func,string _bone, string _prop = "")
        {
            //二重Undo登録対策
            if (bUndoLock)
                return;

            if (_bone.Contains("Bip") || _bone.Contains("_IK_"))
            {
                if (_prop.Contains(".p"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_pos:" + _bone + ":" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_pos:" + _bone + ":" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::"+UndofuncName);
                        
                        //Undo履歴に加える
                        //undoList.Add(new UndoBonePos(trBone[_bone], trBone[_bone].localPosition));
                        undoList.Add(Undo.createUndoBonePos(trBone[_bone], trBone[_bone].localPosition));
                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_rot:" + _bone + ":" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_rot:" + _bone + ":" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                        

                        //Undo履歴に加える
                        //undoList.Add(new UndoBone(trBone[_bone], trBone[_bone].localRotation));
                        undoList.Add(Undo.createUndoBone(trBone[_bone], trBone[_bone].localRotation));
                    }
                }
            }
            else if (_bone == "allpos")
            {
                if (_prop.Contains(".p"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_pos:Allpos:" + _prop)
                    {
                        UndofuncName = _Func + "_pos:Allpos:" + _prop;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        //undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllPos(), true));
                        undoList.Add(Undo.createUndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllPos(), true));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_rot:Allpos:" + _prop)
                    {
                        UndofuncName = _Func + "_rot:Allpos:" + _prop;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        //undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllRot(), false));
                        undoList.Add(Undo.createUndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllRot(), false));

                    }
                }
            }
            else if (_bone == "offset")
            {
                if (_prop.Contains(".p"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_pos:offset:" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_pos:offset:" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        //undoList.Add(new UndoOffset(maid, maid.transform.localPosition, true));
                        undoList.Add(Undo.createUndoOffset(maid, maid.transform.localPosition, true));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_rot:offset:" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_rot:offset:" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        //undoList.Add(new UndoOffset(maid, maid.GetRot(), false));
                        undoList.Add(Undo.createUndoOffset(maid, maid.GetRot(), false));

                    }
                }
            }
            else if (_bone == "eye")
            {
                if (_prop.Contains(".l"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_eye:" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_eye:" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        //undoList.Add(new UndoEye(maid, maid.body0.quaDefEyeL, false));
                        undoList.Add(Undo.createUndoEye(maid, maid.body0.quaDefEyeL, false));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_eye:" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_eye:" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        //undoList.Add(new UndoEye(maid, maid.body0.quaDefEyeR, true));
                        undoList.Add(Undo.createUndoEye(maid, maid.body0.quaDefEyeR, true));

                    }
                }
            }
            else if (_bone == "secret")
            {
                if (_prop.Contains(".ml"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_secret:" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_secret:" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        jiggleBone jbMuneL = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true).gameObject.GetComponent<jiggleBone>();
                        //undoList.Add(new UndoSecret(jbMuneL, jbMuneL.MuneUpDown, jbMuneL.MuneYori));
                        undoList.Add(Undo.createUndoSecret(jbMuneL, jbMuneL.MuneUpDown, jbMuneL.MuneYori));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_secret:" + _prop + ":" + maid.name)
                    {
                        UndofuncName = _Func + "_secret:" + _prop + ":" + maid.name;
                        //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                        //Undo履歴に加える
                        jiggleBone jbMuneR = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true).gameObject.GetComponent<jiggleBone>();
                        //undoList.Add(new UndoSecret(jbMuneR, jbMuneR.MuneUpDown, jbMuneR.MuneYori));
                        undoList.Add(Undo.createUndoSecret(jbMuneR, jbMuneR.MuneUpDown, jbMuneR.MuneYori));

                    }
                }
            }
            else if (_bone == "light")
            {

            }
            else if (_bone == "gravity")
            {

            }
        }

        private void mirrorBone(string bone)
        {

            if (bone.Contains("L") || bone.Contains("R"))
            {
                string mir_bone = bone.Contains("L") ? bone.Replace("L", "R") : bone.Replace("R", "L");

                //クォータニオンで反転するとボーンごとに反転する軸が違ってきて
                //わけわかんなくなるからオイラー角で反転する

                float mir_x = -mp.fValue[bone][bone + ".x"];
                mp.fValue[bone][bone + ".x"] = -mp.fValue[mir_bone][mir_bone + ".x"];
                mp.fValue[mir_bone][mir_bone + ".x"] = mir_x;

                float mir_y = -mp.fValue[bone][bone + ".y"];
                mp.fValue[bone][bone + ".y"] = -mp.fValue[mir_bone][mir_bone + ".y"];
                mp.fValue[mir_bone][mir_bone + ".y"] = mir_y;

                float mir_z = mp.fValue[bone][bone + ".z"];
                mp.fValue[bone][bone + ".z"] = mp.fValue[mir_bone][mir_bone + ".z"];
                mp.fValue[mir_bone][mir_bone + ".z"] = mir_z;

                rotateBone(bone);
                rotateBone(mir_bone);

                SyncTransform2SliderLabel(bone);
                SyncTransform2SliderLabel(mir_bone);

            }
            else
            {

                if (bone == "Bip01")
                {
                    mp.fValue[bone][bone + ".z"] = -mp.fValue[bone][bone + ".z"];
                    mp.fValue[bone][bone + ".x"] = -mp.fValue[bone][bone + ".x"];
                }
                else
                {
                    if ( bone == "Bip01 Spine" || bone == "Bip01 Pelvis")
                    {
                        mp.fValue[bone][bone + ".z"] = -mp.fValue[bone][bone + ".z"];
                    }
                    else
                    {
                        mp.fValue[bone][bone + ".x"] = -mp.fValue[bone][bone + ".x"];
                    }

                    mp.fValue[bone][bone + ".y"] = -mp.fValue[bone][bone + ".y"];
                }
                rotateBone(bone);
                SyncTransform2SliderLabel(bone);

            }
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
            else if (bone == "light")
            {
                GameMain.Instance.MainLight.SetRotation(new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"], mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"], mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]));

                GameMain.Instance.MainLight.SetColor(new Color(mp.fValue[bone][bone + ".r"] + mp.fVzero[bone][bone + ".r"], mp.fValue[bone][bone + ".g"] + mp.fVzero[bone][bone + ".g"], mp.fValue[bone][bone + ".b"] + mp.fVzero[bone][bone + ".b"]));

                GameMain.Instance.MainLight.SetIntensity(mp.fValue[bone][bone + ".int"] + mp.fVzero[bone][bone + ".int"]);

                GameMain.Instance.MainLight.SetShadowStrength(mp.fValue[bone][bone + ".ss"] + mp.fVzero[bone][bone + ".ss"]);

            }
            else if (bone == "gravity")
            {

                Vector3 baseG = new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"], mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"], mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]).normalized * -0.01f;

                maid.body0.goSlot.ForEach(tbs =>
                {
                    if (tbs.bonehair.boSkirt)
                    {
                        var field = tbs.bonehair.GetType().GetField("SkirtList", BindingFlags.Instance | BindingFlags.NonPublic);
                        var list = (field.GetValue(tbs.bonehair)) as THair1[];
                        Array.ForEach(list, m => m.SoftG = baseG);
                        field.SetValue(tbs.bonehair, list);
                    }

                    {
                        var field = tbs.bonehair.GetType().GetField("hair1list", BindingFlags.Instance | BindingFlags.NonPublic);
                        var list = (field.GetValue(tbs.bonehair)) as List<THair1>;
                        list.ForEach(m => m.SoftG = 0.3f * baseG);
                        field.SetValue(tbs.bonehair, list);

                    }

                });

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
        //IK処理後の値を本体とスライダーに反映
        private void syncFromIK()
        {
            if(ikManage.checkIKAttach(currentMaidNo,0))
            {
                SyncTransform2SliderLabel("Bip01 L Thigh");
                SyncTransform2SliderLabel("Bip01 L Calf");
                SyncTransform2SliderLabel("Bip01 L Foot");
            }
            if (ikManage.checkIKAttach(currentMaidNo, 1))
            {
                SyncTransform2SliderLabel("Bip01 R Thigh");
                SyncTransform2SliderLabel("Bip01 R Calf");
                SyncTransform2SliderLabel("Bip01 R Foot");
            }
            if (ikManage.checkIKAttach(currentMaidNo, 2))
            {
                SyncTransform2SliderLabel("Bip01 L UpperArm");
                SyncTransform2SliderLabel("Bip01 L Forearm");
                SyncTransform2SliderLabel("Bip01 L Hand");
            }
            if (ikManage.checkIKAttach(currentMaidNo, 3))
            {
                SyncTransform2SliderLabel("Bip01 R UpperArm");
                SyncTransform2SliderLabel("Bip01 R Forearm");
                SyncTransform2SliderLabel("Bip01 R Hand");
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

                if (posHandle.bHandlePositionMode == false)
                {
                    if (bone == "AllOffset")
                    {

                        //Undoチェック
                        if (UndofuncName != "handle_rot:allpos")
                        {
                            UndofuncName = "handle_rot:allpos";
                            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                            //Undo履歴に加える
                            //undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllRot(), false));
                            undoList.Add(Undo.createUndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllRot(), false));

                        }

                        //まずいかもしれないけどAllOffsetを直に回す
                        //まずかったから変更
                        Vector3 tmpAllRot = GameMain.Instance.CharacterMgr.GetCharaAllRot();
                        Quaternion tempAllQua = Quaternion.Euler(tmpAllRot.x, tmpAllRot.y, tmpAllRot.z);

                        tempAllQua *= posHandle.DeltaQuaternion();

                        tmpAllRot = tempAllQua.eulerAngles;
                        GameMain.Instance.CharacterMgr.SetCharaAllRot(tmpAllRot);
                        //回転結果をプラグイン側の数値に反映


                        tmpAllRot = GameMain.Instance.CharacterMgr.GetCharaAllRot();


                        mp.fValue["allpos"]["allpos" + ".x"] = tmpAllRot.x - mp.fVzero["allpos"]["allpos" + ".x"];
                        mp.fValue["allpos"]["allpos" + ".y"] = tmpAllRot.y - mp.fVzero["allpos"]["allpos" + ".y"];
                        mp.fValue["allpos"]["allpos" + ".z"] = tmpAllRot.z - mp.fVzero["allpos"]["allpos" + ".z"];

                        foreach (Transform tr in trBoneUnit["allpos"])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);
                                
                                slider.value = codecSliderValue("allpos", prop);
                                

                                uiValueLable["allpos"][prop].text = mp.fValue["allpos"][prop].ToString("F4");
                                uiValueLable["allpos"][prop].gameObject.GetComponent<UIInput>().value = mp.fValue["allpos"][prop].ToString("F4");

                            }
                        }
                       

                    }
                    else if (bone.Contains("Maid"))
                    {

                        //Undoチェック
                        if (UndofuncName != "handle_rot:offset" + ":" + maid.name)
                        {
                            UndofuncName = "handle_rot:offset" + ":" + maid.name;
                            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);


                            //Undo履歴に加える
                            //undoList.Add(new UndoOffset(maid, maid.GetRot(), false));
                            undoList.Add(Undo.createUndoOffset(maid, maid.GetRot(), false));

                        }

                        //まずいかもしれないけどメイドさんを直に回す
                        maid.transform.rotation *= posHandle.DeltaQuaternion();
                        //回転結果をプラグイン側の数値に反映

                        Vector3 tmpMaidRot = maid.GetRot();

                        mp.fValue["offset"]["offset" + ".x"] = tmpMaidRot.x - mp.fVzero["offset"]["offset" + ".x"];
                        mp.fValue["offset"]["offset" + ".y"] = tmpMaidRot.y - mp.fVzero["offset"]["offset" + ".y"];
                        mp.fValue["offset"]["offset" + ".z"] = tmpMaidRot.z - mp.fVzero["offset"]["offset" + ".z"];

                        foreach (Transform tr in trBoneUnit["offset"])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);
                                slider.value = codecSliderValue("offset", prop);
                                uiValueLable["offset"][prop].text = mp.fValue["offset"][prop].ToString("F4");
                                uiValueLable["offset"][prop].gameObject.GetComponent<UIInput>().value = mp.fValue["offset"][prop].ToString("F4");

                            }
                        }
                        
                    }
                    else
                    {
                        


                        //Undoチェック
                        if (UndofuncName != "handle_rot:" + bone + ":" + maid.name)
                        {
                            UndofuncName = "handle_rot:" + bone + ":" + maid.name;
                            //Debuginfo.Log(LogLabel + "Undo::" +UndofuncName);
                            

                            //Undo履歴に加える
                            //undoList.Add( new UndoBone(trBone[bone],trBone[bone].localRotation));
                            undoList.Add(Undo.createUndoBone(trBone[bone], trBone[bone].localRotation));

                        }


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
                            SyncTransform2SliderLabel(bone);
                        }
                        else
                        {
                            //超えてたらボーン回転取り消し
                            trBone[bone].localRotation = vPastBoneAngle[bone];
                            //vPastBoneAngle[bone] = trBone[bone].localEulerAngles;
                            if (vPastBoneAngle[bone] != trBone[bone].localRotation)
                            {
                                Debuginfo.Log("okasii");
                            }

                            mp.fValue[bone][bone + ".x"] = past_x;
                            mp.fValue[bone][bone + ".y"] = past_y;
                            mp.fValue[bone][bone + ".z"] = past_z;
                        }

                    
                    }
                }
                else
                {
                    if (bone == "AllOffset")
                    {
                        //Undoチェック
                        if (UndofuncName != "handle_pos:allpos")
                        {
                            UndofuncName = "handle_pos:allpos";
                            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                            

                            //Undo履歴に加える
                            //undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAlltPos(), true));
                            undoList.Add(Undo.createUndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllPos(), true));

                        }


                        //まずいかもしれないけどAllOffsetを直に動かす
                        //まずかったので変更

                        //移動結果をプラグイン側の数値に反映
                        Vector3 tmpAllPos = GameMain.Instance.CharacterMgr.GetCharaAllPos();
                        tmpAllPos　+= posHandle.DeltaVector();
                        GameMain.Instance.CharacterMgr.SetCharaAllPos(tmpAllPos);

                        mp.fValue["allpos"]["allpos" + ".px"] = tmpAllPos.x - mp.fVzero["allpos"]["allpos" + ".px"];
                        mp.fValue["allpos"]["allpos" + ".py"] = tmpAllPos.y - mp.fVzero["allpos"]["allpos" + ".py"];
                        mp.fValue["allpos"]["allpos" + ".pz"] = tmpAllPos.z - mp.fVzero["allpos"]["allpos" + ".pz"];

                        foreach (Transform tr in trBoneUnit["allpos"])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);
                                slider.value = codecSliderValue("allpos", prop);
                                uiValueLable["allpos"][prop].text = mp.fValue["allpos"][prop].ToString("F4");
                                uiValueLable["allpos"][prop].gameObject.GetComponent<UIInput>().value = mp.fValue["allpos"][prop].ToString("F4");

                            }
                        }
                        

                    }
                    else if (bone.Contains("Maid"))
                    {
                        //Undoチェック
                        if (UndofuncName != "handle_pos:offset" + ":" + maid.name)
                        {
                            UndofuncName = "handle_pos:offset" + ":" + maid.name;
                            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                            

                            //Undo履歴に加える
                            //undoList.Add(new UndoOffset(maid,maid.transform.localPosition,true) );
                            undoList.Add(Undo.createUndoOffset(maid, maid.transform.localPosition, true));

                        }


                        //まずいかもしれないけどメイドさんを直に動かす
                        maid.transform.position += posHandle.DeltaVector();
                        //移動結果をプラグイン側の数値に反映

                        Vector3 tmpMaidPos = maid.transform.localPosition;

                        mp.fValue["offset"]["offset" + ".px"] = tmpMaidPos.x - mp.fVzero["offset"]["offset" + ".px"];
                        mp.fValue["offset"]["offset" + ".py"] = tmpMaidPos.y - mp.fVzero["offset"]["offset" + ".py"];
                        mp.fValue["offset"]["offset" + ".pz"] = tmpMaidPos.z - mp.fVzero["offset"]["offset" + ".pz"];

                        foreach (Transform tr in trBoneUnit["offset"])
                        {
                            if (tr.name == "SliderUnit")
                            {
                                UISlider slider = FindChildByTag(tr, "Slider").GetComponent<UISlider>();
                                string prop = getTag(slider, 2);
                                slider.value = codecSliderValue("offset", prop);
                                uiValueLable["offset"][prop].text = mp.fValue["offset"][prop].ToString("F4");
                                uiValueLable["offset"][prop].gameObject.GetComponent<UIInput>().value = mp.fValue["offset"][prop].ToString("F4");

                            }
                        }
                        
                    }
                    else if (bone == "Bip01")
                    {
                        //Undoチェック
                        if (UndofuncName != "handle_pos:" + bone + ":" + maid.name)
                        {
                            UndofuncName = "handle_pos:" + bone + ":" + maid.name;
                            //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);
                            

                            //Undo履歴に加える
                            //undoList.Add(new UndoBonePos(trBone[bone], trBone[bone].localPosition));
                            undoList.Add(Undo.createUndoBonePos(trBone[bone], trBone[bone].localPosition));

                        }


                        float past_px = mp.fValue[bone][bone + ".px"];
                        float past_py = mp.fValue[bone][bone + ".py"];
                        float past_pz = mp.fValue[bone][bone + ".pz"];
                        //先にボーンを移動させておく

                        //Debuginfo.Log(posHandle.DeltaVector().ToString());

                        trBone[bone].position += posHandle.DeltaVector();

                        Vector3 tmpPosition = trBone[bone].localPosition;

                        mp.fValue[bone][bone + ".px"] = tmpPosition.x - mp.fVzero[bone][bone + ".px"];
                        mp.fValue[bone][bone + ".py"] = tmpPosition.y - mp.fVzero[bone][bone + ".py"];
                        mp.fValue[bone][bone + ".pz"] = tmpPosition.z - mp.fVzero[bone][bone + ".pz"];

                        //スライダー限界値を超えてないかのチェック
                        bool reTrans = false;

                        if (mp.fValue[bone][bone + ".px"] > mp.fVmax[bone][bone + ".px"])
                        {
                            reTrans = true;
                        }
                        if (mp.fValue[bone][bone + ".py"] > mp.fVmax[bone][bone + ".py"])
                        {
                            reTrans = true;
                        }

                        if (mp.fValue[bone][bone + ".pz"] > mp.fVmax[bone][bone + ".pz"])
                        {
                            reTrans = true;
                        }
                        if (mp.fValue[bone][bone + ".px"] < mp.fVmin[bone][bone + ".px"])
                        {
                            reTrans = true;
                        }
                        if (mp.fValue[bone][bone + ".py"] < mp.fVmin[bone][bone + ".py"])
                        {
                            reTrans = true;
                        }
                        if (mp.fValue[bone][bone + ".pz"] < mp.fVmin[bone][bone + ".pz"])
                        {
                            reTrans = true;
                        }

                        if (!reTrans)
                        {
                            //限界値をこえてなかったらスライダーと入力ラベルに反映
                            SyncTransform2SliderLabel(bone);
                        }
                        else
                        {
                            //超えてたらボーン移動取り消し
                            trBone[bone].localPosition = vPastBoneTrans;

                            mp.fValue[bone][bone + ".px"] = past_px;
                            mp.fValue[bone][bone + ".py"] = past_py;
                            mp.fValue[bone][bone + ".pz"] = past_pz;
                        }
                    }
                    else
                    {
                        Debug.Log(LogLabel + "Bone position drag exception.");
                    }
                }
                bLocked = false;
            }
            
        }

        //本体側の数値をスライダーとプラグイン側の数値に反映
        private void syncSlider(bool allSlider,bool _noUndo = false)
        {
            bLocked = true;

            //前回の操作がUndoかRedoであれば履歴に記録しない
            bool notUndo = true; 
            if(UndofuncName == "Undo" || UndofuncName == "Redo" || _noUndo == true)
            {
                notUndo = false;
            }

            foreach (string bone in mp.sBone)
            {
                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera" && bone != "light" && bone != "gravity")
                {
                    if (bone == "Bip01")
                    {
                        if (vPastBoneAngle[bone] != trBone[bone].localRotation || vPastBoneTrans != trBone[bone].localPosition || allSlider)
                        {
                            
                            //Undoチェック
                            if ((UndofuncName != "syncAllBone:" + maid.name) && notUndo )
                            {
                                UndofuncName = "syncAllBone:" + maid.name;
                                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                                //Undo履歴に加える
                                //undoList.Add(new UndoBoneAll(trBone,vPastBoneAngle, vPastBoneTrans));
                                undoList.Add(Undo.createUndoBoneAll(trBone, vPastBoneAngle, vPastBoneTrans));

                            }

                            Vector3 tmpPosition = trBone[bone].localPosition;

                            mp.fValue[bone][bone + ".px"] = tmpPosition.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpPosition.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpPosition.z - mp.fVzero[bone][bone + ".pz"];

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
                            vPastBoneAngle[bone] = trBone[bone].localRotation;
                            vPastBoneTrans = trBone[bone].localPosition;
                        }
                    }
                    else
                    {
                        if (vPastBoneAngle[bone] != trBone[bone].localRotation || allSlider)
                        {
                            
                            //Undoチェック
                            if ((UndofuncName != "syncAllBone:" + maid.name) && notUndo)
                            {
                                UndofuncName = "syncAllBone:" + maid.name;
                                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                                //Undo履歴に加える
                                //undoList.Add(new UndoBoneAll(trBone, vPastBoneAngle, vPastBoneTrans));
                                undoList.Add(Undo.createUndoBoneAll(trBone, vPastBoneAngle, vPastBoneTrans));

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
                            vPastBoneAngle[bone] = trBone[bone].localRotation;

                        }
                    }
                    
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
                            tmpAllPos.z != mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]
                            )
                        {
                            //Undoチェック
                            if (UndofuncName != "sync:Allpos:Pos" && notUndo)
                            {
                                UndofuncName = "sync:Allpos:Pos";
                                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                                //Undo履歴に加える
                                Vector3 oldPos = new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"],
                                    mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"],
                                    mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]);
                                //undoList.Add(new UndoAllpos(oldPos, true));
                                undoList.Add(Undo.createUndoAllpos(oldPos, true));

                            }

                            mp.fValue[bone][bone + ".px"] = tmpAllPos.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpAllPos.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpAllPos.z - mp.fVzero[bone][bone + ".pz"];

                            b_changed = true;
                        }
                        if (
                             System.Math.Floor(mp.fValue[bone][bone + ".x"] * 1000) != System.Math.Floor((tmpAllRot.x - mp.fVzero[bone][bone + ".x"])*1000) ||
                             System.Math.Floor(mp.fValue[bone][bone + ".y"] * 1000) != System.Math.Floor((tmpAllRot.y - mp.fVzero[bone][bone + ".y"])*1000) ||
                             System.Math.Floor(mp.fValue[bone][bone + ".z"] * 1000) != System.Math.Floor((tmpAllRot.z - mp.fVzero[bone][bone + ".z"])*1000)
                            )
                        {
                            //Undoチェック
                            if (UndofuncName != "sync:Allpos:Rot" && notUndo)
                            {
                                UndofuncName = "sync:Allpos:Rot" ;
                                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);

                                Vector3 oldRot = new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"],
                                    mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"],
                                    mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]);
                                //Undo履歴に加える
                                //undoList.Add(new UndoAllpos(oldRot, false));
                                undoList.Add(Undo.createUndoAllpos(oldRot, false));

                            }

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
                            tmpMaidPos.z != mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"] 
                            )
                        {
                            //Undoチェック
                            if (UndofuncName != "sync:Offset:Pos:" + maid.name && notUndo)
                            {
                                UndofuncName = "sync:Offset:Pos:" + maid.name;
                                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);


                                Vector3 oldPos = new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"],
                                    mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"],
                                    mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]);
                                //Undo履歴に加える
                                //undoList.Add(new UndoOffset(maid, oldPos, true));
                                undoList.Add(Undo.createUndoOffset(maid, oldPos, true));

                            }

                            mp.fValue[bone][bone + ".px"] = tmpMaidPos.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpMaidPos.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpMaidPos.z - mp.fVzero[bone][bone + ".pz"];

                            b_changed = true;
                        }

                        if (
                            tmpMaidRot.x != mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"] ||
                            tmpMaidRot.y != mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"] ||
                            tmpMaidRot.z != mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]
                            )
                        {
                            //Undoチェック
                            if (UndofuncName != "sync:Offset:Rot:" + maid.name && notUndo)
                            {
                                UndofuncName = "sync:Offset:Rot:" + maid.name;
                                //Debuginfo.Log(LogLabel + "Undo::" + UndofuncName);


                                Vector3 oldRot = new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"],
                                    mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"],
                                    mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]);
                                //Undo履歴に加える
                                //undoList.Add(new UndoOffset(maid, oldRot, false));
                                undoList.Add(Undo.createUndoOffset(maid, oldRot, false));

                            }


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
                    else if (bone == "light")
                    {

                        Quaternion LightRot = GameMain.Instance.MainLight.gameObject.transform.rotation;
                        Color LightColor = GameMain.Instance.MainLight.GetColor();
                        float Intensity = GameMain.Instance.MainLight.GetIntensity();
                        float ShadowStrength = GameMain.Instance.MainLight.GetShadowStrength();


                        bool f1 = LightRot != Quaternion.Euler(new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"], mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"], mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]));
                        bool f2 = LightColor.r != mp.fValue[bone][bone + ".r"] + mp.fVzero[bone][bone + ".r"];
                        bool f3 = LightColor.g != mp.fValue[bone][bone + ".g"] + mp.fVzero[bone][bone + ".g"];
                        bool f4 = LightColor.b != mp.fValue[bone][bone + ".b"] + mp.fVzero[bone][bone + ".b"];

                        bool f5 = Intensity != mp.fValue[bone][bone + ".int"] + mp.fVzero[bone][bone + ".int"];
                        bool f6 = ShadowStrength != mp.fValue[bone][bone + ".ss"] + mp.fVzero[bone][bone + ".ss"];

                        if (f1 || f2 || f3 || f4 || f5 || f6)
                        {
                            float tempx = LightRot.eulerAngles.x - mp.fVzero[bone][bone + ".x"];
                            float tempy = LightRot.eulerAngles.y - mp.fVzero[bone][bone + ".y"];
                            float tempz = LightRot.eulerAngles.z - mp.fVzero[bone][bone + ".z"];

                            //クォータニオンからオイラーに変換したときに値がずれることがあるため
                            //値が動かないZ軸の値を調べてずれを直す
                            if (LightRot.eulerAngles.z > 180)
                            {
                                tempz -= 180;
                                tempy -= 180;
                                tempx = 180 - tempx;
                            }
                            if (tempy < 0 )
                            {
                                tempy += 360;
                            }
                                

                            mp.fValue[bone][bone + ".x"] = tempx;
                            mp.fValue[bone][bone + ".y"] = tempy;
                            mp.fValue[bone][bone + ".z"] = tempz;


                            GameMain.Instance.MainLight.SetRotation(new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"], mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"], mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]));


                            mp.fValue[bone][bone + ".r"] = LightColor.r - mp.fVzero[bone][bone + ".r"];
                            mp.fValue[bone][bone + ".g"] = LightColor.g - mp.fVzero[bone][bone + ".g"];
                            mp.fValue[bone][bone + ".b"] = LightColor.b - mp.fVzero[bone][bone + ".b"];

                            mp.fValue[bone][bone + ".int"] = Intensity - mp.fVzero[bone][bone + ".int"];
                            mp.fValue[bone][bone + ".ss"] = ShadowStrength - mp.fVzero[bone][bone + ".ss"];
                            
                            b_changed = true;
                        }

                    }
                    else if (bone == "gravity")
                    {
                        /* クッソ重そうなので重力のフィードバックはしない
                        THair1 getG = null;
                        maid.body0.goSlot.ForEach(tbs =>
                        {
                           if (tbs.bonehair.boSkirt)
                           {
                               var field = tbs.bonehair.GetType().GetField("SkirtList", BindingFlags.Instance | BindingFlags.NonPublic);
                               var list = (field.GetValue(tbs.bonehair)) as THair1[];
                               getG =  Array.Find(list, m =>
                               m.SoftG.x != mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"] ||
                               m.SoftG.y != mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"] ||
                               m.SoftG.x != mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]
                               );
                           }
                           else
                           {
                               var field = tbs.bonehair.GetType().GetField("hair1list", BindingFlags.Instance | BindingFlags.NonPublic);
                               var list = (field.GetValue(tbs.bonehair)) as List<THair1>;
                               getG =  list.Find(m =>
                               m.SoftG.x != 0.3f * (mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"]) ||
                               m.SoftG.y != 0.3f * (mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"]) ||
                               m.SoftG.x != 0.3f * (mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"])
                               );

                           }
                        }); 
                        if(getG != null)
                        {
                            mp.fValue[bone][bone + ".x"] = getG.SoftG.x - mp.fVzero[bone][bone + ".x"];
                            mp.fValue[bone][bone + ".y"] = getG.SoftG.y - mp.fVzero[bone][bone + ".y"];
                            mp.fValue[bone][bone + ".z"] = getG.SoftG.z - mp.fVzero[bone][bone + ".z"];

                            b_changed = true;
                        }
                        */


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
                            //Debuginfo.Log(LogLabel + "Eye Undo");

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
                            //Debuginfo.Log(LogLabel + "Secret Undo");

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

            if (UndofuncName == "Undo" || UndofuncName == "Redo")
            {
                UndofuncName = "";
            }

            bLocked = false;

        }
        void SyncTransform2SliderLabel(string bone)
        {
            //スライダーと入力ラベルに反映
            /*
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
            */
            
            foreach(var prop in uiSlider[bone])
            {
                prop.Value.value = codecSliderValue(bone, prop.Key);
                uiValueLable[bone][prop.Key].text = mp.fValue[bone][prop.Key].ToString("F4");
                uiValueLable[bone][prop.Key].gameObject.GetComponent<UIInput>().value = mp.fValue[bone][prop.Key].ToString("F4");

            }
            
            vPastBoneAngle[bone] = trBone[bone].localRotation;
            if(bone == "Bip01")
                vPastBoneTrans = trBone[bone].localPosition;


            
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

                float qx = trBone[bone].localRotation.x;
                float qy = trBone[bone].localRotation.y;
                float qz = trBone[bone].localRotation.z;
                float qw = trBone[bone].localRotation.w;

                //float m10 = 2 * (qx * qy - qw * qz);
                //if (m10 > 1.0f) m10 = 1.0f;
                //if (m10 < -1.0f) m10 = -1.0f;

                float m02 = 2 * (qx * qz - qw * qy);
                if (m02 > 1.0f) m02 = 1.0f;
                if (m02 < -1.0f) m02 = -1.0f;

                float pastX = mp.fValue[bone][bone + ".x"];
                float pastY = mp.fValue[bone][bone + ".y"];
                float pastZ = mp.fValue[bone][bone + ".z"];

                mp.fValue[bone][bone + ".y"] = Mathf.Asin(-m02) * Mathf.Rad2Deg;

                if (System.Math.Floor(Mathf.Cos(mp.fValue[bone][bone + ".y"] * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
                {
                    float m00 = 1 - 2 * (qy * qy + qz * qz);
                    float m01 = 2 * (qx * qy + qw * qz);
                    float m12 = 2 * (qy * qz + qw * qx);
                    float m22 = 1 - 2 * (qx * qx + qy * qy);

                    //float m11 = 1 - 2 * (qx * qx + qz * qz);                    
                    //float m20 = 2 * (qx * qz + qw * qy);
                    

                    if (m00 > 1.0f) m00 = 1.0f;
                    if (m00 < -1.0f) m00 = -1.0f;
                    if (m01 > 1.0f) m01 = 1.0f;
                    if (m01 < -1.0f) m01 = -1.0f;

                    if (m12 > 1.0f) m12 = 1.0f;
                    if (m12 < -1.0f) m12 = -1.0f;
                    if (m22 > 1.0f) m22 = 1.0f;
                    if (m22 < -1.0f) m22 = -1.0f;

                    //if (m20 > 1.0f) m20 = 1.0f;
                    //if (m20 < -1.0f) m20 = -1.0f;
                    //if (m11 > 1.0f) m11 = 1.0f;
                    //if (m11 < -1.0f) m11 = -1.0f;


                    mp.fValue[bone][bone + ".z"] = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg;
                    float before = m12 / Mathf.Cos(mp.fValue[bone][bone + ".y"] * Mathf.Deg2Rad);
                    if (before > 1.0f) before = 1.0f;
                    if (before < -1.0f) before = -1.0f;
                    mp.fValue[bone][bone + ".x"] = Mathf.Asin(before) * Mathf.Rad2Deg;
                    if (m22 < 0)
                    {
                        mp.fValue[bone][bone + ".x"] = 180 - mp.fValue[bone][bone + ".x"];
                    }
                }
                if (System.Math.Floor(Mathf.Cos(mp.fValue[bone][bone + ".y"] * Mathf.Deg2Rad) * 10000) / 10000 == 0f || Double.IsNaN(mp.fValue[bone][bone + ".x"]))
                {
                    //float m21 = 2 * (qy * qz - qw * qx);
                    //float m22 = 1 - 2 * (qx * qx + qy * qy);

                    //if (m21 > 1.0f) m21 = 1.0f;
                    //if (m21 < -1.0f) m21 = -1.0f;
                    //if (m22 > 1.0f) m22 = 1.0f;
                    //if (m22 < -1.0f) m22 = -1.0f;

                    float m10 = 2 * (qx * qy - qw * qz);
                    float m11 = 1 - 2 * (qx * qx + qz * qz);

                    if (m10 > 1.0f) m10 = 1.0f;
                    if (m10 < -1.0f) m10 = -1.0f;
                    if (m11 > 1.0f) m11 = 1.0f;
                    if (m11 < -1.0f) m11 = -1.0f;

                    mp.fValue[bone][bone + ".x"] = 0f;
                    mp.fValue[bone][bone + ".z"] = Mathf.Atan2(-m10, m11) * Mathf.Rad2Deg ;
                }


                mp.fValue[bone][bone + ".z"] -= mp.fVzero[bone][bone + ".z"];
                mp.fValue[bone][bone + ".x"] -= mp.fVzero[bone][bone + ".x"];
                mp.fValue[bone][bone + ".y"] -= mp.fVzero[bone][bone + ".y"];


                if ((mp.fValue[bone][bone + ".x"] - pastX) > (mp.fValue[bone][bone + ".x"] - 180f - pastX) &&(mp.fValue[bone][bone + ".y"] - pastY) > (180f - mp.fValue[bone][bone + ".y"] - 2 * mp.fVzero[bone][bone + ".y"] - pastY) && (mp.fValue[bone][bone + ".z"] - pastZ) > (mp.fValue[bone][bone + ".z"] - 180f - pastZ))
                {
                    mp.fValue[bone][bone + ".x"] -= 180f;
                    mp.fValue[bone][bone + ".y"] = 180f - mp.fValue[bone][bone + ".y"] - 2 * mp.fVzero[bone][bone + ".y"];
                    mp.fValue[bone][bone + ".z"] -= 180f; 
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
                    //mp.fValue[bone][bone + ".x"] = (pastX + mp.fVzero[bone][bone + ".x"] > 0) ? 90f : -90f;//asin(m12)
                    mp.fValue[bone][bone + ".z"] = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg ;

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

        //ハンドルの値をIKターゲットオブジェクトに反映
        /*private void inversekinematicFromHandle(HandleKun.IKMODE _ikmode)
        {
            if (posHandle.controllDragged())
            {

                bLocked = true;

                maid.body0.m_Bones.animation.Stop();
                maid.body0.boHeadToCam = false;
                maid.body0.boEyeToCam = false;
                
                this.inversekinematicHandle(posHandle,maid,currentMaidNo,_ikmode);
                

                bLocked = false;
            }
        }
       
        
        private bool inversekinematicFromHandle(Transform _ikParent,bool ikInitted, HandleKun.IKMODE _ikmode)
        {
            //右クリック時はIKをボーンにアタッチ
            //
         
            if (posHandle.controllDragged())
            {
                
                bLocked = true;

                maid.body0.m_Bones.animation.Stop();
                maid.body0.boHeadToCam = false;
                maid.body0.boEyeToCam = false;

                
                //IK開始時は設定の初期化
                if (!ikInitted)
                {
                    ikInit(_ikParent);
                }
                //ハンドル君から値取得
                _ikParent.position += posHandle.DeltaVector();

                

                bLocked = false;

                return true;
            }
            else
            {
                return false;
            }
        }
        */

        //IK設定の初期化
        private void ikInit(Transform _ikParent)
        {
            Vector3 prePosition = posHandle.Pos;
            //まず、ハンドルの親をIKターゲットオブジェクトに変更
            posHandle.SetParentBone(_ikParent);
            //IKターゲットオブジェクトの位置を親変更前のハンドルの位置の値に設定
            _ikParent.position = prePosition;


            Debuginfo.Log(_ikParent.position.ToString());

        }

        //IKターゲットオブジェクトクリック時の動作
        private void ikTargetClicked()
        {
            if (posHandle.IKTargetClicked())// && trTargetIKTemp != null)
            {

                //Debuginfo.Log(LogLabel + "IKTarget:" + trTargetIKTemp.name.ToString());

                bLocked = true;

                maid.body0.m_Bones.animation.Stop();
                maid.body0.boHeadToCam = false;
                maid.body0.boEyeToCam = false;

                ikManage.ikTargetClicked(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), posHandle,currentMaidNo,UndofuncName);
                
                posHandle.IKTargetClickAfter();

                bLocked = false;

            }
        }

        //IKのボーンアタッチ解除用
        //IK関係のコレクションリスト自体は要素を削除せずそのまま
        private void detachIKfromBone()
        {
            ikManage.detachIKfromBone(undoList.Add, this.setIKButtonActiveforUndo, getIKButtonActiveforUndo(), posHandle,trBone,currentMaidNo, UndofuncName);
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


                            tempMaid = GetMaid(stockNo);

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

                maid.SetProp(MPN.HeadX, 100);
                maid.SetProp(MPN.HeadY, 100);

                maid.SetProp(MPN.DouPer, 20);
                maid.SetProp(MPN.sintyou, 20);

                maid.SetProp(MPN.MuneL, 50);
                maid.SetProp(MPN.MuneTare, 0);
                maid.SetProp(MPN.MuneUpDown, 0);
                maid.SetProp(MPN.MuneYori, 0);

                maid.SetProp(MPN.west, 50);
                maid.SetProp(MPN.Hara, 20);
                maid.SetProp(MPN.kata, 0);
                maid.SetProp(MPN.UdeScl, 50);
                maid.SetProp(MPN.ArmL, 20);
                maid.SetProp(MPN.KubiScl, 20);

                maid.SetProp(MPN.koshi, 50);
                maid.SetProp(MPN.RegMeet, 30);
                maid.SetProp(MPN.RegFat, 30);
                maid.AllProcProp();

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
                if ((sceneLevel == getEditModeSceneNo()) && maid != GameMain.Instance.CharacterMgr.GetMaid(0))//bVisivle.Count > 0)
                {
                    foreach (TBodySkin goSlotID in maid.body0.goSlot)
                    {
                        //複数メイドの2人目はアイテムスロットIDが一致しないらしいので
                        //中のカテゴリ名を調べて消すかどうか判断する
                        if (slotList.Contains(goSlotID.Category))
                        {
                            existGoSlot.Add(goSlotID);
                            visibleList.Add(goSlotID.boVisible);

                            if (goSlotID.boVisible)
                            {
                                goSlotID.boVisible = false;
                                goSlotID.Update();
                                //Debuginfo.Log(goSlotID.Category);

                            }
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
                        //Debuginfo.Log(maid.body0.goSlot[(int)TBody.hashSlotName[slotID]].Category);
                    }
                    
                }
                maid.body0.FixVisibleFlag(false);
                

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

                
                if ((sceneLevel == getEditModeSceneNo()) && maid != GameMain.Instance.CharacterMgr.GetMaid(0))//bVisivle.Count > 0)
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
                maid.body0.FixVisibleFlag(false);


                //ここで消してたメイドを元に戻す

                foreach (int MaidNo in visMaidNo)
                {
                    Maid tempMaid;

                    tempMaid = GetMaid(MaidNo);
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
                if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["hohol"]] == 1)
                {
                    strFace2 += "３涙";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["hoho"]] == 1)
                {
                    strFace2 += "２涙";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["hohos"]] == 1)
                {
                    strFace2 += "１涙";
                }
                else
                {
                    strFace2 += "０涙";
                }

                if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["tear3"]] == 1)
                {
                    strFace2 += "３";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["tear2"]] == 1)
                {
                    strFace2 += "２";
                }
                else if (maid.body0.Face.morph.BlendValues[(int)maid.body0.Face.morph.hash["tear1"]] == 1)
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

