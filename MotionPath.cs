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
    [HideInInspector] public List<PathInfoData> PathInfo = new List<PathInfoData>(2);
    [System.Serializable]
    public class PathInfoData
    {
        [HideInInspector] public GameObject TargetObject;
        //Path Info 1
        [HideInInspector] public Vector3[] PathPos = new Vector3[0]; //실제 쓰는 최종 위치 데이터

        [HideInInspector] public int PathFrame = 120;

        [HideInInspector] public bool PathViewerSetting = false;
        [HideInInspector] public bool AutoUpdate = true;
        [HideInInspector] public Color PathColor = GetRandomColor_HighSaturation();
        [HideInInspector] public float PathWidth = 2;

        //버텍스 평균치 구하는 계산 변수들
        [HideInInspector] public bool Vertex_AutoUpdate = true; //자동 업데이트
        public Vector3[] VertexPos = new Vector3[0]; //버텍스 평균 위치
        [HideInInspector] public int Vertex_Detail = 10; //디테일값
        [HideInInspector] public float Vertex_Distance = 0.25f; //버텍스들의 거리값

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
        [HideInInspector] public bool InvertUV_X = false;
        [HideInInspector] public bool InvertUV_Y = false;
        [HideInInspector] public bool FlipFace = false;
        [HideInInspector] public int Count_Y = 1;
    }

    //랜덤 컬러 (채도 높게)
    static Color GetRandomColor_HighSaturation()
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
    [HideInInspector] public int SelectPath = 0; //인스펙터에서 선택한 패스 번호
    [HideInInspector] public string[] ToolbarName = { "Animation", "Path 1", "Path 2", "Path 3", "Mesh" };


    private void Awake()
    {
        Debug.Log("최초 생성합니다.(패스2개)");
        if (PathInfo.Count == 0)
        {
            PathInfo.Add(new PathInfoData());
            PathInfo.Add(new PathInfoData());
        }

        transform.position = new Vector3(0, 0, 0);

        //만들 메쉬 데이터 구성
        SetMesh();
    }

    //씬 GUI 그리기 위한 델리게이트 추가
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += DrawSceneGUI;
        Undo.undoRedoPerformed += MyUndoCallback;
    }
    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= DrawSceneGUI;
        Undo.undoRedoPerformed -= MyUndoCallback;
    }

    void MyUndoCallback()
    {
        //Debug.Log("언두 했음");
        // code for the action to take on Undo
        GenerateMesh(PathInfo.Count, CreateMeshInfo.Count_Y, PathInfo[0].VertexPos.Length);
    }

    ///////////////////////////////////////
    /////////       씬 GUI      ///////////
    ///////////////////////////////////////
    void DrawSceneGUI(SceneView sceneView)
    {
        MotionPath Ge = this;

        DrawPathInfo(Ge);

        Viwer_MeshInfo();
    }

    //패스 정보 그리기
    void DrawPathInfo(MotionPath Ge)
    {
        //버텍스 위치 아이콘
        GUIStyle Style_Vert = new GUIStyle();
        Style_Vert.contentOffset = new Vector2(-7, -9); //아이콘 위치 조정

        //버텍스 위치 아이콘
        GUIStyle Style_Target = new GUIStyle();
        Style_Target.contentOffset = new Vector2(-9.5f, -9.5f); //아이콘 위치 조정

        //패스 정보 그리기
        for (int i = 0; i < PathInfo.Count; i++)
        {
            //선 그리기
            Handles.color = (SelectPath == i && Ge.SelectToolbar == 1 ? new Color(5, 5, 5, 1) : PathInfo[i].PathColor);

            Handles.DrawAAPolyLine(PathInfo[i].PathWidth * (SelectPath == i && Ge.SelectToolbar == 1 ? 3 : 1), Ge.PathInfo[i].PathPos.Length, Ge.PathInfo[i].PathPos);
            Handles.color = GUI.color;

            //////////////////////////////////////////////////
            /////////       버텍스 위치 그리기      ///////////
            //////////////////////////////////////////////////
            if (PathInfo[i].EditMode)
            {
                VertEditMode(Ge, PathInfo[i]);
            }
            else
            {
                for (int j = 0; j < PathInfo[i].VertexPos.Length; j++)
                {
                    Handles.Label(PathInfo[i].VertexPos[j], EditorGUIUtility.IconContent("winbtn_mac_close"), Style_Vert); //버텍스 위치들
                }
            }

            //패스 타겟 아이콘
            if (PathInfo[i].TargetObject != null)
            {
                Handles.Label(PathInfo[i].TargetObject.transform.position, EditorGUIUtility.IconContent("DotFrame"), Style_Target); //버텍스 위치들
            }
        }
    }


    ///////////////////////////////////////////////////
    //////////      버텍스 에딧모드     /////////////////
    ///////////////////////////////////////////////////
    void VertEditMode(MotionPath Ge, PathInfoData GetPathInfo)
    {
        //선택한 버텍스용 스타일
        GUIStyle Style = new GUIStyle();
        Style.contentOffset = new Vector2(-7, -9); //아이콘 위치 조정

        //수정 모드 했을 때 버튼 크기 
        float ButtonSize = 15;
        float ButtonPosAdd = -ButtonSize / 2;
        //패스 그리기
        for (int j = 0; j < GetPathInfo.VertexPos.Length; j++)
        {
            if (j != GetPathInfo.EditVertIdx)
            {
                Handles.BeginGUI();
                GUI.backgroundColor = new Color(2, 2, 2);
                if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(GetPathInfo.VertexPos[j]).x + ButtonPosAdd, HandleUtility.WorldToGUIPoint(GetPathInfo.VertexPos[j]).y + ButtonPosAdd, ButtonSize, ButtonSize), ""))
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
                Handles.Label(GetPathInfo.VertexPos[j], EditorGUIUtility.IconContent("d_winbtn_mac_min"), Style);
            }
        }

        //위치 기즈모
        if (GetPathInfo.VertexPos.Length > GetPathInfo.EditVertIdx)
        {
            Undo.RecordObject(Ge, "SaveHandlePos");
            EditorGUI.BeginChangeCheck();
            GetPathInfo.VertexPos[GetPathInfo.EditVertIdx] = Handles.PositionHandle(GetPathInfo.VertexPos[GetPathInfo.EditVertIdx], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Ge.GenerateMesh(Ge.PathInfo.Count, Ge.CreateMeshInfo.Count_Y, Ge.PathInfo[0].VertexPos.Length);
            }
        }
    }




    ////////////////////////////////////////////////////////////
    ////////////////        메쉬 생성 툴       //////////////////
    ////////////////////////////////////////////////////////////
    #region 
    //메쉬 생성할때 디버그용 GUI
    [HideInInspector] public bool Debug_VertInfo = false;
    [HideInInspector] public bool Debug_VertPos = false;
    [HideInInspector] public bool Debug_DrawTriLine = false;
    void Viwer_MeshInfo()
    {
        if (Debug_VertInfo)
        {
            for (int i = 0; i < MeshFilter.mesh.vertices.Length; i++)
            {
                Handles.Label(MeshFilter.mesh.vertices[i], i.ToString() + (Debug_VertPos ? " " + MeshFilter.mesh.vertices[i] : "")); //버텍스 위치들
            }
        }
        if (Debug_DrawTriLine)
        {
            Debug_DrawTriLineData();
        }


    }

    //디버그
    Vector3[] ConvertData = new Vector3[4];
    void Debug_DrawTriLineData()
    {
        for (int i = 0; i < Tri.Count; i++)
        {
            Vector3[] Test = ConvertIndexToPos(Tri[i], MeshFilter.mesh.vertices);
            Handles.Label((Test[0] + Test[1] + Test[2]) / 3, i.ToString());
            Handles.DrawAAPolyLine(Test);
        }

        //인덱스를 포지션으로 바꿔줌.
        Vector3[] ConvertIndexToPos(Vector3 GetVertIndex, Vector3[] VertPos)
        {
            ConvertData[0] = VertPos[(int)GetVertIndex.x];
            ConvertData[1] = VertPos[(int)GetVertIndex.y];
            ConvertData[2] = VertPos[(int)GetVertIndex.z];
            ConvertData[3] = VertPos[(int)GetVertIndex.x];
            return ConvertData;
        }
    }

    //컴포넌트 붙였을 때 최초로 생성할 요소들
    void SetMesh()
    {
        if(MeshFilter == null || MeshRenderer == null)
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            MeshFilter.mesh = new Mesh();
            MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            MeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            MeshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    ////////////////////////////////////////////////////////////
    ////////////////        메쉬 생성       /////////////////////
    ////////////////////////////////////////////////////////////
    bool BeformFlip = false;
    public void GenerateMesh(int PathCount, int Ydetail, int XCount)
    {
        if (IsSamePathVert()) //버텍스들의 갯수가 동일한지 확인
        {
            if (BeformFlip != CreateMeshInfo.FlipFace)
            {
                for (int i = 0; i < PathInfo.Count; i++)
                {
                    System.Array.Reverse(PathInfo[i].VertexPos);
                }
                BeformFlip = CreateMeshInfo.FlipFace;
            }

            //#endregion
            MeshFilter.mesh.Clear(); //메쉬 초기화

            //Vertices
            MeshFilter.mesh.vertices = SetVertPos(Ydetail).ToArray();

            //Triangles
            int YCount = (Ydetail > 1 ? PathCount * Ydetail - Ydetail + 1 : PathCount * Ydetail);
            List<Vector3> Tris = SetTriList(XCount, YCount); //삼각면 할당(X버텍스, Y패스갯수)
            MeshFilter.mesh.triangles = Tri_ConvertIntArray(Tris); //Vector3를 Int배열로 변환

            //UV
            MeshFilter.mesh.uv = SetUV(XCount, YCount);
        }
    }

    //버텍스 위치 할당
    //[Header("디버그 테스트용 (나중에 빼야댐)")]
    //public List<Vector3> VertPosList;
    List<Vector3> SetVertPos(int YCount)
    {
        List<Vector3> VertPos = new List<Vector3>();
        for (int X = 0; X < PathInfo[0].VertexPos.Length; X++) //가로줄
        {
            for (int Y = 0; Y < PathInfo.Count; Y++) //세로줄
            {
                //최초 0번 인덱스는 임의로 추가
                if (Y == 0)
                {
                    VertPos.Add(PathInfo[0].VertexPos[X]);
                }
                //나머지 버텍스들 추가
                if (Y + 1 < PathInfo.Count)
                {
                    for (int i = 1; i <= CreateMeshInfo.Count_Y; i++)
                    {
                        float Value = i / (float)CreateMeshInfo.Count_Y;
                        Vector3 InputPos = Vector3.Lerp(PathInfo[Y].VertexPos[X], PathInfo[Y + 1].VertexPos[X], Value);
                        VertPos.Add(InputPos);
                    }
                }
            }
        }
        return VertPos;
    }

    //Tri 계산
    [HideInInspector] public List<Vector3> Tri = new List<Vector3>(0);
    List<Vector3> SetTriList(int XCount, int YCount)
    {
        //Debug.Log("총갯수 " + "X : " + XCount.ToString() + "  Y : " + YCount.ToString());
        //List<Vector3> NewList = new List<Vector3>();
        Tri.Clear();

        //위갯수
        int idx = 0;
        for (int x = 0; x < XCount - 1; x++)
        {
            //옆갯수
            for (int y = 0; y < YCount - 1; y++)
            {
                //Debug.Log("X : " + x.ToString() + "Y : " + y.ToString() + "  Idx : " + (idx).ToString());
                //Debug.Log(y + " , " + x); //여기서 2곱한 뒤에 3곱하면 인덱스 데이터 들어옴
                Tri.Add(GetTri(idx, YCount, true)); //아랫면
                Tri.Add(GetTri(idx, YCount, false)); //윗면
                idx++;
            }
            idx++;
        }
        //Tri = NewList;
        return Tri;
    }

    //윗면, 아랫면 구성
    Vector3 GetTri(int Index, int YCount, bool Under) //Under는 아래 삼각면
    {
        //int TargetIdx = X * (YCount + Y); //타겟 인덱스
        Vector3 Output = new Vector3();

        if (Under) //아랫면 좌표 계산
        {
            Output.x = Index;
            Output.y = Index + 1;
            Output.z = Index + YCount;
        }
        else //윗면 좌표 계산
        {
            Output.x = Index + YCount;
            Output.y = Index + 1;
            Output.z = Index + YCount + 1;
        }
        //Debug.Log(Output);
        return Output;
    }

    //Tri의 Vector3리스트를 Int배열로 변경
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


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////     패스들의 버텍스 갯수가 모두 일치하는지 확인 안할경우 Mesh버튼 비활성화      ////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool IsSamePathVert()
    {
        if (PathInfo.Count > 0) //한개라도 있을 경우
        {
            bool Output = true;
            int Count = PathInfo[0].VertexPos.Length; //버텍스 갯수 기준
            for (int i = 0; i < PathInfo.Count; i++)
            {
                if (PathInfo[i].TargetObject == null) //타겟오브젝트 없으면 비활성화
                {
                    Output = false;
                    break;
                }
                if (PathInfo[i].VertexPos.Length != Count) //갯수 동일하지 않으면 비활성화
                {
                    Output = false;
                    break;
                }
            }
            return Output;
        }
        else
        {
            return true;
        }
    }

    ////////////////////////////////////////////
    //////////      Save Mesh      /////////////
    ////////////////////////////////////////////
    //string BeforePath = Path
    [HideInInspector] string BeforePath = "";
    public void SaveMesh(Mesh mesh)
    {
        //메쉬 새로 생성
        Mesh NewMesh = (Mesh)UnityEngine.Object.Instantiate(mesh); 

        //이전 경로가 아무것도 없으면 기본 프로젝트 패스로 가져옴
        string NewBeforePath = (BeforePath.Length != 0) ? BeforePath : Application.dataPath;

        //Path 패널 열어서 저장 경로 가져오기
        string filePath = 
        EditorUtility.SaveFilePanelInProject("Save Mesh", "FX_Mesh_" + mesh.name, "asset", "", NewBeforePath);
        if (filePath == "") return;

        //이전 패스 업데이트
        //(계속 저장할 때 경로 이전꺼 가져올려고.)
        BeforePath = filePath;

        //에셋 생성
        AssetDatabase.CreateAsset(NewMesh, filePath);  
        
        //에셋 저장
        AssetDatabase.SaveAssets();

        //폴더장 초기화
        AssetDatabase.Refresh();
    }
}

