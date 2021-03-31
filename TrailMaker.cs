// #if UNITY_EDITOR


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
// public class TrailMaker : MonoBehaviour
// {
     
//     //public Vector2[] UvTest;
//     public int Detail = 10;
//     //public bool FlipSide;
//     [HideInInspector]public bool InvertUV_X;
//     [HideInInspector]public bool InvertUV_Y;
//     public List<Vector3> P_1 = new List<Vector3>(){new Vector3(3, 1, 0), new Vector3(2.25f, 1, 3.5f), new Vector3(-2.25f, 1, 3.5f), new Vector3(-3, 1, 0)};
//     public List<Vector3> P_2 = new List<Vector3>(){new Vector3(1, 1, 0), new Vector3(0.75f, 1, 1.25f), new Vector3(-0.75f, 1, 1.25f), new Vector3(-1, 1, 0)};
//     public List<Vector3> p_Inner = new List<Vector3>();
//     public MeshFilter MeshFilter;

//     //P1과 P2의 데이터를 P_Inner데이터에 할당 (여기서 버텍스 위치 할당을 제대로 못함. 작업중.)
//     public void SetVertex()
//     {
//         p_Inner = new List<Vector3>(new Vector3[Detail * 2]); //P1과 P2의 갯수만큼 버텍스 재할당
//         int HalfCount = (p_Inner.Count / 2);
//         for (int i = 0; i < HalfCount; i++)
//         {
//             float value = i / ((float)HalfCount - 1);
//             p_Inner[i * 2] = GetPos_MulBezier(P_1, value); //짝수 처리
//             p_Inner[i * 2 + 1] = GetPos_MulBezier(P_2, value);  //홀수 처리
//         }
//     }


//     //메쉬 구성
//     public void GenerateMesh(MeshFilter GetMeshFilter)
//     {
//         if(GetMeshFilter == null)
//         {
//             GetMeshFilter = GetComponent<MeshFilter>();
//             GetMeshFilter.mesh = new Mesh();
//             MeshFilter = GetMeshFilter;
//             Debug.Log("새로운 임의의 메쉬 생성");
//         }
//         //GetMeshFilter.mesh = new Mesh();

//         int Vert = p_Inner.Count;

//         #region 버텍스 위치
//         Vector3[] vertices = new Vector3[Vert];
//         //Debug.Log(vertices.Length);
//         for (int i = 0; i < vertices.Length; i++)
//         {
//             vertices[i] = p_Inner[i] - transform.position;
//         }
//         #endregion

//         #region 삼각면 구성
//         //int[] triangles = new int[Vert * 3];
//         int[] triangles = new int[Vert * 2 + (Vert - 6)];
//         //Debug.Log(triangles.Length);

//         for (int i = 0; i < triangles.Length; i++)
//         {
//             int Input = 0;

//             //짝수열 계산 (왼쪽 삼각면)
//             if ((i / 3) % 2 == 0)
//             {
//                 Input = ((i / 3) + (i % 3)); //몫 + 나머지
//             }
//             //홀수열 계산 (오른쪽 삼각면)
//             else if ((i / 3) % 2 == 1)
//             {
//                 if (i % 3 == 0) //나머지가 1일 때
//                 {
//                     Input = (((i + 1) / 3) + ((i + 1) % 3)); //다음 인덱스의 몫 + 나머지
//                 }
//                 else if (i % 3 == 1)
//                 {
//                     Input = (((i - 1) / 3) + ((i - 1) % 3)); //이전 인덱스의 몫 + 나머지
//                 }
//                 else
//                 {
//                     Input = (((i) / 3) + ((i) % 3)); //이전 인덱스의 몫 + 나머지
//                 }
//             }

//             //값 할당
//             triangles[i] = Input;

//             //디버그용
//             /*
//             Debug.Log(
//                 i + 
//                 "      몫 : " + i / 3 + 
//                 "      나머지 : " + i % 3  + 
//                 "      몫 + 나머지 : " + ((i / 3) + (i % 3)) + 
//                 ((i / 3) % 2 == 0 ? "    몫이짝수" : "    몫이홀수") +  
//                 "      최종값 : " + ((i / 3) % 2 == 0 ? ((i / 3) + (i % 3)) : Input)
//                 );
//             */
//         }
//         #endregion

//         #region UV폄
//         Vector2[] UvTest = new Vector2[vertices.Length];
//         for (int i = 0; i < UvTest.Length / 2; i++) {
//             float value = (float)i / (UvTest.Length - 2); // 0 ~ 1 까지 보간
//             float InputX = InvertUV_X ? (value * 2)  : 1 - (value * 2);
//             UvTest[i * 2] = new Vector2(InputX, InvertUV_Y ? 0 : 1); //짝수
//             UvTest[i * 2 + 1] = new Vector2(InputX, InvertUV_Y ? 1 : 0); //홀수
//         }

