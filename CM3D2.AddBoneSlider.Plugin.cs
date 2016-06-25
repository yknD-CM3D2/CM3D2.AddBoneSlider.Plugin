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
<<<<<<< HEAD
    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86"), PluginFilter("CM3D2OHx64"),PluginFilter("CM3D2OHx86")]
    [PluginName("CM3D2 AddBoneSlider"), PluginVersion("0.0.1.4")]
=======
    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86"), PluginFilter("CM3D2OHx64"), PluginFilter("CM3D2OHx86")]
    [PluginName("CM3D2 AddBoneSlider"), PluginVersion("0.0.0.1")]
>>>>>>> develop05

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
    //todo0.0.1.5-02
    //Fキー以外での起動
    //iniの内容
    public class SettingIni
    {
        public string ToggleKey = "f10";
        public string AnmOutputmode = "";
        public int WindowPositionX = -480;
        public int WindowPositionY = 40;
        public string PoseXmlDirectory = "";//Directory.GetCurrentDirectory() + @"\UnityInjector\Config";
        public string PoseImgDirectory = "";//Directory.GetCurrentDirectory() + @"\UnityInjector\Config\PoseImg";
        public string OutputAnmDirectory = "";
        public string OutputJsonDirectory = "";
        public string OutputAnmSybarisDirectory = "";
        public int DebugLogLevel = 0;
        public int HandleLegacymode = 0;
    }

<<<<<<< HEAD
    public class IKManage
