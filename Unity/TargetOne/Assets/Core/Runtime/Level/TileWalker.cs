using GameLib;
using GameLib.Alg;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

public class TileWalker : MonoBehaviour
{
    public enum State
    {
        Walking,
        Standing,
        Terminating,
        Dead
    }

    #region Inspector

    public LogChecker LogChecker;
    public GameObject SubDestinationPointer;
    
    [Tooltip("Movement speed (units per second).")]
    public float Speed = 2.0f;

    [Tooltip("Maximum elevation difference to consider a tile reachable.")]
    public float MaxElevationThreshold = 0.5f;

    [Tooltip("Maximum angular difference (degrees) to consider a tile reachable.")]
    public float MaxAngleThreshold = 20f;

    public Tile CurrentTile;
    #endregion
    
    public float CurrentSpeed => Speed;
    
    
    private  Tile _targetTile;

    private State _currentState;
    private bool _quiting;
    private StateMachine<State> _stateMachine;

    public void Awake()
    {
        Init(CurrentTile);
        _stateMachine = new StateMachine<State>(this, State.Standing);
    }

    public void Start()
    {
        _stateMachine.GoTo(State.Walking);
    }

    private void Init(Tile startTile)
    {
        // Enable debug visualization
        if (LogChecker.Gizmos)
            foreach (var meshRend in GetComponentsInChildren<MeshRenderer>())
                meshRend.enabled = true;
        
        Assert.IsNotNull(startTile);
        SetCurrentTile(startTile);
        transform.position = CurrentTile.gameObject.transform.position;
        transform.rotation = Quaternion.LookRotation(CurrentTile.Forward, CurrentTile.Up);
    }

    private void SetCurrentTile(Tile tile)
    {
        CurrentTile = tile;
        transform.rotation = Quaternion.LookRotation(GetClosestTileDirection(CurrentTile, transform.forward), CurrentTile.Up);
        transform.SetParent(tile.transform);
    }

    private void Update()
    {
        _stateMachine.Update();
    }
    
    
    #region State machine 
    // ( OnEnter, OnUpdate, OnExit )
    
    private void OnEnterWalking()
    {
        if (_stateMachine.CurrentState.State == State.Standing)
        {
            // Start accelerating
            LogChecker.Print(LogChecker.Level.Normal, "Start accelerating");
        }
        StartQuitingTile();
    }



    private void OnUpdateWalking()
    {
        // Calculate the direction to the destination
        Vector3 direction = (SubDestinationPointer.transform.position - transform.position).normalized;

        // Calculate the distance to move this frame
        float distanceToMove = Speed * GameSession.Instance.GameSpeed * Time.deltaTime;

        // Calculate the remaining distance to the destination
        float remainingDistance = Vector3.Distance(transform.position, SubDestinationPointer.transform.position);

        // Check if moving would overshoot the destination
        if (distanceToMove >= remainingDistance)
        {
            if(_quiting)
                StartMoveToNextTile();
            else
            {
                // Trigger reach center
                CurrentTile.GetComponent<TriggerTileReachCenter>()?.HitTriggerReachCenter(this);
                
                StartQuitingTile();
            }
        }

        // Move towards the destination
        transform.position += direction * distanceToMove;
    }

    

    #endregion
    
    private void StartMoveToNextTile()
        {
            // Find next tile
            var sphereHits = Physics.OverlapSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2);

            if (LogChecker.Gizmos)
            {
                GameObjectExtensions.CreateDebugSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2, 1f);
            }

            foreach (var hit in sphereHits)
            {
                var tile = hit.GetComponent<Tile>();
                if (tile == null || tile == CurrentTile) 
                    continue;
                
                _quiting = false;
                var prevCurrentTile = CurrentTile;
                SetCurrentTile(tile);
                SetSubdestinationPointer(CurrentTile, tile.transform.position);
                
                // On attach to a new tile
                CurrentTile.GetComponent<TileWheelRotation>()
                    ?.RegisterEntering(prevCurrentTile);
                    
                CurrentTile.GetComponent<TriggerTileEnter>()
                    ?.HitTriggerEnter(this);
                    
                return;
            }
        }
    private void StartQuitingTile()
    {
        Vector3 closestDirection = GetClosestTileDirection(CurrentTile, transform.forward);

        _quiting = true;
        SetSubdestinationPointer(CurrentTile, CurrentTile.transform.position + closestDirection * Tile.TileSize * 0.5f);
    }

    private Vector3 GetClosestTileDirection(Tile tile, Vector3 direction)
    {
        Vector3[] directions = {
            tile.Forward,
            tile.Right,
            -tile.Right,
            -tile.Forward
        };
        
        Vector3 closestDirection = directions[0];
        float maxDot = Mathf.Abs(Vector3.Dot(closestDirection, direction));

        // Loop through the remaining directions
        for (int i = 1; i < directions.Length; i++)
        {
            float dot = Mathf.Abs(Vector3.Dot(directions[i], direction));
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = directions[i];
            }
        }
        return closestDirection;
    }

    private void SetSubdestinationPointer(Tile parent, Vector3 posistion)
    {
        SubDestinationPointer.transform.position = posistion;
        SubDestinationPointer.transform.SetParent(parent.transform);
    }
}