//         #endregion

//         GetMeshFilter.mesh.Clear();
//         GetMeshFilter.mesh.vertices = vertices;
//         GetMeshFilter.mesh.triangles = triangles;
//         GetMeshFilter.mesh.uv = UvTest;

//         Debug.Log("메쉬생성 완료");
//     }
//     //메쉬 클리어
//     public void ClearMesh(MeshFilter GetMeshFilter)
//     {
//         GetMeshFilter.mesh.Clear();
//     }

//     // //베지어 데이터를 메쉬 데이터에 할당
//     // public void SetMeshFromBezier()
//     // {

//     // }


//     //멀티 베지어 계산 (새롭게 리스트 생성해서 반환)
//     public Vector3 GetPos_MulBezier(List<Vector3> OriginalList, float Time)
//     {
//         List<Vector3> ListInput = new List<Vector3>();
//         for (int i = 0; i < OriginalList.Count; i++)
//         {
//             ListInput.Add(OriginalList[i]);
//         }

//         while (ListInput.Count >= 2)
//         {
//             ListInput = GetLowerBezier(ListInput, Time);
//         }

//         return ListInput[0]; //마지막 한개를 값으로 전달
//     }

//     //리스트 배열 받으면 한 단계 아랫단계 리스트로 리턴 (새롭게 리스트 생성해서 반환)
//     List<Vector3> GetLowerBezier(List<Vector3> OriginalList, float Time)
//     {
//         List<Vector3> Final = new List<Vector3>();

//         if (OriginalList.Count >= 2)
//         {
//             //Debug.Log("갯수 : " + OriginalList.Count);
//             for (int i = 0; i < OriginalList.Count - 1; i++)
//             {
//                 Vector3 AddValue = Vector3.Lerp(OriginalList[i], OriginalList[i + 1], Time);
//                 Final.Add(AddValue);
//             }
//         }
//         return Final;
//     }
// }


// [CanEditMultipleObjects]
// [CustomEditor(typeof(TrailMaker))]
// public class TrailMaker_Editor : Editor
// {
//     int Detail;
//     bool FlipSide_Before;
//     bool InvertUV_X_Before;
//     bool InvertUV_Y_Before;

//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
//         TrailMaker Generator = (TrailMaker)target;

//         if(GUILayout.Button("메쉬 생성(멀티베지어)"))
//         {
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//         }

//         if(GUILayout.Button("저장"))
//         {
//             SaveMesh.Save(Generator.transform);
//         }
        
//         //검기 디테일값 수정 시 메쉬 업데이트
//         if(Detail != Generator.Detail)
//         {
//             if(Generator.Detail < 2)
//             {
//                 Generator.Detail = 2;
//             }
//             Debug.Log("디테일값 변경");
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//             Detail = Generator.Detail; //이전 버텍스값에 적용
//         }

//         // //면 뒤집기
//         // if(FlipSide_Before != Generator.FlipSide)
//         // {
//         //     Generator.P_1.Reverse();
//         //     Generator.P_2.Reverse();
//         //     Generator.SetVertex();
//         //     Generator.GenerateMesh(Generator.MeshFilter);
//         //     FlipSide_Before = Generator.FlipSide;
//         // }
//         // //UV뒤집기X
//         // if(InvertUV_X_Before != Generator.InvertUV_X)
//         // {
//         //     Generator.SetVertex();
//         //     Generator.GenerateMesh(Generator.MeshFilter);
//         //     InvertUV_X_Before = Generator.InvertUV_X;
//         // }   
//         // //UV뒤집기Y
//         // if(InvertUV_Y_Before != Generator.InvertUV_Y)
//         // {
//         //     Generator.SetVertex();
//         //     Generator.GenerateMesh(Generator.MeshFilter);
//         //     InvertUV_Y_Before =  Generator.InvertUV_Y;
//         // }
//     }


//     Vector3 Before_P1 = new Vector3();
//     Vector3 After_P1 = new Vector3();
//     Vector3 Before_P2 = new Vector3();
//     Vector3 After_P2 = new Vector3();

//     Vector3 BeforePosition;

//     Color ButtonColor = new Color(0.5f, 0.5f, 0.5f); //버튼 컬러

//     private void OnSceneGUI()
//     {
//         TrailMaker Generator = (TrailMaker)target;

//         //DrawIcon
//         GUIStyle FrontStyle = new GUIStyle();
//         FrontStyle.normal.textColor = Color.white;
//         FrontStyle.fontStyle = FontStyle.Bold;
//         FrontStyle.fontSize = 15;
//         FrontStyle.contentOffset += new Vector2(5, 5);

