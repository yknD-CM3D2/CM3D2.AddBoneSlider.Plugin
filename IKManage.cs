using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityInjector.Attributes;
using CM3D2.AddBoneSlider;

namespace CM3D2.AddBoneSlider.Plugin
{
    //IK関連変数
    public class IKPropList
    {
        private readonly string LogLabel = AddBoneSlider.PluginName + " : ";

        //本体側のIKが使えないのでこちらで用意
        private Dictionary<int, IKCONSTRAINED> IK ;

        //IK脚腕のアタッチ状態判別用
        private Dictionary<int, bool> bIKAttach ;
        //IKターゲット用
        public Dictionary<int, GameObject> goIKTarget;
        //IK対象ボーンのtransform
        private Dictionary<int, Transform[]> trIKBones ;



        //各関節の可動範囲情報
        private float[,] constrait;

        public IKPropList(float[,] _constrait)
        {
            IK = new Dictionary<int, IKCONSTRAINED>();
            bIKAttach = new Dictionary<int, bool>();
            goIKTarget = new Dictionary<int, GameObject>();
            trIKBones = new Dictionary<int, Transform[]>();

            constrait = _constrait;
        }

        public IKPropList(IKPropList _copy)
        {
            IK = new Dictionary<int, IKCONSTRAINED>(_copy.IK);
            bIKAttach = new Dictionary<int, bool>(_copy.bIKAttach);
            goIKTarget = new Dictionary<int, GameObject>(_copy.goIKTarget);
            trIKBones = new Dictionary<int, Transform[]>(_copy.trIKBones);

            constrait = _copy.constrait.Clone() as float[,];
        }

        public void assignment(IKPropList _rhs)
        {
            IK = _rhs.IK;
            bIKAttach = _rhs.bIKAttach;

           
            foreach(var t in _rhs.goIKTarget.Where((m) => m.Value == null))
            {
                if(goIKTarget.ContainsKey(t.Key))
                    GameObject.Destroy(goIKTarget[t.Key]);
            }

            goIKTarget = _rhs.goIKTarget;
            trIKBones = _rhs.trIKBones;
        }

