#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[ExecuteInEditMode]
public class MotionPath : MonoBehaviour
{
    [HideInInspector]public Animator Animator;
    [HideInInspector]public GameObject TargetObject;
    [HideInInspector] public float StartFrame = 0;
    [HideInInspector] public float EndFrame = 60;
    [HideInInspector] public float AnimationSlider;

    public Vector3[] PathPos = new Vector3[0]; //실제 쓰는 최종 위치 데이터

    [HideInInspector]public AnimationClip[] AnimationClips;
    [HideInInspector]public string[] AniClipsName;
    [HideInInspector]public int SelectAniClip;
    [HideInInspector]public string PlayStateName; //재생할 스테이트 이름

    [HideInInspector]public int PathFrame = 120;

    [HideInInspector]public bool PathViewerSetting = false;
    [HideInInspector]public bool AutoUpdate = true;
    [HideInInspector]public Color PathColor = new Color(0, 1, 0);
    [HideInInspector]public float PathWidth = 2;



    
    //버텍스 평균치 구하는 계산 변수들
    [HideInInspector] public bool Average_AutoUpdate = true; //자동 업데이트
    public Vector3[] AveragePos = new Vector3[0]; //버텍스 평균 위치
    [HideInInspector] public int Average_Detail = 10; //디테일값
    [HideInInspector] public float Average_Distance = 0.25f; //버텍스들의 거리값



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

        //패스 그리기
        Handles.BeginGUI();
        Handles.EndGUI();
        Handles.color = PathColor;
        Handles.DrawAAPolyLine(PathWidth, Ge.PathPos.Length, Ge.PathPos);
        Handles.color = GUI.color;

        //버텍스 평균치 그리기
        GUIStyle Style = new GUIStyle();
        Style.contentOffset = new Vector2(-8, -8); //아이콘 위치 조정

        for (int i = 0; i < AveragePos.Length; i++)
        {
            Handles.Label(AveragePos[i], EditorGUIUtility.IconContent("winbtn_mac_close"), Style);
        }
        
    }




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

        //오브젝트
        EditorGUI.BeginChangeCheck();
        Ge.TargetObject = (GameObject)EditorGUILayout.ObjectField("TargetObject", Ge.TargetObject, typeof(GameObject));
        if(EditorGUI.EndChangeCheck())
        {
            if(Ge.TargetObject != null)
            {
                //추적 오브젝트 변경 시 패스 재생성
                if(Ge.AutoUpdate)
                {
                    CreatePath();
                }
            }
        }

        ///////////////////////////////////////////////////////
        //////////////////      GUI     ///////////////////////
        ///////////////////////////////////////////////////////
        //애니메이터가 있을 때
        if (Ge.Animator != null)
        {
            //애니메이션정보 GUI
            GUILayout.BeginVertical("GroupBox");
            DrawAniInfo();
            GUILayout.EndVertical();

            //오브젝트 있을 때
            if (Ge.TargetObject != null)
            {
                //패스 GUI
                GUILayout.BeginVertical("GroupBox");
                DrawPathInfo();
                GUILayout.EndVertical();

                //패스 To 버텍스위치 GUI
                GUILayout.BeginVertical("GroupBox");
                Viewer_AveragePos();
                GUILayout.EndVertical();
            }
        }

        //패스 정보
        void DrawPathInfo()
        {
            PathViewerSetting();
            Ge.AutoUpdate = EditorGUILayout.Toggle("Auto Update", Ge.AutoUpdate);

            EditorGUI.BeginChangeCheck();
            int PathDetail = EditorGUILayout.IntSlider("PathDetail (Frame)", Ge.PathFrame, 1, 500);
            PathDetail = Mathf.Max(PathDetail, 1); //최소값
            Ge.PathFrame = PathDetail;
            if(EditorGUI.EndChangeCheck()) //패스 디테일 수정하면 업데이트
            {
                if(Ge.AutoUpdate)
                {
                    CreatePath();
                }
            }

            if(!Ge.AutoUpdate)
            {
                if (GUILayout.Button("Create Path"))
                {
                    CreatePath();
                }
            }
        }

        //CreatePath (패스 생성)
        void CreatePath()
        {
            float FirstFrame = Ge.AnimationSlider; //현재 포즈 시간 백업
            List<Vector3> NewPathPosition = new List<Vector3>();
            for (float i = Ge.StartFrame; i < Ge.EndFrame; i += (1f / (float)Ge.PathFrame))
            {
                Ge.AnimationSlider = i; //포즈 시간 업데이트
                UpDatePos(); //포즈 업데이트
                NewPathPosition.Add(Ge.TargetObject.transform.position);
            }
            Ge.PathPos = NewPathPosition.ToArray();

            Ge.AnimationSlider = FirstFrame; //처음 설정한 포즈 시간으로 백업
            UpDatePos(); //포즈 업데이트

            //버텍스 평균 위치값 자동 업데이트
            if(Ge.Average_AutoUpdate)
            {
                CountAveragePos(Ge); //버텍스 평균 위치값 업데이트
            }
        }

        //패스 보기 설정
        void PathViewerSetting()
        {
            Ge.PathViewerSetting = EditorGUILayout.Toggle("Path View Setting", Ge.PathViewerSetting);
            if (Ge.PathViewerSetting)
            {
                GUILayout.BeginVertical("GroupBox");
                Ge.PathColor = EditorGUILayout.ColorField("Path Color", Ge.PathColor);
                Ge.PathWidth = EditorGUILayout.FloatField("Path Width", Ge.PathWidth);
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

                if(Ge.AutoUpdate)
                {
                    CreatePath();
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
            if(ChangeMinMax) //패스랑 관련된 값들 변경되면 패스 새로 생성
            {
                if(Ge.AutoUpdate)
                {
                    CreatePath();
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
            Ge.Animator.Update(Time.deltaTime);
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

    //패스의 평균치를 구해서 버텍스 할당 뷰어
    #region 패스 평균이 구해서 버텍스 할당 하는기능
    void Viewer_AveragePos()
    {
        MotionPath Ge = (MotionPath)target;

        //값 변경시 업데이트
        EditorGUI.BeginChangeCheck();
        Ge.Average_AutoUpdate = EditorGUILayout.Toggle("Average_AutoUpdate", Ge.Average_AutoUpdate);
        Ge.Average_Detail = EditorGUILayout.IntSlider("Average_Detail", Ge.Average_Detail, 1, 100);
        Ge.Average_Distance = EditorGUILayout.Slider("Average_Distance", Ge.Average_Distance, 0.01f, 1);
        if(EditorGUI.EndChangeCheck())
        {
            CountAveragePos(Ge);
        }

        if(!Ge.Average_AutoUpdate)
        {
            if(GUILayout.Button("라인의 평균 버텍스 거리값 계산"))
            {
                CountAveragePos(Ge);
            }
        }
    }

    //평균 거리값 계산
    void CountAveragePos(MotionPath Ge)
    {
        Ge.AveragePos = GetAveragePos(Ge.PathPos, Ge.Average_Detail, Ge.Average_Distance);
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
                if(GetDistance(BeforePoint, NextPos, VertDistance)) //거리 비교해서 해당 거리보다 크면 반환
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
        bool Output = Vector3.Distance(BeforePos, NextPos) >= Distance ?  true : false;
        return Output;
    }
    #endregion


}

#endif
