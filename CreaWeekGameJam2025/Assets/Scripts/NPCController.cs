using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class NPCController : MonoBehaviour
{

    [HideInInspector] public StateMachine StateMachine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateMachine = new StateMachine(new WanderingState(Vector3.zero,this));
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.Update();
    }
}

public class StateMachine
{
    IState currentState;
    public StateMachine(IState startState)
    {
        currentState = startState;
        currentState.OnEnter();
    }

    public void Update()
    {
        currentState.OnUpdate();
    }

    public void MoveToState(IState newState)
    {
        if (newState == null) return;

        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}

public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}


public class WanderingState : IState
{

    NPCController context;

    float speed = .5f;
    float dst = 3;

    public WanderingState(Vector3 startForward,NPCController ctx)
    {
        context = ctx;

        startPos = context.transform.position;

        if (startForward != Vector3.zero)
        {
            dir1 = startForward.normalized * dst;
        }
        else
        {
            do
            {
                time = 0;

                dir1 = Random.insideUnitSphere;
                dir1.y = 0;
                dir1.Normalize();
                dir1 *= dst;
                
            } while (Physics.Raycast(startPos +  Vector3.up, dir1, dst, 1 << 16));

        }

        do
        {
            dir2 = Quaternion.Euler(0, Random.Range(-145, 145), 0) * dir1;
        } while (Physics.Raycast(startPos+dir1 + Vector3.up,dir2, dst * 2, 1 << 16));
    }

    float time = 0;

    Vector3 startPos;

    Vector3 dir1;
    Vector3 dir2;

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    { 
        time += Time.deltaTime;
        
        if(time <= speed)
        {
            float normalizedTime = time / speed;

            Vector3 pos1 = startPos + Vector3.Lerp(Vector3.zero, dir1, normalizedTime);
            Vector3 pos2 = startPos + dir1 + Vector3.Lerp(Vector3.zero, dir2, normalizedTime);

            Vector3 targetPosition = Vector3.Lerp(pos1,pos2,normalizedTime);

            context.transform.position = targetPosition;

        }
        else
        {
            context.StateMachine.MoveToState(new WanderingState(dir2, context));
        }
        
    }
}