        //各変数の初期化処理
        //初期化されていれば何もしない
        public void addList(Transform Bip01, Transform[] boneList, Maid maid, int currentMaidNo)
        {
            if (!IK.ContainsKey(currentMaidNo))
            {
                IKCONSTRAINED ikTempLeftLeg = new IKCONSTRAINED();

                ikTempLeftLeg.Init(boneList, maid.body0, constrait);
                IK.Add(currentMaidNo, ikTempLeftLeg);
            }


            //IK対象ボーンリストが設定されていなければ初期化
            if (!trIKBones.ContainsKey(currentMaidNo))
            {
                trIKBones.Add(currentMaidNo, boneList);
            }

            //IKアタッチ状態が設定されていなければ一時表示[None]で初期化設定
            if (!bIKAttach.ContainsKey(currentMaidNo))
            {
                bIKAttach.Add(currentMaidNo, false);
            }

            //IKターゲットが生成されてなければ生成
            if (!goIKTarget.ContainsKey(currentMaidNo))
            {
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


            //Debuginfo.Log(goIKTarget[currentMaidNo].transform.position.ToString());

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

        public void lateupdateFunc(int m, Maid maid, bool isArm, bool isLeft)
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
                    else if (maid.body0.tgtHandR != null || maid.body0.tgtHandR_AttachName != string.Empty)
                        return;
                }

                IK[m].Proc(trIKBones[m][0], trIKBones[m][1], trIKBones[m][2], goIKTarget[m].transform.position);


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
                    goIKTarget[removeNo] = null;
                }
            }
            goIKTarget.Remove(removeNo);
            trIKBones.Remove(removeNo);
        }

        public void ikTargetClicked(HandleKun posHandle, int currentMaidNo, Transform trTargetIKTemp)
        {
            ikInit(posHandle, currentMaidNo);

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
                    goIKTarget[removeNo] = null;
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

        public Transform getParentTransform(int currentMaidNo)
        {
            return goIKTarget.ContainsKey(currentMaidNo) ? (goIKTarget[currentMaidNo].transform.parent) : null;
        }

        public void setParentTransform(int currentMaidNo,Transform _parent)
        {
            if(goIKTarget.ContainsKey(currentMaidNo))
            {
                goIKTarget[currentMaidNo].transform.parent = _parent;
            }
        }

        public Transform getIKTransform(int currentMaidNo)
        {
            return goIKTarget.ContainsKey(currentMaidNo) ? (goIKTarget[currentMaidNo].transform) : null;
        }

        public Vector3 getIKPosition(int currentMaidNo)
        {
            return goIKTarget.ContainsKey(currentMaidNo) ? (goIKTarget[currentMaidNo].transform.localPosition) : Vector3.zero;
        }

        public void setIKPosition(int currentMaidNo,Vector3 _vec)
        {
            if (goIKTarget.ContainsKey(currentMaidNo))
            {
                goIKTarget[currentMaidNo].transform.position = _vec;
            }
        }

        public Vector3 getIKPositionCurrent(int currentMaidNo)
        {
            if (trIKBones.ContainsKey(currentMaidNo))
            {
                return trIKBones[currentMaidNo][2].position;
            }
            else
                return Vector3.zero; ;
        }

        public void doUndoFunc(int currentMaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos)
        {
            if (bIKAttach.ContainsKey(currentMaidNo))
            {

                goIKTarget[currentMaidNo].transform.parent = _trParent;
                goIKTarget[currentMaidNo].transform.localPosition = _vPos;

                bIKAttach[currentMaidNo] = _isAttach;
                
            }
        }
    }
    public class IKManage
    {
        private readonly string LogLabel = AddBoneSlider.PluginName + " : ";
        private readonly float clickCheckOffsetInit = 40f;

        public IKPropList[] IKList = new IKPropList[4];

        //アタッチ状態が付与されたメイドリスト
        public Dictionary<int, Maid> attachIKMaidList = new Dictionary<int, Maid>();

        //アタッチ状態が付与されたメイドのメイドNoリスト
        public List<int> attachIKMaidNo = new List<int>();

        //IKをアタッチする_IK_ボーン類のtransform
        public Dictionary<int, Transform[]> trTargetIKBones = new Dictionary<int, Transform[]>();

        //IKターゲット一時保存用
        public Transform trTargetIKTemp = null;

        //IK名の辞書
        public readonly Dictionary<string, string> sIKBoneName = new Dictionary<string, string>()
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

        //
        private readonly Func<int, Maid> GetMaid;

        public IKManage(IKManage _copy)
        {
            IKList[0] = new IKPropList(_copy.IKList[0]);
            IKList[1] = new IKPropList(_copy.IKList[1]);
            IKList[2] = new IKPropList(_copy.IKList[2]);
            IKList[3] = new IKPropList(_copy.IKList[3]);
            

            attachIKMaidList = new Dictionary<int, Maid>(_copy.attachIKMaidList);
            attachIKMaidNo = new List<int>(_copy.attachIKMaidNo);
            trTargetIKBones = new Dictionary<int, Transform[]>(_copy.trTargetIKBones);

            GetMaid = _copy.GetMaid;
        }

        public IKManage(float[,] _constraitLeftLeg, float[,] _constraitRightLeg, float[,] _constraitLeftArm, float[,] _constraitRightArm,Func<int,Maid> _GetMaid)
        {

            IKList[0] = new IKPropList(_constraitLeftLeg);
            IKList[1] = new IKPropList(_constraitRightLeg);
            IKList[2] = new IKPropList(_constraitLeftArm);
            IKList[3] = new IKPropList(_constraitRightArm);
            
            GetMaid = _GetMaid;
        }

        public void addList(Transform Bip01, Transform[] boneList, Maid maid, int currentMaidNo, int No)
        {
            IKList[No].addList(Bip01, boneList, maid, currentMaidNo);
        }

        public void Destroy()
        {
            attachIKMaidNo.Clear();
            trTargetIKBones.Clear();
            
            Array.ForEach(IKList,(ikprop) =>
            {
                ikprop.Destroy();
            });

        }

        public void lateupdateFunc(HandleKun posHandle)
        {

            //メイドさんがいなくなっていればリストから除外
            var deleteList =  attachIKMaidNo.FindAll((m) =>(attachIKMaidList[m] == null || attachIKMaidList[m].Visible == false));
            deleteList.ForEach(removeAttachMaidList);
            attachIKMaidNo.RemoveAll(deleteList.Contains);
                    

            attachIKMaidNo.ForEach( m =>
            {
                Array.ForEach(IKList, (List) => List.lateupdateFunc(m, attachIKMaidList[m], false, false) );
            });
        }

        public void updateFunc(HandleKun posHandle, Dictionary<string, Transform> trBone)
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

                    Array.ForEach(trArray.Value, tr => 
                    {
                        float magnitude = (Camera.main.WorldToScreenPoint(tr.position) - mousePos).magnitude;
                        if (magnitude < clickCheckOffset)
                        {
                            clickCheckOffset = magnitude;
                            trTargetIKTemp = tr;
                            //Debuginfo.Log(trIK.name + ":" + Camera.main.WorldToScreenPoint(trIK.position).ToString());
                        }
                    });

                }
                

                RemoveNo.ForEach( m => trTargetIKBones.Remove(m) );


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

        //ハンドル君から値を受け取る処理
        public string  inversekinematicHandle(Action<Undo> undoListAdd, Func<int, int, Quaternion[]> getBones, Action<int, int, Quaternion[]> setBones, Quaternion[] bones, Func<bool[], bool[]> setIKButtonActive, Func<bool[]> getIKButtonActive, HandleKun posHandle, Maid maid, int currentMaidNo, string UndoFuncName)
        {

            if ((int)posHandle.IKmode <= 3 && posHandle.controllDragged())
            {
                //Undoチェック
                if (UndoFuncName != "IK:" + (int)posHandle.IKmode + ":"+ IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo).ToString() + ":"+ currentMaidNo)
                {
                    UndoFuncName = "IK:" + (int)posHandle.IKmode + ":" + IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo).ToString() + ":" + currentMaidNo;

                    bool[] boolList = getIKButtonActive();

                    //IKでのUndo登録
                    if (IKList[(int)posHandle.IKmode].checkIKAttach(currentMaidNo))
                    {
                        undoListAdd(Undo.createUndoIK(this.doUndoFunc, this.checkIKAttach, this.getIKTransform, setIKButtonActive, boolList, maid, currentMaidNo, 
                            true, IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo), IKList[(int)posHandle.IKmode].getIKPosition(currentMaidNo), (int)posHandle.IKmode));
                    }
                    else
                    {
                        boolList[(int)posHandle.IKmode] = false;
                        undoListAdd(Undo.createUndoIKFirst(this.doUndoFunc, this.checkIKAttach, this.getIKTransform,getBones,setBones,bones, setIKButtonActive, boolList, maid, currentMaidNo,
                            false, IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo), IKList[(int)posHandle.IKmode].getIKPosition(currentMaidNo), (int)posHandle.IKmode));
                    }
                    //Debuginfo.Log(LogLabel + "Undo::" + UndoFuncName );
                }

                IKList[(int)posHandle.IKmode].inversekinematicHandle(posHandle, currentMaidNo);

                if (!attachIKMaidNo.Contains(currentMaidNo))
                {
                    attachIKMaidList.Add(currentMaidNo, maid);
                    attachIKMaidNo.Add(currentMaidNo);
                }
                
            }
            
            return UndoFuncName;

        }

        //IK設定の初期化
        private void ikInit(HandleKun.IKMODE ikmode, HandleKun posHandle, Maid maid, int currentMaidNo)
        {
            if ((int)posHandle.IKmode <= 3)
            {
                IKList[(int)posHandle.IKmode].ikInit(posHandle, currentMaidNo);

                if (!attachIKMaidNo.Contains(currentMaidNo))
                {
                    attachIKMaidList.Add(currentMaidNo, maid);
                    attachIKMaidNo.Add(currentMaidNo);
                }
            }
        }

        public void ikTargetClicked(Action<Undo> undoListAdd, Func<bool[], bool[]> setIKButtonActive, bool[] boolList, HandleKun posHandle, int currentMaidNo,string UndoFuncName)
        {
            if (trTargetIKTemp != null)
            {
                Debuginfo.Log(LogLabel + "IKTarget:" + trTargetIKTemp.name.ToString());

                if((int)posHandle.IKmode <= 3)
                {
                    //Undoチェック
                    if (UndoFuncName != "IK:" + (int)posHandle.IKmode + ":" + IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo).ToString() + ":" + currentMaidNo)
                    {
                        UndoFuncName = "IK:" + (int)posHandle.IKmode + ":" + IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo).ToString() + ":" + currentMaidNo;
                        //IKでのUndo登録
                        undoListAdd(Undo.createUndoIK(this.doUndoFunc, this.checkIKAttach, this.getIKTransform, setIKButtonActive, boolList, GetMaid(currentMaidNo), currentMaidNo, true, IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo), IKList[(int)posHandle.IKmode].getIKPosition(currentMaidNo), (int)posHandle.IKmode));
                        //Debuginfo.Log(LogLabel + "Undo::" + UndoFuncName);
                    }

                    IKList[(int)posHandle.IKmode].ikTargetClicked(posHandle, currentMaidNo, trTargetIKTemp);

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
                attachIKMaidNo.Remove(removeNo);
                attachIKMaidList.Remove(removeNo);
                trTargetIKBones.Remove(removeNo);


                Array.ForEach(IKList, list => list.removeAttachMaidList(removeNo));
            }
        }

        public void detachIKfromBone(Action<Undo> undoListAdd, Func<bool[], bool[]> setIKButtonActive, bool[] boolList, HandleKun posHandle, Dictionary<string, Transform> trBone, int currentMaidNo,string UndoFuncName)
        {

            //Undoチェック
            if (UndoFuncName != "IKDetach:" + (int)posHandle.IKmode + ":" + IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo).ToString() + ":" + currentMaidNo)
            {
                UndoFuncName = "IKDetach:" + (int)posHandle.IKmode + ":" + IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo).ToString() + ":" + currentMaidNo;
                //IKでのUndo登録
                undoListAdd(Undo.createUndoIK(this.doUndoFunc, this.checkIKAttach, this.getIKTransform, setIKButtonActive, boolList, GetMaid(currentMaidNo), currentMaidNo, false, IKList[(int)posHandle.IKmode].getParentTransform(currentMaidNo), IKList[(int)posHandle.IKmode].getIKPosition(currentMaidNo), (int)posHandle.IKmode));
                Debuginfo.Log(LogLabel + UndoFuncName);
            }

            //Vector3 postPosition = ikHandle.Pos;
            //IKターゲットの位置を初期化
            Quaternion temp = posHandle.Rot;
            //posHandle.transform.parent.parent = trBone["Bip01"];
            //posHandle.transform.parent.localPosition = Vector3.zero;

            if (posHandle.IKmode == HandleKun.IKMODE.LeftLeg)
            {
                posHandle.SetParentBone(trBone["Bip01 L Foot"]);
            }
            else if (posHandle.IKmode == HandleKun.IKMODE.RightLeg)
            {
                posHandle.SetParentBone(trBone["Bip01 R Foot"]);
            }
            else if (posHandle.IKmode == HandleKun.IKMODE.LeftArm)
            {
                posHandle.SetParentBone(trBone["Bip01 L Hand"]);
            }
            else if (posHandle.IKmode == HandleKun.IKMODE.RightArm)
            {
                posHandle.SetParentBone(trBone["Bip01 R Hand"]);
            }
            else
            {
                return;
            }

            IKList[(int)posHandle.IKmode].detachIKfromBone(trBone["Bip01"], currentMaidNo);
            
            //ikHandle.Pos = postPosition;
            posHandle.transform.localPosition = Vector3.zero;
            posHandle.Scale = 0.2f;

            posHandle.Rot = temp;//Quaternion.Euler(-90, 0, 90);



        }
        public bool checkIKAttach(int currentMaidNo)
        {
            return checkIKAttach(currentMaidNo,0) || checkIKAttach(currentMaidNo, 1) || 
                checkIKAttach(currentMaidNo, 2) || checkIKAttach(currentMaidNo, 3);

        }

        public bool checkIKAttach(int currentMaidNo, int No)
        {
            if (No <= 3)
            {
                return IKList[No].checkIKAttach(currentMaidNo);
            }
            else
                return false;

        }
        public Transform getIKTransform(int currentMaidNo, int No)
        {
            if (No <= 3)
            {
                return IKList[No].getIKTransform(currentMaidNo);
            }
            else
                return null;
        }

        public void detachIK(Action<Undo> undoListAdd, Func<bool[], bool[]> setIKButtonActive, bool[] boolList, Transform Bip01, int currentMaidNo, int No,string UndoFuncName)
        {

            if (No <= 3)
            {
                //Undoチェック
                if (UndoFuncName != "IKDetach:" + No + ":" + Bip01.ToString() + ":" + currentMaidNo)
                {
                    UndoFuncName = "IKDetach:" + No + ":" + Bip01.ToString() + ":" + currentMaidNo;
                    //IKでのUndo登録
                    undoListAdd(Undo.createUndoIK(this.doUndoFunc, this.checkIKAttach, this.getIKTransform, setIKButtonActive, boolList, GetMaid(currentMaidNo), currentMaidNo, 
                        IKList[No].checkIKAttach(currentMaidNo), IKList[No].getParentTransform(currentMaidNo), IKList[No].getIKPosition(currentMaidNo), No) );
                    //Debuginfo.Log(LogLabel + "Undo::" + UndoFuncName);
                }

                IKList[No].detachIK(Bip01, currentMaidNo);
            }
        }

        public string detachIKAll(Action<Undo> undoListAdd, Func<bool[], bool[]> setIKButtonActive, bool[] boolList, Transform Bip01, int currentMaidNo,string UndoFuncName)
        {
            //Undoチェック
            if (UndoFuncName != "IKDetachAll:" +  currentMaidNo)
            {
                UndoFuncName = "IKDetachAll:" + currentMaidNo;
                //IKでのUndo登録

                bool[] tempAttachArray = new bool[4] { false,false,false,false};
                Transform[] tempParentArray = new Transform[4] { null,null,null,null,};
                Vector3[] tempPosArray = new Vector3[4] { Vector3.zero, Vector3.zero , Vector3.zero , Vector3.zero };

                for (int No = 0; No < 4; ++No)
                {
                    tempAttachArray[No] = checkIKAttach(currentMaidNo, No);

                    if (tempAttachArray[No] == true)
                    {
                        tempParentArray[No] = getIKTransform(currentMaidNo, No).parent;
                        tempPosArray[No] = getIKTransform(currentMaidNo, No).localPosition;
                    }
                }
                undoListAdd(Undo.createUndoIKAll(this.doUndoFunc, this.checkIKAttach, this.getIKTransform, setIKButtonActive, boolList, GetMaid(currentMaidNo), currentMaidNo, tempAttachArray, tempParentArray, tempPosArray));
                //Debuginfo.Log(LogLabel +"Undo::" + UndoFuncName);

                Array.ForEach(IKList,list=>list.detachIK(Bip01, currentMaidNo));
            }

            return UndoFuncName;

        }

        public string detachAll(Action<Undo> undoListAdd, Func<bool[], bool[]> setIKButtonActive, bool[] boolList, string UndoFuncName)
        {
            
            //Undoチェック
            if (UndoFuncName != "IKDetachAll:MaidAll:")
            {
                UndoFuncName = "IKDetachAll:MaidAll:";
                //IKでのUndo登録

                IKManage tempIKManage = new IKManage(this);

                //Dictionary<string, Transform> goParentTransform;
                
                //var tempDic = attachIKMaidNo.Select((m) =>  new KeyValuePair<int ,Dictionary<int ,Transform>>(m, IKList.Select((list,index) => new KeyValuePair<int,Transform>( index, list.getParentTransform(m))).Where( n  => n.Value != null).ToDictionary(l=>l.Key,l=>l.Value) )).ToDictionary(k=>k.Key,k=>k.Value);


                undoListAdd(Undo.createUndoIKAllMaidAll(setIKButtonActive, boolList.Clone() as bool[], this, tempIKManage , getIKTransformParentDictionary, getIKTransformParentDictionary()));

                //Debuginfo.Log(LogLabel + "Undo::" + UndoFuncName);

                
                attachIKMaidNo.ForEach( m => Array.ForEach(IKList, list => list.detachAll(m)) );
            
                attachIKMaidNo.Clear();
                attachIKMaidList.Clear();
                trTargetIKBones.Clear();

            }
            return UndoFuncName;
        }

        public Dictionary<int,Dictionary<int ,Transform>> getIKTransformParentDictionary()
        {
            return attachIKMaidNo.Select((m) => new KeyValuePair<int, Dictionary<int, Transform>>(m, IKList.Select((list, index) => new KeyValuePair<int, Transform>(index, list.getParentTransform(m))).Where(n => n.Value != null).ToDictionary(l => l.Key, l => l.Value))).ToDictionary(k => k.Key, k => k.Value);
        }

        public bool checkParentName(int currentMaidNo, int No)
        {
            if (No <= 3)
            {
                return IKList[No].checkParentName(currentMaidNo);
            }
            else
                return false;
        }

        public bool checkMaidActive(int currentMaidNo)
        {
            return attachIKMaidList[currentMaidNo] != null && attachIKMaidList[currentMaidNo].Visible == true;
        }

        public int attachIKMaidNoCount()
        {
            return attachIKMaidNo.Count;
        }

        public bool trTargetIKBoneContainsNo(int currentMaidNo)
        {
            return trTargetIKBones.ContainsKey(currentMaidNo);
        }

        public void trTargetIKBoneAdd(int currentMaidNo, Transform[] tempTransformList)
        {
            trTargetIKBones.Add(currentMaidNo, tempTransformList);
        }

        //Undo用

        public void doUndoFuncAll(IKPropList[] _IKList, List<int> _attachIKMaidNo, Dictionary<int, Maid> _attachIKMaidList, Dictionary<int, Transform[]> _trTargetIKBones)
        {
            IKList = _IKList;
            attachIKMaidNo = _attachIKMaidNo;
            attachIKMaidList = _attachIKMaidList;
            trTargetIKBones = _trTargetIKBones;
        }

        public void doUndoFunc(int currentMaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos, int No)
        {
            if (No > 3)
                return;

            IKList[No].doUndoFunc(currentMaidNo, _isAttach, _trParent, _vPos);

            //ここで各種チェックを行う
            doUndoCheck(currentMaidNo);
        }


        public void doUndoFuncLeftLeg(int currentMaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos)
        {

            IKList[0].doUndoFunc(currentMaidNo, _isAttach, _trParent, _vPos);

            //ここで各種チェックを行う
            doUndoCheck(currentMaidNo);

        }

        public void doUndoFuncRightLeg(int currentMaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos)
        {

            IKList[1].doUndoFunc(currentMaidNo, _isAttach, _trParent, _vPos);

            //ここで各種チェックを行う
            doUndoCheck(currentMaidNo);

        }

        public void doUndoFuncLeftArm(int currentMaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos)
        {
            IKList[2].doUndoFunc(currentMaidNo, _isAttach, _trParent, _vPos);

            //ここで各種チェックを行う
            doUndoCheck(currentMaidNo);

        }

        public void doUndoFuncRightArm(int currentMaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos)
        {

            IKList[3].doUndoFunc(currentMaidNo, _isAttach, _trParent, _vPos);

            //ここで各種チェックを行う
            doUndoCheck(currentMaidNo);

        }

        //Undo用
        public void doUndoCheck(int currentMaidNo)
        {

            //ここで各種チェックを行う

            bool tempAttach =
            (IKList[0].checkIKAttach(currentMaidNo) ||
             IKList[1].checkIKAttach(currentMaidNo) ||
             IKList[2].checkIKAttach(currentMaidNo) ||
             IKList[3].checkIKAttach(currentMaidNo));

            if (tempAttach == true && !attachIKMaidNo.Contains(currentMaidNo))
            {
                attachIKMaidNo.Add(currentMaidNo);
                attachIKMaidList.Add(currentMaidNo, GetMaid(currentMaidNo));
            }
            else if (tempAttach == false && attachIKMaidNo.Contains(currentMaidNo))
            {
                attachIKMaidNo.Remove(currentMaidNo);
                attachIKMaidList.Remove(currentMaidNo);
            }
            
        }
    }
}