//         bool ChangePosHandles = false;
//         int MinusOneDetail = Generator.Detail - 1; //베지어 조절 기즈모 선 출력용

//         PointViewer(Generator.P_1, Color.red); //1번 베지어 그리기
//         PointViewer(Generator.P_2, Color.green); //2번 베지어 그리기

//         void PointViewer(List<Vector3> GetPoint, Color GetColor)
//         {
//             //2번 리스트배열
//             Handles.color = GetColor;
//             if (GetPoint.Count >= 2)
//             {
//                 //DrawLine (라인 출력)
//                 for (float i = 0; i < MinusOneDetail; i++)
//                 {
//                     float value_Before = i / MinusOneDetail;
//                     Before_P2 = Generator.GetPos_MulBezier(GetPoint, value_Before);

//                     float value_After = (i + 1) / MinusOneDetail;
//                     After_P2 = Generator.GetPos_MulBezier(GetPoint, value_After);
//                     Handles.DrawAAPolyLine(7, Before_P2, After_P2);
//                 }

//                 //각 베지어 포인트 위치 출력 및 버튼 출력
//                 EditorGUI.BeginChangeCheck();
//                 for (int i = 0; i < GetPoint.Count; i++)
//                 {
//                     GetPoint[i] = Handles.PositionHandle(GetPoint[i], Quaternion.identity);

//                     //위치 설정용 라인 처리
//                     Handles.DrawAAPolyLine(5, GetPoint[i], Generator.GetPos_MulBezier(GetPoint, (float)i / (GetPoint.Count - 1)));

//                     //DrawIcon
//                     GUIStyle Style = new GUIStyle();
//                     Style.contentOffset = new Vector2(-7.5f, -7.5f); //아이콘 위치 조정
//                     Handles.Label(GetPoint[i], EditorGUIUtility.IconContent("d_winbtn_mac_max"), Style);

//                     Handles.Label(GetPoint[i], i.ToString(), FrontStyle); //인덱스 번호

//                     Handles.BeginGUI();
//                     GUI.backgroundColor = ButtonColor;
//                     //버텍스 추가
//                     if(GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(GetPoint[i]).x + 10, HandleUtility.WorldToGUIPoint(GetPoint[i]).y - 30, 20, 20),EditorGUIUtility.IconContent("CreateAddNew")))
//                     {
//                         if(i + 1 == GetPoint.Count)
//                         {
//                             Vector3 Pos = GetPoint[i] + (GetPoint[i] - GetPoint[i-1]).normalized;
//                             GetPoint.Insert(i + 1, Pos);
//                         }
//                         else
//                         {
//                             GetPoint.Insert(i + 1, (GetPoint[i] + GetPoint[i+1]) / 2);
//                         }
//                         Debug.Log("버텍스 추가");
//                     }
//                     //버텍스 제거
//                     if(GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(GetPoint[i]).x + 35, HandleUtility.WorldToGUIPoint(GetPoint[i]).y - 30, 20, 20),EditorGUIUtility.IconContent("Toolbar Minus")))
//                     {
//                         GetPoint.RemoveAt(i);
//                         Debug.Log("버텍스 제거");
//                     }
//                     GUI.backgroundColor = GUI.color;
//                     Handles.EndGUI();
//                 }
//                 if(EditorGUI.EndChangeCheck())
//                 {
//                     ChangePosHandles = true;
//                 }
//             }
//             Handles.color = GUI.color;
//         }
        
//         //포지션값 수정 할때 메쉬 재구성
//         if(ChangePosHandles) //위치 기즈모움직임
//         {
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//         }

//         //피봇 수정 모드
//         EditPivot();
//         void EditPivot()
//         {
//             GUIStyle Style = new GUIStyle();
//             Style.contentOffset = new Vector2(-15, -15); //아이콘 위치 조정
//             //DrawIcon
//             Handles.Label(Generator.transform.position, EditorGUIUtility.IconContent("AvatarPivot@2x"), Style);

//             if(BeforePosition != Generator.transform.position)
//             {
//                 Generator.SetVertex();
//                 Generator.GenerateMesh(Generator.MeshFilter);
//                 BeforePosition = Generator.transform.position;
//             }
//         }

//         Handles.BeginGUI();
//         //버텍스 갯수 표시
//         GUI.Label(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 120, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 60 -(FrontStyle.fontSize /2), 100, 100),
//         Generator.Detail.ToString(),
//         FrontStyle
//         );

//         //버텍스 갯수 변경 슬라이더
//         GUI.backgroundColor = Color.white * 5;
//         EditorGUI.BeginChangeCheck();
//         Generator.Detail = (int)GUI.HorizontalSlider(
//             new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 20, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 60, 100, 0),
//             Generator.Detail, 2, 200
//             );
//         if(EditorGUI.EndChangeCheck())
//         {
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//         }
//         GUI.backgroundColor = GUI.color;

