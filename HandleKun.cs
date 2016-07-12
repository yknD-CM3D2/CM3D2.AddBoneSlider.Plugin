using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.AddBoneSlider.Plugin
{
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

        private ControllOnMouse controllOnMouseH;



        private ClickOnlyControll CoC;

        private int Legacymode;

        private bool bIKAttached = false;

        public bool bHandlePositionMode;

        public bool rightClicked = false;



        Texture2D m_texture_red = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_green = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_blue = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_red_2 = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_green_2 = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_blue_2 = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_white = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_yellow = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_cyan = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        Texture2D m_texture_magenta = new Texture2D(4, 4, TextureFormat.ARGB32, false);

        GameObject redring;
        GameObject bluering;
        GameObject greenring;
        GameObject redvector;
        GameObject bluevector;
        GameObject greenvector;
        GameObject whitecenter;

        GameObject cyancube;

        GizmoRender gizmoRender;

        private UILabel uiLabelIKBoneName;

        public enum IKMODE
        {
            None = 99,
            LeftLeg = 0,
            RightLeg = 1,
            LeftArm = 2,
            RightArm = 3
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

        public string IKTargetLabelString
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
                PosCenter,
                RotHead
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
                else if(wheelType == WheelType.PosCenter)
                {
                    clickPointVector = Input.mousePosition;
                    clickPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    oldValue = Camera.main.WorldToScreenPoint(transform.position).z;
                    clickPointVector = Camera.main.ScreenToWorldPoint(clickPointVector);
                }
                else if (wheelType == WheelType.RotHead)
                {
                    //ここから
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
                else if(wheelType == WheelType.PosCenter)
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
                else if (wheelType == WheelType.RotHead)
                {
                    Vector3 dragPointVector = Input.mousePosition;
                    dragPointVector.z = oldValue;//Camera.main.WorldToScreenPoint(transform.position).z;
                    dragPointVector = Camera.main.ScreenToWorldPoint(dragPointVector);
                    
                    switch (axisType)
                    {
                        case AxisType.NONE:
                            
                            //何かすごく回りくどい計算してるかもしれないので
                            //もっと簡潔に計算できる方法見つけたらここ書き換える
                            dragQuaternion = Quaternion.AngleAxis(
                                Vector3.Angle(clickPointVector - transform.parent.position, dragPointVector - transform.parent.position), 
                                new Vector3(0, 
                                Vector3.Dot(transform.parent.forward, (dragPointVector - clickPointVector)),
                                -Vector3.Dot(transform.parent.up, (dragPointVector - clickPointVector))));

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

        public HandleKun(int _Legacymode, UIAtlas _systemAtlas, Maid _maid = null, Transform _transform = null)
        {
            this.Legacymode = _Legacymode;

            SetMaterial(m_texture_red, new Color(1f, 0f, 0f, 0.5f), "red");
            SetMaterial(m_texture_green, new Color(0f, 1f, 0f, 0.5f), "green");
            SetMaterial(m_texture_blue, new Color(0f, 0f, 1f, 0.5f), "blue");

            SetMaterial(m_texture_red_2, new Color(1f, 0f, 0f, 0.5f), "red_2");
            SetMaterial(m_texture_green_2, new Color(0f, 1f, 0f, 0.5f), "green_2");
            SetMaterial(m_texture_blue_2, new Color(0f, 0f, 1f, 0.5f), "blue_2");

            SetMaterial(m_texture_white, new Color(1f, 1f, 1f, 0.5f), "white");
            SetMaterial(m_texture_yellow, new Color(1f, 0.92f, 0.04f, 0.3f), "yellow");
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
            goAngleHandle.name = "AngleHandle";
            goPositionHandle.name = "PositionHandle";

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
            

            
            cyancube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cyancube.renderer.receiveShadows = false;
            cyancube.renderer.castShadows = false;
            cyancube.renderer.useLightProbes = false;

            cyancube.renderer.material.mainTexture = m_texture_cyan;
            if (Legacymode == 0)
            {
                cyancube.renderer.material.shader = Shader.Find("Hidden/Transplant_Internal-Colored");

                
                //StreamReader sr = new StreamReader(@"C:\KISS\CM3D2\test\CM3D2_Toony_Lighted_Matrix.Shader");
                //string shader = sr.ReadToEnd();
                //sr.Close();
                //Material mat = new Material(shader);
                //cyancube.renderer.material.shader = mat.shader;
                

                cyancube.renderer.material.SetFloat("_ZTest", 6);
                cyancube.renderer.material.SetFloat("_Cull", 2);
                cyancube.renderer.material.SetFloat("_ZWrite", 0);
            }
            else
            {
                cyancube.renderer.material.shader = Shader.Find("CM3D2/Toony_Lighted_Trans");
                
            }
            cyancube.renderer.material.SetColor("_Color", new Color(0f, 1f, 1f, 0.5f));

            cyancube.renderer.material.renderQueue = BaseRenderQueue + 9;
            cyancube.transform.localPosition = new Vector3(-1, 0, 0);
            cyancube.transform.localScale = new Vector3(0.20f, 0.20f, 0.20f);
            cyancube.name = "cyancube";
            cyancube.transform.parent = this.goAngleHandle.transform;

            this.controllOnMouseH = cyancube.AddComponent<ControllOnMouse>();
            this.controllOnMouseH.wheelType = ControllOnMouse.WheelType.RotHead;
            this.controllOnMouseH.axisType = ControllOnMouse.AxisType.NONE;
            


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
        private void SetMaterial(Texture2D m_texture, Color _color, string name)
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
                            handleScale = 0.5f;
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
            {
                return false;
            }
                        
            if (bHandlePositionMode == false)
            {

                if (controllOnMouseX.DragFinished || controllOnMouseY.DragFinished || controllOnMouseZ.DragFinished || controllOnMouseH.DragFinished)
                {
                    Visible = false;
                    controllOnMouseX.DragFinished = false;
                    controllOnMouseY.DragFinished = false;
                    controllOnMouseZ.DragFinished = false;

                    controllOnMouseH.DragFinished = false;

                    redring.renderer.material.mainTexture = m_texture_red_2;
                    greenring.renderer.material.mainTexture = m_texture_green_2;
                    bluering.renderer.material.mainTexture = m_texture_blue_2;

                    cyancube.renderer.material.mainTexture = m_texture_cyan;


                    redring.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                    greenring.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                    bluering.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));

                    cyancube.renderer.material.SetColor("_Color", new Color(0, 1, 1, 0.5f));

                    //SetParentBone(trParentBone);
                    Visible = true;
                }

                if (!controllOnMouseX.Dragged && !controllOnMouseY.Dragged && !controllOnMouseZ.Dragged && !controllOnMouseH.Dragged)
                {
                    if (controllOnMouseX.mouseOver)
                    {
                        redring.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        redring.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        redring.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                        redring.renderer.material.mainTexture = m_texture_red_2;
                    }

                    if (controllOnMouseY.mouseOver)
                    {
                        greenring.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        greenring.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        greenring.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                        greenring.renderer.material.mainTexture = m_texture_green_2;

                    }

                    if (controllOnMouseZ.mouseOver)
                    {
                        bluering.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        bluering.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        bluering.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));
                        bluering.renderer.material.mainTexture = m_texture_blue_2;
                    }

                    
                    if (controllOnMouseH.mouseOver)
                    {
                        cyancube.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        cyancube.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        cyancube.renderer.material.SetColor("_Color", new Color(0, 1, 1, 0.5f));
                        cyancube.renderer.material.mainTexture = m_texture_cyan;
                    }
                    
                }

                return (controllOnMouseX.Dragged || controllOnMouseY.Dragged || controllOnMouseZ.Dragged || controllOnMouseH.Dragged);
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
                        redvector.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        redvector.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        redvector.renderer.material.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                        redvector.renderer.material.mainTexture = m_texture_red_2;

                    }

                    if (controllOnMousePY.mouseOver)
                    {
                        greenvector.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        greenvector.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        greenvector.renderer.material.SetColor("_Color", new Color(0, 1, 0, 0.5f));
                        greenvector.renderer.material.mainTexture = m_texture_green_2;

                    }

                    if (controllOnMousePZ.mouseOver)
                    {
                        bluevector.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
                        bluevector.renderer.material.mainTexture = m_texture_yellow;
                    }
                    else
                    {
                        bluevector.renderer.material.SetColor("_Color", new Color(0, 0, 1, 0.5f));
                        bluevector.renderer.material.mainTexture = m_texture_blue_2;
                    }

                    if (controllOnMouseC.mouseOver)
                    {
                        whitecenter.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
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
                        goIKBoneTarget.renderer.material.SetColor("_Color", new Color(1f, 0.92f, 0.04f, 0.5f));
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
            
            if (Legacymode == 0 && gizmoRender.Visible != Visible)
            {
                Visible = gizmoRender.Visible;
            }
            
        }

        public void setVisible(bool bVisible)
        {
            if(Legacymode == 0)
                gizmoRender.Visible = bVisible;

            Visible = bVisible;
        }

        //ハンドル君生存チェック
        public bool checkAlive()
        {
            return goHandleMasterObject != null;
        }

        //ハンドル君消去チェック
        public bool checkBanishment()
        {
            return GetParentBone() == null ? true : GetParentBone().gameObject.activeInHierarchy;
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
            
            else if (controllOnMouseH.Dragged)
            {
                return controllOnMouseH.dragQuaternion;
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
}
