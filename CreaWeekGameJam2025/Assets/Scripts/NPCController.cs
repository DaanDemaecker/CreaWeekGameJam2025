using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.VFX;

public class NPCController : MonoBehaviour
{
    [HideInInspector] public StateMachine StateMachine;
    [SerializeField] private Animator animator;

    [SerializeField] public UnityEvent<bool> OnDeath;

    [SerializeField]
    public VisualEffect _bleeding;

    // Bleeding variables
    float minDelay = 1.5f;
    float maxDelay = 2.0f;
    float bloodSize = 2.0f;

    float bloodCooldown = 0;
    float bloodTimer = 0;

    float bleedingTime = 5.0f;
    float bleedingTimer = 0.0f;
    bool isDead = false;
    public bool IsDead
    {
        set { isDead = value; }
        get { return isDead; }
    }

    

    private bool _isBleeding = false;
    public bool IsBleeding
    {
        get { return _isBleeding; }
        set {
            if (_isBleeding == value) return;

            _isBleeding = value;
            StartCoroutine(ChangeBleeding(value));
        }
    }
    public delegate void SmallBloodDropped(Vector3 pos, float size);
    public static event SmallBloodDropped onBloodDropped;
    void Start()
    {
        StateMachine = new StateMachine(new WanderingState(Vector3.zero,this));

        _bleeding.Stop();
    }

    IEnumerator ChangeBleeding(bool v)
    {
        _bleeding.Play();

        float startTime = Time.time;

        while(startTime + .5f >= Time.time)
        {
            animator.SetFloat("Bleeding", Mathf.Lerp(v ? 0f : 1f, v ? 1f : 0f, (Time.time - startTime) / .5f));
            yield return null;
        }

        

    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.Update();

        if(_isBleeding)
        {
            HandleBleeding();
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void HandleBleeding()
    {
        bloodTimer += Time.deltaTime;

        if (bloodTimer > bloodCooldown)
        {
            DropBlood();
        }

        if(!isDead)
        {
            bleedingTimer += Time.deltaTime;
            if(bleedingTimer >= bleedingTime)
            {
                var player = FindFirstObjectByType<PlayerMovement>();
                //StateMachine.MoveToState(new DeadNPCState(player.transform.position, this));
            }
        }

        
    }

    private void DropBlood()
    {
        onBloodDropped.Invoke(transform.position, bloodSize);

        bloodTimer = 0;
        bloodCooldown = Random.Range(minDelay, maxDelay);
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
    float _bloodSize = 3.0f;
    Vector3 _playerPos;

    float _time = 0;
    bool _sinking = false;

    float _degPerSecond = 180.0f;

    public delegate void SmallBloodDropped(Vector3 pos, float size);
    public static event SmallBloodDropped onBloodDropped;

    public delegate void EnemyKilled();
    public static event EnemyKilled onEnemyKilled;

    NPCController context;
    public DeadNPCState(Vector3 playerPos, NPCController ctx)
    {
        _playerPos = playerPos;
        context = ctx;
    }
    public void OnEnter()
    {
        context.OnDeath.Invoke(true);

        onEnemyKilled.Invoke();

        context.IsDead = true;

        onBloodDropped.Invoke(context.transform.position, _bloodSize);

        //screenshake
        ScreenShake cameraShake = null;
        if (Camera.main.TryGetComponent<ScreenShake>(out cameraShake))
        {
            cameraShake.StartShake(0.2f);
        }
        else
        {
            Debug.LogError("Please add a ScreenShake Component to the main camera!");
        }
    }

    private void EndRotation()
    {
        _time = 0;
        _sinking = false;
    }

    public void OnExit()
    {
        //throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        _time += Time.deltaTime;

        if(_time >= 5)
        {
            _sinking = true;

            context.transform.position -= Vector3.up * Time.deltaTime;
            
            if(_time >= 6)
            {
                 context.Destroy();
            }
        }
    }
}
public class EnterBuildingState : IState
{
    NPCController context;

    float speed = .6f;
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

            Vector3 direction = targetPosition - context.transform.position;
            context.transform.forward = direction;

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

            } while (!IsValidPosition(startPos + dir1 + dir1) || Physics.Raycast(startPos + Vector3.up, dir1, dst, 1 << 16));

        }

        do
        {
            dir2 = Quaternion.Euler(0, Random.Range(-145, 145), 0) * dir1;
        } while (!IsValidPosition(startPos + dir1 + dir2) || Physics.Raycast(startPos + dir1 + Vector3.up, dir2, dst * 2, 1 << 16));
    }

    bool IsValidPosition(Vector3 pos)
    {
        return Physics.CheckSphere(pos, .1f, 1 << 18);
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

            Vector3 direction = targetPosition - context.transform.position;
            context.transform.forward = direction;

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

        //if(Doors.Length >= 1)
        if(Doors.Length > 0 && Random.Range(0,1f) > .8f)
        {
            door = Doors[(int)Random.Range(0, Doors.Length)].transform.position;
            return true;
        } 

        return false;
    }
}

public class ChasingState : IState
{

    NPCController context;

    Transform _player;

    float time = 0;

    float speed = 2f;
    float dst = 3;

    Vector3 startPos;

    Vector3 dir1;
    Vector3 dir2;

    public ChasingState(Vector3 startForward, Transform player, NPCController ctx)
    {
        _player = player;
        context = ctx;

        float time = 0;

        startPos = context.transform.position;

        if (startForward != Vector3.zero)
        {
            dir1 = startForward;
        } else
        {
            dir1 = (_player.position - context.transform.position).normalized * dst;
        }
        

        do
        {
            dir2 = Quaternion.Euler(0, Random.Range(-180, 180), 0) * dir1;
        } 
        while (!IsFacingTowardsPlayer() || 
        (!IsValidPosition(startPos + dir1 + dir2) || 
        Physics.Raycast(startPos + dir1 + Vector3.up, dir2, dst * 2, 1 << 16)));
    }

    bool IsValidPosition(Vector3 pos)
    {
        return Physics.CheckSphere(pos, .1f, 1 << 18);
    }

    bool IsFacingTowardsPlayer()
    {
        float dot = Vector3.Dot((_player.position - context.transform.position).normalized, (dir1 + dir2).normalized);
        return dot > 0.8f;
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

            Vector3 direction = targetPosition - context.transform.position;
            
            context.transform.forward = direction;
            context.transform.position = targetPosition;

        }
        else
        {
            if (Vector3.Distance(context.transform.position, _player.position) <= 6)
            {
                context.StateMachine.MoveToState(new WanderingState(dir2, context));
            }
            else
            {
                context.StateMachine.MoveToState(new ChasingState(dir2,_player, context));
            }
        }

        //if (time > _chasingDuration)
        //{
        //    context.StateMachine.MoveToState(new WanderingState(Vector3.zero, context));
        //}
    }
}