//         //X
//         GUI.backgroundColor = ButtonColor;
//         if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 20, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 20, 20), "X"))
//         {
//             Generator.InvertUV_X = !Generator.InvertUV_X;
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//         }

//         //Y
//         if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 45, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 20, 20), "Y"))
//         {
//             Generator.InvertUV_Y = !Generator.InvertUV_Y;
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//         }
//         //Flip
//         if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 70, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 35, 20), "Flip"))
//         {
//             //Generator.FlipSide = !Generator.FlipSide;
//             Generator.P_1.Reverse();
//             Generator.P_2.Reverse();
//             Generator.SetVertex();
//             Generator.SetVertex();
//             Generator.GenerateMesh(Generator.MeshFilter);
//         }
//         //저장
//         if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 110, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 50, 20), "저장"))
//         {
//             SaveMesh.Save(Generator.transform);
//         }
//         GUI.backgroundColor = GUI.color;

//         Handles.EndGUI();
//     }
// }

// public class TrailMaker_EditorWindow : EditorWindow
// {
//     // Add menu item named "My Window" to the Window menu
//     [MenuItem("DMKFactory/FX/TrailMaker")]
//     public static void ShowWindow()
//     {
//         GameObject OB = new GameObject("FX_Mesh_Trail"); //오브젝트 생성
//         OB.AddComponent<TrailMaker>();
//         Selection.activeObject = OB;
//         OB.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Diffuse"));
//         Debug.Log("실행");
//     }
// }


// #endif

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class TrailMaker : MonoBehaviour
{

    public bool EditPivot;
    [HideInInspector]public Vector3 Position_Before;
    [HideInInspector]public Quaternion Rot_Bezier = new Quaternion(); //베지어 점 회전용 데이터

    public int XCount = 10;
    public int YCount = 2;
    //public bool FlipSide;
    [HideInInspector]public bool InvertUV_X;
    [HideInInspector]public bool InvertUV_Y;
    public List<Vector3> P_1 = new List<Vector3>(){new Vector3(3, 1, 0), new Vector3(2.25f, 1, 3.5f), new Vector3(-2.25f, 1, 3.5f), new Vector3(-3, 1, 0)};
    public List<Vector3> P_2 = new List<Vector3>(){new Vector3(1, 1, 0), new Vector3(0.75f, 1, 1.25f), new Vector3(-0.75f, 1, 1.25f), new Vector3(-1, 1, 0)};
    //public List<Vector3> p_Inner = new List<Vector3>();
    public MeshFilter MeshFilter;
    public Vector2 GUIPos = new Vector2(20, 150);

    //메쉬 구성
    public void GenerateMesh(MeshFilter GetMeshFilter)
    {
        if(GetMeshFilter == null)
        {
            GetMeshFilter = GetComponent<MeshFilter>();
            GetMeshFilter.mesh = new Mesh();

            var Renderer = GetComponent<MeshRenderer>();
            Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            Renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            MeshFilter = GetMeshFilter;
            //Debug.Log("새로운 임의의 메쉬 생성");
        }

        // #endregion
        GetMeshFilter.mesh.Clear();

        //Vertices
        GetMeshFilter.mesh.vertices = SetVertPos(XCount, YCount).ToArray();

        //Triangles
        List<Vector3> Tris = SetTriList(XCount, YCount); //삼각면 할당
        GetMeshFilter.mesh.triangles = Tri_ConvertIntArray(Tris); //Vector3를 Int배열로 변환
        
        //UV
        GetMeshFilter.mesh.uv = SetUV(XCount, YCount);
            
        //Debug.Log("메쉬생성 완료");
    }

        //버텍스 위치 할당
        List<Vector3> SetVertPos(int XCount, int YCount)
        {
            List<Vector3> VertPos = new List<Vector3>();
            for (int X = 0; X < XCount; X++)
            {
                float ValueX = (float)X / (XCount - 1); //베지어에 들어가는 Value값
                for (int Y = 0; Y < YCount; Y++)
                {
                    float ValueY = (float)Y / (YCount - 1); //이게 0이면 P1 위치에 가까워짐, 이게 1dlaus P2위치에 가까워짐

                    Vector3 P1Bezier = GetPos_MulBezier(P_1, ValueX);
                    Vector3 P2Bezier = GetPos_MulBezier(P_2, ValueX);
                    Vector3 InputVector3 = Vector3.Lerp(P1Bezier, P2Bezier, ValueY) - transform.position;
                    VertPos.Add(InputVector3);
                }
            }
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
        List<Vector3> SetTriList(int XCount, int YCount)
        {
            List<Vector3> NewList = new List<Vector3>();
            NewList.Clear();
            for (int x = 0; x < XCount - 1; x++)
            {
                for (int y = 0; y < YCount - 1; y++)
                {
                    //Debug.Log(y + " , " + x); //여기서 2곱한 뒤에 3곱하면 인덱스 데이터 들어옴
                    NewList.Add(GetTri(XCount, YCount, y, x, true)); //윗면
                    NewList.Add(GetTri(XCount, YCount, y, x, false)); //아랫면
                }
            }
            //Debug.Log("삼각면 갯수 : " + NewList.Count);
            return NewList;
        }

        Vector3 GetTri(int XCount, int YCount, int Y, int X, bool Under) //Under는 아래 삼각면
        {
            int TargetIdx = X * YCount + Y; //타겟 인덱스
            Vector3 Output = new Vector3();

            if (Under) //아랫면 좌표 계산
            {
                Output.x = TargetIdx;
                Output.y = TargetIdx + 1;
                Output.z = TargetIdx + YCount;
            }
            else //윗면 좌표 계산
            {
                Output.x = TargetIdx + YCount;
                Output.y = TargetIdx + 1;
                Output.z = TargetIdx + YCount + 1;
            }
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
                    UvList.Add(new Vector2(InvertUV_X ? 1 - ValueX : ValueX, InvertUV_Y ? 1 - ValueY : ValueY));
                }
            }
            return UvList.ToArray();
        }





    //////////////////////////////////////////////////////////////////
    //멀티 베지어 계산 (새롭게 리스트 생성해서 반환)
    //////////////////////////////////////////////////////////////
    #region //멀티 베지어 계산
    public Vector3 GetPos_MulBezier(List<Vector3> OriginalList, float Time)
    {
        List<Vector3> ListInput = new List<Vector3>();
        for (int i = 0; i < OriginalList.Count; i++)
        {
            ListInput.Add(OriginalList[i]);
        }

        while (ListInput.Count >= 2)
        {
            ListInput = GetLowerBezier(ListInput, Time);
        }

        return ListInput[0]; //마지막 한개를 값으로 전달
    }

    //리스트 배열 받으면 한 단계 아랫단계 리스트로 리턴 (새롭게 리스트 생성해서 반환)
    List<Vector3> GetLowerBezier(List<Vector3> OriginalList, float Time)
    {
        List<Vector3> Final = new List<Vector3>();

        if (OriginalList.Count >= 2)
        {
            //Debug.Log("갯수 : " + OriginalList.Count);
            for (int i = 0; i < OriginalList.Count - 1; i++)
            {
                Vector3 AddValue = Vector3.Lerp(OriginalList[i], OriginalList[i + 1], Time);
                Final.Add(AddValue);
            }
        }
        return Final;
    }
    #endregion


}


