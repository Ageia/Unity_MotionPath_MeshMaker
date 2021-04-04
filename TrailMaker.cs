#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MotionPath : MonoBehaviour
{
    [HideInInspector] public Animator Animator;

    [HideInInspector] public float StartFrame = 0;
    [HideInInspector] public float EndFrame = 60;
    [HideInInspector] public float AnimationSlider;


    [HideInInspector] public AnimationClip[] AnimationClips;
    [HideInInspector] public string[] AniClipsName;
    [HideInInspector] public int SelectAniClip;
    [HideInInspector] public string PlayStateName; //재생할 스테이트 이름


    /////////////////////////////////////////
    ////////        Path Info        ////////
    /////////////////////////////////////////
    public List<PathInfoData> PathInfo = new List<PathInfoData>(2);
    [System.Serializable]
    public class PathInfoData
    {
        [HideInInspector] public GameObject TargetObject;
        //Path Info 1
        [HideInInspector] public Vector3[] PathPos = new Vector3[0]; //실제 쓰는 최종 위치 데이터

        [HideInInspector] public int PathFrame = 120;

        [HideInInspector] public bool PathViewerSetting = false;
        [HideInInspector] public bool AutoUpdate = true;
        [HideInInspector] public Color PathColor = GetRandomColor();
        [HideInInspector] public float PathWidth = 2;

        //버텍스 평균치 구하는 계산 변수들
        [HideInInspector] public bool Average_AutoUpdate = true; //자동 업데이트
        public Vector3[] AveragePos = new Vector3[0]; //버텍스 평균 위치
        [HideInInspector] public int Average_Detail = 10; //디테일값
        [HideInInspector] public float Average_Distance = 0.25f; //버텍스들의 거리값

        [HideInInspector] public int EditVertIdx = int.MaxValue; //버텍스 위치를 조정하는 기능
        [HideInInspector] public bool EditMode = false; //버텍스 수정 모드
    }

    ////////////////////////////////////////////
    ////////        Generatemesh        ////////
    ////////////////////////////////////////////
    [HideInInspector] public MeshFilter MeshFilter;
    [HideInInspector] public MeshRenderer MeshRenderer;
    [HideInInspector] public CreateMeshInfoData CreateMeshInfo;
    [System.Serializable]
    public class CreateMeshInfoData
    {
        [HideInInspector] public MeshFilter MeshFilter;
        [HideInInspector] public bool InvertUV_X;
        [HideInInspector] public bool InvertUV_Y;
        [HideInInspector] public int Count_Y = 1;
    }

    //랜덤 컬러 (채도 높게)
    static Color GetRandomColor()
    {
        Color OutputColor = new Color();

        int HightColor = Random.Range(0, 3);
        if (HightColor == 0)
        {
            OutputColor.r = 1;
            OutputColor.g = Random.Range((float)0, (float)1);
            OutputColor.b = Random.Range((float)0, (float)1);
            OutputColor.a = 1;
        }
        else if (HightColor == 1)
        {
            OutputColor.r = Random.Range((float)0, (float)1);
            OutputColor.g = 1;
            OutputColor.b = Random.Range((float)0, (float)1);
            OutputColor.a = 1;
        }
        else
        {
            OutputColor.r = Random.Range((float)0, (float)1);
            OutputColor.g = Random.Range((float)0, (float)1);
            OutputColor.b = 1;
            OutputColor.a = 1;
        }

        return OutputColor;
    }

    //Toolbar
    [HideInInspector] public int SelectToolbar = 0;
    [HideInInspector] public int SelectPath = 0;
    [HideInInspector] public string[] ToolbarName = { "Animation", "Path 1", "Path 2", "Path 3", "Mesh" };


    private void Awake()
    {
        Debug.Log("최초 생성합니다.(패스2개)");
        if (PathInfo.Count == 0)
        {
            PathInfo.Add(new PathInfoData());
            PathInfo.Add(new PathInfoData());
        }

        //만들 메쉬 가져오기
        SetMesh();
    }

    //씬 GUI 그리기 위한 델리게이트 추가
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += DrawSceneGUI;
    }
    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= DrawSceneGUI;
    }

    void DrawSceneGUI(SceneView sceneView)
    {
        MotionPath Ge = this;


        for (int i = 0; i < PathInfo.Count; i++)
        {

            Handles.color = PathInfo[i].PathColor;
            Handles.DrawAAPolyLine(PathInfo[i].PathWidth, Ge.PathInfo[i].PathPos.Length, Ge.PathInfo[i].PathPos);
            Handles.color = GUI.color;

            //버텍스 평균치 그리기
            GUIStyle Style = new GUIStyle();
            Style.contentOffset = new Vector2(-7, -9); //아이콘 위치 조정
            GUIContent Test = new GUIContent();

            //////////////////////////////////////////////////
            /////////       버텍스 위치 그리기      ///////////
            //////////////////////////////////////////////////
            if (PathInfo[i].EditMode)
            {
                VertEditMode(PathInfo[i]);
            }
            else
            {
                for (int j = 0; j < PathInfo[i].AveragePos.Length; j++)
                {
                    Handles.Label(PathInfo[i].AveragePos[j], EditorGUIUtility.IconContent("winbtn_mac_close"), Style);
                }
            }

        }
    }

    //버텍스 위치 수정 기능
    void VertEditMode(PathInfoData GetPathInfo)
    {
        //선택한 버텍스용 스타일
        GUIStyle Style = new GUIStyle();
        Style.contentOffset = new Vector2(-7, -9); //아이콘 위치 조정

        float ButtonSize = 15;
        float ButtonPosAdd = -ButtonSize / 2;
        //패스 그리기

        for (int j = 0; j < GetPathInfo.AveragePos.Length; j++)
        {
            if (j != GetPathInfo.EditVertIdx)
            {
                Handles.BeginGUI();
                GUI.backgroundColor = new Color(2, 2, 2);
                if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(GetPathInfo.AveragePos[j]).x + ButtonPosAdd, HandleUtility.WorldToGUIPoint(GetPathInfo.AveragePos[j]).y + ButtonPosAdd, ButtonSize, ButtonSize), ""))
                {
                    GetPathInfo.EditVertIdx = j;
                    //Genarator.MovePosition.Insert(i, new Vector3(Genarator.MovePosition[i].x, Genarator.MovePosition[i].y, Genarator.MovePosition[i].z));
                }
                GUI.backgroundColor = GUI.color;
                Handles.EndGUI();
            }
            //현재 선택중인 버텍스
            else
            {
                Handles.Label(GetPathInfo.AveragePos[j], EditorGUIUtility.IconContent("d_winbtn_mac_min"), Style);
            }
        }

        //위치 기즈모
        if (GetPathInfo.AveragePos.Length > GetPathInfo.EditVertIdx)
        {
            GetPathInfo.AveragePos[GetPathInfo.EditVertIdx] = Handles.PositionHandle(GetPathInfo.AveragePos[GetPathInfo.EditVertIdx], Quaternion.identity);
        }
    }




    ////////////////////////////////////////////////////////////
    ////////////////        메쉬 구성       /////////////////////
    ////////////////////////////////////////////////////////////
    #region 
    void SetMesh()
    {
        MeshFilter = GetComponent<MeshFilter>();
        MeshRenderer = GetComponent<MeshRenderer>();

        MeshFilter.mesh = new Mesh();
        MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        MeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        MeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        MeshRenderer.materials[0] = new Material(Shader.Find("Diffuse"));
    }

    //메쉬 생성
    public void GenerateMesh(int YCount)
    {
        // #endregion
        MeshFilter.mesh.Clear();

        //Vertices
        MeshFilter.mesh.vertices = SetVertPos(YCount).ToArray();
        Debug.Log(MeshFilter.mesh.vertices.Length);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////일단 여기까지는 완료.... 

        //Triangles
        List<Vector3> Tris = SetTriList(YCount); //삼각면 할당
        MeshFilter.mesh.triangles = Tri_ConvertIntArray(Tris); //Vector3를 Int배열로 변환

        //UV
        //MeshFilter.mesh.uv = SetUV(XCount, YCount);

        //Debug.Log("메쉬생성 완료");
    }

    //버텍스 위치 할당
    public List<Vector3> VertPosList;
    List<Vector3> SetVertPos(int YCount)
    {
        List<Vector3> VertPos = new List<Vector3>();
        for (int X = 0; X < PathInfo[0].AveragePos.Length; X++) //가로줄
        {
            for (int Y = 0; Y < PathInfo.Count ; Y++) //세로줄
            {
                Debug.Log(Y);
                for (int R = 0; R < CreateMeshInfo.Count_Y; R++)  //세로줄 디테일값
                {
                    
                    // float ValueY = (float)Y / ((PathInfo.Count / 2) * CreateMeshInfo.Count_Y); //이게 0이면 P1 위치에 가까워짐, 이게 1dlaus P2위치에 가까워짐

                    // //Vector3 P1Bezier = GetPos_MulBezier(P_1, ValueX);
                    // //Vector3 P2Bezier = GetPos_MulBezier(P_2, ValueX);

                    // Vector3 InputVector3 = Vector3.Lerp(PathInfo[0].AveragePos[X], PathInfo[1].AveragePos[X], ValueY) - transform.position;
                    // VertPos.Add(InputVector3);
                    //Debug.Log(X.ToString() + InputVector3);

                    //if(Y + 1 < PathInfo.Count)
                    //{
                    //    float Value = (float)R / (float)CreateMeshInfo.Count_Y; // 0 ~ 1 까지 값
                    //    Vector3 InputVector3 = Vector3.Lerp(PathInfo[Y].AveragePos[X], PathInfo[Y + 1].AveragePos[X], Value);
                    //    VertPos.Add(InputVector3);
                    //}
                }
                VertPos.Add(PathInfo[Y].AveragePos[X]);
            }
        }
        VertPosList = VertPos;
        return VertPos;
    }

    // //버텍스 갯수
    // int GetVertCount(int XCount, int YCount)
    // {
    //     int Final = XCount * YCount;
    //     Debug.Log("버텍스 인덱스 갯수 : " + Final);
    //     return Final;
    // }

    //삼각면 할당 계산
    public List<Vector3> Tri;
    List<Vector3> SetTriList(int YCount)
    {
        List<Vector3> NewList = new List<Vector3>();
        NewList.Clear();
        for (int x = 0; x < PathInfo[0].AveragePos.Length; x++)
        {
            for (int y = 0; y < (PathInfo.Count / 2) * YCount; y++)
            {
                //Debug.Log(y + " , " + x); //여기서 2곱한 뒤에 3곱하면 인덱스 데이터 들어옴
                //NewList.Add(GetTri(YCount, y, x, true)); //윗면
                //NewList.Add(GetTri(YCount, y, x, false)); //아랫면
            }
        }
        Tri = NewList;
        return NewList;
    }

    // Vector3 GetTri(int YCount, int Y, int X, bool Under) //Under는 아래 삼각면
    // {
    //     int TargetIdx = X * (YCount + Y); //타겟 인덱스
    //     Vector3 Output = new Vector3();

    //     if (Under) //아랫면 좌표 계산
    //     {
    //         Output.x = TargetIdx;
    //         Output.y = TargetIdx + 1;
    //         Output.z = TargetIdx + YCount + 1;
    //     }
    //     else //윗면 좌표 계산
    //     {
    //         Output.x = TargetIdx + YCount + 1;
    //         Output.y = TargetIdx + 1;
    //         Output.z = TargetIdx + YCount + 2;
    //     }
    //     return Output;
    // }

    //Tri의 Vector3리스트를 Int배열로 변경 (깊은복사)
    int[] Tri_ConvertIntArray(List<Vector3> GetTris)
    {
        List<int> Array = new List<int>();
        for (int i = 0; i < GetTris.Count; i++)
        {
            Array.Add((int)GetTris[i].x);
            Array.Add((int)GetTris[i].y);
            Array.Add((int)GetTris[i].z);
        }
        //Debug.Log("삼각면 인덱스 갯수 : " + Array.Count);
        return Array.ToArray();
    }

    //UV 처리
    Vector2[] SetUV(int XCount, int YCount)
    {
        List<Vector2> UvList = new List<Vector2>();
        for (int X = 0; X < XCount; X++)
        {
            float ValueX = (float)X / (XCount - 1);
            for (int Y = 0; Y < YCount; Y++)
            {
                float ValueY = (float)Y / (YCount - 1);
                UvList.Add(new Vector2(CreateMeshInfo.InvertUV_X ? 1 - ValueX : ValueX, CreateMeshInfo.InvertUV_Y ? 1 - ValueY : ValueY));
            }
        }
        return UvList.ToArray();
    }
    #endregion


}

