using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.AddBoneSlider.Plugin
{

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
}