[CanEditMultipleObjects]
[CustomEditor(typeof(TrailMaker))]
public class TrailMaker_Editor : Editor
{
    int Detail;
    bool FlipSide_Before;
    bool InvertUV_X_Before;
    bool InvertUV_Y_Before;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TrailMaker Generator = (TrailMaker)target;

        if(GUILayout.Button("메쉬 생성(멀티베지어)"))
        {
            //Generator.SetVertex();
            Generator.GenerateMesh(Generator.MeshFilter);
        }

        if(GUILayout.Button("저장"))
        {
            //SaveMesh.Save(Generator.transform);
        }
        
        //검기 디테일값 수정 시 메쉬 업데이트
        if(Detail != Generator.XCount)
        {
            if(Generator.XCount < 2)
            {
                Generator.XCount = 2;
            }
            //Debug.Log("디테일값 변경");
            //Generator.SetVertex();
            Generator.GenerateMesh(Generator.MeshFilter);
            Detail = Generator.XCount; //이전 버텍스값에 적용
        }

        // //면 뒤집기
        // if(FlipSide_Before != Generator.FlipSide)
        // {
        //     Generator.P_1.Reverse();
        //     Generator.P_2.Reverse();
        //     Generator.SetVertex();
        //     Generator.GenerateMesh(Generator.MeshFilter);
        //     FlipSide_Before = Generator.FlipSide;
        // }
        // //UV뒤집기X
        // if(InvertUV_X_Before != Generator.InvertUV_X)
        // {
        //     Generator.SetVertex();
        //     Generator.GenerateMesh(Generator.MeshFilter);
        //     InvertUV_X_Before = Generator.InvertUV_X;
        // }   
        // //UV뒤집기Y
        // if(InvertUV_Y_Before != Generator.InvertUV_Y)
        // {
        //     Generator.SetVertex();
        //     Generator.GenerateMesh(Generator.MeshFilter);
        //     InvertUV_Y_Before =  Generator.InvertUV_Y;
        // }
    }


    Vector3 Before_P1 = new Vector3();
    Vector3 After_P1 = new Vector3();
    Vector3 Before_P2 = new Vector3();
    Vector3 After_P2 = new Vector3();
    float SetScale = 0;
    float SetScale_Before = 0;

    //Vector3 BeforePosition;
    Color ButtonColor = new Color(0.5f, 0.5f, 0.5f); //버튼 컬러

    bool Scale_Now;
    bool Scale_Before;

    Vector3 BeforeRot;

    // float GUI_X = 20;
    // float GUI_Y = 100;

    private void OnSceneGUI()
    {
        var Event_current = Event.current.type;
        TrailMaker Generator = (TrailMaker)target;

        //DrawIcon
        GUIStyle FrontStyle = new GUIStyle();
        FrontStyle.normal.textColor = Color.white;
        FrontStyle.fontStyle = FontStyle.Bold;
        FrontStyle.fontSize = 15;
        FrontStyle.contentOffset += new Vector2(5, 5);

        bool ChangePosHandles = false;
        int MinusOneDetail = Generator.XCount - 1; //베지어 조절 기즈모 선 출력용

        PointViewer(Generator.P_1, Color.red); //1번 베지어 그리기
        PointViewer(Generator.P_2, Color.green); //2번 베지어 그리기

        void PointViewer(List<Vector3> GetPoint, Color GetColor)
        {
            //2번 리스트배열
            Handles.color = GetColor;
            if (GetPoint.Count >= 2)
            {
                //DrawLine (라인 출력)
                for (float i = 0; i < MinusOneDetail; i++)
                {
                    float value_Before = i / MinusOneDetail;
                    Before_P2 = Generator.GetPos_MulBezier(GetPoint, value_Before);

                    float value_After = (i + 1) / MinusOneDetail;
                    After_P2 = Generator.GetPos_MulBezier(GetPoint, value_After);
                    Handles.DrawAAPolyLine(7, Before_P2, After_P2);
                }

                //각 베지어 포인트 위치 출력 및 버튼 출력
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < GetPoint.Count; i++)
                {
                    GetPoint[i] = Handles.PositionHandle(GetPoint[i], Quaternion.identity);

                    //위치 설정용 라인 처리
                    Handles.DrawAAPolyLine(5, GetPoint[i], Generator.GetPos_MulBezier(GetPoint, (float)i / (GetPoint.Count - 1)));

                    //DrawIcon
                    GUIStyle Style = new GUIStyle();
                    Style.contentOffset = new Vector2(-7.5f, -7.5f); //아이콘 위치 조정
                    Handles.Label(GetPoint[i], EditorGUIUtility.IconContent("d_winbtn_mac_max"), Style);

                    Handles.Label(GetPoint[i], i.ToString(), FrontStyle); //인덱스 번호

                    Handles.BeginGUI();
                    GUI.backgroundColor = ButtonColor;
                    //버텍스 추가
                    if(GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(GetPoint[i]).x + 10, HandleUtility.WorldToGUIPoint(GetPoint[i]).y - 30, 20, 20),EditorGUIUtility.IconContent("CreateAddNew")))
                    {
                        if(i + 1 == GetPoint.Count)
                        {
                            Vector3 Pos = GetPoint[i] + (GetPoint[i] - GetPoint[i-1]).normalized;
                            GetPoint.Insert(i + 1, Pos);
                        }
                        else
                        {
                            GetPoint.Insert(i + 1, (GetPoint[i] + GetPoint[i+1]) / 2);
                        }
                        Debug.Log("버텍스 추가");
                    }
                    //버텍스 제거
                    if(GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(GetPoint[i]).x + 35, HandleUtility.WorldToGUIPoint(GetPoint[i]).y - 30, 20, 20),EditorGUIUtility.IconContent("Toolbar Minus")))
                    {
                        GetPoint.RemoveAt(i);
                        Debug.Log("버텍스 제거");
                    }
                    GUI.backgroundColor = GUI.color;
                    Handles.EndGUI();
                }
                if(EditorGUI.EndChangeCheck())
                {
                    ChangePosHandles = true;
                }
            }
            Handles.color = GUI.color;
        }
        
        //베지어 포지션값 수정 할때 메쉬 재구성
        if(ChangePosHandles) //위치 기즈모움직임
        {
            //Generator.SetVertex();
            Generator.GenerateMesh(Generator.MeshFilter);
        }

        //피봇 수정 모드
        EditPivot();
        void EditPivot()
        {
            GUIStyle Style = new GUIStyle();
            Style.contentOffset = new Vector2(-15, -15); //아이콘 위치 조정
            //DrawIcon
            if(Generator.EditPivot)
            {
                Handles.Label(Generator.transform.position, EditorGUIUtility.IconContent("d_ToolHandlePivot@2x"), Style);
            }
            else
            {
                Handles.Label(Generator.transform.position, EditorGUIUtility.IconContent("d_MoveTool On@2x"), Style);
            }

            if (Generator.Position_Before != Generator.transform.position) //위치가 바뀌었을 때
            {
                if (Generator.EditPivot)
                {
                    //Generator.SetVertex();
                    Generator.GenerateMesh(Generator.MeshFilter);
                    Generator.Position_Before = Generator.transform.position;
                }
                else
                {
                    for (int i = 0; i < Generator.P_1.Count; i++)
                    {
                        Generator.P_1[i] += Generator.transform.position - Generator.Position_Before;
                    }
                    for (int i = 0; i < Generator.P_2.Count; i++)
                    {
                        Generator.P_2[i] += Generator.transform.position - Generator.Position_Before;
                    }
                    Generator.Position_Before = Generator.transform.position;
                    Generator.GenerateMesh(Generator.MeshFilter);
                }
            }
        }


        //베지어 전체 회전
        EditorGUI.BeginChangeCheck();
        Generator.Rot_Bezier = Handles.RotationHandle(Generator.Rot_Bezier, Generator.transform.position);
        if(EditorGUI.EndChangeCheck())
        {
            Vector3 RotAmount = Generator.Rot_Bezier.eulerAngles - BeforeRot;
            //BeforeRot = Generator.rotation.eulerAngles;

            for (int i = 0; i < Generator.P_1.Count; i++)
            {
                Generator.P_1[i] = RotatePointAroundPivot(Generator.P_1[i], Generator.transform.position, RotAmount);
            }
            for (int i = 0; i < Generator.P_2.Count; i++)
            {
                Generator.P_2[i] = RotatePointAroundPivot(Generator.P_2[i], Generator.transform.position, RotAmount);
            }

            Generator.GenerateMesh(Generator.MeshFilter); //메쉬 재구성
        }

        if(Generator.Rot_Bezier.eulerAngles != BeforeRot)
        {
            BeforeRot = Generator.Rot_Bezier.eulerAngles;
        }

        Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) 
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
        
        //마우스 때면 회전값 초기화
        if(Event_current == EventType.MouseUp)
        {
            BeforeRot = new Vector3(0, 0, 0);
            Generator.Rot_Bezier = Quaternion.identity;
        }



        Handles.BeginGUI();
        GUI.Box(new Rect(Generator.GUIPos.x - 10, Generator.GUIPos.y + 10, 200, 110), "");

        //버텍스 갯수 표시
        GUI.Label(new Rect( 100 + Generator.GUIPos.x, Generator.GUIPos.y + 22 -(FrontStyle.fontSize /2), 100, 100),
        "X : " + Generator.XCount.ToString(),
        FrontStyle
        );

        GUI.Label(new Rect(100 + Generator.GUIPos.x,  Generator.GUIPos.y + 42 -(FrontStyle.fontSize /2), 100, 100),
        "Y : " + Generator.YCount.ToString(),
        FrontStyle
        );

        //버텍스 갯수 변경 슬라이더
        GUI.backgroundColor = Color.white * 5;
        EditorGUI.BeginChangeCheck();

        // Generator.XCount = (int)GUI.HorizontalSlider(
        //     new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 20, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 80, 100, 0),
        //     Generator.XCount, 2, 100
        //     );

        // Generator.YCount = (int)GUI.HorizontalSlider(
        //     new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 20, HandleUtility.WorldToGUIPoint(Generator.transform.position).y -60, 100, 0),
        //     Generator.YCount, 2, 10
        //     );
        Generator.XCount = (int)GUI.HorizontalSlider(
            new Rect(Generator.GUIPos.x, Generator.GUIPos.y + 20, 100, 0),
            Generator.XCount, 2, 100
            );

        Generator.YCount = (int)GUI.HorizontalSlider(
            new Rect(Generator.GUIPos.x, Generator.GUIPos.y + 40, 100, 0),
            Generator.YCount, 2, 10
            );

        if(EditorGUI.EndChangeCheck())
        {
            //Generator.SetVertex();
            Generator.GenerateMesh(Generator.MeshFilter);
        }
        GUI.backgroundColor = GUI.color;

        GUI.backgroundColor = Color.white * 5;
        GUI.Label(new Rect(100 + Generator.GUIPos.x,  Generator.GUIPos.y + 62 - (FrontStyle.fontSize / 2), 100, 100),
        "Scale",
        FrontStyle
        );

        //베지어 전체 스케일
        EditorGUI.BeginChangeCheck();
        SetScale = GUI.HorizontalSlider(
            new Rect(Generator.GUIPos.x, Generator.GUIPos.y + 60, 100, 0),
            SetScale, -1, 1
            );
        if(EditorGUI.EndChangeCheck())
        {
            for (int i = 0; i < Generator.P_1.Count; i++)
            {
                Generator.P_1[i] = Generator.P_1[i] * (SetScale - SetScale_Before + 1)  - (Generator.transform.position) * (SetScale - SetScale_Before);
            }
            for (int i = 0; i < Generator.P_2.Count; i++)
            {
                Generator.P_2[i] = Generator.P_2[i] * (SetScale - SetScale_Before + 1) - (Generator.transform.position) * (SetScale - SetScale_Before);
            }
            SetScale_Before = SetScale;
            Generator.GenerateMesh(Generator.MeshFilter);
        }
        //Debug.Log(Event.current.type);
        if(Event_current == EventType.MouseUp)
        {
            SetScale_Before = 0;
            SetScale = 0;
        }
        GUI.backgroundColor = GUI.color;

