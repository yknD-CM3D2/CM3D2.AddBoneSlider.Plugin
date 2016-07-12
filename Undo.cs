using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.AddBoneSlider.Plugin
{
    //Undo履歴管理用
    public class UndoList
    {
        //Undo履歴
        LinkedList<Undo> _llundolist;
        //Redo履歴
        LinkedList<Undo> _llredolist;
        //Undo容量
        int size;

        public UndoList(int _size = 10)
        {
            _llundolist = new LinkedList<Undo>();
            _llredolist = new LinkedList<Undo>();
            size = _size;
        }

        public void Add(Undo _undo)
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
                Undo redo = _llundolist.Last().doUndo();

                //Debuginfo.Log(AddBoneSlider.PluginName + " : doUndo");

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
                Undo undo = _llredolist.Last().doUndo();

                //Debuginfo.Log(AddBoneSlider.PluginName + " : doRedo");

                //Redoが成功していればUndo要素をUndo履歴に加える
                if (undo != null)
                {
                    _llundolist.AddLast(undo);
                }

                //Redo履歴の最後尾の要素を削除
                _llredolist.RemoveLast();
            }
        }

        public bool containName(string _Name)
        {
            return _llundolist.Any(n=>n.undoName == _Name) || _llredolist.Any(n => n.undoName == _Name);
        }
    }

    //引数5つの関数用
    public delegate void Action<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

    public class Undo
    {

        public readonly string undoName;
        public readonly Func<Undo> doUndo;

        public Undo(Func<Undo> _doUndoFunc,string _undoName ="")
        {
            doUndo = _doUndoFunc;
            undoName = _undoName;

        }

        public static Undo createUndoBone(Dictionary<Transform, Quaternion> qBonelocal)
        {
            return new Undo(() =>
            {
                //対象ボーンが消えてないかチェック
                if (qBonelocal.Any(n => (n.Key == null && n.Key.gameObject.activeInHierarchy == false)))
                    return null;

                //現在の状態を元にRedo要素を作成
                Dictionary<Transform, Quaternion> re_qBonelocal = qBonelocal.ToDictionary(n => n.Key, n => n.Key.localRotation);

                //状態復元
                foreach (KeyValuePair<Transform, Quaternion> pair in qBonelocal)
                {
                    pair.Key.localRotation = pair.Value;
                }

                return createUndoBone(re_qBonelocal);
            });
        }

        public static Undo createUndoBone(Transform _tr, Quaternion _qua)
        {
            return new Undo(() =>
            {
                //対象ボーンが消えてないかチェック
                if (_tr == null && _tr.gameObject.activeInHierarchy == false)
                    return null;

                //現在の状態を元にRedo要素を作成
                Quaternion re_qua = _tr.localRotation;

                //状態復元
                _tr.localRotation = _qua;

                return createUndoBone(_tr, re_qua);
            });
        }

        public static Undo createUndoBone(IEnumerable<KeyValuePair<string, Transform>> _Dic)
        {
            return createUndoBone(_Dic.ToDictionary(n => n.Value, n => n.Value.localRotation));
        }

        public static Undo createUndoBonePos(Transform _tr, Vector3 _vPos)
        {
            return new Undo(() =>
            {
                if (_tr != null && _tr.gameObject.activeInHierarchy)
                {
                    Vector3 _tempPos = _tr.localPosition;

                    _tr.localPosition = _vPos;

                    return createUndoBonePos(_tr, _tempPos);
                }
                else
                {
                    return null;
                }

            });
        }

        public static Undo createUndoBoneBoth(Transform _tr)
        {
            if (_tr != null && _tr.gameObject.activeInHierarchy)
                return createUndoBoneBoth(_tr, _tr.localRotation, _tr.localPosition);
            else
                return null;
        }

        public static Undo createUndoBoneBoth(Transform _tr, Quaternion _qua, Vector3 _vPos)
        {
            return new Undo(() =>
            {
                if (_tr != null && _tr.gameObject.activeInHierarchy)
                {

                    Quaternion _tempq = _tr.localRotation;
                    Vector3 _tempPos = _tr.localPosition;


                    _tr.localPosition = _vPos;
                    _tr.localRotation = _qua;

                    return createUndoBoneBoth(_tr, _tempq, _tempPos);
                }
                else
                {
                    return null;
                }

            });
        }

        public static Undo createUndoBoneAll(Dictionary<Transform, Quaternion> _qBone, Vector3 _vPos)
        {
            return new Undo(() =>
            {
                //対象ボーンが消えてないかチェック
                if (_qBone.Any(n => (n.Key == null && n.Key.gameObject.activeInHierarchy == false)))
                    return null;

                //現在の状態を元にRedo要素を作成
                Dictionary<Transform, Quaternion> re_qBonelocal = _qBone.ToDictionary(n => n.Key, n => n.Key.localRotation);
                Vector3 _tempPos = _qBone.Where(n => n.Key.name == "Bip01").First().Key.localPosition;

                //状態復元
                foreach (KeyValuePair<Transform, Quaternion> pair in _qBone)
                {
                    pair.Key.localRotation = pair.Value;
                    if (pair.Key.name == "Bip01")
                    {
                        pair.Key.localPosition = _vPos;
                    }
                }

                return createUndoBoneAll(re_qBonelocal, _tempPos);
            });
        }

        public static Undo createUndoBoneAll(IEnumerable<KeyValuePair<string, Transform>> _Dic)
        {
            return createUndoBoneAll(_Dic.ToDictionary(n => n.Value, n => n.Value.localRotation), (_Dic.Where(n => n.Value.name == "Bip01").First().Value.localPosition));
        }

        public static Undo createUndoBoneAll(Dictionary<string, Transform> _trDic, Dictionary<string, Quaternion> _qDic, Vector3 _vPos)
        {
            return createUndoBoneAll(_trDic.Select(n => new { k = n.Value, v = _qDic[n.Key] }).ToDictionary(m => m.k, m => m.v), _vPos);
        }

        public static Undo createUndoAllpos(Vector3 _vPos, bool _isPos = false)
        {
            return new Undo(() =>
            {
                Vector3 _temp;
                if (_isPos == true)
                {
                    _temp = GameMain.Instance.CharacterMgr.GetCharaAllPos();

                    GameMain.Instance.CharacterMgr.SetCharaAllPos(_vPos);
                }
                else
                {
                    _temp = GameMain.Instance.CharacterMgr.GetCharaAllRot();

                    GameMain.Instance.CharacterMgr.SetCharaAllRot(_vPos);
                }
                return createUndoAllpos(_temp, _isPos);
            });
        }

        public static Undo createUndoAllposBoth(Vector3 _vPos, Vector3 _vRot)
        {
            return new Undo(() =>
            {

                Vector3 _tempP = GameMain.Instance.CharacterMgr.GetCharaAllPos();

                GameMain.Instance.CharacterMgr.SetCharaAllPos(_vPos);

                Vector3 _tempR = GameMain.Instance.CharacterMgr.GetCharaAllRot();

                GameMain.Instance.CharacterMgr.SetCharaAllRot(_vPos);

                return createUndoAllposBoth(_tempP, _tempR);
            });
        }

        public static Undo createUndoOffset(Maid _maid, Vector3 _offset, bool _isPos = false)
        {
            return new Undo(() =>
            {
                if (_maid != null && _maid.Visible == true)
                {
                    Vector3 _temp;
                    if (_isPos == true)
                    {
                        _temp = _maid.transform.localPosition;

                        _maid.SetPos(_offset);
                    }
                    else
                    {
                        _temp = _maid.GetRot();

                        _maid.SetRot(_offset);
                    }

                    return createUndoOffset(_maid, _temp, _isPos);
                }
                else
                {
                    return null;
                }
            });
        }

        public static Undo createUndoOffsetBoth(Maid _maid, Vector3 _offsetP, Vector3 _offsetR)
        {
            return new Undo(() =>
            {
                if (_maid != null && _maid.Visible == true)
                {

                    Vector3 _tempP = _maid.transform.localPosition;

                    _maid.SetPos(_offsetP);

                    Vector3 _tempR = _maid.GetRot();

                    _maid.SetRot(_offsetR);

                    return createUndoOffsetBoth(_maid, _tempP, _tempR);
                }
                else
                {
                    return null;
                }
            });
        }

        public static Undo createUndoEye(Maid _maid, Quaternion _qEye, bool _isRight)
        {
            return new Undo(() =>
            {
                if (_maid != null && _maid.Visible == true)
                {
                    Quaternion eye;
                    if (_isRight)
                    {
                        eye = _maid.body0.quaDefEyeR;
                        _maid.body0.quaDefEyeR = _qEye;
                    }
                    else
                    {
                        eye = _maid.body0.quaDefEyeL;
                        _maid.body0.quaDefEyeL = _qEye;
                    }
                    return createUndoEye(_maid, eye, _isRight);
                }
                else
                {
                    return null;
                }
            });
        }
        public static Undo createUndoEyeBoth(Maid _maid, Quaternion _qEyeR, Quaternion _qEyeL)
        {
            return new Undo(() =>
            {
                if (_maid != null && _maid.Visible == true)
                {
                    Quaternion eyeR = _maid.body0.quaDefEyeR;
                    _maid.body0.quaDefEyeR = _qEyeR;

                    Quaternion eyeL = _maid.body0.quaDefEyeL;
                    _maid.body0.quaDefEyeL = _qEyeL;

                    return createUndoEyeBoth(_maid, eyeR, eyeL);
                }
                else
                {
                    return null;
                }
            });
        }

        public static Undo createUndoSecret(jiggleBone _jMune, float _updown, float _yori)
        {
            return new Undo(() =>
            {
                if (_jMune != null && _jMune.gameObject.activeInHierarchy == true)
                {
                    float tempud = _jMune.MuneUpDown;
                    float _tempy = _jMune.MuneYori;
                    _jMune.MuneUpDown = _updown;
                    _jMune.MuneYori = _yori;
                    return createUndoSecret(_jMune, tempud, _tempy);
                }
                else
                {
                    return null;
                }
            });
        }

        public static Undo createUndoSecretBoth(jiggleBone _jMuneR, float _updownR, float _yoriR, jiggleBone _jMuneL, float _updownL, float _yoriL)
        {
            return new Undo(() =>
            {
                if (_jMuneR != null && _jMuneR.gameObject.activeInHierarchy == true && _jMuneL != null && _jMuneL.gameObject.activeInHierarchy == true)
                {
                    float tempudR = _jMuneR.MuneUpDown;
                    float _tempyR = _jMuneR.MuneYori;
                    _jMuneR.MuneUpDown = _updownR;
                    _jMuneR.MuneYori = _yoriR;

                    float tempudL = _jMuneL.MuneUpDown;
                    float _tempyL = _jMuneL.MuneYori;
                    _jMuneL.MuneUpDown = _updownL;
                    _jMuneL.MuneYori = _yoriL;
                    return createUndoSecretBoth(_jMuneR, tempudR, _tempyR, _jMuneL, tempudL, _tempyL);
                }
                else
                {
                    return null;
                }
            });
        }


        public static Undo createUndoAll(Maid _maid, Dictionary<Transform, Quaternion> _qBone, Vector3 _vPos, 
            Vector3 _vAllPos, Vector3 _vAllRot, Vector3 _offsetP, Vector3 _offsetR,
            Quaternion _qEyeR, Quaternion _qEyeL,
            jiggleBone _jMuneR, float _updownR, float _yoriR, jiggleBone _jMuneL, float _updownL, float _yoriL)
        {
            return new Undo(() =>
            {
                if (_maid != null && _maid.Visible == true &&_jMuneR != null && _jMuneR.gameObject.activeInHierarchy == true && _jMuneL != null && _jMuneL.gameObject.activeInHierarchy == true)
                {

                    //対象ボーンが消えてないかチェック
                    if (_qBone.Any(n => (n.Key == null && n.Key.gameObject.activeInHierarchy == false)))
                        return null;

                    //現在の状態を元にRedo要素を作成
                    Dictionary<Transform, Quaternion> re_qBonelocal = _qBone.ToDictionary(n => n.Key, n => n.Key.localRotation);
                    Vector3 _tempPos = _qBone.Where(n => n.Key.name == "Bip01").First().Key.localPosition;

                    //状態復元
                    foreach (KeyValuePair<Transform, Quaternion> pair in _qBone)
                    {
                        pair.Key.localRotation = pair.Value;
                        if (pair.Key.name == "Bip01")
                        {
                            pair.Key.localPosition = _vPos;
                        }
                    }

                    Vector3 _tempAllP = GameMain.Instance.CharacterMgr.GetCharaAllPos();

                    GameMain.Instance.CharacterMgr.SetCharaAllPos(_vAllPos);

                    Vector3 _tempAllR = GameMain.Instance.CharacterMgr.GetCharaAllRot();

                    GameMain.Instance.CharacterMgr.SetCharaAllRot(_vAllRot);

                    Vector3 _tempOffsetP = _maid.transform.localPosition;

                    _maid.SetPos(_offsetP);

                    Vector3 _tempOffsetR = _maid.GetRot();

                    _maid.SetRot(_offsetR);

                    Quaternion eyeR = _maid.body0.quaDefEyeR;
                    _maid.body0.quaDefEyeR = _qEyeR;

                    Quaternion eyeL = _maid.body0.quaDefEyeL;
                    _maid.body0.quaDefEyeL = _qEyeL;

                    float tempudR = _jMuneR.MuneUpDown;
                    float _tempyR = _jMuneR.MuneYori;
                    _jMuneR.MuneUpDown = _updownR;
                    _jMuneR.MuneYori = _yoriR;

                    float tempudL = _jMuneL.MuneUpDown;
                    float _tempyL = _jMuneL.MuneYori;
                    _jMuneL.MuneUpDown = _updownL;
                    _jMuneL.MuneYori = _yoriL;

                    return createUndoAll(_maid, re_qBonelocal, _tempPos,
                        _tempAllP,_tempAllR, _tempOffsetP, _tempOffsetR,
                        eyeR, eyeL, _jMuneR, tempudR, _tempyR, _jMuneL, tempudL, _tempyL);
                }
                else
                {
                    return null;
                }
            });
        }

        public static Undo createUndoIK(Action<int, bool, Transform, Vector3, int> doUndoFunc, Func<int, int, bool> checkIKAttach, Func<int, int, Transform> getIKTransform, Func<bool[], bool[]> setIKButtonActive, bool[] _boolList, Maid _maid, int _MaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos, int No)
        {
            return new Undo(() =>
            {
                if (_trParent != null && _maid != null && _maid.Visible == true)
                {
                    bool tempAttach = checkIKAttach(_MaidNo, No);
                    Transform tempParent = getIKTransform(_MaidNo, No).parent;
                    Vector3 tempPos = getIKTransform(_MaidNo, No).localPosition;

                    doUndoFunc(_MaidNo, _isAttach, _trParent, _vPos, No);

                    bool[] tempboolList = setIKButtonActive(_boolList);


                    return createUndoIK(doUndoFunc, checkIKAttach, getIKTransform, setIKButtonActive, tempboolList, _maid, _MaidNo, tempAttach, tempParent, tempPos, No);

                }
                else
                {
                    return null;
                }


            });
        }

        public static Undo createUndoIKFirst(Action<int, bool, Transform, Vector3, int> doUndoFunc, Func<int, int, bool> checkIKAttach, Func<int, int, Transform> getIKTransform, Func<int, int, Quaternion[]> getBones, Action<int, int, Quaternion[]> setBones, Quaternion[] bones, Func<bool[], bool[]> setIKButtonActive, bool[] _boolList, Maid _maid, int _MaidNo, bool _isAttach, Transform _trParent, Vector3 _vPos, int No)
        {
            return new Undo(() =>
            {
                if (_trParent != null && _maid != null && _maid.Visible == true)
                {
                    bool tempAttach = checkIKAttach(_MaidNo, No);
                    Transform tempParent = getIKTransform(_MaidNo, No).parent;
                    Vector3 tempPos = getIKTransform(_MaidNo, No).localPosition;

                    doUndoFunc(_MaidNo, _isAttach, _trParent, _vPos, No);
                    if (!_isAttach)
                    {
                        //ここでIKで動いたボーンを元に戻す
                        setBones(_MaidNo, No, bones);
                    }
                    bool[] tempboolList = setIKButtonActive(_boolList);

                    return createUndoIKFirst(doUndoFunc, checkIKAttach, getIKTransform, getBones, setBones, getBones(_MaidNo, No), setIKButtonActive, tempboolList, _maid, _MaidNo, tempAttach, tempParent, tempPos, No);
                }
                else
                {
                    return null;
                }


            });
        }

        public static Undo createUndoIKAll(Action<int, bool, Transform, Vector3, int> doUndoFunc, Func<int, int, bool> checkIKAttach, Func<int, int, Transform> getIKTransform, Func<bool[], bool[]> setIKButtonActive, bool[] _boolList, Maid _maid, int _MaidNo, bool[] _isAttach, Transform[] _trParent, Vector3[] _vPos)
        {
            return new Undo(() =>
            {
                if (_trParent.Any(n => n != null) && _maid != null && _maid.Visible == true)
                {

                    bool[] tempAttachArray = new bool[4] {false,false,false,false};
                    Transform[] tempParentArray = new Transform[4] { null,null,null,null};
                    Vector3[] tempPosArray = new Vector3[4] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

                    for (int No = 0; No < 4; ++No)
                    {
                        if (_trParent[No] != null)
                        {
                            tempAttachArray[No] = checkIKAttach(_MaidNo, No);
                            
                            tempParentArray[No] = getIKTransform(_MaidNo, No).parent;
                            tempPosArray[No] = getIKTransform(_MaidNo, No).localPosition;
                            doUndoFunc(_MaidNo, _isAttach[No], _trParent[No], _vPos[No], No);
                        }
                    }

                    bool[] tempboolList = setIKButtonActive(_boolList);

                    return createUndoIKAll(doUndoFunc, checkIKAttach, getIKTransform, setIKButtonActive, tempboolList, _maid, _MaidNo, tempAttachArray, tempParentArray, tempPosArray);
                }
                else
                {
                    return null;
                }


            });
        }

        public static Undo createUndoIKAllMaidAll(Action<IKPropList[], List<int>, Dictionary<int, Maid>, Dictionary<int, Transform[]>> doUndoFuncAll, Func<bool[], bool[]> setIKButtonActive, bool[] _boolList, IKPropList[] IKList, List<int> attachIKMaidNo, Dictionary<int, Maid> attachIKMaidList, Dictionary<int, Transform[]> trTargetIKBones)
        {
            return new Undo(() =>
            {
                IKPropList[] tempIKList = {
                    new IKPropList(IKList[0]), new IKPropList(IKList[1]),
                    new IKPropList(IKList[2]), new IKPropList(IKList[3])};

                List<int> tempattachIKMaidNo = new List<int>(attachIKMaidNo);
                Dictionary<int, Maid> tempattachIKMaidList = new Dictionary<int, Maid>(attachIKMaidList);
                Dictionary<int, Transform[]> temptrTargetIKBones = new Dictionary<int, Transform[]>(trTargetIKBones);

                doUndoFuncAll(IKList, attachIKMaidNo, attachIKMaidList, trTargetIKBones);

                bool[] tempboolList = setIKButtonActive(_boolList);

                return createUndoIKAllMaidAll(doUndoFuncAll, setIKButtonActive, tempboolList, tempIKList, tempattachIKMaidNo, tempattachIKMaidList, temptrTargetIKBones);


            });
        }

        public static Undo createUndoIKAllMaidAll(Func<bool[], bool[]> setIKButtonActive, bool[] _boolList, IKManage _source,IKManage _copy,Func<Dictionary<int,Dictionary<int,Transform>>>getDicFunc, Dictionary<int, Dictionary<int, Transform>>_Dic)
        {
            return new Undo(() =>
            {
                var tempDic = getDicFunc();

                IKManage temp = new IKManage(_source);
                
                _source.IKList[0].assignment(_copy.IKList[0]);
                _source.IKList[1].assignment(_copy.IKList[1]);
                _source.IKList[2].assignment(_copy.IKList[2]);
                _source.IKList[3].assignment(_copy.IKList[3]);

                _source.trTargetIKBones = _copy.trTargetIKBones;
                _source.attachIKMaidList = _copy.attachIKMaidList;
                _source.attachIKMaidNo = _copy.attachIKMaidNo;

                foreach(var i in _Dic)
                {
                    foreach(var j in i.Value)
                    {
                        _source.IKList[j.Key].goIKTarget[i.Key] =  new GameObject();
                        Vector3 tempVec = _source.IKList[j.Key].getIKPositionCurrent(i.Key);
                        _source.IKList[j.Key].setParentTransform(i.Key, j.Value);
                        _source.IKList[j.Key].setIKPosition(i.Key, tempVec);
                    }
                }

                bool[] tempboolList = setIKButtonActive(_boolList);

                return createUndoIKAllMaidAll(setIKButtonActive, tempboolList, _source, temp,getDicFunc,tempDic);


            }, "IKAllMaidAll");
        }

    }
}