[CustomEditor(typeof(MotionPath))]
public class MotionPath_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //GUILayout.Space(100);

        MotionPath Ge = (MotionPath)target;

        //애니메이터 
        EditorGUI.BeginChangeCheck();
        Ge.Animator = (Animator)EditorGUILayout.ObjectField("Animator", Ge.Animator, typeof(Animator));
        bool ChangeAniamtor = EditorGUI.EndChangeCheck(); //애니메이터 바뀜

        // //오브젝트
        // for (int i = 0; i < Ge.PathInfo.Count; i++)
        // {
        //     EditorGUI.BeginChangeCheck();
        //     Ge.PathInfo[i].TargetObject = (GameObject)EditorGUILayout.ObjectField("Target " + "(" + "Path " + (i + 1).ToString() + ")", Ge.PathInfo[i].TargetObject, typeof(GameObject));
        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         if (Ge.PathInfo[i].TargetObject != null)
        //         {
        //             //추적 오브젝트 변경 시 패스 재생성

        //             if (Ge.PathInfo[i].AutoUpdate)
        //             {
        //                 CreatePath(Ge.PathInfo[i]);
        //             }

        //         }
        //     }
        // }

        ///////////////////////////////////////////////////////
        //////////////////      GUI     ///////////////////////
        ///////////////////////////////////////////////////////
        GUILayout.Space(10);



        //애니메이터가 있을 때
        if (Ge.Animator != null)
        {
            //Ge.SelectToolbar = GUILayout.Toolbar(Ge.SelectToolbar, Ge.ToolbarName, GUILayout.MinHeight(35));

            //////////////////////////////////////////
            //////////      버튼 리스트     ///////////
            //////////////////////////////////////////
            GUILayout.BeginHorizontal();

            //Animation버튼
            GUI.backgroundColor = Ge.SelectToolbar == 0 ? new Color(2, 1, 0) : GUI.color;
            if(GUILayout.Button("Animation", GUILayout.MinHeight(35)))
            {
                Ge.SelectToolbar = 0;
            }
            GUI.backgroundColor = GUI.color;


            //Path 버튼
            for (int i = 0; i < Ge.PathInfo.Count; i++)
            {
                GUI.backgroundColor = Ge.SelectToolbar == 1 && Ge.SelectPath == i ? new Color(2, 1, 0) : GUI.color;
                if(GUILayout.Button("Path " + (i + 1).ToString(), GUILayout.MinHeight(35)))
                {
                    Ge.SelectToolbar = 1;
                    Ge.SelectPath = i;
                }
                GUI.backgroundColor = GUI.color;
            }

            //메쉬 버튼
            GUI.backgroundColor = Ge.SelectToolbar == 2 ? new Color(2, 1, 0) : GUI.color;
            if(GUILayout.Button("Mesh", GUILayout.MinHeight(35)))
            {
                Ge.SelectToolbar = 2;
            }
            GUI.backgroundColor = GUI.color;

            GUILayout.EndHorizontal();


            //버튼 선택에 따른 인스펙터 드로우
            if (Ge.SelectToolbar == 0)
            {
                //애니메이션정보 GUI
                GUILayout.BeginVertical("GroupBox");
                DrawAniInfo();
                GUILayout.EndVertical();
            }
            //패스
            if (Ge.SelectToolbar == 1)
            {
                //패스가 그릴 오브젝트
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(10);
                Ge.PathInfo[Ge.SelectPath].TargetObject = (GameObject)EditorGUILayout.ObjectField("Path Target", Ge.PathInfo[Ge.SelectPath].TargetObject, typeof(GameObject));
                if (EditorGUI.EndChangeCheck())
                {
                    if (Ge.PathInfo[Ge.SelectPath].TargetObject != null)
                    {
                        //추적 오브젝트 변경 시 패스 재생성

                        if (Ge.PathInfo[Ge.SelectPath].AutoUpdate)
                        {
                            CreatePath(Ge.PathInfo[Ge.SelectPath]);
                        }
                    }
                }

                //오브젝트 있을 때
                if (Ge.PathInfo[Ge.SelectPath].TargetObject != null)
                {
                    //패스 GUI
                    GUILayout.BeginVertical("GroupBox");
                    DrawPathInfo(Ge.PathInfo[Ge.SelectPath]);
                    GUILayout.EndVertical();

                    //패스 To 버텍스위치 GUI
                    GUILayout.BeginVertical("GroupBox");
                    Viewer_AveragePos(Ge.PathInfo[Ge.SelectPath]);
                    GUILayout.EndVertical();
                }
            }
            //메쉬 생성
            else if (Ge.SelectToolbar == 2)
            {
                GUILayout.BeginVertical("GroupBox");
                GenerateMeshViewer(Ge);
                GUILayout.EndVertical();
            }

        }

        // //패스 리스트들 그리기
        // void DrawSelectPath_Inspector()
        // {


        //     //패스 GUI
        //     GUILayout.BeginVertical("GroupBox");
        //     DrawPathInfo(Ge.PathInfo[Ge.SelectPath]);
        //     GUILayout.EndVertical();

        //     //패스 To 버텍스위치 GUI
        //     GUILayout.BeginVertical("GroupBox");
        //     Viewer_AveragePos(Ge.PathInfo[Ge.SelectPath]);
        //     GUILayout.EndVertical();
        // }

        //패스 정보
        void DrawPathInfo(MotionPath.PathInfoData GetPathInfo)
        {


            PathViewerSetting(GetPathInfo);
            GetPathInfo.AutoUpdate = EditorGUILayout.Toggle("Auto Update", GetPathInfo.AutoUpdate);

            EditorGUI.BeginChangeCheck();
            int PathDetail = EditorGUILayout.IntSlider("PathDetail (Frame)", GetPathInfo.PathFrame, 1, 500);
            PathDetail = Mathf.Max(PathDetail, 1); //최소값
            GetPathInfo.PathFrame = PathDetail;
            if (EditorGUI.EndChangeCheck()) //패스 디테일 수정하면 업데이트
            {
                if (GetPathInfo.AutoUpdate)
                {
                    CreatePath(GetPathInfo);
                }
            }

            if (!GetPathInfo.AutoUpdate)
            {
                if (GUILayout.Button("Create Path"))
                {
                    CreatePath(GetPathInfo);
                }
            }
        }

        //CreatePath (패스 생성)
        void CreatePath(MotionPath.PathInfoData GetPathInfo)
        {
            float FirstFrame = Ge.AnimationSlider; //현재 포즈 시간 백업

            List<Vector3> NewPathPosition = new List<Vector3>();
            for (float i = Ge.StartFrame; i < Ge.EndFrame; i += (1f / (float)GetPathInfo.PathFrame))
            {
                Ge.AnimationSlider = i; //포즈 시간 업데이트
                UpDatePos(); //포즈 업데이트
                NewPathPosition.Add(GetPathInfo.TargetObject.transform.position);
            }
            GetPathInfo.PathPos = NewPathPosition.ToArray();


            Ge.AnimationSlider = FirstFrame; //처음 설정한 포즈 시간으로 백업
            UpDatePos(); //포즈 업데이트

            //버텍스 평균 위치값 자동 업데이트
            if (GetPathInfo.Average_AutoUpdate)
            {
                CountAveragePos(GetPathInfo); //버텍스 평균 위치값 업데이트
            }
        }

        //패스 보기 설정
        void PathViewerSetting(MotionPath.PathInfoData GetPathInfo)
        {
            GetPathInfo.PathViewerSetting = EditorGUILayout.Toggle("Path View Setting", GetPathInfo.PathViewerSetting);
            if (GetPathInfo.PathViewerSetting)
            {
                GUILayout.BeginVertical("GroupBox");
                EditorGUI.BeginChangeCheck();
                GetPathInfo.PathColor = EditorGUILayout.ColorField("Path Color", GetPathInfo.PathColor);
                GetPathInfo.PathWidth = EditorGUILayout.FloatField("Path Width", GetPathInfo.PathWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    if (GetPathInfo.AutoUpdate)
                    {
                        CreatePath(GetPathInfo);
                    }
                }
                GUILayout.EndVertical();
            }
        }

        //애니메이션 정보
        void DrawAniInfo()
        {
            //애니메이터 바뀜
            if (ChangeAniamtor)
            {
                Ge.AnimationClips = Ge.Animator.runtimeAnimatorController.animationClips;
                Ge.AniClipsName = new string[Ge.AnimationClips.Length];
                for (int i = 0; i < Ge.AniClipsName.Length; i++)
                {
                    Ge.AniClipsName[i] = Ge.AnimationClips[i].name;
                }
            }

            float SelectClipLength = Ge.AnimationClips[Ge.SelectAniClip].length; //선택한 클립의 최대 길이


            //플레이할 애니메이션 선택
            EditorGUI.BeginChangeCheck();
            Ge.SelectAniClip = EditorGUILayout.Popup("재생할 애니메이션", Ge.SelectAniClip, Ge.AniClipsName);
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("애니 변경");
                Ge.PlayStateName = GetStringFromAniClip(Ge.Animator, Ge.AnimationClips[Ge.SelectAniClip]); //재생할 애니메이션 스테이트 이름 가져오기

                //시작, 끝 프레임 기본 설정
                //Ge.StartFrame = 0;
                //Ge.EndFrame = Ge.AnimationClips[Ge.SelectAniClip].length;
                UpDatePos();

                for (int i = 0; i < Ge.PathInfo.Count; i++)
                {
                    if (Ge.PathInfo[i].AutoUpdate)
                    {
                        CreatePath(Ge.PathInfo[i]);
                    }
                }
            }

            EditorGUI.BeginChangeCheck(); //포즈 관련 변수 업데이트 되는지 확인
            //GUILayout.BeginHorizontal();
            //최소값
            float SetStartFrame = EditorGUILayout.FloatField("Start Frame", Ge.StartFrame);
            SetStartFrame = Mathf.Clamp(SetStartFrame, 0, Ge.EndFrame);
            Ge.StartFrame = SetStartFrame;

            //최대값
            float SetEndFrame = EditorGUILayout.FloatField("End Frame", Ge.EndFrame);
            SetEndFrame = Mathf.Clamp(SetEndFrame, Ge.StartFrame, SelectClipLength);
            Ge.EndFrame = SetEndFrame;

            //GUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider("Set Frame", ref Ge.StartFrame, ref Ge.EndFrame, 0, SelectClipLength);
            bool ChangeMinMax = EditorGUI.EndChangeCheck();
            if (ChangeMinMax) //패스랑 관련된 값들 변경되면 패스 새로 생성
            {
                for (int i = 0; i < Ge.PathInfo.Count; i++)
                {
                    if (Ge.PathInfo[i].AutoUpdate)
                    {
                        CreatePath(Ge.PathInfo[i]);
                    }
                }
            }

            //애니 재생
            EditorGUI.BeginChangeCheck(); //포즈 관련 변수 업데이트 되는지 확인
            Ge.AnimationSlider = EditorGUILayout.Slider("Ani Play", Ge.AnimationSlider, Ge.StartFrame, Ge.EndFrame);
            //포즈 관련된 변수들이 변했을 경우
            if (EditorGUI.EndChangeCheck() || ChangeMinMax)
            {
                UpDatePos(); //애니메이션 포즈 업데이트
            }
        }

        //캐릭터 애니메이션 업데이트
        void UpDatePos()
        {
            Ge.Animator.Play(Ge.PlayStateName, -1, Ge.AnimationSlider);
            Ge.Animator.Update(Ge.AnimationSlider - 1);
        }
    }


    #region 애니메이션 관련 기능
    AnimatorState[] AllState;
    //클립으로 애니메이션 스테이트 이름 가져오기 (AnimatorPlay용 String 받아올 때 씀)
    string GetStringFromAniClip(Animator GetAnimator, AnimationClip Clip)
    {
        string OutString = "";
        AllState = GetAnimatorStates(GetAnimator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController); //모든 스테이트 가져오기   
        OutString = GetStateFromClip(AllState, Clip).name; //애니메이션 클립으로 부터 State가져 와서 이름 할당
        return OutString;
    }

    //모든 애니메이터 스테이트 가져오기
    AnimatorState[] GetAnimatorStates(UnityEditor.Animations.AnimatorController anicon)
    {
        List<AnimatorState> ret = new List<AnimatorState>();
        foreach (var layer in anicon.layers)
        {
            foreach (var subsm in layer.stateMachine.stateMachines)
            {
                foreach (var state in subsm.stateMachine.states)
                {
                    ret.Add(state.state);
                }
            }
            foreach (var s in layer.stateMachine.states)
            {
                ret.Add(s.state);
            }
        }
        return ret.ToArray();
    }

    //모든 애니메이터 스테이트 중에 애니메이션 클립에 해당하는스테이트 가져오기
    AnimatorState GetStateFromClip(AnimatorState[] StateList, AnimationClip GetClip)
    {
        AnimatorState OutState = null;
        for (int i = 0; i < StateList.Length; i++)
        {
            if (StateList[i].motion == GetClip)
            {
                OutState = StateList[i];
                break; //정지
            }
        }
        return OutState;
    }
    #endregion



    //////////////////////////////////////////////////////////////////////
    /////////////////////       VertexAverage      ///////////////////////
    //////////////////////////////////////////////////////////////////////
    //패스의 평균치를 구해서 버텍스 할당 뷰어
    #region 패스 평균이 구해서 버텍스 할당 하는기능
    void Viewer_AveragePos(MotionPath.PathInfoData GetPathInfo)
    {
        EditorGUILayout.LabelField("VertexCount", GetPathInfo.AveragePos.Length.ToString());

        //값 변경시 업데이트
        EditorGUI.BeginChangeCheck();
        GetPathInfo.Average_AutoUpdate = EditorGUILayout.Toggle("Average_AutoUpdate", GetPathInfo.Average_AutoUpdate);
        GetPathInfo.Average_Detail = EditorGUILayout.IntSlider("Average_Detail", GetPathInfo.Average_Detail, 1, 100);
        GetPathInfo.Average_Distance = EditorGUILayout.Slider("Average_Distance", GetPathInfo.Average_Distance, 0.01f, 1);
        if (EditorGUI.EndChangeCheck())
        {
            CountAveragePos(GetPathInfo);
        }

        //버텍스 수정모드
        #region 
        EditorGUI.BeginChangeCheck();
        GetPathInfo.EditMode = EditorGUILayout.Toggle("Vertex EditMode", GetPathInfo.EditMode);
        if (GetPathInfo.EditMode)
        {
            GUILayout.BeginVertical("GroupBox");

            //폰트 스타일
            GUIStyle InputFrontStyle = new GUIStyle();
            InputFrontStyle.fontStyle = FontStyle.Bold;
            InputFrontStyle.normal.textColor = Color.green;
            InputFrontStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("버텍스 수정모드는 상단의 값들 수정 시 버텍스가 초기화 됩니다.", InputFrontStyle);
            GUILayout.EndHorizontal();
        }
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll(); //에딧모드 수정 시
        }
        #endregion


        if (!GetPathInfo.Average_AutoUpdate)
        {
            if (GUILayout.Button("라인의 평균 버텍스 거리값 계산"))
            {
                CountAveragePos(GetPathInfo);
            }
        }
    }

    //평균 거리값 계산
    void CountAveragePos(MotionPath.PathInfoData GetPathInfo)
    {
        GetPathInfo.AveragePos = GetAveragePos(GetPathInfo.PathPos, GetPathInfo.Average_Detail, GetPathInfo.Average_Distance);
        SceneView.RepaintAll();
    }

    //평균 위치 구하기 (Detail은 두점 사이에 디테일, 10개면 Lerp를 10번 계산함)
    //Detail = 두점 사이에 디테일, 10개면 Lerp를 10번 계산함 (10 정도가 적당)
    //VertDistance = 버텍스간의 거리
    Vector3[] GetAveragePos(Vector3[] GetPos, int Detail, float VertDistance)
    {
        //거리를 비교할 최초값 생성
        Vector3 BeforePoint = new Vector3(0, 0, 0);

        BeforePoint.x = GetPos[0].x;
        BeforePoint.y = GetPos[0].y;
        BeforePoint.z = GetPos[0].z;

        //Debug.Log(BeforePoint);

        List<Vector3> OutPut = new List<Vector3>();
        OutPut.Add(BeforePoint); //최초값 추가

        for (int i = 0; i < GetPos.Length - 1; i++) //버텍스 위치들
        {
            for (int j = 0; j <= Detail; j++) //버텍스 안에서의 Lerp 디테일
            {
                float Value = ((float)j / (float)Detail); // 0~1
                Vector3 NextPos = Vector3.Lerp(GetPos[i], GetPos[i + 1], Value); //디테일 값 긁어서 거리 계산용으로 던짐
                if (GetDistance(BeforePoint, NextPos, VertDistance)) //거리 비교해서 해당 거리보다 크면 반환
                {
                    OutPut.Add(NextPos); //해당 위치값 추가
                    BeforePoint = NextPos; //이전 값을 다음 값으로 업데이트
                }
            }
        }

        return OutPut.ToArray();
    }

    //이전값과 이후 값을 비교하여 해당 거리 만큼 멀어져 있을 경우 True
    bool GetDistance(Vector3 BeforePos, Vector3 NextPos, float Distance)
    {
        bool Output = Vector3.Distance(BeforePos, NextPos) >= Distance ? true : false;
        return Output;
    }
    #endregion




    ///////////////////////////////////////////////////////////////////
    /////////////////////       MeshViewer      ///////////////////////
    ///////////////////////////////////////////////////////////////////
    void GenerateMeshViewer(MotionPath GetMotionPath)
    {
        GetMotionPath.CreateMeshInfo.InvertUV_X = EditorGUILayout.Toggle("InvertUV_X", GetMotionPath.CreateMeshInfo.InvertUV_X);
        GetMotionPath.CreateMeshInfo.InvertUV_Y = EditorGUILayout.Toggle("InvertUV_Y", GetMotionPath.CreateMeshInfo.InvertUV_Y);
        GetMotionPath.CreateMeshInfo.Count_Y = EditorGUILayout.IntSlider("Count_Y", GetMotionPath.CreateMeshInfo.Count_Y, 1, 10);
        if (GUILayout.Button("GenerateMesh"))
        {
            GetMotionPath.GenerateMesh(GetMotionPath.CreateMeshInfo.Count_Y);
        }
    }


}

#endif