#region 안씀
        // //X UV반전
        // GUI.backgroundColor = ButtonColor;
        // if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 20, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 20, 20), "X"))
        // {
        //     Generator.InvertUV_X = !Generator.InvertUV_X;
        //     //Generator.SetVertex();
        //     Generator.GenerateMesh(Generator.MeshFilter);
        // }

        // //Y UV반전
        // if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 45, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 20, 20), "Y"))
        // {
        //     Generator.InvertUV_Y = !Generator.InvertUV_Y;
        //     //Generator.SetVertex();
        //     Generator.GenerateMesh(Generator.MeshFilter);
        // }

        // //Flip
        // if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 70, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 35, 20), "Flip"))
        // {
        //     Generator.P_1.Reverse();
        //     Generator.P_2.Reverse();
        //     Generator.GenerateMesh(Generator.MeshFilter);
        // }

        // //Move & Pivot
        // GUIStyle Front_Move = new GUIStyle();
        // Front_Move.fontSize  = 90;
        // if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 110, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 45, 20), Generator.Move ? "Move" : "Pivot"))
        // {
        //     Generator.Move = !Generator.Move;
        // }
        
        // //저장
        // if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Generator.transform.position).x + 160, HandleUtility.WorldToGUIPoint(Generator.transform.position).y - 40, 40, 20), "저장"))
        // {
        //     SaveMesh.Save(Generator.transform);
        // }
        // GUI.backgroundColor = GUI.color;