=======
    //キー入力でキー名がややこしいやつの対策
    public static class FlexKeycode
    {
        static Dictionary<String, KeyCode> dicKey = new Dictionary<string, KeyCode>()
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

    public class AddBoneSlider : UnityInjector.PluginBase
>>>>>>> develop05
    {
        private readonly string LogLabel = AddBoneSlider.PluginName + " : ";
        private readonly float clickCheckOffsetInit = 40f;

        private class IKPropList
        {
            private readonly string LogLabel = AddBoneSlider.PluginName + " : ";

<<<<<<< HEAD
            //本体側にIKが使えないのでこちらで用意
            Dictionary<int, IKCONSTRAINED> IK = new Dictionary<int, IKCONSTRAINED>();
            //IK脚腕のアタッチ状態判別用
            Dictionary<int, bool> bIKAttach = new Dictionary<int, bool>();
            //IKターゲット用
            public Dictionary<int, GameObject> goIKTarget = new Dictionary<int, GameObject>();
            //IK対象ボーンのtransform
            public Dictionary<int, Transform[]> trIKBones = new Dictionary<int, Transform[]>();
=======
        public const string PluginName = "AddBoneSlider";
        public const string Version = "0.0.1.4pre";

        private readonly int iSceneEdit = 5; //メイン版エディットモード
        private readonly int iScenePhoto = 27; //メイン版公式撮影モード
        private readonly int iSceneEditCBL = 4; //CBL版エディットモード
        private readonly int iScenePhotoCBL= 21; //CBL版公式撮影モード

>>>>>>> develop05

            private float[,] constrait;

            public IKPropList(float[,] _constrait)
            {
                constrait = _constrait;
            }

            public void initList(Transform Bip01,Transform[] boneList, Maid maid,int currentMaidNo)
            {
                if (!IK.ContainsKey(currentMaidNo))
                {
                    //Debuginfo.Log("init IKLeftLeg");
                    IKCONSTRAINED ikTempLeftLeg = new IKCONSTRAINED();
                    /*
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
                    */
                    ikTempLeftLeg.Init(boneList, maid.body0, constrait);
                    IK.Add(currentMaidNo, ikTempLeftLeg);
                }


                //IK対象ボーンリストが設定されていなければ初期化
                if (!trIKBones.ContainsKey(currentMaidNo))
                {
                    //Debuginfo.Log("init trIKLeftLegBones");
                    //Transform[] boneList = { trBone["Bip01 L Thigh"], trBone["Bip01 L Calf"], trBone["Bip01 L Foot"] };
                    trIKBones.Add(currentMaidNo, boneList);
                }

                //IKアタッチ状態が設定されていなければ一時表示[None]で初期化設定
                if (!bIKAttach.ContainsKey(currentMaidNo))
                {
                    //Debuginfo.Log("init bIKAttachLeftLeg");
                    bIKAttach.Add(currentMaidNo, false);
                }

                //IKターゲットが生成されてなければ生成
                if (!goIKTarget.ContainsKey(currentMaidNo))
                {
                    //Debuginfo.Log("init goIKLeftLegTarget");
                    GameObject tempIKLeftLegTarget = new GameObject();
                    tempIKLeftLegTarget.transform.parent = Bip01;
                    goIKTarget.Add(currentMaidNo, tempIKLeftLegTarget);
                    //念のため
                    bIKAttach[currentMaidNo] = false;
                }
            }

            public void Destroy()
            {
                IK.Clear();
                bIKAttach.Clear();
                goIKTarget.Clear();
                trIKBones.Clear();
            }

            //IK設定の初期化
            public void ikInit(HandleKun posHandle, int currentMaidNo)
            {
                Vector3 prePosition = posHandle.Pos;
                //まず、ハンドルの親をIKターゲットオブジェクトに変更
                posHandle.SetParentBone(goIKTarget[currentMaidNo].transform);
                //IKターゲットオブジェクトの位置を親変更前のハンドルの位置の値に設定
                goIKTarget[currentMaidNo].transform.position = prePosition;


                Debuginfo.Log(goIKTarget[currentMaidNo].transform.position.ToString());
                
            }

            public void inversekinematicHandle(HandleKun posHandle, int currentMaidNo)
            {

                //IK開始時は設定の初期化
                if (!bIKAttach[currentMaidNo])
                {
                    ikInit(posHandle, currentMaidNo);
                }
                //ハンドル君から値取得
                goIKTarget[currentMaidNo].transform.position += posHandle.DeltaVector();

                bIKAttach[currentMaidNo] = true;
            }

            public void lateupdateFunc(int m,Maid maid,bool isArm ,bool isLeft)
            {
                if (bIKAttach.ContainsKey(m) && bIKAttach[m])
                {

                    //公式撮影でアタッチ対象のメイドさんがいなくなった場合
                    if (goIKTarget[m] == null)
                    {
                        Debuginfo.Log(LogLabel + "IK is null!");

                        GameObject tempIKTarget = new GameObject();
                        tempIKTarget.transform.parent = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01", true);
                        goIKTarget[m] = tempIKTarget;
                        goIKTarget[m].transform.position = trIKBones[m][2].position;

                        //if(trTargetIKBones.ContainsKey(m))
                        //   trTargetIKBones.Remove(m);
                        
                    }
                    else if (goIKTarget[m].activeInHierarchy == false)
                    {
                        //複数撮影でアタッチ対象のメイドさんがいなくなった場合
                        Debuginfo.Log(LogLabel + "IK is invisible!");

                        goIKTarget[m].transform.parent = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01", true);
                        goIKTarget[m].transform.position = trIKBones[m][2].position;
                        
                    }

                    if (isArm)
                    {
                        if (isLeft && (maid.body0.tgtHandL != null || maid.body0.tgtHandL_AttachName != string.Empty))
                            return;
                        else if(maid.body0.tgtHandR != null || maid.body0.tgtHandR_AttachName != string.Empty)
                            return;
                    }

                    IK[m].Proc(trIKBones[m][0], trIKBones[m][1], trIKBones[m][2], goIKTarget[m].transform.position);


                    //for (int i = 0; i < IKCalc; ++i)
                    //    IKLeftLeg[m].Porc(trIKLeftLegBones[m][0], trIKLeftLegBones[m][1], trIKLeftLegBones[m][2], goIKLeftLegTarget[m].transform.position, Vector3.zero);
                }
            }

            public void removeAttachMaidList(int removeNo)
            {
                IK.Remove(removeNo);
                bIKAttach.Remove(removeNo);

                //ターゲット用オブジェクトを1秒後に消す
                if (goIKTarget.ContainsKey(removeNo))
                {
                    if (goIKTarget[removeNo] != null)
                    {
                        goIKTarget[removeNo].transform.DetachChildren();

                        GameObject.Destroy(goIKTarget[removeNo], 1f);
                    }
                }
                goIKTarget.Remove(removeNo);
                trIKBones.Remove(removeNo);
            }

            public void ikTargetClicked(HandleKun posHandle, int currentMaidNo, Transform trTargetIKTemp)
            {
                ikInit(posHandle,currentMaidNo);

                goIKTarget[currentMaidNo].transform.parent = trTargetIKTemp;
                if (trTargetIKTemp.name != "Bip01")
                    goIKTarget[currentMaidNo].transform.localPosition = Vector3.zero;


                bIKAttach[currentMaidNo] = true;
            }

            public void detachIKfromBone(Transform parent, int currentMaidNo)
            {
                goIKTarget[currentMaidNo].transform.parent = parent;
                bIKAttach[currentMaidNo] = false;
            }

            public bool checkIKAttach(int currentMaidNo)
            {
                return (bIKAttach.ContainsKey(currentMaidNo) && bIKAttach[currentMaidNo]);
            }

            public void detachIK(Transform Bip01, int currentMaidNo)
            {
                if (bIKAttach.ContainsKey(currentMaidNo))
                {
                    bIKAttach[currentMaidNo] = false;
                    goIKTarget[currentMaidNo].transform.parent = Bip01;
                }
            }

            public void detachAll(int removeNo)
            {
                if (goIKTarget.ContainsKey(removeNo))
                {
                    if (goIKTarget[removeNo] != null)
                    {
                        goIKTarget[removeNo].transform.DetachChildren();
                        GameObject.Destroy(goIKTarget[removeNo], 0.5f);
                    }
                }
                
                IK.Clear();
                bIKAttach.Clear();
                goIKTarget.Clear();
                trIKBones.Clear();
            }

            public bool checkParentName(int currentMaidNo)
            {
                return goIKTarget.ContainsKey(currentMaidNo) && (goIKTarget[currentMaidNo].transform.parent.name != "Bip01");
            }
        }

        private IKPropList IKListLeftLeg;//= new IKPropList();
        private IKPropList IKListRightLeg; //= new IKPropList();
        private IKPropList IKListLeftArm;//= new IKPropList();
        private IKPropList IKListRightArm;//= new IKPropList();

        //アタッチ状態が付与されたメイドリスト
        public Dictionary<int, Maid> attachIKMaidList = new Dictionary<int, Maid>();
        public List<int> attachIKMaidNo = new List<int>();
        public HashSet<int> DeleteNoList = new HashSet<int>();


        //IKをアタッチする_IK_ボーンのtransform
        //public Dictionary<int, Transform[]> trTargetIKBones = new Dictionary<int, Transform[]>();
        public Dictionary<int, Transform[]> trTargetIKBones = new Dictionary<int, Transform[]>();
        public Transform trTargetIKTemp = null;
        public Dictionary<string, string> sIKBoneName = new Dictionary<string, string>()
        {
            {"_IK_handL","左手" },
            {"_IK_handR","右手" },
            {"_IK_footL","左足" },
            {"_IK_footR","右足" },
            {"_IK_hohoL","左頬" },
            {"_IK_hohoR","右頬" },
            {"_IK_muneL","左胸" },
            {"_IK_muneR","右胸" },
            {"_IK_hara","お腹" },
            {"_IK_hipL","左尻" },
            {"_IK_hipR","右尻" },
            {"_IK_anal","後穴" },
            {"_IK_vagina","前穴" },
            {"Bip01","解除" }
        };

        bool bIKTargetGet = false;

        public IKManage(float[,]_constraitLeftLeg, float[,] _constraitRightLeg, float[,] _constraitLeftArm, float[,] _constraitRightArm)
        {
            IKListLeftLeg = new IKPropList(_constraitLeftLeg);
            IKListRightLeg = new IKPropList(_constraitRightLeg);
            IKListLeftArm = new IKPropList(_constraitLeftArm);
            IKListRightArm = new IKPropList(_constraitRightArm);
    }

        public void initList(Transform Bip01, Transform[] boneList, Maid maid,int currentMaidNo,int No)
        {
            switch(No)
            {
                case 1:
                    IKListLeftLeg.initList(Bip01, boneList, maid, currentMaidNo);
                    break;
                case 2:
                    IKListRightLeg.initList(Bip01, boneList, maid, currentMaidNo);
                    break;
                case 3:
                    IKListLeftArm.initList(Bip01, boneList, maid, currentMaidNo);
                    break;
                case 4:
                    IKListRightArm.initList(Bip01, boneList, maid, currentMaidNo);
                    break;
                default:
                    break;

            }
        }

        public void Destroy()
        {
            attachIKMaidNo.Clear();
            attachIKMaidList.Clear();
            trTargetIKBones.Clear();

            IKListLeftLeg.Destroy();
            IKListRightLeg.Destroy();
            IKListLeftArm.Destroy();
            IKListRightArm.Destroy();
        }

        public void lateupdateFunc(HandleKun posHandle)
        {
            foreach (int m in attachIKMaidNo)
            {
                Debuginfo.Log(LogLabel + "finalize1");
                //メイドさんがいなくなっていればスキップして
                //リストから除外
                if (attachIKMaidList[m] == null || attachIKMaidList[m].Visible == false)
                {
                    Debuginfo.Log(LogLabel + "maid[" + m + "] is LOST");
                    removeAttachMaidList(m);
                    DeleteNoList.Add(m);

                }
            }

            //このタイミングでattachIKMaidNoの要素削除しないとInvalidOperationExceptionが出る


            attachIKMaidNo.RemoveAll(DeleteNoList.Contains);

            DeleteNoList.Clear();


            foreach (int m in attachIKMaidNo)
            {
                Debuginfo.Log(LogLabel + "finalize2");

                IKListLeftLeg.lateupdateFunc(m, attachIKMaidList[m],false,false);
                IKListRightLeg.lateupdateFunc(m, attachIKMaidList[m],false,false);

                IKListLeftArm.lateupdateFunc(m, attachIKMaidList[m], true, true);
                IKListRightArm.lateupdateFunc(m, attachIKMaidList[m], true, false);

            }
        }

        public void updateFunc(HandleKun posHandle,Dictionary<String,Transform>trBone)
        {
            //カメラ操作の右ドラッグと区別するため
            if (Input.GetMouseButtonDown(1))
            {
                //Debuginfo.Log("右クリック開始");
                bIKTargetGet = true;

            }
            else if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.mouseScrollDelta != Vector3.zero)
            {
                //IKハンドル君表示中にカメラ操作があったらラベルも付随して移動させる
                posHandle.IKBoneLabelPos = 1.5f * (Camera.main.WorldToScreenPoint(posHandle.IKTargetPos) - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f - 30f, Camera.main.WorldToScreenPoint(posHandle.IKTargetPos).z));

                bIKTargetGet = false;
            }
            else if (Input.GetMouseButtonUp(1) && bIKTargetGet == true)
            {
                //Debuginfo.Log("右クリック終了");
                //ここで画面内オブジェクトの検知を済ませておく
                //今表示されているメイドさんの_IK_ボーン情報をコレクションに収納
                //settrTargetIKBones();

                //Debuginfo.Log("mouse:" + Input.mousePosition.ToString());
                Vector3 mousePos = Input.mousePosition;

                trTargetIKTemp = null;

                float clickCheckOffset = clickCheckOffsetInit;


                float magnitude0 = (Camera.main.WorldToScreenPoint(trBone["Bip01 Neck"].position) - mousePos).magnitude;
                if (magnitude0 < clickCheckOffset)
                {
                    clickCheckOffset = magnitude0;
                    trTargetIKTemp = trBone["Bip01 Neck"];
                }

                List<int> RemoveNo = new List<int>();
                foreach (var trArray in trTargetIKBones)
                {
                    if (trArray.Value[0] == null)
                    {
                        RemoveNo.Add(trArray.Key);
                        continue;
                    }
                    foreach (Transform trIK in trArray.Value)
                    {

                        float magnitude = (Camera.main.WorldToScreenPoint(trIK.position) - mousePos).magnitude;
                        if (magnitude < clickCheckOffset)
                        {
                            clickCheckOffset = magnitude;
                            trTargetIKTemp = trIK;
                            //Debuginfo.Log(trIK.name + ":" + Camera.main.WorldToScreenPoint(trIK.position).ToString());
                        }
                    }
                }
                foreach (int t in RemoveNo)
                {
                    trTargetIKBones.Remove(t);
                }

                if (trTargetIKTemp != null)
                {
                    //Debuginfo.Log("右クリック何かあった");

                    posHandle.IKTargetPos = trTargetIKTemp.position;

                    if (trTargetIKTemp.name == "Bip01 Neck")
                    {
                        trTargetIKTemp = trBone["Bip01"];
                    }

                    //なぜかNGUIの位置指定と実際のピクセルで1.5倍の違いがあるので1.5倍する
                    posHandle.IKBoneLabelPos = 1.5f * (Camera.main.WorldToScreenPoint(posHandle.IKTargetPos) - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f - 30f, Camera.main.WorldToScreenPoint(posHandle.IKTargetPos).z));

                    posHandle.IKTargetLabelString = sIKBoneName[trTargetIKTemp.name];

                    posHandle.IKTargetVisible = true;
                }
            }

        }

        public void inversekinematicHandle(HandleKun posHandle,Maid maid,int currentMaidNo)
        {
            switch (posHandle.IKmode)
            {
                case HandleKun.IKMODE.LeftLeg:
                    IKListLeftLeg.inversekinematicHandle(posHandle, currentMaidNo);
                    break;
                case HandleKun.IKMODE.RightLeg:
                    IKListRightLeg.inversekinematicHandle(posHandle, currentMaidNo);
                    break;
                case HandleKun.IKMODE.LeftArm:
                    IKListLeftArm.inversekinematicHandle(posHandle, currentMaidNo);
                    break;
                case HandleKun.IKMODE.RightArm:
                    IKListRightArm.inversekinematicHandle(posHandle, currentMaidNo);
                    break;
                default:
                    Debug.LogError("IKMODE.Error");
                    break;
            }

            if (!attachIKMaidNo.Contains(currentMaidNo))
            {
                attachIKMaidList.Add(currentMaidNo, maid);
                attachIKMaidNo.Add(currentMaidNo);
            }
        }

        //IK設定の初期化
        private void ikInit(HandleKun.IKMODE ikmode, HandleKun posHandle,Maid maid,int currentMaidNo)
        {
            switch(ikmode)
            {
                case HandleKun.IKMODE.LeftLeg:
                    IKListLeftLeg.ikInit(posHandle, currentMaidNo);
                    break;
                case HandleKun.IKMODE.RightLeg:
                    IKListRightLeg.ikInit(posHandle, currentMaidNo);
                    break;
                case HandleKun.IKMODE.LeftArm:
                    IKListLeftArm.ikInit(posHandle, currentMaidNo);
                    break;
                case HandleKun.IKMODE.RightArm:
                    IKListRightArm.ikInit(posHandle, currentMaidNo);
                    break;
                default:
                    Debug.LogError("IKMODE.Error");
                    break;
            }

            if (!attachIKMaidNo.Contains(currentMaidNo))
            {
                attachIKMaidList.Add(currentMaidNo, maid);
                attachIKMaidNo.Add(currentMaidNo);
            }
        }

        public void ikTargetClicked(HandleKun posHandle,int currentMaidNo)
        {
            if (trTargetIKTemp != null)
            {
                Debuginfo.Log(LogLabel + "IKTarget:" + trTargetIKTemp.name.ToString());

                switch (posHandle.IKmode)
                {
                    case HandleKun.IKMODE.LeftLeg:
                        IKListLeftLeg.ikTargetClicked(posHandle, currentMaidNo, trTargetIKTemp);

                        break;

                    case HandleKun.IKMODE.RightLeg:
                        IKListRightLeg.ikTargetClicked(posHandle, currentMaidNo, trTargetIKTemp);

                        break;

                    case HandleKun.IKMODE.LeftArm:
                        IKListLeftArm.ikTargetClicked(posHandle, currentMaidNo, trTargetIKTemp);

                        break;

                    case HandleKun.IKMODE.RightArm:
                        IKListRightArm.ikTargetClicked(posHandle, currentMaidNo, trTargetIKTemp);

                        break;

                    default:

                        Debug.Log(LogLabel + "Handle IKmode target select exception.");
                        break;
                }


                if (trTargetIKTemp.name != "Bip01")
                {
                    posHandle.IKTargetAttachedColor(true);
                }
                else
                {
                    posHandle.IKTargetAttachedColor(false);
                }

                bIKTargetGet = false;

                trTargetIKTemp = null;
            }

        }

        //いなくなったメイドさんのIKアタッチ情報の要素をコレクションリストから削除
        public void removeAttachMaidList(int removeNo)
        {
            if (attachIKMaidNo.Contains(removeNo))
            {
                //attachIKMaidNo.Remove(removeNo);
                attachIKMaidList.Remove(removeNo);
                trTargetIKBones.Remove(removeNo);


                IKListLeftLeg.removeAttachMaidList(removeNo);
                IKListRightLeg.removeAttachMaidList(removeNo);

                IKListLeftArm.removeAttachMaidList(removeNo);
                IKListRightArm.removeAttachMaidList(removeNo);



            }
        }

        public void detachIKfromBone(HandleKun posHandle, Dictionary<String,Transform> trBone, int currentMaidNo)
        {
            //Vector3 postPosition = ikHandle.Pos;
            //IKターゲットの位置を初期化
            Quaternion temp = posHandle.Rot;
            //posHandle.transform.parent.parent = trBone["Bip01"];
            //posHandle.transform.parent.localPosition = Vector3.zero;

            if (posHandle.IKmode == HandleKun.IKMODE.LeftLeg)
            {
                posHandle.SetParentBone(trBone["Bip01 L Foot"]);
                IKListLeftLeg.detachIKfromBone(trBone["Bip01"], currentMaidNo);
                
            }
            else if (posHandle.IKmode == HandleKun.IKMODE.RightLeg)
            {
                posHandle.SetParentBone(trBone["Bip01 R Foot"]);
                IKListRightLeg.detachIKfromBone(trBone["Bip01"], currentMaidNo);
            }
            else if (posHandle.IKmode == HandleKun.IKMODE.LeftArm)
            {
                posHandle.SetParentBone(trBone["Bip01 L Hand"]);
                IKListLeftArm.detachIKfromBone(trBone["Bip01"], currentMaidNo);
            }
            else if (posHandle.IKmode == HandleKun.IKMODE.RightArm)
            {
                posHandle.SetParentBone(trBone["Bip01 R Hand"]);
                IKListRightArm.detachIKfromBone(trBone["Bip01"], currentMaidNo);
            }
            else
            {

            }
            //ikHandle.Pos = postPosition;
            posHandle.transform.localPosition = Vector3.zero;
            posHandle.Scale = 0.2f;

            posHandle.Rot = temp;//Quaternion.Euler(-90, 0, 90);
        }

        public bool checkIKAttach(int currentMaidNo, int No)
        {
            switch (No)
            {
                case 1:
                    return IKListLeftLeg.checkIKAttach(currentMaidNo);
                    break;
                case 2:
                    return IKListRightLeg.checkIKAttach(currentMaidNo);
                    break;
                case 3:
                    return IKListLeftArm.checkIKAttach(currentMaidNo);
                    break;
                case 4:
                    return IKListRightArm.checkIKAttach(currentMaidNo);
                    break;
                default:
                    return false;
            }
        }
        public Transform getIKTarget(int currentMaidNo, int No)
        {
            switch (No)
            {
                case 1:
                    return IKListLeftLeg.goIKTarget[currentMaidNo].transform;
                    break;
                case 2:
                    return IKListRightLeg.goIKTarget[currentMaidNo].transform;
                    break;
                case 3:
                    return IKListLeftArm.goIKTarget[currentMaidNo].transform;
                    break;
                case 4:
                    return IKListRightArm.goIKTarget[currentMaidNo].transform;
                    break;
                default:
                    return null;
            }
        }

        public void detachIK(Transform Bip01 ,int currentMaidNo, int No)
        {
            switch (No)
            {
                case 1:
                    IKListLeftLeg.detachIK(Bip01, currentMaidNo);
                    break;
                case 2:
                    IKListRightLeg.detachIK(Bip01, currentMaidNo);
                    break;
                case 3:
                    IKListLeftArm.detachIK(Bip01, currentMaidNo);
                    break;
                case 4:
                    IKListRightArm.detachIK(Bip01, currentMaidNo);
                    break;
                default:
                    break;
            }
        }
        public void detachAll(int currentMaidNo)
        {
            //全ターゲット用オブジェクトを0.5秒後に消す
            foreach (int removeNo in attachIKMaidNo)
            {
                IKListLeftLeg.detachAll(currentMaidNo);
                IKListRightLeg.detachAll(currentMaidNo);
                IKListLeftArm.detachAll(currentMaidNo);
                IKListRightArm.detachAll(currentMaidNo);
            }

            attachIKMaidNo.Clear();
            attachIKMaidList.Clear();
            trTargetIKBones.Clear();
            
        }

        public bool checkParentName(int currentMaidNo,int No)
        {
            switch (No)
            {
                case 1:
                    return IKListLeftLeg.checkParentName(currentMaidNo);
                    break;
                case 2:
                    return IKListRightLeg.checkParentName(currentMaidNo);
                    break;
                case 3:
                    return IKListLeftArm.checkParentName(currentMaidNo);
                    break;
                case 4:
                    return IKListRightArm.checkParentName(currentMaidNo);
                    break;
                default:
                    return false;
            }
        }


        public int attachIKMaidNoCount()
        {
            return attachIKMaidNo.Count;
        }

        public bool trTargetIKBoneContainsNo(int currentMaidNo)
        {
            return trTargetIKBones.ContainsKey(currentMaidNo);
        }

        public void trTargetIKBoneAdd(int currentMaidNo,Transform[] tempTransformList)
        {
            trTargetIKBones.Add(currentMaidNo,tempTransformList);
        }

    }

    //制限角度つきIK
    public class IKCONSTRAINED
    {
        private TBody body;

        private float defLENroroot;

        private float defLENroot;

        private float defLENhead;

        private Vector3 mid_old;

        private Quaternion defRorootlocalRotation;

        private Quaternion defRootlocalRotation;

        private Quaternion defMidlocalRotation;

        private Vector3 vechand;
        //制限角度多段配列
        //それぞれの添え字は
        //[ボーンの先端からの順番,xの下限値→zの上限値]
        private float[,] constrait;

        public void Init(Transform initRoot, Transform initMid, Transform initHead, TBody b, float[,] initConstrait, Transform initRoroot = null)
        {

            this.body = b;
            this.defLENroot = (initRoot.position - initMid.position).magnitude;
            this.defLENhead = (initHead.position - initMid.position).magnitude;
            this.mid_old = initMid.position;
            this.defRootlocalRotation = initRoot.localRotation;
            this.defMidlocalRotation = initMid.localRotation;
            this.vechand = Vector3.zero;

            if (initRoroot != null)
            {
                this.defLENroroot = (initRoroot.position - initRoot.position).magnitude;
                this.defRorootlocalRotation = initRoroot.localRotation;
            }

            this.constrait = initConstrait;

        }

        public void Init(Transform[] boneList, TBody b, float[,] initConstrait, Transform initRoroot = null)
        {

            this.body = b;
            this.defLENroot = (boneList[0].position - boneList[1].position).magnitude;
            this.defLENhead = (boneList[2].position - boneList[1].position).magnitude;
            this.mid_old = boneList[1].position;
            this.defRootlocalRotation = boneList[0].localRotation;
            this.defMidlocalRotation = boneList[1].localRotation;
            this.vechand = Vector3.zero;

            if (initRoroot != null)
            {
                this.defLENroroot = (initRoroot.position - boneList[0].position).magnitude;
                this.defRorootlocalRotation = initRoroot.localRotation;
            }

            this.constrait = initConstrait;

        }
        
        //腕・脚に特化させた肘・膝可動範囲制限付きIK
        //本体側と関数名をあわせないためにあえてProcにする
        public void Proc(Transform root, Transform mid, Transform head, Vector3 tgt)
        {
            //tgt += this.vechand;

            //先ボーンの曲げる前の回転状態を保持しておく
            Quaternion oldHeadRotation = head.transform.rotation;

            float LENtgt = (tgt - root.position).magnitude;

            //中間のボーンのZ軸角度(肘or膝)を一意に決める
            float midAngle;

            //ボーンの合計長さ　<　根元ボーンからターゲットまでの距離　の場合
            if (LENtgt >= defLENroot + defLENhead)
            {
                midAngle = 0f;//180f;
            }
            else
            {
                float s = (float)Math.Sqrt((-LENtgt + defLENroot + defLENhead) * (LENtgt - defLENroot + defLENhead) * (LENtgt + defLENroot - defLENhead) * (LENtgt + defLENroot + defLENhead)) / 4f;

                //各ボーンの長さ　< 根元ボーンからターゲットまでの距離　の場合
                if (LENtgt > ((defLENroot > defLENhead) ? defLENroot : defLENhead))
                {
                    float h = 2 * s / (LENtgt);
                    //midAngle = 180f - Mathf.Asin(h / defLENhead) * Mathf.Rad2Deg - Mathf.Asin(h / defLENroot) * Mathf.Rad2Deg;
                    midAngle = Mathf.Asin(h / defLENhead) * Mathf.Rad2Deg + Mathf.Asin(h / defLENroot) * Mathf.Rad2Deg;

                }
                else //ボーンのどちらか長い方　> 根元ボーンからターゲットまでの距離の場合
                {
                    float h = 2 * s / ((defLENroot > defLENhead) ? defLENroot : defLENhead);
                    midAngle = 180f - Mathf.Asin(h / ((defLENroot > defLENhead) ? defLENhead : defLENroot)) * Mathf.Rad2Deg;
                }

            }

            //角度制限チェック
            //肘も膝もZ軸周りの角度でしか回らないものとして計算する
            //他の関節では適用できないので注意

            if (this.constrait[1, 4] > midAngle)
            {
                midAngle = this.constrait[1, 4];
            }
            if (this.constrait[1, 5] < midAngle)
            {
                midAngle = this.constrait[1, 5];
            }

            //中間のボーンのX・Y軸周りの現在の角度を求める
            Vector3 pastEuler = calcEulerfromRotation(mid.localRotation);

            //計算結果の角度がずれたときのことを考えて一応チェック
            //今後の検証次第ではここのチェックをスキップするかも
            if (kaiten_ichii(pastEuler, this.constrait[1, 0], this.constrait[1, 1], this.constrait[1, 2], this.constrait[1, 3], this.constrait[1, 4], this.constrait[1, 5]))
            {
                //pastEuler.z -= 180f;
                pastEuler.y -= 180f;
                pastEuler.x = 180f - pastEuler.x;
            }

            //ここで中間のボーンを曲げる
            mid.localRotation = Quaternion.identity;
            mid.localRotation *= Quaternion.AngleAxis(midAngle, Vector3.forward);
            mid.localRotation *= Quaternion.AngleAxis(pastEuler.x, Vector3.right);
            mid.localRotation *= Quaternion.AngleAxis(pastEuler.y, Vector3.up);


            if (!this.body.boMAN)
            {

                Vector3 zero = Vector3.zero;
                Vector3 mid_old = mid.position;
                bool flag = this.body.goSlot[0].bonehair.bodyhit.SphereMove_hair(ref mid_old, ref zero, Vector3.zero);
                //if (flag)
                //{
                //    Debug.DrawLine(this.body.Spine0a.position, this.mid_old, Color.white);
                //}
                //mid.position = mid_old;
            }

            //中間のボーン(肘or膝)を曲げた状態で
            //根元のボーン(肩or股)から先端のボーン(手or足)までのベクトルを
            //根元のボーン(肩or股)からターゲットまでのベクトルに向ける回転を求めて
            //根元のボーンをターゲットに向けるように回転させる

            //root.localRotation = this.defRootlocalRotation;
            root.transform.rotation = Quaternion.FromToRotation(head.transform.position - root.transform.position, tgt - root.transform.position) * root.transform.rotation;
            //↑のタイミングで根元のボーンが回転する

            //ボーンの角度範囲チェック＆超えてたら範囲内に収める
            root.localRotation = constrainIK(root, 2);


            //最後に先ボーンを元の回転になるように曲げる
            head.transform.rotation = oldHeadRotation;
            //ボーンの角度範囲チェック＆超えてたら範囲内に収める
            head.localRotation = constrainIK(head, 0, head.name.Contains("Hand"));


            //Debug.DrawLine(root.position, this.mid_old, Color.yellow);
            //Debug.DrawLine(head.position, this.mid_old, Color.yellow);
            //this.vechand = head.rotation * vechand_offset;
        }

        public Vector3 calcEulerfromRotation(Quaternion tr)
        {
            float qx = tr.x;
            float qy = tr.y;
            float qz = tr.z;
            float qw = tr.w;

            float m02 = 2 * (qx * qz - qw * qy);
            float m10 = 2 * (qx * qy - qw * qz);
            float m11 = 1 - 2 * (qx * qx + qz * qz);
            float m12 = 2 * (qy * qz + qw * qx);
            //float m20 = 2 * (qx * qz + qw * qy);
            //float m21 = 2 * (qy * qz - qw * qx);
            float m22 = 1 - 2 * (qx * qx + qy * qy);

            if (m12 > 1.0f) m12 = 1.0f;
            if (m12 < -1.0f) m12 = -1.0f;
            if (m11 > 1.0f) m11 = 1.0f;
            if (m11 < -1.0f) m11 = -1.0f;
            if (m10 > 1.0f) m10 = 1.0f;
            if (m10 < -1.0f) m10 = -1.0f;
            if (m22 > 1.0f) m22 = 1.0f;
            if (m22 < -1.0f) m22 = -1.0f;

            float rotate_x = Mathf.Asin(m12) * Mathf.Rad2Deg;
            float rotate_y = 0f;
            float rotate_z = 0f;

            if (System.Math.Floor(Mathf.Cos(rotate_x * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
            {

                rotate_z = Mathf.Atan2(-m10, m11) * Mathf.Rad2Deg;
                float before = -m02 / Mathf.Cos(rotate_x * Mathf.Deg2Rad);
                if (before > 1.0f) before = 1.0f;
                if (before < -1.0f) before = -1.0f;

                rotate_y = Mathf.Asin(before) * Mathf.Rad2Deg;
                if (m22 < 0)
                {
                    rotate_y = 180 - rotate_y;
                }

            }
            if (System.Math.Floor(Mathf.Cos(rotate_x * Mathf.Deg2Rad) * 10000) / 10000 == 0f || Double.IsNaN(rotate_y))
            {
                float m00 = 1 - 2 * (qy * qy + qz * qz);
                float m01 = 2 * (qx * qy + qw * qz);

                if (m01 > 1.0f) m01 = 1.0f;
                if (m01 < -1.0f) m01 = -1.0f;
                if (m00 > 1.0f) m00 = 1.0f;
                if (m00 < -1.0f) m00 = -1.0f;

                rotate_y = 0f;
                //rotate_x = (tr.x > 0) ? 90f : -90f;
                rotate_z = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg;

            }

            return new Vector3(rotate_x, rotate_y, rotate_z);
        }

        public Vector3 calcEulerfromRotation_hand(Quaternion tr)
        {
            float qx = tr.x;
            float qy = tr.y;
            float qz = tr.z;
            float qw = tr.w;

            float m02 = 2 * (qx * qz - qw * qy);
            if (m02 > 1.0f) m02 = 1.0f;
            if (m02 < -1.0f) m02 = -1.0f;

            float rotate_x = 0f;
            float rotate_y = Mathf.Asin(-m02) * Mathf.Rad2Deg;
            float rotate_z = 0f;


            if (System.Math.Floor(Mathf.Cos(rotate_y * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
            {
                float m00 = 1 - 2 * (qy * qy + qz * qz);
                float m01 = 2 * (qx * qy + qw * qz);
                float m12 = 2 * (qy * qz + qw * qx);
                float m22 = 1 - 2 * (qx * qx + qy * qy);

                if (m00 > 1.0f) m00 = 1.0f;
                if (m00 < -1.0f) m00 = -1.0f;
                if (m01 > 1.0f) m01 = 1.0f;
                if (m01 < -1.0f) m01 = -1.0f;

                if (m12 > 1.0f) m12 = 1.0f;
                if (m12 < -1.0f) m12 = -1.0f;
                if (m22 > 1.0f) m22 = 1.0f;
                if (m22 < -1.0f) m22 = -1.0f;

                rotate_z = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg;
                float before = m12 / Mathf.Cos(rotate_y * Mathf.Deg2Rad);
                if (before > 1.0f) before = 1.0f;
                if (before < -1.0f) before = -1.0f;
                rotate_x = Mathf.Asin(before) * Mathf.Rad2Deg;
                if (m22 < 0)
                {
                    rotate_x = 180 - rotate_x;
                }
            }
            if (System.Math.Floor(Mathf.Cos(rotate_y * Mathf.Deg2Rad) * 10000) / 10000 == 0f || Double.IsNaN(rotate_x))
            {
                float m10 = 2 * (qx * qy - qw * qz);
                float m11 = 1 - 2 * (qx * qx + qz * qz);

                if (m10 > 1.0f) m10 = 1.0f;
                if (m10 < -1.0f) m10 = -1.0f;
                if (m11 > 1.0f) m11 = 1.0f;
                if (m11 < -1.0f) m11 = -1.0f;

                rotate_x = 0f;
                rotate_z = Mathf.Atan2(-m10, m11) * Mathf.Rad2Deg;
            }


            return new Vector3(rotate_x, rotate_y, rotate_z);
        }

        //角度が一意に決まらないときの苦肉の策
        public bool kaiten_ichii(Vector3 rotate, float xmin, float xmax, float ymin, float ymax, float zmin, float zmax)
        {
            return ((180f - rotate.x) > xmin && (180f - rotate.x) < xmax
                  && (rotate.y - 180f) > ymin && (rotate.y - 180f) < ymax
                  && (rotate.z - 180f) > zmin && (rotate.z - 180f) < zmax);
        }
        public bool kaiten_ichii_hand(Vector3 rotate, float xmin, float xmax, float ymin, float ymax, float zmin, float zmax)
        {
            return ((rotate.x - 180f) > xmin && (rotate.x - 180f) < xmax
                  && (180f - rotate.y) > ymin && (180f - rotate.y) < ymax
                  && (rotate.z - 180f) > zmin && (rotate.z - 180f) < zmax);
        }

        public Quaternion constrainIK(Transform tr, int boneOrder, bool isHand = false)
        {
            return constrainIK(tr.localRotation, boneOrder, isHand);
        }

        public Quaternion constrainIK(Quaternion tr, int boneOrder, bool isHand = false)
        {
            Vector3 rotate;

            if (!isHand)
            {
                rotate = calcEulerfromRotation(tr);
                if (kaiten_ichii(rotate, this.constrait[boneOrder, 0], this.constrait[boneOrder, 1], this.constrait[boneOrder, 2], this.constrait[boneOrder, 3], this.constrait[boneOrder, 4], this.constrait[boneOrder, 5]))
                {
                    //rotate.z -= 180f;
                    //rotate.y -= 180f;
                    //rotate.x = 180f - rotate.x ;

                    return tr;
                }
            }
            else
            {
                rotate = calcEulerfromRotation_hand(tr);
                if (kaiten_ichii_hand(rotate, this.constrait[boneOrder, 0], this.constrait[boneOrder, 1], this.constrait[boneOrder, 2], this.constrait[boneOrder, 3], this.constrait[boneOrder, 4], this.constrait[boneOrder, 5]))
                {
                    return tr;
                }
            }

            bool bOver = false;

            if (this.constrait[boneOrder, 0] > rotate.x)
            {
                if (this.constrait[boneOrder, 0] > rotate.x + 360f || this.constrait[boneOrder, 1] < rotate.x + 360f)
                {
                    rotate.x = this.constrait[boneOrder, 0];
                    bOver = true;
                }
            }
            if (this.constrait[boneOrder, 1] < rotate.x)
            {
                if (this.constrait[boneOrder, 1] < rotate.x - 360f || this.constrait[boneOrder, 0] > rotate.x - 360f)
                {
                    rotate.x = this.constrait[boneOrder, 1];
                    bOver = true;
                }
            }
            if (this.constrait[boneOrder, 2] > rotate.y)
            {
                if (this.constrait[boneOrder, 2] > rotate.y + 360f || this.constrait[boneOrder, 3] < rotate.y + 360f)
                {
                    rotate.y = this.constrait[boneOrder, 2];
                    bOver = true;
                }
            }
            if (this.constrait[boneOrder, 3] < rotate.y)
            {
                if (this.constrait[boneOrder, 3] < rotate.y - 360f || this.constrait[boneOrder, 2] > rotate.y - 360f)
                {
                    rotate.y = this.constrait[boneOrder, 3];
                    bOver = true;
                }
            }
            if (this.constrait[boneOrder, 4] > rotate.z)
            {
                if (this.constrait[boneOrder, 4] > rotate.z + 360f || this.constrait[boneOrder, 5] < rotate.z + 360f)
                {
                    rotate.z = this.constrait[boneOrder, 4];
                    bOver = true;
                }
            }
            if (this.constrait[boneOrder, 5] < rotate.z)
            {
                if (this.constrait[boneOrder, 5] < rotate.z - 360f || this.constrait[boneOrder, 4] > rotate.z - 360f)
                {
                    rotate.z = this.constrait[boneOrder, 5];
                    bOver = true;
                }
            }

            if (bOver)
            {
                if (!isHand)
                {

                    tr = Quaternion.identity;
                    tr *= Quaternion.AngleAxis(rotate.z, Vector3.forward);
                    tr *= Quaternion.AngleAxis(rotate.x, Vector3.right);
                    tr *= Quaternion.AngleAxis(rotate.y, Vector3.up);
                }
                else
                {
                    //苦肉の策のツケ
                    tr = Quaternion.identity;
                    tr *= Quaternion.AngleAxis(rotate.z, Vector3.forward);
                    tr *= Quaternion.AngleAxis(rotate.y, Vector3.up);
                    tr *= Quaternion.AngleAxis(rotate.x, Vector3.right);

                }
            }

            return tr;
        }

    }
 
    //ハンドル君
    public class HandleKun
    {
        private readonly int BaseRenderQueue = 3500;


        private bool initComplete = false;

        private Maid maid = null;
        private Transform trParentBone;

        private GameObject goHandleMasterObject;
        private GameObject goAngleHandle;
        private GameObject goPositionHandle;

        private GameObject goIKBoneTarget;

        private ControllOnMouse controllOnMouseX;
        private ControllOnMouse controllOnMouseY;
        private ControllOnMouse controllOnMouseZ;

        private ControllOnMouse controllOnMousePX;
        private ControllOnMouse controllOnMousePY;
        private ControllOnMouse controllOnMousePZ;

        private ControllOnMouse controllOnMouseC;

        private ClickOnlyControll CoC;

        private int Legacymode;

        private bool bIKAttached = false;

        public bool bHandlePositionMode;

        public bool rightClicked = false;



        Texture2D m_texture_red = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_green = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_blue = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_red_2 = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_green_2 = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_blue_2 = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_white = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_yellow = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_cyan = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        Texture2D m_texture_magenta = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        GameObject redring;
        GameObject bluering;
        GameObject greenring;
        GameObject redvector;
        GameObject bluevector;
        GameObject greenvector;
        GameObject whitecenter;

        GizmoRender gizmoRender;

        private UILabel uiLabelIKBoneName;

        public enum IKMODE
        {
            None,
            LeftLeg,
            RightLeg,
            LeftArm,
            RightArm
        }

        private IKMODE ikmode = IKMODE.None;

        private float handleScale = 1.0f;

        public Transform transform
        {
            get
            {
                return (initComplete) ? this.goHandleMasterObject.transform : null;
            }
        }
        public Vector3 Pos
        {
            get { return (initComplete) ? this.goHandleMasterObject.transform.position : default(Vector3); }
            set { if (initComplete) this.goHandleMasterObject.transform.position = value; }
        }
        public Quaternion Rot
        {
            get { return (initComplete) ? this.goHandleMasterObject.transform.rotation : default(Quaternion); }
            set { if (initComplete) this.goHandleMasterObject.transform.rotation = value; }
        }

        public float Scale
        {
            get { return (initComplete) ? this.handleScale : 0; }
            set
            {
                if (initComplete)
                {
                    this.handleScale = value;
                    this.goHandleMasterObject.transform.localScale = Vector3.one * handleScale;
                }
            }


        }
        public bool Visible
        {
            get
            {
                return (initComplete && this.goHandleMasterObject != null) ? this.goHandleMasterObject.activeSelf : default(bool);
            }
            set
            {
                if (initComplete && this.goHandleMasterObject != null) this.goHandleMasterObject.SetActive(value);
            }
        }

        public IKMODE IKmode
        {
            get { return (initComplete) ? this.ikmode : IKMODE.None; }
            set
            {
                //Debuginfo.Log("IKmode set");
                //if (initComplete)
                //{
                //Debuginfo.Log("IKmode:" + (int)value);
                this.ikmode = value;
                if (value == IKMODE.None)
                {
                    //Rot = Quaternion.identity;
                    controllOnMouseC.ikmode = false;
                }
                else
                {
                    //Rot = Quaternion.Euler(-90, 0, 90);

                    controllOnMouseC.ikmode = true;
                }
                //}
            }
        }

        public bool IKTargetVisible
        {
            get { return (initComplete) ? this.goIKBoneTarget.activeSelf : false; }
            set
            {
                if (initComplete)
                {
                    this.uiLabelIKBoneName.gameObject.SetActive(value);
                    this.goIKBoneTarget.SetActive(value);
                    goIKBoneTarget.renderer.material.mainTexture = m_texture_magenta;
                    goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0f, 1f, 0.5f));

                }

            }
        }
        public Vector3 IKTargetPos
        {
            get { return (initComplete) ? this.goIKBoneTarget.transform.position : default(Vector3); }
            set { if (initComplete) this.goIKBoneTarget.transform.position = value; }
        }

        public Vector3 IKBoneLabelPos
        {
            set { if (initComplete) this.uiLabelIKBoneName.gameObject.transform.localPosition = value; }
        }

        public String IKTargetLabelString
        {
            set { if (initComplete) this.uiLabelIKBoneName.text = value; }
        }

        private class ClickOnlyControll : MonoBehaviour
        {
            public bool Dragged = false;
            public bool DragFinished = false;
            public bool mouseOver = false;
            public bool centerClicked = false;

            public void OnMouseDown()
            {

            }

            public void OnMouseDrag()
            {

                Dragged = true;
            }
            public void OnMouseUp()
            {
                if (Dragged)
                {
                    //
                    Debuginfo.Log("IKハンドル君左ドラッグ終了");
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
                if (Input.GetMouseButton(2))
                {


                    if (centerClicked == false)
                    {
                        Debuginfo.Log("中クリック");
                        centerClicked = true;

                    }

                }

                else if (centerClicked == true)
                {
                    Debuginfo.Log("中クリック終了");
                    centerClicked = false;
                }



            }
            public void OnMouseExit()
            {
                mouseOver = false;
            }

            public void Update()
            {

                if (mouseOver)
                {

                }

            }

            public void OnGui()
            {
                if (DragFinished)
                {
                    DragFinished = false;
                }
            }
        }

        private class ControllOnMouse : MonoBehaviour
        {
            public enum WheelType
            {
                Angle,
                Position,
                PosCenter
            }

            public enum AxisType
            {
                RX,
                RY,
                RZ,
                NONE
            }

            public bool rightClicked = false;

            public bool mouseOver = false;

            private Vector3 objectPoint = Vector3.zero;


            public WheelType wheelType = WheelType.Angle;
            public AxisType axisType = AxisType.RX;
            public bool ShouldReset = false;

            public bool ikmode = false;

            public bool Dragged = false;
            public bool DragFinished = false;

            public Vector3 clickPointVector = Vector3.zero;
            public float oldValue = 0f;
            Vector3 identitytoScreen = Vector3.zero;

            public Quaternion dragQuaternion = Quaternion.identity;
            public Vector3 dragVector = Vector3.zero;


            public void Destroy()
            {
            }

            public void Awake()
            {
            }

            public void OnMouseDown()
            {


                if (wheelType == WheelType.Angle)
                {
                    //カメラから見たオブジェクトの現在位置を画面位置座標に変換
                    //screenPoint = Camera.main.WorldToScreenPoint(transform.position);

                    //リングが直線状になるとき
                    if (Math.Abs(Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) - 90f) < 10f)
                    {
                        if ((Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) - 90f) < 0f)
                            objectPoint = Camera.main.WorldToScreenPoint(transform.position + 0.2f * transform.up);
                        else
                            objectPoint = Camera.main.WorldToScreenPoint(transform.position - 0.2f * transform.up);
                    }
                    else
                    {
                        objectPoint = Camera.main.WorldToScreenPoint(transform.position);
                    }


                    clickPointVector = Input.mousePosition;
                    clickPointVector.z = 0;
                    clickPointVector -= objectPoint;

                    oldValue = 0.0f;
                }
                else if (wheelType == WheelType.Position)
                {
                    clickPointVector = Input.mousePosition;
                    //clickPointVector.z = 0;//Camera.main.WorldToScreenPoint(transform.position).z;

                    oldValue = 0.0f;

                    identitytoScreen = Camera.main.WorldToScreenPoint(transform.up + transform.position) - Camera.main.WorldToScreenPoint(transform.position);
                    //identitytoScreen.z = 0;

                }
                else//wheelType == WheelType.PosCenter
                {

                    clickPointVector = Input.mousePosition;
                    clickPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    oldValue = Camera.main.WorldToScreenPoint(transform.position).z;
                    clickPointVector = Camera.main.ScreenToWorldPoint(clickPointVector);


                }
            }

            public void OnMouseDrag()
            {


                if (wheelType == WheelType.Angle)
                {
                    Vector3 dragPoint = Input.mousePosition;

                    dragPoint.z = 0;

                    dragPoint -= objectPoint;

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

                    float offsetAngle = dragAngle - oldValue;


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


                    oldValue = dragAngle;

                }
                else if (wheelType == WheelType.Position)
                {

                    Vector3 dragPointVector = Input.mousePosition;

                    float dragLength = (dragPointVector - clickPointVector).magnitude;

                    Vector3 yajirushi = Camera.main.WorldToScreenPoint(transform.up + transform.position) - Camera.main.WorldToScreenPoint(transform.position);//Camera.main.worldToCameraMatrix.MultiplyVector(transform.up);

                    dragLength = dragLength != 0 ? (yajirushi.x * (dragPointVector - clickPointVector).x + yajirushi.y * (dragPointVector - clickPointVector).y) / (yajirushi.magnitude * dragLength) : 0;


                    clickPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    dragPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    Vector3 clickPoint = Camera.main.ScreenToWorldPoint(clickPointVector);
                    Vector3 dragPoint = Camera.main.ScreenToWorldPoint(dragPointVector);


                    dragLength = (dragPoint - clickPoint).magnitude * dragLength;


                    float offsetLength = dragLength - oldValue;

                    switch (axisType)
                    {
                        case AxisType.RY:

                            dragVector = offsetLength * transform.up;//(-Vector3.right);

                            break;

                        case AxisType.RZ:

                            dragVector = offsetLength * transform.up;//Vector3.up;

                            break;

                        case AxisType.RX:

                            dragVector = offsetLength * transform.up;// Vector3.forward;

                            break;


                        default:
                            break;

                    }


                    oldValue = dragLength;

                }
                else//wheelType == WheelType.PosCenter
                {

                    Vector3 dragPointVector = Input.mousePosition;
                    dragPointVector.z = oldValue;//Camera.main.WorldToScreenPoint(transform.position).z;
                    dragPointVector = Camera.main.ScreenToWorldPoint(dragPointVector);


                    switch (axisType)
                    {
                        case AxisType.NONE:

                            dragVector = dragPointVector - clickPointVector;

                            break;

                        default:
                            break;

                    }
                    clickPointVector = dragPointVector;
                }
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
                /*
                //ここじゃないと右クリックを検知できないのでここで検知
                if (ikmode && Input.GetMouseButton(1) )
                {
                    if (rightClicked == false)
                    {
                        Debuginfo.Log("右クリック");
                        rightClicked = true;
                    }
                }
                else if(rightClicked == true)
                {
                    Debuginfo.Log("右クリック終了");
                    rightClicked = false;
                }
                */

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
                if (DragFinished)
                {
                    DragFinished = false;
                }
            }

        }

        public HandleKun(int _Legacymode, UIAtlas _systemAtlas,Maid _maid = null, Transform _transform = null)
        {
            this.Legacymode = _Legacymode;

            SetMaterial(m_texture_red, new Color(1f, 0f, 0f, 0.5f), "red");
            SetMaterial(m_texture_green, new Color(0f, 1f, 0f, 0.5f), "green");
            SetMaterial(m_texture_blue, new Color(0f, 0f, 1f, 0.5f), "blue");

            SetMaterial(m_texture_red_2, new Color(1f, 0f, 0f, 0.5f), "red_2");
            SetMaterial(m_texture_green_2, new Color(0f, 1f, 0f, 0.5f), "green_2");
            SetMaterial(m_texture_blue_2, new Color(0f, 0f, 1f, 0.5f), "blue_2");

            SetMaterial(m_texture_white, new Color(1f, 1f, 1f, 0.5f), "white");
            SetMaterial(m_texture_yellow, new Color(1f, 0.92f, 0.016f, 0.3f), "yellow");
            SetMaterial(m_texture_cyan, new Color(0f, 1f, 1f, 0.5f), "cyan");
            SetMaterial(m_texture_magenta, new Color(1f, 0f, 1f, 0.5f), "magenta");

            Init();

            //IKボーン表示用ラベル
            UIPanel uiPanelIKBoneName = NGUITools.AddChild<UIPanel>(GameObject.Find("UI Root"));

            UISprite uiIKBNSprite = uiPanelIKBoneName.gameObject.AddComponent<UISprite>();
            //uiIKBNSprite.depth = uiBGSprite.depth - 1;
            uiIKBNSprite.atlas = _systemAtlas;
            uiIKBNSprite.spriteName = "cm3d2_dialog_frame";
            uiIKBNSprite.type = UIBasicSprite.Type.Sliced;
            uiIKBNSprite.SetDimensions(150, 70);

            uiLabelIKBoneName = uiIKBNSprite.gameObject.AddComponent<UILabel>();
            uiLabelIKBoneName.name = "IKBoneLabel";
            uiLabelIKBoneName.trueTypeFont = GameObject.Find("SystemUI Root").GetComponentsInChildren<UILabel>()[0].trueTypeFont;
            uiLabelIKBoneName.fontSize = 20;
            uiLabelIKBoneName.text = "未設定";
            //uiLabelIKBoneName.width = 110;
            uiLabelIKBoneName.fontStyle = FontStyle.Bold;
            uiLabelIKBoneName.depth = uiIKBNSprite.depth + 1;
            //uiLabelIKBoneName.overflowMethod = UILabel.Overflow.ShrinkContent;

            uiLabelIKBoneName.gameObject.SetActive(false);

            if (_maid != null)
            {
                SetMaid(_maid, _transform);
            }

            this.goHandleMasterObject.SetActive(false);


        }

        //ハンドル君初期化生成処理
        public void Init(bool isPositionMode = false)
        {
            #region Init
            this.goHandleMasterObject = new GameObject();

            this.goAngleHandle = new GameObject();
            this.goPositionHandle = new GameObject();
            goAngleHandle.transform.parent = this.goHandleMasterObject.transform;
            goPositionHandle.transform.parent = this.goHandleMasterObject.transform;


            //SSでハンドル君を消すために
            //公式のハンドルを線の太さ0にして所持しとく
            //公式のハンドルが消えたらハンドル君も消す
            //ここまでやるなら公式のハンドル流用しろよとは思うけどなんとなく
            if (Legacymode == 0)
            {
                gizmoRender = this.goHandleMasterObject.AddComponent<GizmoRender>();
                gizmoRender.Visible = true;
                gizmoRender.offsetScale = 0;
            }



            SetHandleObject(PrimitiveType.Sphere, m_texture_white, new Vector3(0.125f, 0.125f, 0.125f), new Vector3(0f, 0f, 0f), 0);

            SetHandleObject(PrimitiveType.Cylinder, m_texture_blue, new Vector3(0.025f, 1f, 0.025f), new Vector3(0f, 0f, 0f), 1);
            SetHandleObject(PrimitiveType.Cylinder, m_texture_red, new Vector3(0.025f, 1f, 0.025f), new Vector3(90f, 0f, 0f), 2);
            SetHandleObject(PrimitiveType.Cylinder, m_texture_green, new Vector3(0.025f, 1f, 0.025f), new Vector3(0f, 0f, 90f), 3);

            this.controllOnMouseZ = SetHandleRingObject(m_texture_blue_2, new Vector3(0f, 0f, 0f), new Color(0, 0, 1, 0.5f), 4);//Z
            this.controllOnMouseZ.wheelType = ControllOnMouse.WheelType.Angle;
            this.controllOnMouseZ.axisType = ControllOnMouse.AxisType.RZ;

            this.controllOnMouseX = SetHandleRingObject(m_texture_red_2, new Vector3(90f, 0f, 0f), new Color(1, 0, 0, 0.5f), 5);//X
            this.controllOnMouseX.wheelType = ControllOnMouse.WheelType.Angle;
            this.controllOnMouseX.axisType = ControllOnMouse.AxisType.RX;

            this.controllOnMouseY = SetHandleRingObject(m_texture_green_2, new Vector3(0f, 0f, 90f), new Color(0, 1, 0, 0.5f), 6);//Y
            this.controllOnMouseY.wheelType = ControllOnMouse.WheelType.Angle;
            this.controllOnMouseY.axisType = ControllOnMouse.AxisType.RY;

            this.controllOnMousePZ = SetHandleVectorObject(m_texture_blue, new Vector3(0f, 0f, 0f), new Color(0, 0, 1, 0.5f), 4);//Z
            this.controllOnMousePZ.wheelType = ControllOnMouse.WheelType.Position;
            this.controllOnMousePZ.axisType = ControllOnMouse.AxisType.RZ;

            this.controllOnMousePX = SetHandleVectorObject(m_texture_red, new Vector3(90f, 0f, 0f), new Color(1, 0, 0, 0.5f), 5);//X
            this.controllOnMousePX.wheelType = ControllOnMouse.WheelType.Position;
            this.controllOnMousePX.axisType = ControllOnMouse.AxisType.RX;

            this.controllOnMousePY = SetHandleVectorObject(m_texture_green, new Vector3(0f, 0f, 90f), new Color(0, 1, 0, 0.5f), 6);//Y
            this.controllOnMousePY.wheelType = ControllOnMouse.WheelType.Position;
            this.controllOnMousePY.axisType = ControllOnMouse.AxisType.RY;

            whitecenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            whitecenter.renderer.receiveShadows = false;
            whitecenter.renderer.castShadows = false;
            whitecenter.renderer.useLightProbes = false;

            whitecenter.renderer.material.mainTexture = m_texture_white;
            if (Legacymode == 0)
            {
                whitecenter.renderer.material.shader = Shader.Find("Hidden/Transplant_Internal-Colored");

                whitecenter.renderer.material.SetFloat("_ZTest", 6);
                whitecenter.renderer.material.SetFloat("_Cull", 2);
                whitecenter.renderer.material.SetFloat("_ZWrite", 0);

            }
            else
            {
                whitecenter.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
            }
            whitecenter.renderer.material.SetColor("_Color", new Color(1, 1, 1, 0.5f));

            whitecenter.renderer.material.renderQueue = BaseRenderQueue + 9;

            whitecenter.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            whitecenter.name = "whitecenter";
            whitecenter.transform.parent = this.goPositionHandle.transform;

            this.controllOnMouseC = whitecenter.AddComponent<ControllOnMouse>();
            this.controllOnMouseC.wheelType = ControllOnMouse.WheelType.PosCenter;
            this.controllOnMouseC.axisType = ControllOnMouse.AxisType.NONE;



            goIKBoneTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goIKBoneTarget.renderer.receiveShadows = false;
            goIKBoneTarget.renderer.castShadows = false;
            goIKBoneTarget.renderer.useLightProbes = false;

            goIKBoneTarget.renderer.material.mainTexture = m_texture_magenta;
            if (Legacymode == 0)
            {
                goIKBoneTarget.renderer.material.shader = Shader.Find("Hidden/Transplant_Internal-Colored");
                goIKBoneTarget.renderer.material.SetFloat("_ZTest", 6);
                goIKBoneTarget.renderer.material.SetFloat("_Cull", 2);
                goIKBoneTarget.renderer.material.SetFloat("_ZWrite", 0);

            }
            else
            {
                goIKBoneTarget.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
            }

            goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0f, 1f, 0.5f));
            goIKBoneTarget.renderer.material.renderQueue = BaseRenderQueue + 9;

            goIKBoneTarget.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            goIKBoneTarget.name = "goIKBoneTarget";
            goIKBoneTarget.transform.parent = this.goHandleMasterObject.transform;
            goIKBoneTarget.SetActive(false);

            this.CoC = goIKBoneTarget.AddComponent<ClickOnlyControll>();


            #endregion

            ChangeHandleModePosition(isPositionMode);
        }

        //テクスチャ設定
        private void SetMaterial(Texture2D m_texture, Color _color, String name)
        {

            for (int y = 0; y < m_texture.height; y++)
            {
                for (int x = 0; x < m_texture.width; x++)
                {
                    m_texture.SetPixel(x, y, _color);
                }
            }
            m_texture.Apply();
            m_texture.name = name;
        }

        //ハンドル君汎用パーツを作る
        private void SetHandleObject(PrimitiveType _type, Texture2D m_texture, Vector3 _position, Vector3 _angle, int RQ)
        {
            GameObject PartsObject = GameObject.CreatePrimitive(_type);

            PartsObject.renderer.receiveShadows = false;
            PartsObject.renderer.castShadows = false;
            PartsObject.renderer.useLightProbes = false;
            PartsObject.renderer.material.mainTexture = m_texture;
            if (Legacymode == 0)
            {
                PartsObject.renderer.material.shader = Shader.Find("Custom/GizmoShader");
                PartsObject.renderer.material.SetInt("unity_GUIZTestMode", 6);
            }
            else
            {
                PartsObject.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
            }
            PartsObject.renderer.material.renderQueue = BaseRenderQueue + RQ;
            PartsObject.transform.localScale = _position;
            PartsObject.transform.localEulerAngles = _angle;


            PartsObject.transform.parent = this.goAngleHandle.transform;
        }

        //ハンドル君のリング部分を作る
        private ControllOnMouse SetHandleRingObject(Texture2D m_texture, Vector3 handleAngle, Color m_color, int RQ)
        {
            #region createPrimitive
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
            Ring.renderer.material.renderQueue = BaseRenderQueue + RQ;

            Ring.transform.localScale = new Vector3(2f, 0.05f, 2f);
            Ring.transform.localEulerAngles = handleAngle;

            UnityEngine.Object.Destroy(Ring.GetComponent<Collider>());

            MeshCollider meshCollider = Ring.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = Ring.GetComponent<MeshFilter>().sharedMesh;

            Ring.name = m_texture.name + "ring";
            Ring.transform.parent = this.goAngleHandle.transform;

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
            #endregion
            return Ring.AddComponent<ControllOnMouse>();

        }

        //ハンドル君の矢印部分を作る
        private ControllOnMouse SetHandleVectorObject(Texture2D m_texture, Vector3 handleAngle, Color m_color, int RQ)
        {
            #region createPrimitive
            GameObject Segare = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            Mesh mesh = Segare.GetComponent<MeshFilter>().mesh;

            //円筒プリミティブを矢印形に修正
            Vector3[] newMesh = new Vector3[88];
            Vector2[] newUV = new Vector2[88];

            for (int i = 0; i < 88; ++i)
            {
                if (i == 41)
                {
                    newMesh[i] = new Vector3(0f, 1.25f, 0f);
                }
                else if (i >= 68)
                {
                    newMesh[i] = mesh.vertices[i];
                    newMesh[i].x *= 2.0f;
                    newMesh[i].z *= 2.0f;
                    //newMesh[i].y += 1.0f;
                }
                else
                {
                    newMesh[i] = mesh.vertices[i];
                    ///newMesh[i].y += 1.0f;
                }
                newUV[i] = mesh.uv[i];

            }

            int[] newTri = new int[360];
            for (int i = 0; i < 240; ++i)
            {
                newTri[i] = mesh.triangles[i];
            }
            for (int i = 0; i < 19; ++i)
            {
                newTri[6 * i + 240] = 20 + i;
                newTri[6 * i + 241] = 68 + i;
                newTri[6 * i + 242] = 21 + i;
                newTri[6 * i + 243] = 69 + i;
                newTri[6 * i + 244] = 21 + i;
                newTri[6 * i + 245] = 68 + i;
            }
            {
                newTri[354] = 39;
                newTri[355] = 87;
                newTri[356] = 20;
                newTri[357] = 68;
                newTri[358] = 20;
                newTri[359] = 87;
            }
            mesh.Clear();
            mesh.vertices = newMesh;
            mesh.uv = newUV;
            mesh.triangles = newTri;

            Segare.renderer.receiveShadows = false;
            Segare.renderer.castShadows = false;
            Segare.renderer.useLightProbes = false;
            Segare.renderer.material.mainTexture = m_texture;

            if (Legacymode == 0)
            {
                Segare.renderer.material.shader = Shader.Find("Hidden/Transplant_Internal-Colored");
                Segare.renderer.material.SetFloat("_ZTest", 6);
                Segare.renderer.material.SetFloat("_Cull", 2);
                Segare.renderer.material.SetFloat("_ZWrite", 0);

            }
            else
            {
                Segare.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
            }

            Segare.renderer.material.SetColor("_Color", m_color);
            Segare.renderer.material.renderQueue = BaseRenderQueue + RQ;

            Segare.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f);
            Segare.transform.localEulerAngles = handleAngle;


            UnityEngine.Object.Destroy(Segare.GetComponent<Collider>());

            MeshCollider meshCollider = Segare.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = Segare.GetComponent<MeshFilter>().sharedMesh;

            Segare.name = m_texture.name + "vector";
            Segare.transform.parent = this.goPositionHandle.transform;

            if (Segare.name == "redvector")
            {
                redvector = Segare;

            }
            else if (Segare.name == "greenvector")
            {

                greenvector = Segare;

            }
            else if (Segare.name == "bluevector")
            {
                bluevector = Segare;

            }
            else
            {
                Debug.LogError("ControllOnMouse: material name is invalid.");
            }
            #endregion

            return Segare.AddComponent<ControllOnMouse>();

        }


        //ハンドル君変形
        public void ChangeHandleModePosition(bool isPositionMode)
        {
            bHandlePositionMode = isPositionMode;
            this.goAngleHandle.SetActive(!isPositionMode);
            this.goPositionHandle.SetActive(isPositionMode);

            //Rot = isPositionMode ? Quaternion.Euler(-90, 0, 90): Quaternion.identity;

            IKTargetVisible = false;
        }

        //動かすメイド設定
        public void SetMaid(Maid _maid, Transform _parentBone = null)
        {
            if (_maid == this.maid)
            {
                if (_parentBone == null)
                {
                    SetParentBone(this.trParentBone);
                }
                else
                {
                    SetParentBone(_parentBone);
                }
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
            return (initComplete && this.goHandleMasterObject != null) ? this.trParentBone : null;
        }

        //動かすボーン設定
        public void SetParentBone(Transform _trParentBone)
        {

            if (this.goHandleMasterObject == null)
            {

                Init();
            }
            if (_trParentBone != null)
            {
                //現在のボーンが渡されてきたら処理をスキップ
                if (_trParentBone != this.trParentBone)
                {
                    this.trParentBone = _trParentBone;
                    this.goHandleMasterObject.transform.parent = _trParentBone;//FindParent(maid.transform, "AllOffset");
                    this.goHandleMasterObject.transform.localPosition = Vector3.zero;//_parentBone.localPosition;//this.maid.gameObject.transform.localPosition;
                    this.goHandleMasterObject.transform.localRotation = Quaternion.identity;//_parentBone.localRotation;//this.maid.gameObject.transform.localRotation;

                    initComplete = true;

                    //ハンドル君の大きさ調整
                    if (IKmode == IKMODE.None)
                    {
                        if (_trParentBone.name == "Bip01" || _trParentBone.name == "AllOffset" || _trParentBone.name.Contains("Maid"))
                        {
                            handleScale = 1.0f;
                            /*
                            if(bHandlePositionMode)
                                Rot = Quaternion.Euler(-90, 0, 90);
                            else
                                Rot = Quaternion.identity;
                            */
                        }
                        else if (_trParentBone.name.Contains("Bip01") || _trParentBone.name.Contains("_IK_"))
                        {   //ハンドル君の大きさは子ボーンまでの長さに比例させる
                            int childBoneCount = 0;
                            handleScale = 0.0f;
                            for (int i = 0; i < trParentBone.childCount; ++i)
                            {
                                Transform childBone = _trParentBone.GetChild(i);
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
                        }
                        else
                        {
                            handleScale = 0.2f;
                        }
                    }
                    else
                    {   //ハンドル君がIKモードのときは大きさ固定
                        //ついでに角度も固定
                        //Rot = Quaternion.Euler(-90, 0, 90);
                        Rot = Quaternion.identity;
                        handleScale = 0.2f;
                    }
                    this.goHandleMasterObject.transform.localScale = Vector3.one * handleScale;
                }
            }
            else
            {
                //nullが来たら非表示にしとく
                this.goHandleMasterObject.SetActive(false);
                initComplete = false;
            }
        }

        //ハンドルのドラッグ状態取得
        public bool controllDragged()
        {
            if (!initComplete)
                return false;

            if (bHandlePositionMode == false)
            {
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



                    SetParentBone(trParentBone);
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
            else
            {
                if (controllOnMousePX.DragFinished || controllOnMousePY.DragFinished || controllOnMousePZ.DragFinished || controllOnMouseC.DragFinished)
                {
                    Visible = false;
                    controllOnMousePX.DragFinished = false;
                    controllOnMousePY.DragFinished = false;
                    controllOnMousePZ.DragFinished = false;
                    controllOnMouseC.DragFinished = false;


                    /*
                    if (this.rightClicked == true)
                    {
                        this.rightClicked = false;
                        Debuginfo.Log("右クリック2終了その2");
                    }
                    */

                    redvector.renderer.material.mainTexture = m_texture_red;
                    greenvector.renderer.material.mainTexture = m_texture_green;
                    bluevector.renderer.material.mainTexture = m_texture_blue;
                    whitecenter.renderer.material.mainTexture = bIKAttached ? m_texture_cyan : m_texture_white;

                    redvector.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                    greenvector.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                    bluevector.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));
                    whitecenter.renderer.material.SetColor("_Color", bIKAttached ? new Color(0, 1, 1, 0.5f) : new Color(1, 1, 1, 0.5f));

                    Visible = true;

                }

                //Debuginfo.Log("PX.Dragged" + controllOnMousePX.Dragged + "PY.Dragged" + controllOnMousePY.Dragged + "PZ.Dragged" + controllOnMousePZ.Dragged　+  "C.Dragged" + controllOnMouseC.Dragged);

                if (!controllOnMousePX.Dragged && !controllOnMousePY.Dragged && !controllOnMousePZ.Dragged && !controllOnMouseC.Dragged)
                {
                    //右クリック情報伝達用
                    //this.rightClicked |= controllOnMousePX.rightClicked;
                    //this.rightClicked |= controllOnMousePY.rightClicked;
                    //this.rightClicked |= controllOnMousePZ.rightClicked;


                    if (controllOnMousePX.mouseOver)
                    {
                        redvector.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        redvector.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        redvector.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                        redvector.renderer.material.mainTexture = m_texture_red_2;

                    }

                    if (controllOnMousePY.mouseOver)
                    {
                        greenvector.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        greenvector.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        greenvector.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                        greenvector.renderer.material.mainTexture = m_texture_green_2;

                    }

                    if (controllOnMousePZ.mouseOver)
                    {
                        bluevector.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        bluevector.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        bluevector.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));
                        bluevector.renderer.material.mainTexture = m_texture_blue_2;
                    }

                    if (controllOnMouseC.mouseOver)
                    {
                        /*
                        if (controllOnMouseC.rightClicked)
                        {
                            if (this.rightClicked == false)
                            {
                                this.rightClicked |= controllOnMouseC.rightClicked;
                                Debuginfo.Log("右クリック2");
                            }
                        }
                        else
                        {
                            if (this.rightClicked == true)
                            {
                                this.rightClicked = false;
                                Debuginfo.Log("右クリック2終了");
                            }                                
                        }
                        */
                        whitecenter.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        whitecenter.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        whitecenter.renderer.material.SetColor("_Color", bIKAttached ? new Color(0, 1, 1, 0.5f) : new Color(1, 1, 1, 0.5f));
                        whitecenter.renderer.material.mainTexture = bIKAttached ? m_texture_cyan : m_texture_white;
                    }
                }

                return (controllOnMousePX.Dragged || controllOnMousePY.Dragged || controllOnMousePZ.Dragged || controllOnMouseC.Dragged);
            }
        }

        public void resetHandleCoreColor()
        {
            whitecenter.renderer.material.SetColor("_Color", bIKAttached ? new Color(0, 1, 1, 0.5f) : new Color(1, 1, 1, 0.5f));
            whitecenter.renderer.material.mainTexture = bIKAttached ? m_texture_cyan : m_texture_white;
        }

        //IKターゲットのクリック終了後に呼び出すよう
        public void IKTargetClickAfter()
        {
            //Debuginfo.Log("IKハンドル君左クリック終了後");
            CoC.DragFinished = false;
            CoC.Dragged = false;
            goIKBoneTarget.renderer.material.mainTexture = m_texture_magenta;
            goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0f, 1f, 0.5f));

            whitecenter.renderer.material.SetColor("_Color", bIKAttached ? new Color(0, 1, 1, 0.5f) : new Color(1, 1, 1, 0.5f));
            whitecenter.renderer.material.mainTexture = bIKAttached ? m_texture_cyan : m_texture_white;

            //一旦再表示させないとハンドル君のコアが消える
            Visible = false;
            Visible = true;

            IKTargetVisible = false;
        }

        //IKターゲットのクリック状態取得
        public bool IKTargetClicked()
        {
            if (!initComplete)
            {
                return false;
            }
            else if (CoC.centerClicked == true)
            {
                CoC.centerClicked = false;

                IKTargetVisible = false;

                //Debuginfo.Log("IKハンドル君中クリック");
                return false;
            }
            else
            {
                if (CoC.DragFinished)
                {
                    //Debuginfo.Log("IKハンドル君左クリック終了");
                    CoC.DragFinished = false;

                    goIKBoneTarget.renderer.material.mainTexture = m_texture_magenta;
                    goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0f, 1f, 0.5f));

                    IKTargetVisible = false;
                }
                if (!CoC.Dragged)
                {
                    //Debuginfo.Log("IKハンドル君左クリック");
                    if (CoC.mouseOver)
                    {
                        goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                        goIKBoneTarget.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        goIKBoneTarget.renderer.material.mainTexture = m_texture_magenta;
                        goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0f, 1f, 0.5f));
                    }
                }
                return CoC.Dragged;
            }
        }

        public void IKTargetAttachedColor(bool _attached)
        {
            if (!initComplete)
            {
                return;
            }

            bIKAttached = _attached;
        }

        public void Proc()
        {
            if (!initComplete) return;

            //検知用のGizmoRenderが消えたらハンドル君も消える
            if (gizmoRender.Visible != Visible)
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

        //どの軸がドラッグされてるのか判別してドラッグ移動量を返す
        public Vector3 DeltaVector()
        {
            if (!initComplete) return Vector3.zero;
            if (controllOnMousePX.Dragged)
            {
                if (IKTargetVisible == true) IKTargetVisible = false;

                return /*transform.localToWorldMatrix.MultiplyVector*/(controllOnMousePX.dragVector);
            }
            else if (controllOnMousePY.Dragged)
            {
                if (IKTargetVisible == true) IKTargetVisible = false;

                return /*transform.localToWorldMatrix.MultiplyVector*/(controllOnMousePY.dragVector);
            }
            else if (controllOnMousePZ.Dragged)
            {
                if (IKTargetVisible == true) IKTargetVisible = false;

                return /*transform.localToWorldMatrix.MultiplyVector*/(controllOnMousePZ.dragVector);
            }
            else if (controllOnMouseC.Dragged)
            {
                if (IKTargetVisible == true) IKTargetVisible = false;

                return controllOnMouseC.dragVector;
            }
            else
            {
                return Vector3.zero;
            }
        }

        /*
        public void setIKmode(bool _IKenable)
        {
            controllOnMouseC.ikmode = _IKenable;
        }
        */
        public void Destroy()
        {
            if (this.goHandleMasterObject)
            {
                Debuginfo.Log("AngleHandle:Destroy!");
                GameObject.Destroy(this.goHandleMasterObject);

            }
            initComplete = false;
        }
    }

    public class AddBoneSlider : UnityInjector.PluginBase
    {

        #region Constants
        

        public const string PluginName = "AddBoneSlider";
        public const string Version = "0.0.1.4";

        private readonly string LogLabel = AddBoneSlider.PluginName + " : ";

        private readonly float TimePerInit = 1.00f;

        //private readonly int IKCalc = 3;

        private readonly float clickCheckOffsetInit = 40f;


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
        //private Dictionary<string, Dictionary<string, float>> undoValue = new Dictionary<string, Dictionary<string, float>>();

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
        public Dictionary<string, Quaternion> vPastBoneAngle = new Dictionary<string, Quaternion>();
        private Dictionary<string, Transform> trPoseImgUnit = new Dictionary<string, Transform>();
        public Vector3 vPastBoneTrans;

        //Undo履歴
        //private LinkedList<UndoBase> llUndoList　= new LinkedList<UndoBase>();
        private UndoList undoList = new UndoList();
        private String UndofuncName;

        public string activeHandleName = "";
        HandleKun posHandle;

        public string PoseXmlFileName;
        public string PoseTexDirectoryName;
        public string iniFileName;
        public string PoseName  ="";

        //本体側にIKが使えないのでこちらで用意
        /*
        public Dictionary<int, TBody.IKCMO> IKLeftLeg = new Dictionary<int, TBody.IKCMO>();
        public Dictionary<int, TBody.IKCMO> IKRightLeg = new Dictionary<int, TBody.IKCMO>();
        public Dictionary<int, TBody.IKCMO> IKLeftArm = new Dictionary<int, TBody.IKCMO>();
        public Dictionary<int, TBody.IKCMO> IKRightArm = new Dictionary<int, TBody.IKCMO>();
        
        public Dictionary<int, IKCONSTRAINED> IKLeftLeg = new Dictionary<int, IKCONSTRAINED>();
        public Dictionary<int, IKCONSTRAINED> IKRightLeg = new Dictionary<int, IKCONSTRAINED>();
        public Dictionary<int, IKCONSTRAINED> IKLeftArm = new Dictionary<int, IKCONSTRAINED>();
        public Dictionary<int, IKCONSTRAINED> IKRightArm = new Dictionary<int, IKCONSTRAINED>();

        //IK脚腕のアタッチ状態判別用
        public Dictionary<int, bool> bIKAttachLeftLeg = new Dictionary<int, bool>();
        public Dictionary<int, bool> bIKAttachRightLeg = new Dictionary<int, bool>();
        public Dictionary<int, bool> bIKAttachLeftArm = new Dictionary<int, bool>();
        public Dictionary<int, bool> bIKAttachRightArm = new Dictionary<int, bool>();

        //IKターゲット用
        public Dictionary<int, GameObject> goIKLeftLegTarget = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> goIKRightLegTarget = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> goIKLeftArmTarget = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> goIKRightArmTarget = new Dictionary<int, GameObject>();

        //IK対象ボーンのtransform
        public Dictionary<int, Transform[]> trIKLeftLegBones = new Dictionary<int, Transform[]>();
        public Dictionary<int, Transform[]> trIKRightLegBones = new Dictionary<int, Transform[]>();
        public Dictionary<int, Transform[]> trIKLeftArmBones = new Dictionary<int, Transform[]>();
        public Dictionary<int, Transform[]> trIKRightArmBones = new Dictionary<int, Transform[]>();


        //アタッチ状態が付与されたメイドリスト
        public Dictionary<int, Maid> attachIKMaidList = new Dictionary<int, Maid>();
        public List<int> attachIKMaidNo = new List<int>();
        public HashSet<int> DeleteNoList = new HashSet<int>();


        //IKをアタッチする_IK_ボーンのtransform
        //public Dictionary<int, Transform[]> trTargetIKBones = new Dictionary<int, Transform[]>();
        public Dictionary<int, Transform[]> trTargetIKBones = new Dictionary<int, Transform[] >();
        public Transform trTargetIKTemp = null;
        public Dictionary<string, string> sIKBoneName = new Dictionary<string, string>()
        {
            {"_IK_handL","左手" },
            {"_IK_handR","右手" },
            {"_IK_footL","左足" },
            {"_IK_footR","右足" },
            {"_IK_hohoL","左頬" },
            {"_IK_hohoR","右頬" },
            {"_IK_muneL","左胸" },
            {"_IK_muneR","右胸" },
            {"_IK_hara","お腹" },
            {"_IK_hipL","左尻" },
            {"_IK_hipR","右尻" },
            {"_IK_anal","後穴" },
            {"_IK_vagina","前穴" },
            {"Bip01","解除" }
        };
        bool bIKTargetGet = false;

        */
        private IKManage ikManage;//= new IKManage();

        //


        //IKターゲット操作用ハンドル君×4
        //HandleKun LeftLegHandle;
        //HandleKun RightLegHandle;
        //HandleKun LeftArmHandle;
        //HandleKun RightArmHandle;
        //リストラ



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

        //Undo履歴管理用
        private class UndoList
        {
            //Undo履歴
            LinkedList<UndoBase> _llundolist;
            //Redo履歴
            LinkedList<UndoBase> _llredolist;
            //Undo容量
            int size;

            public UndoList(int _size = 10)
            {
                _llundolist = new LinkedList<UndoBase>();
                _llredolist = new LinkedList<UndoBase>();
                size = _size;
            }
            
            public void Add(UndoBase _undo)
            {
                //Redo履歴があれば消去
                if (_llredolist.Count() > 0)
                {
                    _llredolist.Clear();
                }
                //Undo履歴が容量を超えてたら
                //先頭を削除
                if (_llundolist.Count() >= size)
                {
                    _llundolist.RemoveFirst();
                }
                //Undo要素を最後尾に追加
                _llundolist.AddLast(_undo);

            }

            public void doUndo()
            {
                if (_llundolist.Count() > 0)
                {

                    //Undo履歴の最後尾の要素のUndoを実行
                    UndoBase redo = _llundolist.Last().doUndo();

                    //Undoが成功していればRedo要素をRedo履歴に加える
                    if (redo != null)
                    {
                        _llredolist.AddLast(redo);
                    }

                    //Undo履歴の最後尾の要素を削除
                    _llundolist.RemoveLast();
                }
            }

            public void doRedo()
            {
                if (_llredolist.Count() > 0)
                {
                    //Redo履歴の最後尾の要素のUndo(Redo)を実行
                    UndoBase undo = _llredolist.Last().doUndo();

                    //Redoが成功していればUndo要素をUndo履歴に加える
                    if (undo != null)
                    {
                        _llundolist.AddLast(undo);
                    }

                    //Redo履歴の最後尾の要素を削除
                    _llredolist.RemoveLast();
                }
            }
        }
        

        //各Undo要素の大元
        private interface UndoBase
        {
            UndoBase doUndo();

            //void changeRedo();


        }

        private class UndoBone : UndoBase
        {
            private Dictionary<Transform, Quaternion> qBonelocal;

            public UndoBone(Dictionary<Transform, Quaternion> _qBone)
            {
                qBonelocal = _qBone;
            }

            public UndoBone(Transform _tr, Quaternion _qua)
            {
                qBonelocal = new Dictionary<Transform, Quaternion>(){ {_tr, _qua} };

            }

            public UndoBone(Dictionary<String, Transform> _Dic)
            {
                qBonelocal = new Dictionary<Transform, Quaternion>();
                foreach(KeyValuePair < String, Transform> pair in _Dic)
                {
                    qBonelocal.Add(pair.Value, pair.Value.localRotation);
                }
            }
            public UndoBone()
            {
                qBonelocal = new Dictionary<Transform, Quaternion>();

            }
            public UndoBase doUndo()
            {
                foreach(Transform tr in qBonelocal.Keys)
                {
                    if (tr == null && tr.gameObject.activeInHierarchy == false)
                    {
                        return null;
                    }
                }

                //現在の状態を元にRedo要素を作成
                Dictionary<Transform, Quaternion> re_qBonelocal = new Dictionary<Transform, Quaternion>();
                foreach (KeyValuePair<Transform, Quaternion> pair in qBonelocal)
                {
                    re_qBonelocal.Add(pair.Key, pair.Key.localRotation);
                }

                foreach (KeyValuePair<Transform, Quaternion> pair in qBonelocal)
                {
                    pair.Key.localRotation = pair.Value;
                }

                return new UndoBone(re_qBonelocal);
            }

            public void Add(Transform _tr, Quaternion _qua)
            {
                qBonelocal.Add(_tr, _qua);
            }
            
        }

        private class UndoBonePos : UndoBase
        {

            private Vector3 vBonelocalPos;
            private Transform tr;
            
            public UndoBonePos(Transform _tr, Vector3 _vPos)
            {
                tr = _tr;
                vBonelocalPos = _vPos;

            }
            public UndoBase doUndo()
            {
                if (tr != null && tr.gameObject.activeInHierarchy)
                {
                    Vector3 _tempPos = tr.localPosition;

                    tr.localPosition = vBonelocalPos;

                    return new UndoBonePos(tr, _tempPos);
                }
                else
                {
                    return null;
                }
            }
            
        }

        private class UndoBoneAll : UndoBase
        {
            private Dictionary<Transform, Quaternion> qBonelocal;
            private Vector3 vBonelocalPos;
            //private Transform tr;

            public UndoBoneAll(Dictionary<Transform, Quaternion> _qBone, Vector3 _vPos)
            {
                qBonelocal = _qBone;
                vBonelocalPos = _vPos;
            }
            public UndoBoneAll(Dictionary<String, Transform> _Dic)
            {
                qBonelocal = new Dictionary<Transform, Quaternion>();
                foreach (KeyValuePair<String, Transform> pair in _Dic)
                {
                    qBonelocal.Add(pair.Value, pair.Value.localRotation);
                    if(pair.Key == "Bip01")
                    {
                        vBonelocalPos = pair.Value.localPosition;
                    }
                }
            }

            public UndoBoneAll(Dictionary<String, Transform> _trDic, Dictionary<String, Quaternion> _qDic,Vector3 _vPos)
            {
                qBonelocal = _trDic.Select( (k ,i) => new {k.Value, v = _qDic[k.Key]}).ToDictionary(a => a.Value, a => a.v);
                vBonelocalPos = _vPos;
            }

            public UndoBase doUndo()
            {
                //現在の状態を元にRedo要素を作成
                Vector3 _tempPos = Vector3.zero;
                Dictionary<Transform, Quaternion> re_qBonelocal = new Dictionary<Transform, Quaternion>();
                foreach (KeyValuePair<Transform, Quaternion> pair in qBonelocal)
                {
                    re_qBonelocal.Add(pair.Key, pair.Key.localRotation);
                    if(pair.Key.name == "Bip01")
                    {
                        _tempPos = pair.Key.localPosition;
                    }
                }

                foreach (KeyValuePair<Transform, Quaternion> pair in qBonelocal)
                {
                    pair.Key.localRotation = pair.Value;
                    if (pair.Key.name == "Bip01")
                    {
                        pair.Key.localPosition = vBonelocalPos;
                    }
                }

                return new UndoBoneAll(re_qBonelocal,_tempPos);

            }

        }

        private class UndoAllpos : UndoBase
        {
            private Vector3 allpos;
            private bool isPos;

            public UndoAllpos(Vector3 _vPos,bool _isPos = false)
            {
                allpos = _vPos;
                isPos = _isPos;
            }
            public UndoBase doUndo()
            {
                Vector3 _temp;
                if (isPos == true)
                {
                    _temp = GameMain.Instance.CharacterMgr.GetCharaAllOfsetPos();

                    GameMain.Instance.CharacterMgr.SetCharaAllPos(allpos);
                }
                else
                {
                    _temp = GameMain.Instance.CharacterMgr.GetCharaAllOfsetRot();

                    GameMain.Instance.CharacterMgr.SetCharaAllRot(allpos);
                }
                return new UndoAllpos( _temp,isPos);
            }

        }

        private class UndoOffset : UndoBase
        {
            private Vector3 offset;
            private Maid maid;
            private bool isPos;

            public UndoOffset(Maid _maid, Vector3 _Offset,bool _isPos = false)
            {
                maid = _maid;
                offset = _Offset;
                isPos = _isPos;
            }
            public UndoBase doUndo()
            {
                if(maid != null && maid.Visible == true)
                {
                    Vector3 _temp;
                    if (isPos == true)
                    {
                        _temp = maid.transform.localPosition;

                        maid.SetPos(offset);
                    }
                    else
                    {
                        _temp = maid.GetRot();

                        maid.SetRot(offset);
                    }

                    return new UndoOffset(maid, _temp,isPos);
                }
                else
                {
                    return null;
                }
            }
        }

        /*
        private class UndoCamera : UndoListBase
        {

            public UndoCamera()
            {

            }
        }
        */

        private class UndoEye : UndoBase
        {
            Maid maid;
            Quaternion qEye;
            bool isRight;

            public UndoEye(Maid _maid, Quaternion _qEye,bool _isRight )
            {
                maid = _maid;
                qEye = _qEye;
                isRight = _isRight;
            }
            public UndoBase doUndo()
            {
                if (maid != null && maid.Visible == true)
                {
                    Quaternion eye;
                    if (isRight)
                    {
                        eye = maid.body0.quaDefEyeR;
                        maid.body0.quaDefEyeR = qEye;
                    }
                    else
                    {
                        eye = maid.body0.quaDefEyeL;
                        maid.body0.quaDefEyeL = qEye;
                    }
                    return new UndoEye(maid,eye,isRight);
                }
                else
                {
                    return null;
                }
            }
        }

        private class UndoSecret : UndoBase
        {
            jiggleBone jMune;
            float updown;
            float yori;

            public UndoSecret(jiggleBone _mune, float _updown, float _yori)
            {
                jMune = _mune;
                updown = _updown;
                yori = _yori;
            }
            public UndoBase doUndo()
            {
                if (jMune != null && jMune.gameObject.activeInHierarchy == true)
                {
                    float tempud = jMune.MuneUpDown;
                    float _tempy = jMune.MuneYori;
                    jMune.MuneUpDown = updown;
                    jMune.MuneYori = yori;
                    return new UndoSecret(jMune,tempud,_tempy);
                }
                else
                {
                    return null;
                }
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
            //SceneLevel == 4(CBL版エディットモード)  と　SceneLevel21 == 21（CBL版公式撮影モード） も追加
            if (level != sceneLevel && ((sceneLevel == iSceneEdit) || (sceneLevel == iScenePhoto)||(sceneLevel == iSceneEditCBL) || (sceneLevel == iScenePhotoCBL)))
            {
                finalize();
            }

            if ((level == iSceneEdit) || (level == iScenePhoto)|| (level == iSceneEditCBL) || (level == iScenePhotoCBL) )
            {
                mp = new BoneParam();
                if (xmlLoad = mp.Init()) StartCoroutine(initCoroutine());
            }

            sceneLevel = level;
        }

        public void Update()
        {
            if (((sceneLevel == iSceneEdit) || (sceneLevel == iScenePhoto)|| (sceneLevel == iSceneEditCBL) || (sceneLevel == iScenePhotoCBL)) && bInitCompleted)
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
                                ikTargetClicked();

                            }

                            if(posHandle.IKmode == HandleKun.IKMODE.None)
                            {
                                syncFromHandle();
                            }
                            else
                            {
                                ikManage.inversekinematicHandle(posHandle, maid, currentMaidNo);
                            }
                            /*
                            switch (posHandle.IKmode)
                            {
                                case HandleKun.IKMODE.None:
                                    syncFromHandle();
                                    break;

                                case HandleKun.IKMODE.LeftLeg:
                                    bool temp1 = inversekinematicFromHandle(goIKLeftLegTarget[currentMaidNo].transform, bIKAttachLeftLeg[currentMaidNo]);
                                    bIKAttachLeftLeg[currentMaidNo] |= temp1;
                                    break;

                                case HandleKun.IKMODE.RightLeg:
                                    bool temp2 = inversekinematicFromHandle(goIKRightLegTarget[currentMaidNo].transform, bIKAttachRightLeg[currentMaidNo]);
                                    bIKAttachRightLeg[currentMaidNo] |= temp2;
                                    break;

                                case HandleKun.IKMODE.LeftArm:
                                    bool temp3 = inversekinematicFromHandle(goIKLeftArmTarget[currentMaidNo].transform, bIKAttachLeftArm[currentMaidNo]);
                                    bIKAttachLeftArm[currentMaidNo] |= temp3;
                                    break;

                                case HandleKun.IKMODE.RightArm:
                                    bool temp4 = inversekinematicFromHandle(goIKRightArmTarget[currentMaidNo].transform, bIKAttachRightArm[currentMaidNo]);
                                    bIKAttachRightArm[currentMaidNo] |= temp4;
                                    break;

                                default:

                                    Debug.Log(LogLabel+"Handle IKmode exception.");
                                    break;
                            }
                            */




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

            ikManage.lateupdateFunc(posHandle);


            //ハンドル君がIKボーンにアタッチした状態で
            //そのIKボーンをもつメイドさんがいなくなると
            //ハンドル君もいっしょに死ぬので
            //ハンドル君が死んでないかチェック
            if (posHandle.GetParentBone() == null)
            {
                Debuginfo.Log(LogLabel + "ハンドル君にリザします");
                posHandle.Init(true);
                rebootHandle();

                /*
                posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
                */

            }
            //ハンドル君がIKボーンにアタッチした状態で
            //そのIKボーンをもつメイドさんが消えると
            //ハンドル君もいっしょに消えるので
            //ハンドル君が消えてないかチェック
            if (posHandle.GetParentBone().gameObject.activeInHierarchy == false)
            {
                Debuginfo.Log(LogLabel + "ハンドル君のバニシュを解除します");
                rebootHandle();

                /*
                posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
                */
            }




                /*
                foreach (int m in attachIKMaidNo)
                {
                    Debuginfo.Log(LogLabel + "finalize1");
                    //メイドさんがいなくなっていればスキップして
                    //リストから除外
                    if (attachIKMaidList[m] == null || attachIKMaidList[m].Visible == false)
                    {
                        Debuginfo.Log(LogLabel + "maid[" + m + "] is LOST");
                        removeAttachMaidList(m);
                        DeleteNoList.Add(m);

                    }
                }

                //このタイミングでattachIKMaidNoの要素削除しないとInvalidOperationExceptionが出る


                attachIKMaidNo.RemoveAll(DeleteNoList.Contains);

                DeleteNoList.Clear();


                foreach (int m in attachIKMaidNo)
                {
                    Debuginfo.Log(LogLabel + "finalize2");

                    if (bIKAttachLeftLeg.ContainsKey(m) && bIKAttachLeftLeg[m])
                    {

                        //公式撮影でアタッチ対象のメイドさんがいなくなった場合
                        if (goIKLeftLegTarget[m] == null)
                        {
                            Debuginfo.Log(LogLabel + "LeftLegIK is null!");

                            GameObject tempIKTarget = new GameObject();
                            tempIKTarget.transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKLeftLegTarget[m] = tempIKTarget;
                            goIKLeftLegTarget[m].transform.position = trIKLeftLegBones[m][2].position;

                            //if(trTargetIKBones.ContainsKey(m))
                            //   trTargetIKBones.Remove(m);

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんがいなくなると
                            //ハンドル君もいっしょに死ぬので
                            //ハンドル君が死んでないかチェック


                            if (posHandle.GetParentBone() == null)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君にリザします");
                                posHandle.Init(true);
                                posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.resetHandleCoreColor();
                                //posHandle.setVisible(false);
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);

                            }
                        }
                        else if (goIKLeftLegTarget[m].activeInHierarchy == false)
                        {
                            //複数撮影でアタッチ対象のメイドさんがいなくなった場合
                            Debuginfo.Log(LogLabel + "LeftLegIK is invisible!");

                            goIKLeftLegTarget[m].transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKLeftLegTarget[m].transform.position = trIKLeftLegBones[m][2].position;

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんが消えると
                            //ハンドル君もいっしょに消えるので
                            //ハンドル君が消えてないかチェック
                            if (posHandle.GetParentBone().gameObject.activeInHierarchy == false)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君のバニシュを解除します");
                                posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.resetHandleCoreColor();
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
                            }

                        }

                        IKLeftLeg[m].Proc(trIKLeftLegBones[m][0], trIKLeftLegBones[m][1], trIKLeftLegBones[m][2], goIKLeftLegTarget[m].transform.position);

                        //for (int i = 0; i < IKCalc; ++i)
                        //    IKLeftLeg[m].Porc(trIKLeftLegBones[m][0], trIKLeftLegBones[m][1], trIKLeftLegBones[m][2], goIKLeftLegTarget[m].transform.position, Vector3.zero);
                    }

                    if (bIKAttachRightLeg.ContainsKey(m) && bIKAttachRightLeg[m])
                    {
                        if (goIKRightLegTarget[m] == null)
                        {
                            Debuginfo.Log(LogLabel + "RightLegIK is null!");

                            GameObject tempIKTarget = new GameObject();
                            tempIKTarget.transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKRightLegTarget[m] = tempIKTarget;
                            goIKRightLegTarget[m].transform.position = trIKRightLegBones[m][2].position;

                            //if(trTargetIKBones.ContainsKey(m))
                            //   trTargetIKBones.Remove(m);

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんがいなくなると
                            //ハンドル君もいっしょに死ぬので
                            //ハンドル君が死んでないかチェック
                            if (posHandle.GetParentBone() == null)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君にリザします");
                                posHandle.Init(true);
                                posHandle.SetMaid(maid, trBone["Bip01 R Foot"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.resetHandleCoreColor();
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);

                            }
                            else if (goIKRightLegTarget[m].activeInHierarchy == false)
                            {
                                Debuginfo.Log(LogLabel + "RightLegIK is invisible!");

                                goIKRightLegTarget[m].transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                                goIKRightLegTarget[m].transform.position = trIKRightLegBones[m][2].position;

                                //ハンドル君がIKボーンにアタッチした状態で
                                //そのIKボーンをもつメイドさんが消えると
                                //ハンドル君もいっしょに消えるので
                                //ハンドル君が消えてないかチェック
                                if (posHandle.GetParentBone().gameObject.activeInHierarchy == false)
                                {
                                    Debuginfo.Log(LogLabel + "ハンドル君のバニシュを解除します");
                                    posHandle.SetMaid(maid, trBone["Bip01 R Foot"]);
                                    posHandle.IKTargetAttachedColor(false);
                                    posHandle.setVisible(false);

                                    FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);
                                }

                            }

                        }
                        IKRightLeg[m].Proc(trIKRightLegBones[m][0], trIKRightLegBones[m][1], trIKRightLegBones[m][2], goIKRightLegTarget[m].transform.position);

                        //for (int i = 0; i < IKCalc; ++i)
                        //    IKRightLeg[m].Porc(trIKRightLegBones[m][0], trIKRightLegBones[m][1], trIKRightLegBones[m][2], goIKRightLegTarget[m].transform.position, Vector3.zero);
                    }


                    if (bIKAttachLeftArm.ContainsKey(m) && bIKAttachLeftArm[m])
                    {

                        if (goIKLeftArmTarget[m] == null)
                        {
                            Debuginfo.Log(LogLabel + "LeftArmIK　is null!");

                            GameObject tempIKTarget = new GameObject();
                            tempIKTarget.transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKLeftArmTarget[m] = tempIKTarget;
                            goIKLeftArmTarget[m].transform.position = trIKLeftArmBones[m][2].position;

                            //if(trTargetIKBones.ContainsKey(m))
                            //   trTargetIKBones.Remove(m);

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんがいなくなると
                            //ハンドル君もいっしょに死ぬので
                            //ハンドル君が死んでないかチェック
                            if (posHandle.GetParentBone() == null)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君にリザします");
                                posHandle.Init(true);
                                posHandle.SetMaid(maid, trBone["Bip01 L Hand"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);

                            }
                        }
                        else if (goIKLeftArmTarget[m].activeInHierarchy == false)
                        {
                            Debuginfo.Log(LogLabel + "LeftArmIK is invisible!");

                            goIKLeftArmTarget[m].transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKLeftArmTarget[m].transform.position = trIKLeftArmBones[m][2].position;

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんが消えると
                            //ハンドル君もいっしょに消えるので
                            //ハンドル君が消えてないかチェック
                            if (posHandle.GetParentBone().gameObject.activeInHierarchy == false)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君のバニシュを解除します");
                                posHandle.SetMaid(maid, trBone["Bip01 L Hand"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.resetHandleCoreColor();
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);
                            }

                        }
                        if (attachIKMaidList[m].body0.tgtHandL == null && attachIKMaidList[m].body0.tgtHandL_AttachName == string.Empty)
                            IKLeftArm[m].Proc(trIKLeftArmBones[m][0], trIKLeftArmBones[m][1], trIKLeftArmBones[m][2], goIKLeftArmTarget[m].transform.position);

                        //for (int i = 0; i < IKCalc; ++i)
                        //    IKLeftArm[m].Porc(trIKLeftArmBones[m][0], trIKLeftArmBones[m][1], trIKLeftArmBones[m][2], goIKLeftArmTarget[m].transform.position, Vector3.zero);
                    }
                    if (bIKAttachRightArm.ContainsKey(m) && bIKAttachRightArm[m])
                    {

                        if (goIKRightArmTarget[m] == null)
                        {
                            Debuginfo.Log(LogLabel + "RightArmIK is null!");

                            GameObject tempIKTarget = new GameObject();
                            tempIKTarget.transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKRightArmTarget[m] = tempIKTarget;
                            goIKRightArmTarget[m].transform.position = trIKRightArmBones[m][2].position;

                            //if(trTargetIKBones.ContainsKey(m))
                            //   trTargetIKBones.Remove(m);

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんがいなくなると
                            //ハンドル君もいっしょに死ぬので
                            //ハンドル君が死んでないかチェック
                            if (posHandle.GetParentBone() == null)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君にリザします");
                                posHandle.Init(true);
                                posHandle.SetMaid(maid, trBone["Bip01 R Hand"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);

                            }
                        }
                        else if (goIKRightArmTarget[m].activeInHierarchy == false)
                        {
                            Debuginfo.Log(LogLabel + "RightArmIK is invisible!");

                            goIKRightArmTarget[m].transform.parent = CMT.SearchObjName(attachIKMaidList[m].body0.m_Bones.transform, "Bip01", true);
                            goIKRightArmTarget[m].transform.position = trIKRightArmBones[m][2].position;

                            //ハンドル君がIKボーンにアタッチした状態で
                            //そのIKボーンをもつメイドさんが消えると
                            //ハンドル君もいっしょに消えるので
                            //ハンドル君が消えてないかチェック
                            if (posHandle.GetParentBone().gameObject.activeInHierarchy == false)
                            {
                                Debuginfo.Log(LogLabel + "ハンドル君のバニシュを解除します");
                                posHandle.SetMaid(maid, trBone["Bip01 R Hand"]);
                                posHandle.IKTargetAttachedColor(false);
                                posHandle.resetHandleCoreColor();
                                posHandle.setVisible(false);

                                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);
                            }

                        }
                        //腕IKが本体側で何か設定されていればそちらを優先
                        if (attachIKMaidList[m].body0.tgtHandR == null && attachIKMaidList[m].body0.tgtHandR_AttachName == string.Empty)
                            IKRightArm[m].Proc(trIKRightArmBones[m][0], trIKRightArmBones[m][1], trIKRightArmBones[m][2], goIKRightArmTarget[m].transform.position);

                        //for (int i = 0; i < IKCalc; ++i)
                        //    IKRightArm[m].Porc(trIKRightArmBones[m][0], trIKRightArmBones[m][1], trIKRightArmBones[m][2], goIKRightArmTarget[m].transform.position, Vector3.zero);
                    }

                }
                */
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

                if ((sceneLevel == iSceneEdit || sceneLevel == iSceneEditCBL) && bone == "allpos")
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


                    //Undoチェック
                    Debuginfo.Log(LogLabel + "PoseLoad:" + maid.name);
                    UndofuncName = "PoseLoad:" + maid.name;

                    //Undo履歴に加える
                    undoList.Add(new UndoBoneAll(trBone));


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
                            /*
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
                            */
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
                    /*
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
                    */
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
            //右クリックしたとき、anmファイル出力用のパネルを展開
            else if (UICamera.currentTouchID == -2)
            {
                
                goPNamePanel.SetActive(true);
            }
        }


        public void OnClickIKLeftLeg()
        {
            try
            {
                if (UICamera.currentTouchID == -1)
                {
                    //カーソルの状態で判別
                    //bool bToggle = !(UIButton.current.defaultColor.a == 1f);
                    bool bToggle = !FindChild(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {

                        //以下、IK用変数初期化処理
                        Transform[] boneList = { trBone["Bip01 L Thigh"], trBone["Bip01 L Calf"], trBone["Bip01 L Foot"] };
                        ikManage.initList(trBone["Bip01"], boneList, maid, currentMaidNo,1);

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
                        if(!ikManage.checkIKAttach(currentMaidNo,1) )
                        {
                            //Debuginfo.Log("init bIKAttachLeftLeg");
                            //posHandle.SetParentBone(trBone["Bip01"]);

                            posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                            //posHandle.Rot = Quaternion.Euler(-90, 0, 90) ;
                            //posHandle.Scale = 0.2f;
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTarget(currentMaidNo,1));
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

                    if (ikManage.checkParentName(currentMaidNo, 1))
                    {
                        posHandle.IKTargetAttachedColor(true);
                    }
                    else
                    {
                        posHandle.IKTargetAttachedColor(false);
                    }

                    //IKボタン類の表示状態を一括処理

                    setIKButtonActive(bToggle);

                }
                else if (UICamera.currentTouchID == -2)
                {   //右クリックでIK解除
                    /*
                    if (bIKAttachLeftLeg.ContainsKey(currentMaidNo))
                    {
                        bIKAttachLeftLeg[currentMaidNo] = false;
                        goIKLeftLegTarget[currentMaidNo].transform.parent = trBone["Bip01"];
                    }
                    */
                    ikManage.detachIK(trBone["Bip01"], currentMaidNo, 1);

                    if (posHandle.IKmode ==HandleKun.IKMODE.LeftLeg)
                    {
                        posHandle.SetParentBone(trBone["Bip01 L Foot"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }
                   
                    setIKButtonActive(false);
                    setButtonColor(UIButton.current, false);
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
                    bool bToggle = !FindChild(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {

                        //以下、IK用変数初期化処理
                        Transform[] boneList = { trBone["Bip01 R Thigh"], trBone["Bip01 R Calf"], trBone["Bip01 R Foot"] };
                        ikManage.initList(trBone["Bip01"], boneList, maid, currentMaidNo, 2);

                        /*
                        if (!IKRightLeg.ContainsKey(currentMaidNo))
                        {
                            IKCONSTRAINED ikTempRightLeg = new IKCONSTRAINED();
                            float[,] constrait =
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
                            ikTempRightLeg.Init(trBone["Bip01 R Thigh"], trBone["Bip01 R Calf"], trBone["Bip01 R Foot"], maid.body0, constrait);
                            IKRightLeg.Add(currentMaidNo, ikTempRightLeg);
                        }

                        //IK対象ボーンリストが設定されていなければ初期化

                        if (!trIKRightLegBones.ContainsKey(currentMaidNo))
                        {
                            Transform[] boneList = { trBone["Bip01 R Thigh"], trBone["Bip01 R Calf"], trBone["Bip01 R Foot"] };
                            trIKRightLegBones.Add(currentMaidNo, boneList);
                        }

                        //IKアタッチ状態が設定されていなければ一時表示[None]で初期化設定

                        if (!bIKAttachRightLeg.ContainsKey(currentMaidNo))
                        {
                            bIKAttachRightLeg.Add(currentMaidNo, false);
                        }

                        //IKターゲットが生成されてなければ生成

                        if (!goIKRightLegTarget.ContainsKey(currentMaidNo))
                        {
                            GameObject tempIKRightLegTarget = new GameObject();
                            tempIKRightLegTarget.transform.parent = trBone["Bip01"];
                            goIKRightLegTarget.Add(currentMaidNo, tempIKRightLegTarget);

                            bIKAttachRightLeg[currentMaidNo] = false;
                        }
                        */

                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.RightLeg;
                        //if (!bIKAttachRightLeg.ContainsKey(currentMaidNo) || !bIKAttachRightLeg[currentMaidNo])
                        if (!ikManage.checkIKAttach(currentMaidNo, 2))
                        {
                            posHandle.SetMaid(maid, trBone["Bip01 R Foot"]);
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTarget(currentMaidNo, 2));
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

                    setIKButtonActive(bToggle);

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
                    /*
                    if (bIKAttachRightLeg.ContainsKey(currentMaidNo))
                    {
                        bIKAttachRightLeg[currentMaidNo] = false;
                        goIKRightLegTarget[currentMaidNo].transform.parent = trBone["Bip01"];
                    }
                    */
                    ikManage.detachIK(trBone["Bip01"], currentMaidNo, 2);

                    if (posHandle.IKmode == HandleKun.IKMODE.RightLeg)
                    {
                        posHandle.SetParentBone(trBone["Bip01 R Foot"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }

                    setIKButtonActive(false);
                    setButtonColor(UIButton.current, false);
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
                    bool bToggle = !FindChild(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {
                        //本体側で腕IKが設定されていれば解除
                        if (maid.body0.tgtHandL != null || maid.body0.tgtHandL_AttachName != string.Empty)
                        {
                            maid.IKTargetToBone("左手", null, "無し", Vector3.zero);
                        }


                        //以下、IK用変数初期化処理

                        Transform[] boneList = { trBone["Bip01 L UpperArm"], trBone["Bip01 L Forearm"], trBone["Bip01 L Hand"] };
                        ikManage.initList(trBone["Bip01"], boneList, maid, currentMaidNo, 3);

                        //プラグイン側のIKが設定されていなければ初期化設定
                        /*

                        if (!IKLeftArm.ContainsKey(currentMaidNo))
                        {
                            TBody.IKCMO ikTempLeftArm = new TBody.IKCMO();
                            ikTempLeftArm.Init(trBone["Bip01 L UpperArm"], trBone["Bip01 L Forearm"], trBone["Bip01 L Hand"], maid.body0);
                            IKLeftArm.Add(currentMaidNo, ikTempLeftArm);
                        }

                        */
                        /*
                        if (!IKLeftArm.ContainsKey(currentMaidNo))
                        {
                            IKCONSTRAINED ikTempLeftArm = new IKCONSTRAINED();
                            float[,] constrait =
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
                            ikTempLeftArm.Init(trBone["Bip01 L UpperArm"], trBone["Bip01 L Forearm"], trBone["Bip01 L Hand"], maid.body0, constrait);
                            IKLeftArm.Add(currentMaidNo, ikTempLeftArm);
                        }

                        //IK対象ボーンリストが設定されていなければ初期化

                        if (!trIKLeftArmBones.ContainsKey(currentMaidNo))
                        {
                            Transform[] boneList = { trBone["Bip01 L UpperArm"], trBone["Bip01 L Forearm"], trBone["Bip01 L Hand"] };
                            trIKLeftArmBones.Add(currentMaidNo, boneList);
                        }

                        //IKアタッチ状態が設定されていなければ一時表示[None]で初期化設定

                        if (!bIKAttachLeftArm.ContainsKey(currentMaidNo))
                        {
                            bIKAttachLeftArm.Add(currentMaidNo, false);
                        }

                        //IKターゲットが生成されてなければ生成

                        if (!goIKLeftArmTarget.ContainsKey(currentMaidNo))
                        {
                            GameObject tempIKLeftArmTarget = new GameObject();
                            tempIKLeftArmTarget.transform.parent = trBone["Bip01"];
                            goIKLeftArmTarget.Add(currentMaidNo, tempIKLeftArmTarget);

                            bIKAttachLeftArm[currentMaidNo] = false;
                        }
                        */

                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.LeftArm;
                        //if (!bIKAttachLeftArm.ContainsKey(currentMaidNo) || !bIKAttachLeftArm[currentMaidNo])
                        if (!ikManage.checkIKAttach(currentMaidNo, 3))
                        {
                            posHandle.SetMaid(maid, trBone["Bip01 L Hand"]);
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTarget(currentMaidNo, 3));
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

                    setIKButtonActive(bToggle);

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
                    /*
                    if (bIKAttachLeftArm.ContainsKey(currentMaidNo))
                    {
                        bIKAttachLeftArm[currentMaidNo] = false;
                        goIKLeftArmTarget[currentMaidNo].transform.parent = trBone["Bip01"];
                    }
                    */
                    ikManage.detachIK(trBone["Bip01"], currentMaidNo, 3);

                    if (posHandle.IKmode == HandleKun.IKMODE.LeftArm)
                    {
                        posHandle.SetParentBone(trBone["Bip01 L Hand"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }

                    setIKButtonActive(false);
                    setButtonColor(UIButton.current, false);
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
                    bool bToggle = !FindChild(UIButton.current.gameObject, "SelectCursor").activeSelf;

                    //IKターゲットオブジェクトとターゲット操作用ハンドル君表示
                    if (bToggle)
                    {
                        //本体側で腕IKが設定されていれば解除

                        if (maid.body0.tgtHandR != null || maid.body0.tgtHandR_AttachName != string.Empty)
                        {
                            maid.IKTargetToBone("右手", null, "無し", Vector3.zero);
                        }

                        //以下、IK用変数初期化処理

                        Transform[] boneList = { trBone["Bip01 R UpperArm"], trBone["Bip01 R Forearm"], trBone["Bip01 R Hand"] };
                        ikManage.initList(trBone["Bip01"], boneList, maid, currentMaidNo, 4);

                        //プラグイン側のIKが設定されていなければ初期化設定
                        /*
                        if (!IKLeftLeg.ContainsKey(currentMaidNo))
                        {
                            TBody.IKCMO ikTempLeftLeg = new TBody.IKCMO();
                            ikTempLeftLeg.Init(trBone["Bip01 L Thigh"], trBone["Bip01 L Calf"], trBone["Bip01 L Foot"], maid.body0);
                            IKLeftLeg.Add(currentMaidNo, ikTempLeftLeg);
                        }
                        if (!IKRightLeg.ContainsKey(currentMaidNo))
                        {
                            TBody.IKCMO ikTempRightLeg = new TBody.IKCMO();
                            ikTempRightLeg.Init(trBone["Bip01 R Thigh"], trBone["Bip01 R Calf"], trBone["Bip01 R Foot"], maid.body0);
                            IKRightLeg.Add(currentMaidNo, ikTempRightLeg);
                        }
                        if (!IKLeftArm.ContainsKey(currentMaidNo))
                        {
                            TBody.IKCMO ikTempLeftArm = new TBody.IKCMO();
                            ikTempLeftArm.Init(trBone["Bip01 L UpperArm"], trBone["Bip01 L Forearm"], trBone["Bip01 L Hand"], maid.body0);
                            IKLeftArm.Add(currentMaidNo, ikTempLeftArm);
                        }
                        if (!IKRightArm.ContainsKey(currentMaidNo))
                        {
                            TBody.IKCMO ikTempRightArm = new TBody.IKCMO();
                            ikTempRightArm.Init(trBone["Bip01 R UpperArm"], trBone["Bip01 R Forearm"], trBone["Bip01 R Hand"], maid.body0);
                            IKRightArm.Add(currentMaidNo, ikTempRightArm);
                        }
                        */
                        /*
                        if (!IKRightArm.ContainsKey(currentMaidNo))
                        {
                            IKCONSTRAINED ikTempRightArm = new IKCONSTRAINED();
                            float[,] constrait =
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
                            ikTempRightArm.Init(trBone["Bip01 R UpperArm"], trBone["Bip01 R Forearm"], trBone["Bip01 R Hand"], maid.body0, constrait);
                            IKRightArm.Add(currentMaidNo, ikTempRightArm);
                        }

                        //IK対象ボーンリストが設定されていなければ初期化

                        if (!trIKRightArmBones.ContainsKey(currentMaidNo))
                        {
                            Transform[] boneList = { trBone["Bip01 R UpperArm"], trBone["Bip01 R Forearm"], trBone["Bip01 R Hand"] };
                            trIKRightArmBones.Add(currentMaidNo, boneList);
                        }

                        //IKアタッチ状態が設定されていなければ一時表示[None]で初期化設定

                        if (!bIKAttachRightArm.ContainsKey(currentMaidNo))
                        {
                            bIKAttachRightArm.Add(currentMaidNo, false);
                        }

                        //IKターゲットが生成されてなければ生成
                        if (!goIKRightArmTarget.ContainsKey(currentMaidNo))
                        {
                            GameObject tempIKRightArmTarget = new GameObject();
                            tempIKRightArmTarget.transform.parent = trBone["Bip01"];
                            goIKRightArmTarget.Add(currentMaidNo, tempIKRightArmTarget);

                            bIKAttachRightArm[currentMaidNo] = false;
                        }
                        */

                        //IK用ハンドル君のターゲットを今のメイドに設定
                        posHandle.ChangeHandleModePosition(true);
                        posHandle.IKmode = HandleKun.IKMODE.RightArm;
                        //if (!bIKAttachRightArm.ContainsKey(currentMaidNo) || !bIKAttachRightArm[currentMaidNo])
                        if (!ikManage.checkIKAttach(currentMaidNo, 4))
                        {
                            posHandle.SetMaid(maid, trBone["Bip01 R Hand"]);
                        }
                        else
                        {
                            posHandle.SetMaid(maid, ikManage.getIKTarget(currentMaidNo, 4));
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

                    setIKButtonActive(bToggle);

                    if (ikManage.checkParentName(currentMaidNo, 4))
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
                    /*
                    if (bIKAttachRightArm.ContainsKey(currentMaidNo))
                    {
                        bIKAttachRightArm[currentMaidNo] = false;
                        goIKRightArmTarget[currentMaidNo].transform.parent = trBone["Bip01"];
                    }
                    */
                    ikManage.detachIK(trBone["Bip01"], currentMaidNo, 4);

                    if (posHandle.IKmode == HandleKun.IKMODE.RightArm)
                    {
                        posHandle.SetParentBone(trBone["Bip01 R Hand"]);

                        posHandle.transform.localPosition = Vector3.zero;
                        posHandle.Scale = 0.2f;

                        posHandle.setVisible(false);
                    }

                    setIKButtonActive(false);
                    setButtonColor(UIButton.current, false);
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
                //IKを解除

                //ハンドル君の位置を初期化
                posHandle.SetParentBone(trBone["Bip01"]);

                //現在のメイドのIK情報をコレクションリストから全削除
                removeAttachMaidList(currentMaidNo);
                ikManage.attachIKMaidNo.Remove(currentMaidNo);

                //IK用ハンドル君非表示
                posHandle.setVisible(false);

                //IKボタンをオフらせる
                setIKButtonActive(false,true, ikManage.attachIKMaidNo.Count == 0);

                posHandle.IKTargetAttachedColor(false);

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKDetach() " + ex); return; }

        }
        //全メイドさんのIK全解除
        public void OnClickIKDetachAll()
        {
            try
            {
                //ハンドル君の位置を初期化
                posHandle.SetParentBone(trBone["Bip01"]);


                //全メイドのIK情報をコレクションリストから全削除
                ikManage.detachAll(currentMaidNo);
                /*
                //全ターゲット用オブジェクトを0.5秒後に消す
                foreach (int removeNo in attachIKMaidNo)
                {
                    
                    //GameObject.Destroy(goIKLeftLegTarget[removeNo], 0.5f);
                    //GameObject.Destroy(goIKRightLegTarget[removeNo], 0.5f);
                    //GameObject.Destroy(goIKLeftArmTarget[removeNo], 0.5f);
                    //GameObject.Destroy(goIKRightArmTarget[removeNo], 0.5f);
                    
                    if (goIKLeftLegTarget.ContainsKey(removeNo))
                    {

                        if (goIKLeftLegTarget[removeNo] != null)
                        {
                            goIKLeftLegTarget[removeNo].transform.DetachChildren();
                            GameObject.Destroy(goIKLeftLegTarget[removeNo], 0.5f);
                        }
                    }
                    if (goIKRightLegTarget.ContainsKey(removeNo))
                    {
                        if (goIKRightLegTarget[removeNo] != null)
                        {
                            goIKRightLegTarget[removeNo].transform.DetachChildren();
                            GameObject.Destroy(goIKRightLegTarget[removeNo], 0.5f);
                        }
                    }
                    if (goIKLeftArmTarget.ContainsKey(removeNo))
                    {
                        if (goIKLeftArmTarget[removeNo] != null)
                        {
                            goIKLeftArmTarget[removeNo].transform.DetachChildren();
                            GameObject.Destroy(goIKLeftArmTarget[removeNo], 1f);
                        }
                    }
                    if (goIKRightArmTarget.ContainsKey(removeNo))
                    {
                        if (goIKRightArmTarget[removeNo] != null)
                        {
                            goIKRightArmTarget[removeNo].transform.DetachChildren();
                            GameObject.Destroy(goIKRightArmTarget[removeNo], 1f);
                        }
                    }
                }

                attachIKMaidNo.Clear();
                attachIKMaidList.Clear();
                trTargetIKBones.Clear();

                IKLeftLeg.Clear();
                IKRightLeg.Clear();
                IKLeftArm.Clear();
                IKRightArm.Clear();

                bIKAttachLeftLeg.Clear();
                bIKAttachRightLeg.Clear();
                bIKAttachLeftArm.Clear();
                bIKAttachRightArm.Clear();
                
                goIKLeftLegTarget.Clear();
                goIKRightLegTarget.Clear();
                goIKLeftArmTarget.Clear();
                goIKRightArmTarget.Clear();

                trIKLeftLegBones.Clear();
                trIKRightLegBones.Clear();
                trIKLeftArmBones.Clear();
                trIKRightArmBones.Clear();
                */


                //IK用ハンドル君非表示
                posHandle.setVisible(false);
                posHandle.IKTargetAttachedColor(false);

                //IKボタンを全部オフらせる
                setIKButtonActive(false, true, true);

            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickIKDetachAll() " + ex); return; }
        }

        public void OnClickHandleButton()
        {
            try
            {
                posHandle.IKmode = HandleKun.IKMODE.None;

                string bone = getTag(UIButton.current, 1);

<<<<<<< HEAD
                

                    if (sceneLevel == 5 && bone == "allpos")
=======
                if ((sceneLevel == iSceneEdit|| sceneLevel == iSceneEditCBL) && bone == "allpos")
>>>>>>> develop05
                {
                    setParentAllOffset();
                }

                //IKハンドルモードからの切り替え処理も書く

                //if (UICamera.currentTouchID == -1)
                {
                    if (bone == "secret" || bone == "eye" || bone == "camera" )//|| bone == "offset" || bone == "allpos")
                        return;

                    if (activeHandleName != bone)
                    {
                        if (activeHandleName != "")
                        {
                            visibleHandle(activeHandleName);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);

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
                            return;
                        }
                    }
                    
                    //posHandle.Visible = b;
                }
                /*
                else if (UICamera.currentTouchID == -2)
                {
                    if (bone != "Bip01" && bone !="allpos" && bone !="offset" )
                        return;


                    if (activeHandleName != bone)
                    {
                        if (activeHandleName != "")
                        {
                            visibleHandle(activeHandleName);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);

                        }

                        if (bone == "allpos")
                            posHandle.SetParentBone(maid.gameObject.transform.parent);
                        else if (bone == "offset")
                            posHandle.SetParentBone(maid.gameObject.transform);
                        else
                            posHandle.SetParentBone(trBone[bone]);


                    }

                    if (posHandle.bHandlePositionMode == false )
                    {
                        posHandle.ChangeHandleModePosition(true);
                        if (posHandle.Visible == true && activeHandleName == bone)
                        {
                            return;
                        }
                    }
                    
                }
                */
                bool b = visibleHandle(bone);
                setUnitButtonColor(UIButton.current, b);
                //setUnitButtonColor(UIButton.current, b);
                activeHandleName = b ? bone : "";

                posHandle.IKTargetAttachedColor(false);
                posHandle.setVisible(b);
                

                //IKボタンのカーソルだけをオフらせる
                //setIKButtonActive(true, true);
                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);


            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickHandleButton() " + ex); return; }
        }

        //todo0014-03
        //ハンドルボタンを分割したので
        //色変え処理の変更
        public void OnClickPosHandleButton()
        {
            try
            {
                posHandle.IKmode = HandleKun.IKMODE.None;

                string bone = getTag(UIButton.current, 1);

                if (sceneLevel == 5 && bone == "allpos")
                {
                    setParentAllOffset();
                }

                //IKハンドルモードからの切り替え処理も書く
                /*
                if (UICamera.currentTouchID == -1)
                {
                    if (bone == "secret" || bone == "eye" || bone == "camera")//|| bone == "offset" || bone == "allpos")
                        return;

                    if (activeHandleName != bone)
                    {
                        if (activeHandleName != "")
                        {
                            visibleHandle(activeHandleName);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);

                        }


                        if (bone == "allpos")
                            posHandle.SetParentBone(maid.gameObject.transform.parent);
                        else if (bone == "offset")
                            posHandle.SetParentBone(maid.gameObject.transform);
                        else
                            posHandle.SetParentBone(trBone[bone]);
                    }

                    if (posHandle.bHandlePositionMode == true)
                    {
                        posHandle.ChangeHandleModePosition(false);

                        if (posHandle.Visible == true && activeHandleName == bone)
                        {
                            return;
                        }
                    }

                    //posHandle.Visible = b;
                }
                else if (UICamera.currentTouchID == -2)
                */
                {
                    
                    if (bone != "Bip01" && bone != "allpos" && bone != "offset")
                        return;


                    if (activeHandleName != bone)
                    {
                        if (activeHandleName != "")
                        {
                            visibleHandle(activeHandleName);
                            setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);

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
                            return;
                        }
                    }
                    
                }
                
                bool b = visibleHandle(bone);
                setUnitButtonColor(UIButton.current, b);
                activeHandleName = b ? bone : "";

                posHandle.IKTargetAttachedColor(false);
                posHandle.setVisible(b);


                //IKボタンのカーソルだけをオフらせる
                //setIKButtonActive(true, true);
                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);
                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);


            }
            catch (Exception ex) { Debug.LogError(LogLabel + "OnClickPosHandleButton() " + ex); return; }
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
                        setUnitButtonColor(bone, mp.bEnabled[bone]);
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

        public void OnClickMirrorAll()
        {
            
        }

        public void OnClickUndo()
        {
            UndofuncName = "Undo";
            undoList.doUndo();
        }

        public void OnClickRedo()
        {
            UndofuncName = "Redo";
            undoList.doRedo();
        }

        public void OnClickCopyCategoryButton()
        {

        }

        public void OnClickPasteCategoryButton()
        {

        }

        public void OnClickRight2LeftCategoryButton()
        {

        }
        
        public void OnClickLeft2RightCategoryButton()
        {

        }

        public void OnClickResetCategoryButton()
        {

        }

        public void OnClickMirrorCategoryButton()
        {

        }

        public void OnClickCopyButton()
        {

        }

        public void OnClickPasteButton()
        {

        }

        public void OnClickRight2LeftButton()
        {

        }

        public void OnClickLeft2RightButton()
        {

        }


        public void OnClickMirrorButton()
        {

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

                checkUndo("slider",bone,prop);

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

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
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

                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
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
                if(sceneLevel == iSceneEdit || sceneLevel == iSceneEditCBL)
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
                uiScrollPanel.SetRect(0f, 0f, uiBGSprite.width, uiBGSprite.height - 130 - systemUnitHeight * 7.5f);
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

<<<<<<< HEAD
                    //todo0.0.1.5-01
                    //サムネポーズ画像がないときに
                    //起動失敗する問題の修正
=======

                    //サムネポーズ紛失した時用
                    List<XmlNode> deleteNodes = new List<XmlNode>();



>>>>>>> develop05
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
                GameObject goUndo = SetCloneChild(goSystemUnit, goProfileTabCopy, "Undo");
                goUndo.transform.localPosition = new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);

                UISprite uiSpriteUndo = goUndo.GetComponent<UISprite>();
                uiSpriteUndo.SetDimensions((int)(conWidth * 0.25f) - 4, systemUnitHeight);

                UILabel uiLabelUndo = FindChild(goUndo, "Name").GetComponent<UILabel>();
                uiLabelUndo.width = uiSpriteUndo.width - 10;
                uiLabelUndo.fontSize = 22;
                uiLabelUndo.spacingX = 0;
                uiLabelUndo.supportEncoding = true;
                uiLabelUndo.text = "[111111]Undo";

                UIButton uiButtonUndo = goUndo.GetComponent<UIButton>();
                uiButtonUndo.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonUndo.onClick, new EventDelegate.Callback(this.OnClickUndo));

                FindChild(goUndo, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.5f) - 6, systemUnitHeight - 4);
                FindChild(goUndo, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goUndo);
                goUndo.SetActive(true);

                // Redoボタン
                GameObject goRedo = SetCloneChild(goSystemUnit, goProfileTabCopy, "Redo");
                goRedo.transform.localPosition = new Vector3(-conWidth * 0.125f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);

                UISprite uiSpriteRedo = goRedo.GetComponent<UISprite>();
                uiSpriteRedo.SetDimensions((int)(conWidth * 0.25f) - 4, systemUnitHeight);

                UILabel uiLabelRedo = FindChild(goRedo, "Name").GetComponent<UILabel>();
                uiLabelRedo.width = uiSpriteRedo.width - 10;
                uiLabelRedo.fontSize = 22;
                uiLabelRedo.spacingX = 0;
                uiLabelRedo.supportEncoding = true;
                uiLabelRedo.text = "[111111]Redo";

                UIButton uiButtonRedo = goRedo.GetComponent<UIButton>();
                uiButtonRedo.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonRedo.onClick, new EventDelegate.Callback(this.OnClickRedo));

                FindChild(goRedo, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.5f) - 6, systemUnitHeight - 4);
                FindChild(goRedo, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goRedo);
                goRedo.SetActive(true);


                // MirrorAllボタン
                GameObject goMirrorAll = SetCloneChild(goSystemUnit, goProfileTabCopy, "MirrorAll");
                goMirrorAll.transform.localPosition = new Vector3(conWidth * 0.125f - 4, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);

                UISprite uiSpriteMirrorAll = goMirrorAll.GetComponent<UISprite>();
                uiSpriteMirrorAll.SetDimensions((int)(conWidth * 0.25f) - 6, systemUnitHeight);

                UILabel uiLabelMirrorAll = FindChild(goMirrorAll, "Name").GetComponent<UILabel>();
                uiLabelMirrorAll.width = uiSpriteMirrorAll.width - 10;
                uiLabelMirrorAll.fontSize = 22;
                uiLabelMirrorAll.spacingX = 0;
                uiLabelMirrorAll.supportEncoding = true;
                uiLabelMirrorAll.text = "[111111]MirrorAll";

                UIButton uiButtonMirrorAll = goMirrorAll.GetComponent<UIButton>();
                uiButtonMirrorAll.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonMirrorAll.onClick, new EventDelegate.Callback(this.OnClickMirrorAll));

                FindChild(goMirrorAll, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.5f) - 6, systemUnitHeight - 4);
                FindChild(goMirrorAll, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goMirrorAll);
                goMirrorAll.SetActive(true);


                // ResetAllボタン
                GameObject goResetAll = SetCloneChild(goSystemUnit, goProfileTabCopy, "ResetAll");
                goResetAll.transform.localPosition = new Vector3(conWidth * 0.375f - 4, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);

                UISprite uiSpriteResetAll = goResetAll.GetComponent<UISprite>();
                uiSpriteResetAll.SetDimensions((int)(conWidth * 0.25f) - 2, systemUnitHeight);

                UILabel uiLabelResetAll = FindChild(goResetAll, "Name").GetComponent<UILabel>();
                uiLabelResetAll.width = uiSpriteResetAll.width - 10;
                uiLabelResetAll.fontSize = 22;
                uiLabelResetAll.spacingX = 0;
                uiLabelResetAll.supportEncoding = true;
                uiLabelResetAll.text = "[111111]ResetAll";

                UIButton uiButtonResetAll = goResetAll.GetComponent<UIButton>();
                uiButtonResetAll.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonResetAll.onClick, new EventDelegate.Callback(this.OnClickResetAll));

                FindChild(goResetAll, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.5f) - 6, systemUnitHeight - 4);
                FindChild(goResetAll, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goResetAll);
                goResetAll.SetActive(true);

                Debuginfo.Log(LogLabel + " goResetAll complete.");


                // 全IK解除ボタン
                GameObject goIKDetachAll = SetCloneChild(goSystemUnit, goProfileTabCopy, "IKDetachAll");
                //goIKDetachAll.transform.localPosition = new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);
                goIKDetachAll.transform.localPosition = new Vector3(-conWidth * 0.4167f - 7, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);

                UISprite uiSpriteIKDetachAll = goIKDetachAll.GetComponent<UISprite>();
                //uiSpriteIKDetachAll.SetDimensions((int)(conWidth * 0.25f) - 4, systemUnitHeight);
                uiSpriteIKDetachAll.SetDimensions((int)(conWidth * 0.1667f) - 5, systemUnitHeight);

                UILabel uiLabelIKDetachAll = FindChild(goIKDetachAll, "Name").GetComponent<UILabel>();
                uiLabelIKDetachAll.width = uiSpriteIKDetachAll.width - 10;
                uiLabelIKDetachAll.fontSize = 22;
                uiLabelIKDetachAll.spacingX = 0;
                uiLabelIKDetachAll.supportEncoding = true;
                uiLabelIKDetachAll.text = "[111111]全解除";

                UIButton uiButtonIKDetachAll = goIKDetachAll.GetComponent<UIButton>();
                uiButtonIKDetachAll.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonIKDetachAll.onClick, new EventDelegate.Callback(this.OnClickIKDetachAll));

                //FindChild(goIKDetachAll, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.25f) - 10, systemUnitHeight - 4);
                FindChild(goIKDetachAll, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.1667f) - 14, systemUnitHeight - 4);

                FindChild(goIKDetachAll, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goIKDetachAll);
                goIKDetachAll.SetActive(true);

                // IKアタッチ解除ボタン
                GameObject goIKDetach = SetCloneChild(goSystemUnit, goIKDetachAll, "IKDetach");
                //goIKDetach.transform.localPosition = new Vector3(-conWidth * 0.125f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 4 - 6f, 0f);
                goIKDetach.transform.localPosition = new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);

                UILabel uiLabelIKDetach = FindChild(goIKDetach, "Name").GetComponent<UILabel>();
                uiLabelIKDetach.text = "[111111]IK解除";

                UIButton uiButtonIKDetach = goIKDetach.GetComponent<UIButton>();
                uiButtonIKDetach.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonIKDetach.onClick, new EventDelegate.Callback(this.OnClickIKDetach));

                FindChild(goIKDetach, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.25f) - 10, systemUnitHeight - 4);
                FindChild(goIKDetach, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goIKDetach);
                goIKDetach.SetActive(true);

                // 左足IKボタン
                GameObject goIKLeftLeg = SetCloneChild(goSystemUnit, goIKDetachAll, "IKLeftLeg");
                //goIKLeftLeg.transform.localPosition = new Vector3(-conWidth * 0.375f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                goIKLeftLeg.transform.localPosition = new Vector3(-conWidth * 0.08333f - 5, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);

                UILabel uiLabelIKLeftLeg = FindChild(goIKLeftLeg, "Name").GetComponent<UILabel>();
                uiLabelIKLeftLeg.text = "[111111]左足IK";

                UIButton uiButtonIKLeftLeg = goIKLeftLeg.GetComponent<UIButton>();
                uiButtonIKLeftLeg.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonIKLeftLeg.onClick, new EventDelegate.Callback(this.OnClickIKLeftLeg));

                FindChild(goIKLeftLeg, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.25f) - 10, systemUnitHeight - 4);
                FindChild(goIKLeftLeg, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goIKLeftLeg);
                goIKLeftLeg.SetActive(true);

                // 右足IKボタン
                GameObject goIKRightLeg = SetCloneChild(goSystemUnit, goIKDetachAll, "IKRightLeg");
                //goIKRightLeg.transform.localPosition = new Vector3(-conWidth * 0.125f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                goIKRightLeg.transform.localPosition = new Vector3(conWidth * 0.08333f - 4, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);

                UILabel uiLabelIKRightLeg = FindChild(goIKRightLeg, "Name").GetComponent<UILabel>();
                uiLabelIKRightLeg.text = "[111111]右足IK";

                UIButton uiButtonIKRightLeg = goIKRightLeg.GetComponent<UIButton>();
                uiButtonIKRightLeg.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonIKRightLeg.onClick, new EventDelegate.Callback(this.OnClickIKRightLeg));

                FindChild(goIKRightLeg, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.25f) - 10, systemUnitHeight - 4);
                FindChild(goIKRightLeg, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goIKRightLeg);
                goIKRightLeg.SetActive(true);

                // 左手IKボタン
                GameObject goIKLeftArm = SetCloneChild(goSystemUnit, goIKDetachAll, "IKLeftArm");
                //goIKLeftArm.transform.localPosition = new Vector3(conWidth * 0.125f - 3, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                goIKLeftArm.transform.localPosition = new Vector3(conWidth * 0.25f - 3, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);

                UILabel uiLabelIKLeftArm = FindChild(goIKLeftArm, "Name").GetComponent<UILabel>();
                uiLabelIKLeftArm.text = "[111111]左手IK";

                UIButton uiButtonIKLeftArm = goIKLeftArm.GetComponent<UIButton>();
                uiButtonIKLeftArm.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonIKLeftArm.onClick, new EventDelegate.Callback(this.OnClickIKLeftArm));

                FindChild(goIKLeftArm, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.25f) - 10, systemUnitHeight - 4);
                FindChild(goIKLeftArm, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goIKLeftArm);
                goIKLeftArm.SetActive(true);

                // 右手IKボタン
                GameObject goIKRightArm = SetCloneChild(goSystemUnit, goIKDetachAll, "IKRightArm");
                //goIKRightArm.transform.localPosition = new Vector3(conWidth * 0.375f - 3, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);
                goIKRightArm.transform.localPosition = new Vector3(conWidth * 0.4167f - 2, baseTop - systemUnitHeight / 2f - systemUnitHeight * 5 - 10f, 0f);

                UILabel uiLabelIKRightArm = FindChild(goIKRightArm, "Name").GetComponent<UILabel>();
                uiLabelIKRightArm.text = "[111111]右手IK";

                UIButton uiButtonIKRightArm = goIKRightArm.GetComponent<UIButton>();
                uiButtonIKRightArm.defaultColor = new Color(1f, 1f, 1f, 0.8f);
                EventDelegate.Set(uiButtonIKRightArm.onClick, new EventDelegate.Callback(this.OnClickIKRightArm));

                FindChild(goIKRightArm, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.25f) - 10, systemUnitHeight - 4);
                FindChild(goIKRightArm, "SelectCursor").SetActive(false);

                NGUITools.UpdateWidgetCollider(goIKRightArm);
                goIKRightArm.SetActive(true);

                //ボーンカテゴリー切り替えボタン

                GameObject goBoneCategory0 = SetCloneChild(goSystemUnit, goLoadPose, "BoneCategory0");
                goBoneCategory0.transform.localPosition = new Vector3(-conWidth * 0.4167f - 7, baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 14f, 0f);
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
                goBoneCategory1.transform.localPosition = new Vector3(-conWidth * 0.25f - 6, baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 14f, 0f);
                UILabel uiLabelBoneCategory1 = FindChild(goBoneCategory1, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory1.text = "[111111]上半身";
                UIButton uiButtonBoneCategory1 = goBoneCategory1.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory1.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory1);
                goBoneCategory1.SetActive(true);

                GameObject goBoneCategory2 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory2");
                goBoneCategory2.transform.localPosition = new Vector3(-conWidth * 0.08333f - 5, baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 14f, 0f);
                UILabel uiLabelBoneCategory2 = FindChild(goBoneCategory2, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory2.text = "[111111]下半身";
                UIButton uiButtonBoneCategory2 = goBoneCategory2.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory2.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory2);
                goBoneCategory2.SetActive(true);

                GameObject goBoneCategory3 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory3");
                goBoneCategory3.transform.localPosition = new Vector3(conWidth * 0.08333f - 4, baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 14f, 0f);
                UILabel uiLabelBoneCategory3 = FindChild(goBoneCategory3, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory3.text = "[111111]足指";
                UIButton uiButtonBoneCategory3 = goBoneCategory1.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory3.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory3);
                goBoneCategory3.SetActive(true);

                GameObject goBoneCategory4 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory4");
                goBoneCategory4.transform.localPosition = new Vector3(conWidth * 0.25f - 3, baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 14f, 0f);
                UILabel uiLabelBoneCategory4 = FindChild(goBoneCategory4, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory4.text = "[111111]左手指";
                UIButton uiButtonBoneCategory4 = goBoneCategory4.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory4.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory4);
                goBoneCategory4.SetActive(true);

                GameObject goBoneCategory5 = SetCloneChild(goSystemUnit, goBoneCategory0, "BoneCategory5");
                goBoneCategory5.transform.localPosition = new Vector3(conWidth * 0.4167f - 2, baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 14f, 0f);
                UILabel uiLabelBoneCategory5 = FindChild(goBoneCategory5, "Name").GetComponent<UILabel>();
                uiLabelBoneCategory5.text = "[111111]右手指";
                UIButton uiButtonBoneCategory5 = goBoneCategory5.GetComponent<UIButton>();
                EventDelegate.Set(uiButtonBoneCategory5.onClick, new EventDelegate.Callback(this.OnClickBoneCategory));
                NGUITools.UpdateWidgetCollider(goBoneCategory5);
                goBoneCategory5.SetActive(true);

                uiButtonBoneCategory0.defaultColor = new Color(uiButtonBoneCategory0.defaultColor.r, uiButtonBoneCategory0.defaultColor.g, uiButtonBoneCategory0.defaultColor.b, 1.0f);
                FindChild(uiButtonBoneCategory0.gameObject, "SelectCursor").SetActive(true);

                float CategoryFuncBase = baseTop - systemUnitHeight / 2f - systemUnitHeight * 6 - 56f;

                // CopyCategoryボタン
                GameObject goCopyCategory = SetCloneChild(goSystemUnit, goProfileTabCopy, "CopyCategory");
                goCopyCategory.transform.localPosition = new Vector3(-conWidth * 0.3333f - 4, 11f + CategoryFuncBase, 0f);
                goCopyCategory.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                UISprite uiSpriteCopyCategory = goCopyCategory.GetComponent<UISprite>();
                uiSpriteCopyCategory.SetDimensions((int)(conWidth * 0.3333f) - 4, 19);

                UILabel uiLabelCopyCategory = FindChild(goCopyCategory, "Name").GetComponent<UILabel>();
                uiLabelCopyCategory.width = uiSpriteCopyCategory.width - 10;
                uiLabelCopyCategory.fontSize = 14;
                uiLabelCopyCategory.spacingX = 0;
                uiLabelCopyCategory.supportEncoding = true;
                uiLabelCopyCategory.text = "[111111]CategoryCopy";

                UIButton uiButtonCopyCategory = goCopyCategory.GetComponent<UIButton>();
                uiButtonCopyCategory.defaultColor = new Color(1f, 1f, 1f, 0.8f);

                EventDelegate.Set(uiButtonCopyCategory.onClick, new EventDelegate.Callback(this.OnClickCopyCategoryButton));
                FindChild(goCopyCategory, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.2f) - 12, 15);
                FindChild(goCopyCategory, "SelectCursor").SetActive(false);
                FindChild(goCopyCategory, "SelectCursor").name = "SelectCursorCopyCategory";
                NGUITools.UpdateWidgetCollider(goCopyCategory);
                goCopyCategory.SetActive(true);

                // Right2LeftCategoryボタン
                GameObject goRight2LeftCategory = SetCloneChild(goSystemUnit, goCopyCategory, "Right2LeftCategory");
                goRight2LeftCategory.transform.localPosition = new Vector3(-conWidth * 0.3333f - 4, -11f + CategoryFuncBase, 0f);

                UILabel uiLabelRight2LeftCategory = FindChild(goRight2LeftCategory, "Name").GetComponent<UILabel>();
                uiLabelRight2LeftCategory.text = "[111111]右→左Paste";

                EventDelegate.Set(goRight2LeftCategory.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickRight2LeftCategoryButton));

                NGUITools.UpdateWidgetCollider(goRight2LeftCategory);
                goRight2LeftCategory.SetActive(true);

                // PasteCategoryボタン
                GameObject goPasteCategory = SetCloneChild(goSystemUnit, goCopyCategory, "PasteCategory");
                goPasteCategory.transform.localPosition = new Vector3(-4f, 11f + CategoryFuncBase, 0f);

                UILabel uiLabelPasteCategory = FindChild(goPasteCategory, "Name").GetComponent<UILabel>();
                uiLabelPasteCategory.text = "[111111]CategoryPaste";

                EventDelegate.Set(goPasteCategory.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickPasteCategoryButton));

                NGUITools.UpdateWidgetCollider(goPasteCategory);
                goPasteCategory.SetActive(true);

                // Left2RightCategoryボタン
                GameObject goLeft2RightCategory = SetCloneChild(goSystemUnit, goCopyCategory, "Left2RightCategory");
                goLeft2RightCategory.transform.localPosition = new Vector3(-4f, -11f + CategoryFuncBase, 0f);

                UILabel uiLabelLeft2RightCategory = FindChild(goLeft2RightCategory, "Name").GetComponent<UILabel>();
                uiLabelLeft2RightCategory.text = "[111111]右←左Paste";

                EventDelegate.Set(goLeft2RightCategory.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickLeft2RightCategoryButton));

                NGUITools.UpdateWidgetCollider(goLeft2RightCategory);
                goLeft2RightCategory.SetActive(true);

                // ResetCategoryボタン
                GameObject goResetCategory = SetCloneChild(goSystemUnit, goCopyCategory, "ResetCategory");
                goResetCategory.transform.localPosition = new Vector3(conWidth * 0.3333f -4, 11f + CategoryFuncBase, 0f);

                UILabel uiLabelResetCategory = FindChild(goResetCategory, "Name").GetComponent<UILabel>();
                uiLabelResetCategory.text = "[111111]CategoryReset";

                EventDelegate.Set(goResetCategory.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickResetCategoryButton));

                NGUITools.UpdateWidgetCollider(goResetCategory);
                goResetCategory.SetActive(true);


                // MirrorCategoryボタン
                GameObject goMirrorCategory = SetCloneChild(goSystemUnit, goCopyCategory, "MirrorCategory");
                goMirrorCategory.transform.localPosition = new Vector3(conWidth * 0.3333f -4, -11f + CategoryFuncBase, 0f);

                UILabel uiLabelMirrorCategory = FindChild(goMirrorCategory, "Name").GetComponent<UILabel>();
                uiLabelMirrorCategory.text = "[111111]CategoryMirror";

                EventDelegate.Set(goMirrorCategory.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickMirrorCategoryButton));

                NGUITools.UpdateWidgetCollider(goMirrorCategory);
                goMirrorCategory.SetActive(true);


                #endregion

                Debuginfo.Log(LogLabel + " goBoneCategory complete.");

                //メイドのボーン情報の取得
                if (maid != null && maid.Visible == true)
                {

                    getMaidBonetransform();
                    posHandle = new HandleKun(settingIni.HandleLegacymode, FindAtlas("SystemDialog"), maid);

                }
                else
                {
                    posHandle = new HandleKun(settingIni.HandleLegacymode, FindAtlas("SystemDialog"));

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

                        // PosHandleボタン
                        GameObject goPosHandle = SetCloneChild(goModUnit, goProfileTabCopy, "PosHandle:" + bone);
                        goPosHandle.AddComponent<UIDragScrollView>().scrollView = uiScrollView;
                        goPosHandle.transform.localPosition = new Vector3(conWidth * 0.4f + 2, -10.5f, 0f);

                        UISprite uiSpritePosHandle = goPosHandle.GetComponent<UISprite>();
                        uiSpritePosHandle.SetDimensions((int)(conWidth * 0.2f) - 2, 19);

                        UILabel uiLabelPosHandle = FindChild(goPosHandle, "Name").GetComponent<UILabel>();
                        uiLabelPosHandle.width = uiSpritePosHandle.width - 10;
                        uiLabelPosHandle.fontSize = 14;
                        uiLabelPosHandle.spacingX = 0;
                        uiLabelPosHandle.supportEncoding = true;
                        uiLabelPosHandle.text = "[111111]PosHandle";

                        UIButton uiButtonPosHandle = goPosHandle.GetComponent<UIButton>();
                        uiButtonPosHandle.defaultColor = new Color(1f, 1f, 1f, 0.8f);

                        EventDelegate.Set(uiButtonPosHandle.onClick, new EventDelegate.Callback(this.OnClickPosHandleButton));
                        FindChild(goPosHandle, "SelectCursor").GetComponent<UISprite>().SetDimensions(16, 16);
                        FindChild(goPosHandle, "SelectCursor").SetActive(false);
                        NGUITools.UpdateWidgetCollider(goPosHandle);
                        goPosHandle.SetActive(true);

                        //各パネルの下にボタン追加

                        float PanelFuncBase = -44f;

                        // Copyボタン
                        GameObject goCopy = SetCloneChild(goModUnit, goProfileTabCopy, "Copy:" + bone);
                        goCopy.transform.localPosition = new Vector3(-conWidth * 0.3333f, 11f + PanelFuncBase, 0f);
                        goCopy.AddComponent<UIDragScrollView>().scrollView = uiScrollView;

                        UISprite uiSpriteCopy = goCopy.GetComponent<UISprite>();
                        uiSpriteCopy.SetDimensions((int)(conWidth * 0.3333f) - 4, 19);

                        UILabel uiLabelCopy = FindChild(goCopy, "Name").GetComponent<UILabel>();
                        uiLabelCopy.width = uiSpriteCopy.width - 10;
                        uiLabelCopy.fontSize = 14;
                        uiLabelCopy.spacingX = 0;
                        uiLabelCopy.supportEncoding = true;
                        uiLabelCopy.text = "[111111]Copy";

                        UIButton uiButtonCopy = goCopy.GetComponent<UIButton>();
                        uiButtonCopy.defaultColor = new Color(1f, 1f, 1f, 0.8f);

                        EventDelegate.Set(uiButtonCopy.onClick, new EventDelegate.Callback(this.OnClickCopyButton));
                        FindChild(goCopy, "SelectCursor").GetComponent<UISprite>().SetDimensions((int)(conWidth * 0.2f) - 12, 15);
                        FindChild(goCopy, "SelectCursor").SetActive(false);
                        FindChild(goCopy, "SelectCursor").name = "SelectCursorCopy";
                        NGUITools.UpdateWidgetCollider(goCopy);
                        goCopy.SetActive(false);

                        // Right2Leftボタン
                        GameObject goRight2Left = SetCloneChild(goModUnit, goCopy, "Right2Left:" + bone);
                        goRight2Left.transform.localPosition = new Vector3(-conWidth * 0.3333f, -11f + PanelFuncBase, 0f);

                        UILabel uiLabelRight2Left = FindChild(goRight2Left, "Name").GetComponent<UILabel>();
                        uiLabelRight2Left.text = "[111111]右→左Paste";

                        EventDelegate.Set(goRight2Left.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickRight2LeftButton));

                        NGUITools.UpdateWidgetCollider(goRight2Left);
                        goRight2Left.SetActive(false);

                        // Pasteボタン
                        GameObject goPaste = SetCloneChild(goModUnit, goCopy, "Paste:" + bone);
                        goPaste.transform.localPosition = new Vector3(0f, 11f + PanelFuncBase, 0f);

                        UILabel uiLabelPaste = FindChild(goPaste, "Name").GetComponent<UILabel>();
                        uiLabelPaste.text = "[111111]Paste";

                        EventDelegate.Set(goPaste.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickPasteButton));

                        NGUITools.UpdateWidgetCollider(goPaste);
                        goPaste.SetActive(false);

                        // Left2Rightボタン
                        GameObject goLeft2Right = SetCloneChild(goModUnit, goCopy, "Left2Right:" + bone);
                        goLeft2Right.transform.localPosition = new Vector3(0f, -11f + PanelFuncBase, 0f);

                        UILabel uiLabelLeft2Right = FindChild(goLeft2Right, "Name").GetComponent<UILabel>();
                        uiLabelLeft2Right.text = "[111111]右←左Paste";

                        EventDelegate.Set(goLeft2Right.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickLeft2RightButton));

                        NGUITools.UpdateWidgetCollider(goLeft2Right);
                        goLeft2Right.SetActive(false);

                        // Resetボタン
                        GameObject goReset = SetCloneChild(goModUnit, goCopy, "Reset:"+bone);
                        goReset.transform.localPosition = new Vector3(conWidth * 0.3333f, 11f + PanelFuncBase, 0f);

                        UILabel uiLabelReset = FindChild(goReset, "Name").GetComponent<UILabel>();
                        uiLabelReset.text = "[111111]Reset";

                        EventDelegate.Set(goReset.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickResetButton));

                        NGUITools.UpdateWidgetCollider(goReset);
                        goReset.SetActive(false);


                        // Mirrorボタン
                        GameObject goMirror = SetCloneChild(goModUnit, goCopy, "Mirror:" + bone);
                        goMirror.transform.localPosition = new Vector3(conWidth * 0.3333f , -11f + PanelFuncBase, 0f);

                        UILabel uiLabelMirror = FindChild(goMirror, "Name").GetComponent<UILabel>();
                        uiLabelMirror.text = "[111111]Mirror";

                        EventDelegate.Set(goMirror.GetComponent<UIButton>().onClick, new EventDelegate.Callback(this.OnClickMirrorButton));

                        NGUITools.UpdateWidgetCollider(goMirror);
                        goMirror.SetActive(false);
                        


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

                ikManage = new IKManage(_constraitLeftLeg, _constraitRightLeg,_constraitLeftArm,_constraitRightArm);

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
            
            uiValueLable.Clear(); 
        }

        //----

        public void rebootHandle()
        {
            if (FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 L Foot"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
            }
            else if (FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 R Foot"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);
            }
            else if (FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 L Arm"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);
            }
            else if (FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").activeInHierarchy == true)
            {
                posHandle.SetMaid(maid, trBone["Bip01 R Arm"]);
                posHandle.IKTargetAttachedColor(false);
                posHandle.resetHandleCoreColor();
                //posHandle.setVisible(false);
                posHandle.setVisible(false);

                FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);
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
            /*
            if (attachIKMaidNo.Contains(removeNo))
            {
                //attachIKMaidNo.Remove(removeNo);
                attachIKMaidList.Remove(removeNo);
                trTargetIKBones.Remove(removeNo);
                

                IKLeftLeg.Remove(removeNo);
                IKRightLeg.Remove(removeNo);
                IKLeftArm.Remove(removeNo);
                IKRightArm.Remove(removeNo);
                

                bIKAttachLeftLeg.Remove(removeNo);
                bIKAttachRightLeg.Remove(removeNo);
                bIKAttachLeftArm.Remove(removeNo);
                bIKAttachRightArm.Remove(removeNo);

                

                //ターゲット用オブジェクトを1秒後に消す
                if (goIKLeftLegTarget.ContainsKey(removeNo))
                {
                    if (goIKLeftLegTarget[removeNo] != null)
                    {
                        goIKLeftLegTarget[removeNo].transform.DetachChildren();

                        GameObject.Destroy(goIKLeftLegTarget[removeNo], 1f);
                    }
                }
                if (goIKRightLegTarget.ContainsKey(removeNo))
                {
                    if (goIKRightLegTarget[removeNo] != null)
                    {
                        goIKRightLegTarget[removeNo].transform.DetachChildren();
                        GameObject.Destroy(goIKRightLegTarget[removeNo], 1f);
                    }
                }
                if (goIKLeftArmTarget.ContainsKey(removeNo))
                {
                    if (goIKLeftArmTarget[removeNo] != null)
                    {
                        goIKLeftArmTarget[removeNo].transform.DetachChildren();
                        GameObject.Destroy(goIKLeftArmTarget[removeNo], 1f);
                    }
                }
                if (goIKRightArmTarget.ContainsKey(removeNo))
                {
                    if (goIKRightArmTarget[removeNo] != null)
                    {
                        goIKRightArmTarget[removeNo].transform.DetachChildren();
                        GameObject.Destroy(goIKRightArmTarget[removeNo], 1f);
                    }
                }
                

                goIKLeftLegTarget.Remove(removeNo);
                goIKRightLegTarget.Remove(removeNo);
                goIKLeftArmTarget.Remove(removeNo);
                goIKRightArmTarget.Remove(removeNo);
                

                trIKLeftLegBones.Remove(removeNo);
                trIKRightLegBones.Remove(removeNo);
                trIKLeftArmBones.Remove(removeNo);
                trIKRightArmBones.Remove(removeNo);
                

            }
            */
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

        private void setIKButtonActive(/*GameObject _ButtonObj,*/　bool _enableCursor, bool _allikdisable = false ,bool _allmaiddisable = false)
        {
            //ハンドルボタンが表示されてたら消す
            if (activeHandleName != "")
            {
                visibleHandle(activeHandleName);
                setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);
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

            FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);

            //カーソルは今操作中のIKボタンのものだけ表示
            if (!_allikdisable)
            {
                FindChild(UIButton.current.gameObject, "SelectCursor").SetActive(_enableCursor);
            }


            //ついでにIKボーンアタッチ用オブジェクトも消す
            posHandle.IKTargetVisible = false;
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
            FindChild(FindChild(goAMSPanel, "IKLeftLeg"), "SelectCursor").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightLeg"), "SelectCursor").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKLeftArm"), "SelectCursor").SetActive(false);
            FindChild(FindChild(goAMSPanel, "IKRightArm"), "SelectCursor").SetActive(false);


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
                    type == "Right2Left" || type == "Left2Right" || type == "Mirror" || type == "Reset")
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
            

            /*
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
            }*/

        }

        //公式撮影と複数撮影でメイドさんやメイドさんの数の取得方法が違うので
        //ここで差異を吸収する
        private Maid GetMaid(int _No)
        {
            if(sceneLevel == iScenePhoto || sceneLevel == iScenePhotoCBL)
                return GameMain.Instance.CharacterMgr.GetMaid(_No);
            else
                return GameMain.Instance.CharacterMgr.GetStockMaid(_No);
        }

        private int GetMaidCount()
        {
            if(sceneLevel == iScenePhoto || sceneLevel == iScenePhotoCBL)
                return GameMain.Instance.CharacterMgr.GetMaidCount();
            else
                return GameMain.Instance.CharacterMgr.GetStockMaidCount();
        }

        private void currentMaidChange()
        {

            maid = GetMaid(currentMaidNo);
            /*
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
            */


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

                //LeftLegHandle.SetMaid(maid);
                //RightLegHandle.SetMaid(maid);
                //LeftArmHandle.SetMaid(maid);
                //RightArmHandle.SetMaid(maid);


                //ikLeftLeg = new TBody.IKCMO();
                //ikRightLeg = new TBody.IKCMO();

                syncSlider(true);

                if (activeHandleName != "")
                {
                    visibleHandle(activeHandleName);
                    setUnitButtonColor(FindChildByTag(trBoneUnit[activeHandleName], "Handle:" + activeHandleName).GetComponent<UIButton>(), false);
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
                    if (bone == "secret" || bone == "eye" || bone == "allpos" || bone == "offset" || bone == "camera") continue;


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
            if (_bone.Contains("Bip") || _bone.Contains("_IK_"))
            {
                if (_prop.Contains(".p"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_pos:" + _bone + ":" + _prop + ":" + maid.name)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_pos:" + _bone + ":" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_pos:" + _bone + ":" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        undoList.Add(new UndoBonePos(trBone[_bone], trBone[_bone].localPosition));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_rot:" + _bone + ":" + _prop + ":" + maid.name)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_rot:" + _bone + ":" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_rot:" + _bone + ":" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        undoList.Add(new UndoBone(trBone[_bone], trBone[_bone].localRotation));

                    }
                }
            }
            else if (_bone == "alllpos")
            {
                if (_prop.Contains(".p"))
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_pos:Allpos:" + _prop)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_pos:Allpos:" + _prop);
                        UndofuncName = _Func + "_pos:Allpos:" + _prop;

                        //Undo履歴に加える
                        undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllOfsetPos(), true));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_rot:Allpos:" + _prop)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_rot:Allpos:" + _prop);
                        UndofuncName = _Func + "_rot:Allpos:" + _prop;

                        //Undo履歴に加える
                        undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllOfsetRot(), false));

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
                        Debuginfo.Log(LogLabel + _Func + "_pos:offset:" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_pos:offset:" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        undoList.Add(new UndoOffset(maid, maid.transform.localPosition, true));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_rot:offset:" + _prop + ":" + maid.name)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_rot:offset:" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_rot:offset:" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        undoList.Add(new UndoOffset(maid, maid.GetRot(), false));

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
                        Debuginfo.Log(LogLabel + _Func + "_eye:" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_eye:" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        undoList.Add(new UndoEye(maid, maid.body0.quaDefEyeL, false));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_eye:" + _prop + ":" + maid.name)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_eye:" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_eye:" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        undoList.Add(new UndoEye(maid, maid.body0.quaDefEyeR, true));

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
                        Debuginfo.Log(LogLabel + _Func + "_secret:" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_secret:" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        jiggleBone jbMuneL = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true).gameObject.GetComponent<jiggleBone>();
                        undoList.Add(new UndoSecret(jbMuneL, jbMuneL.MuneUpDown, jbMuneL.MuneYori));

                    }
                }
                else
                {
                    //Undoチェック
                    if (UndofuncName != _Func + "_secret:" + _prop + ":" + maid.name)
                    {
                        Debuginfo.Log(LogLabel + _Func + "_secret:" + _prop + ":" + maid.name);
                        UndofuncName = _Func + "_secret:" + _prop + ":" + maid.name;

                        //Undo履歴に加える
                        jiggleBone jbMuneR = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true).gameObject.GetComponent<jiggleBone>();
                        undoList.Add(new UndoSecret(jbMuneR, jbMuneR.MuneUpDown, jbMuneR.MuneYori));

                    }
                }
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

                if (posHandle.bHandlePositionMode == false)
                {
                    if (bone == "AllOffset")
                    {

                        //Undoチェック
                        if (UndofuncName != "handle_rot:allpos")
                        {
                            Debuginfo.Log(LogLabel + "handle_rot:allpos");
                            UndofuncName = "handle_rot:allpos";

                            //Undo履歴に加える
                            undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllOfsetRot(), false));

                        }

                        //まずいかもしれないけどAllOffsetを直に回す
                        //まずかったから変更
                        Vector3 tmpAllRot = GameMain.Instance.CharacterMgr.GetCharaAllRot();
                        Quaternion tempAllQua = Quaternion.Euler(tmpAllRot.x, tmpAllRot.y, tmpAllRot.z);

                        tempAllQua *= posHandle.DeltaQuaternion();

                        tmpAllRot = tempAllQua.eulerAngles;
                        GameMain.Instance.CharacterMgr.SetCharaAllRot(tmpAllRot);
                        //回転結果をプラグイン側の数値に反映


                        //Vector3 tmpAllRot = GameMain.Instance.CharacterMgr.GetCharaAllRot();


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
                            Debuginfo.Log(LogLabel + "handle_rot:offset" + ":" + maid.name);
                            UndofuncName = "handle_rot:offset" + ":" + maid.name;

                            //Undo履歴に加える
                            undoList.Add(new UndoOffset(maid, maid.GetRot(), false));

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
                            Debuginfo.Log(LogLabel + "handle_rot:" + bone + ":" + maid.name);
                            UndofuncName = "handle_rot:" + bone + ":" + maid.name;

                            //Undo履歴に加える
                            undoList.Add( new UndoBone(trBone[bone],trBone[bone].localRotation));
                            
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
                            vPastBoneAngle[bone] = trBone[bone].localRotation;
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
                            Debuginfo.Log(LogLabel + "handle_pos:allpos");
                            UndofuncName = "handle_pos:allpos";

                            //Undo履歴に加える
                            undoList.Add(new UndoAllpos(GameMain.Instance.CharacterMgr.GetCharaAllOfsetPos(), true));

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
                            Debuginfo.Log(LogLabel + "Undo:handle_pos:offset" + ":" + maid.name);
                            UndofuncName = "handle_pos:offset" + ":" + maid.name;

                            //Undo履歴に加える
                            undoList.Add(new UndoOffset(maid,maid.transform.localPosition,true) );

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
                            Debuginfo.Log(LogLabel + "handle_pos:" + bone + ":" + maid.name);
                            UndofuncName = "handle_pos:" + bone + ":" + maid.name;

                            //Undo履歴に加える
                            undoList.Add(new UndoBonePos(trBone[bone], trBone[bone].localPosition));

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
                            vPastBoneTrans = trBone[bone].localPosition;
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
        private void syncSlider(bool allSlider)
        {
            bLocked = true;

            //前回の操作がUndoかRedoであれば履歴に記録しない
            bool notUndo = true; 
            if(UndofuncName == "Undo" || UndofuncName == "Redo")
            {
                notUndo = false;
            }

            foreach (string bone in mp.sBone)
            {
                if (bone != "secret" && bone != "eye" && bone != "allpos" && bone != "offset" && bone != "camera")
                {
                    if (bone == "Bip01")
                    {
                        if (vPastBoneAngle[bone] != trBone[bone].localRotation || vPastBoneTrans != trBone[bone].localPosition || allSlider)
                        {
                            
                            //Undoチェック
                            if ((UndofuncName != "syncAllBone:" + maid.name) && notUndo)
                            {
                                Debuginfo.Log(LogLabel + "syncAllBone:" + maid.name+":"+bone);
                                UndofuncName = "syncAllBone:" + maid.name;

                                //Undo履歴に加える
                                undoList.Add(new UndoBoneAll(trBone,vPastBoneAngle, vPastBoneTrans));

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
                                Debuginfo.Log(LogLabel + "syncAllBone:" + maid.name + ":" + bone);
                                UndofuncName = "syncAllBone:" + maid.name;

                                //Undo履歴に加える
                                undoList.Add(new UndoBoneAll(trBone, vPastBoneAngle, vPastBoneTrans));

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
                            if (UndofuncName != "sync:Allpos:Pos:" + maid.name && notUndo)
                            {
                                Debuginfo.Log(LogLabel + "sync:Allpos:Pos:" + maid.name);
                                UndofuncName = "sync:Allpos:Pos:" + maid.name;

                                //Undo履歴に加える
                                Vector3 oldPos = new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"],
                                    mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"],
                                    mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]);
                                undoList.Add(new UndoAllpos(oldPos, true));

                            }

                            mp.fValue[bone][bone + ".px"] = tmpAllPos.x - mp.fVzero[bone][bone + ".px"];
                            mp.fValue[bone][bone + ".py"] = tmpAllPos.y - mp.fVzero[bone][bone + ".py"];
                            mp.fValue[bone][bone + ".pz"] = tmpAllPos.z - mp.fVzero[bone][bone + ".pz"];

                            b_changed = true;
                        }
                        if (
                            tmpAllRot.x != mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"] ||
                            tmpAllRot.y != mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"] ||
                            tmpAllRot.z != mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]
                            )
                        {
                            //Undoチェック
                            if (UndofuncName != "sync:Allpos:Rot:" + maid.name && notUndo)
                            {
                                Debuginfo.Log(LogLabel + "sync:Allpos:Rot:" + maid.name);
                                UndofuncName = "sync:Allpos:Rot:" + maid.name;
                                Vector3 oldRot = new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"],
                                    mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"],
                                    mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]);
                                //Undo履歴に加える
                                undoList.Add(new UndoAllpos(oldRot, false));

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
                                Debuginfo.Log(LogLabel + "sync:Offset:Pos:" + maid.name);
                                UndofuncName = "sync:Offset:Pos:" + maid.name;


                                Vector3 oldPos = new Vector3(mp.fValue[bone][bone + ".px"] + mp.fVzero[bone][bone + ".px"],
                                    mp.fValue[bone][bone + ".py"] + mp.fVzero[bone][bone + ".py"],
                                    mp.fValue[bone][bone + ".pz"] + mp.fVzero[bone][bone + ".pz"]);
                                //Undo履歴に加える
                                undoList.Add(new UndoOffset(maid, oldPos, true));

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
                                Debuginfo.Log(LogLabel + "sync:Offset:Rot:" + maid.name);
                                UndofuncName = "sync:Offset:Rot:" + maid.name;


                                Vector3 oldRot = new Vector3(mp.fValue[bone][bone + ".x"] + mp.fVzero[bone][bone + ".x"],
                                    mp.fValue[bone][bone + ".y"] + mp.fVzero[bone][bone + ".y"],
                                    mp.fValue[bone][bone + ".z"] + mp.fVzero[bone][bone + ".z"]);
                                //Undo履歴に加える
                                undoList.Add(new UndoOffset(maid, oldRot, false));

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
        private void inversekinematicFromHandle(HandleKun.IKMODE _ikmode)
        {
            if (posHandle.controllDragged())
            {

                bLocked = true;

                maid.body0.m_Bones.animation.Stop();
                maid.body0.boHeadToCam = false;
                maid.body0.boEyeToCam = false;
                
                ikManage.inversekinematicHandle(posHandle,maid,currentMaidNo);
                

                bLocked = false;
            }
        }
        private bool inversekinematicFromHandle(Transform _ikParent,bool ikInitted)
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

        //IK設定の初期化
        private void ikInit(Transform _ikParent)
        {
            Vector3 prePosition = posHandle.Pos;
            //まず、ハンドルの親をIKターゲットオブジェクトに変更
            posHandle.SetParentBone(_ikParent);
            //IKターゲットオブジェクトの位置を親変更前のハンドルの位置の値に設定
            _ikParent.position = prePosition;


            Debuginfo.Log(_ikParent.position.ToString());

            //posHandle.Rot = Quaternion.Euler(-90, 0, 90);//Quaternion.identity;
            /*
            if (!attachIKMaidNo.Contains(currentMaidNo))
            {
                attachIKMaidList.Add(currentMaidNo, maid);
                attachIKMaidNo.Add(currentMaidNo);
            }
            */

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

                ikManage.ikTargetClicked(posHandle,currentMaidNo);

                /*
                switch (posHandle.IKmode)
                {
                    case HandleKun.IKMODE.LeftLeg:
                        ikInit(goIKLeftLegTarget[currentMaidNo].transform);

                        goIKLeftLegTarget[currentMaidNo].transform.parent = trTargetIKTemp;
                        if(trTargetIKTemp.name !="Bip01")
                            goIKLeftLegTarget[currentMaidNo].transform.localPosition = Vector3.zero;

                        
                        bIKAttachLeftLeg[currentMaidNo] = true;

                        break;

                    case HandleKun.IKMODE.RightLeg:
                        ikInit(goIKRightLegTarget[currentMaidNo].transform);

                        goIKRightLegTarget[currentMaidNo].transform.parent = trTargetIKTemp;
                        if (trTargetIKTemp.name != "Bip01")
                            goIKRightLegTarget[currentMaidNo].transform.localPosition = Vector3.zero;

                        
                        bIKAttachRightLeg[currentMaidNo] = true;

                        break;

                    case HandleKun.IKMODE.LeftArm:
                        ikInit(goIKLeftArmTarget[currentMaidNo].transform);

                        goIKLeftArmTarget[currentMaidNo].transform.parent = trTargetIKTemp;
                        if (trTargetIKTemp.name != "Bip01")
                            goIKLeftArmTarget[currentMaidNo].transform.localPosition = Vector3.zero;
                        

                        bIKAttachLeftArm[currentMaidNo] = true;

                        break;

                    case HandleKun.IKMODE.RightArm:
                        ikInit(goIKRightArmTarget[currentMaidNo].transform);

                        goIKRightArmTarget[currentMaidNo].transform.parent = trTargetIKTemp;
                        if (trTargetIKTemp.name != "Bip01")
                            goIKRightArmTarget[currentMaidNo].transform.localPosition = Vector3.zero;

                        
                        bIKAttachRightArm[currentMaidNo] = true;

                        break;

                    default:

                        Debug.Log(LogLabel + "Handle IKmode target select exception.");
                        break;
                }
                
                if (trTargetIKTemp.name != "Bip01")
                {
                    posHandle.IKTargetAttachedColor(true);
                }
                else
                {
                    posHandle.IKTargetAttachedColor(false);
                }

                bIKTargetGet = false;

                trTargetIKTemp = null;
                */


                posHandle.IKTargetClickAfter();

                bLocked = false;

            }
        }

        //IKのボーンアタッチ解除用
        //IK関係のコレクションリスト自体は要素を削除せずそのまま
        private void detachIKfromBone()
        {
            ikManage.detachIKfromBone(posHandle,trBone,currentMaidNo);
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

                            /*
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
                            */
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
                if ((sceneLevel == iSceneEdit|| sceneLevel == iSceneEditCBL) && maid != GameMain.Instance.CharacterMgr.GetMaid(0))//bVisivle.Count > 0)
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


                if ((sceneLevel == iSceneEdit|| sceneLevel == iSceneEditCBL) && maid != GameMain.Instance.CharacterMgr.GetMaid(0))//bVisivle.Count > 0)
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
                    /*
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
                    */
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

