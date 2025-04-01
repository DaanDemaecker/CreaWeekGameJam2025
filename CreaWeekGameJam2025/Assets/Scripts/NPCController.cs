using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;

public class NPCController : MonoBehaviour
{

    [HideInInspector] public StateMachine StateMachine;

    [SerializeField] public UnityEvent OnDeath;

    void Start()
    {
        StateMachine = new StateMachine(new WanderingState(Vector3.zero,this));
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.Update();
    }

    public void Die()
    {

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
public class DeadNPCState : IState
{
    NPCController context;
    public DeadNPCState(NPCController ctx)
    {
        context = ctx;
    }
    public void OnEnter()
    {
        context.OnDeath.Invoke();
        context.transform.localScale = new Vector3(1, .1f, 1);
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
public class EnterBuildingState : IState
{
    NPCController context;

    float speed = 2f;
    float dst = 3;

    float hideLength = 5f;

    float time = 0;
    float hideTime = 0;

    Vector3 startPos;

    Vector3 dir1;
    Vector3 dir2;

    public EnterBuildingState(Vector3 startForward, Vector3 doorPosition, NPCController ctx)
    {
        time = 0;

        context = ctx;
        startPos = context.transform.position;

        dir1 = startForward;
        dir2 = doorPosition - (context.transform.position + dir1);
        dir2.y = 0;
    }   
    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        time += Time.deltaTime;
        if (time <= speed)
        {
            float normalizedTime = time / speed;

            Vector3 pos1 = startPos + Vector3.Lerp(Vector3.zero, dir1, normalizedTime);
            Vector3 pos2 = startPos + dir1 + Vector3.Lerp(Vector3.zero, dir2, normalizedTime);

            Vector3 targetPosition = Vector3.Lerp(pos1, pos2, normalizedTime);

            context.transform.position = targetPosition;
        } else
        {
            context.transform.GetChild(0).gameObject.SetActive(false);
            hideTime += Time.deltaTime;
        }

        if(hideTime >= hideLength)
        {
            context.transform.GetChild(0).gameObject.SetActive(true);
            context.StateMachine.MoveToState(new WanderingState(-dir2, context));
        }
    }
}
public class WanderingState : IState
{

    NPCController context;

    float speed = 2f;
    float dst = 3;

    float time = 0;

    Vector3 startPos;

    Vector3 dir1;
    Vector3 dir2;

    public WanderingState(Vector3 startForward, NPCController ctx)
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

            } while (Physics.Raycast(startPos + Vector3.up, dir1, dst, 1 << 16));

        }

        do
        {
            dir2 = Quaternion.Euler(0, Random.Range(-145, 145), 0) * dir1;
        } while (Physics.Raycast(startPos + dir1 + Vector3.up, dir2, dst * 2, 1 << 16));
    }

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
            if (BuildingNearby(out Vector3 door))
            {
                context.StateMachine.MoveToState(new EnterBuildingState(dir2, door, context));
            } else
            {
                context.StateMachine.MoveToState(new WanderingState(dir2, context));
            }
            
        }
        
    }

    private bool BuildingNearby(out Vector3 door)
    {
        door = Vector3.zero;
        Collider[] Doors = Physics.OverlapSphere(startPos + dir1 + dir2, dst * 2, 1 << 17);

        Debug.Log("Found " + Doors.Length + " doors");

        //if(Doors.Length >= 1)
        if(Doors.Length > 0 && Random.Range(0,1f) > .8f)
        {
            door = Doors[(int)Random.Range(0, Doors.Length)].transform.position;
            return true;
        } 

        return false;


    }
}