[CustomEditor(typeof(MotionPath))]
public class MotionPath_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //GUILayout.Space(100);

        MotionPath Ge = (MotionPath)target;

        //All Undo
        Undo.RecordObject(Ge, "All State");

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

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Path"))
            {
                Ge.PathInfo.Add(new MotionPath.PathInfoData());
            }

            //패스가 2개 초과일 경우
            if (Ge.PathInfo.Count > 2)
            {
                GUI.backgroundColor = Color.red * 1.5f;
                if (GUILayout.Button("Remove Path" + " (" + (Ge.SelectPath + 1).ToString() + ")", GUILayout.MaxWidth(130)))
                {
                    Ge.PathInfo.RemoveAt(Ge.SelectPath);
                    Ge.SelectPath = Mathf.Min(Ge.PathInfo.Count - 1, Ge.SelectPath); //지운게 마지막꺼면
                    SceneView.RepaintAll();
                    Ge.GenerateMesh(Ge.PathInfo.Count, Ge.CreateMeshInfo.Count_Y, Ge.PathInfo[0].VertexPos.Length);
                }
                GUI.backgroundColor = GUI.color;
            }
            GUILayout.EndHorizontal();

            //////////////////////////////////////////
            //////////      버튼 리스트     ///////////
            //////////////////////////////////////////

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            //Animation버튼
            GUI.backgroundColor = Ge.SelectToolbar == 0 ? new Color(2, 1, 0) : GUI.color;
            if (GUILayout.Button("Animation", GUILayout.MinHeight(35)))
            {
                Ge.SelectToolbar = 0;
            }
            GUI.backgroundColor = GUI.color;

            //Path 버튼
            for (int i = 0; i < Ge.PathInfo.Count; i++)
            {
                //Path 버튼 선택 했을 때 색
                GUI.backgroundColor = Ge.SelectToolbar == 1 && Ge.SelectPath == i ? new Color(2, 1, 0) : GUI.color;
                if (GUILayout.Button("Path " + (i + 1).ToString() + " (" + Ge.PathInfo[i].VertexPos.Length.ToString() + ")", GUILayout.MinHeight(35)))
                {
                    Ge.SelectToolbar = 1;
                    Ge.SelectPath = i;
                }
                GUI.backgroundColor = GUI.color;
            }

            //메쉬 버튼

            GUI.backgroundColor = Ge.SelectToolbar == 2 ? new Color(2, 1, 0) : GUI.color;
            if (GUILayout.Button("Mesh", GUILayout.MinHeight(35)))
            {
                Ge.SelectToolbar = 2;
            }
            GUI.backgroundColor = GUI.color;



            //가로 나열 마무리
            GUILayout.EndHorizontal();

            //버튼들 누르면 패스 표시 정보 업데이트를 위한 씬 다시 로드
            bool ClickToolbar = EditorGUI.EndChangeCheck();
            if (ClickToolbar)
            {
                SceneView.RepaintAll(); //패스 버튼 누르면 씬 정보 다시 그리기
            }


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

                //패스가 2개 초과일 경우
                // if (Ge.PathInfo.Count > 2)
                // {
                //     GUI.backgroundColor = Color.red * 1.5f;
                //     if (GUILayout.Button("Remove Path"))
                //     {
                //         Ge.PathInfo.RemoveAt(Ge.SelectPath);
                //         Ge.SelectPath = Mathf.Min(Ge.PathInfo.Count - 1, Ge.SelectPath); //지운게 마지막꺼면
                //         SceneView.RepaintAll();
                //         Ge.GenerateMesh(Ge.PathInfo.Count, Ge.CreateMeshInfo.Count_Y, Ge.PathInfo[0].VertexPos.Length);
                //     }
                //     GUI.backgroundColor = GUI.color;
                // }

                //오브젝트 있을 때
                if (Ge.PathInfo[Ge.SelectPath].TargetObject != null)
                {
                    //패스 GUI
                    GUILayout.BeginVertical("GroupBox");
                    DrawPathInfo(Ge.PathInfo[Ge.SelectPath]);
                    GUILayout.EndVertical();

                    //패스 To 버텍스위치 GUI
                    GUILayout.BeginVertical("GroupBox");
                    Viewer_VertexPos(Ge.PathInfo[Ge.SelectPath]);
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
        //     Viewer_VertexPos(Ge.PathInfo[Ge.SelectPath]);
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
            if (GetPathInfo.TargetObject != null)
            {
                float FirstFrame = Ge.AnimationSlider; //현재 포즈 시간 백업

                List<Vector3> NewPathPosition = new List<Vector3>(2);
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
                if (GetPathInfo.Vertex_AutoUpdate)
                {
                    CountVertexPos(GetPathInfo); //버텍스 평균 위치값 업데이트
                }
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
    /////////////////////       VertexVertex      ///////////////////////
    //////////////////////////////////////////////////////////////////////
    //패스의 평균치를 구해서 버텍스 할당 뷰어
    #region 패스 평균이 구해서 버텍스 할당 하는기능
    void Viewer_VertexPos(MotionPath.PathInfoData GetPathInfo)
    {
        
        GUIStyle VertCountFont = new GUIStyle();
        VertCountFont.normal.textColor = Color.green;
        VertCountFont.fontStyle = FontStyle.Bold;
        VertCountFont.alignment = TextAnchor.UpperCenter;
        VertCountFont.fontSize = 18;
        GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 1);
        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Count : " + GetPathInfo.VertexPos.Length.ToString(), VertCountFont, GUILayout.MinHeight(18));
        GUILayout.EndVertical();
        GUI.backgroundColor = GUI.color;

        //값 변경시 업데이트
        EditorGUI.BeginChangeCheck();
        GetPathInfo.Vertex_AutoUpdate = EditorGUILayout.Toggle("Vertex_AutoUpdate", GetPathInfo.Vertex_AutoUpdate);
        GetPathInfo.Vertex_Detail = EditorGUILayout.IntSlider("Vertex_Detail(Average)", GetPathInfo.Vertex_Detail, 1, 100);
        GetPathInfo.Vertex_Distance = EditorGUILayout.Slider("Vertex_Distance", GetPathInfo.Vertex_Distance, 0.01f, 1);
        if (EditorGUI.EndChangeCheck())
        {
            if (GetPathInfo.Vertex_AutoUpdate)
            {
                CountVertexPos(GetPathInfo);
            }
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


        if (!GetPathInfo.Vertex_AutoUpdate)
        {
            if (GUILayout.Button("라인의 평균 버텍스 거리값 계산"))
            {
                CountVertexPos(GetPathInfo);
            }
        }
    }

    //평균 거리값 계산
    void CountVertexPos(MotionPath.PathInfoData GetPathInfo)
    {
        GetPathInfo.VertexPos = GetVertexPos(GetPathInfo.PathPos, GetPathInfo.Vertex_Detail, GetPathInfo.Vertex_Distance);
        SceneView.RepaintAll();
    }

    //평균 위치 구하기 (Detail은 두점 사이에 디테일, 10개면 Lerp를 10번 계산함)
    //Detail = 두점 사이에 디테일, 10개면 Lerp를 10번 계산함 (10 정도가 적당)
    //VertDistance = 버텍스간의 거리
    Vector3[] GetVertexPos(Vector3[] GetPos, int Detail, float VertDistance)
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


    void GenerateMeshViewer(MotionPath Ge)
    {
        bool SameVert = Ge.IsSamePathVert();

        GUIStyle Font = new GUIStyle();
        Font.normal.textColor = Color.green;
        Font.alignment = TextAnchor.MiddleCenter;
        Font.fontStyle = FontStyle.Bold;
        Font.fontSize = 18;
        
        if (!SameVert)
        {

            GUILayout.Label("Path의 모든 버텍스 갯수가 동일 해야 합니다", Font);
        }
        else
        {
            GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
            GUILayout.BeginVertical("GroupBox");
            GUILayout.Label(Ge.MeshFilter.mesh.vertices.Length.ToString() + " Verts, " + (Ge.MeshFilter.mesh.triangles.Length / 3).ToString() + " tris", Font);
            GUILayout.EndVertical();
            GUI.backgroundColor = GUI.color;

            EditorGUI.BeginChangeCheck();
            Ge.Debug_VertInfo = EditorGUILayout.Toggle("View VertIdx", Ge.Debug_VertInfo);
            if (Ge.Debug_VertInfo)
            {
                Ge.Debug_VertPos = EditorGUILayout.Toggle("View VertPos", Ge.Debug_VertPos);
            }
            Ge.Debug_DrawTriLine = EditorGUILayout.Toggle("View Tris", Ge.Debug_DrawTriLine);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            

            EditorGUI.BeginChangeCheck();
            Ge.CreateMeshInfo.Count_Y = EditorGUILayout.IntSlider("Count_Y", Ge.CreateMeshInfo.Count_Y, 1, 10);
            if (GUILayout.Button("InvertUV_X"))
            {
                Ge.CreateMeshInfo.InvertUV_X = !Ge.CreateMeshInfo.InvertUV_X;
            }
            if (GUILayout.Button("InvertUV_Y"))
            {
                Ge.CreateMeshInfo.InvertUV_Y = !Ge.CreateMeshInfo.InvertUV_Y;
            }
            if (GUILayout.Button("FlipFace"))
            {
                Ge.CreateMeshInfo.FlipFace = !Ge.CreateMeshInfo.FlipFace;
            }
            bool ChangeMeshData = EditorGUI.EndChangeCheck();
            if (ChangeMeshData) //메쉬 관련 데이터 변경 시 메쉬 다시 구성
            {
                Ge.GenerateMesh(Ge.PathInfo.Count, Ge.CreateMeshInfo.Count_Y, Ge.PathInfo[0].VertexPos.Length);
            }

            if (GUILayout.Button("GenerateMesh (메쉬 생성)"))
            {
                Ge.GenerateMesh(Ge.PathInfo.Count, Ge.CreateMeshInfo.Count_Y, Ge.PathInfo[0].VertexPos.Length);
            }

            EditorGUI.BeginDisabledGroup(Ge.MeshFilter.mesh.vertices.Length == 0); //버튼 비활성화 조건
            {
                if(GUILayout.Button("Save Mesh"))
                {
                    Ge.SaveMesh(Ge.MeshFilter.mesh);
                }
            }
            EditorGUI.EndDisabledGroup();

        }
    }
}

#endif