#endregion
        
        //X UV반전
        GUI.backgroundColor = ButtonColor;
        if (GUI.Button(new Rect(Generator.GUIPos.x, Generator.GUIPos.y + 90, 20, 20), "U"))
        {
            Generator.InvertUV_X = !Generator.InvertUV_X;
            //Generator.SetVertex();
            Generator.GenerateMesh(Generator.MeshFilter);
        }

        //Y UV반전
        if (GUI.Button(new Rect( 25 + Generator.GUIPos.x, Generator.GUIPos.y + 90, 20, 20), "V"))
        {
            Generator.InvertUV_Y = !Generator.InvertUV_Y;
            //Generator.SetVertex();
            Generator.GenerateMesh(Generator.MeshFilter);
        }

        //Flip
        if (GUI.Button(new Rect(50 + Generator.GUIPos.x, Generator.GUIPos.y + 90, 35, 20), "Flip"))
        {
            Generator.P_1.Reverse();
            Generator.P_2.Reverse();
            Generator.GenerateMesh(Generator.MeshFilter);
        }

        //Move & Pivot
        GUIStyle Front_Move = new GUIStyle();
        Front_Move.fontSize  = 90;
        if (GUI.Button(new Rect(90 + Generator.GUIPos.x, Generator.GUIPos.y + 90, 45, 20), Generator.EditPivot ? "Pivot" :  "Move" ))
        {
            Generator.EditPivot = !Generator.EditPivot;
        }
        
        //저장
        if (GUI.Button(new Rect(140 + Generator.GUIPos.x, Generator.GUIPos.y + 90, 40, 20), "저장"))
        {
            //SaveMesh.Save(Generator.transform);
        }
        GUI.backgroundColor = GUI.color;

        Handles.EndGUI();
    }
}

public class TrailMaker_EditorWindow : EditorWindow
{
    // Add menu item named "My Window" to the Window menu
    [MenuItem("DMKFactory/FX/TrailMaker")]
    public static void ShowWindow()
    {
        GameObject OB = new GameObject("FX_Mesh_Trail"); //오브젝트 생성
        OB.AddComponent<TrailMaker>();
        Selection.activeObject = OB;
        OB.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Diffuse"));
        //Debug.Log("실행");
    }
}


#endif