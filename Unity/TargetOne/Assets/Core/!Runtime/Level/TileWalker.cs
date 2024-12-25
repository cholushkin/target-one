using GameLib;
using GameLib.Alg;
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
            Debug.Log("Start accelerating");
        }
        StartQuitingTile();
    }



    private void OnUpdateWalking()
    {
        // Calculate the direction to the destination
        Vector3 direction = (SubDestinationPointer.transform.position - transform.position).normalized;

        // Calculate the distance to move this frame
        float distanceToMove = Speed * Time.deltaTime;

        // Calculate the remaining distance to the destination
        float remainingDistance = Vector3.Distance(transform.position, SubDestinationPointer.transform.position);

        // Check if moving would overshoot the destination
        if (distanceToMove >= remainingDistance)
        {
            if(_quiting)
                StartMoveToNextTile();
            else
            {
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
            //GameObjectExtensions.CreateDebugSphere(SubDestinationPointer.transform.position,Tile.TileSize/2);
            
            foreach (var hit in sphereHits)
            {
                var tile = hit.GetComponent<Tile>();
                if (tile != null && tile != CurrentTile)
                {
                    Vector3 toTile = tile.transform.position - CurrentTile.transform.position;
                    _quiting = false;
                    SetCurrentTile(tile);
                    SetSubdestinationPointer(CurrentTile, tile.transform.position);
                    //Debug.Log($"next tile {tile.name}");
                    return;
                    // float elevationDifference = Mathf.Abs(toTile.y);
                    // float angleDifference = Vector3.Angle(CurrentTile.Normal, tile.Normal);
                    //
                    // if (elevationDifference <= MaxElevationThreshold && angleDifference <= smallestAngle)
                    // {
                    //     nextTile = tile;
                    //     smallestAngle = angleDifference;
                    // }
                }
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
    
    
    private void StartMovementCycle()
    {
        // _movementDirection = transform.forward;
        //
        // var currentTilePosition = CurrentTile.transform.position;
        // var sphereHits = Physics.OverlapSphere(currentTilePosition + _movementDirection * Tile.TileSize, Tile.TileSize / 2);
        //
        // Tile nextTile = null;
        // float smallestAngle = MaxAngleThreshold;
        //
        // foreach (var hit in sphereHits)
        // {
        //     var tile = hit.GetComponent<Tile>();
        //     if (tile != null && tile != CurrentTile)
        //     {
        //         Vector3 toTile = tile.transform.position - currentTilePosition;
        //         float elevationDifference = Mathf.Abs(toTile.y);
        //         float angleDifference = Vector3.Angle(CurrentTile.Normal, tile.Normal);
        //
        //         if (elevationDifference <= MaxElevationThreshold && angleDifference <= smallestAngle)
        //         {
        //             nextTile = tile;
        //             smallestAngle = angleDifference;
        //         }
        //     }
        // }
        //
        // if (nextTile != null)
        // {
        //     //_isMoving = true;
        //     _targetPosition = nextTile.transform.position;
        //     _targetNormal = nextTile.Normal;
        //     SetCurrentTile(nextTile);
        // }
        // else
        // {
        //     // No tile found, move forward a short distance decelerating
        //     StartCoroutine(FallForward());
        // }
    }

    private void MoveTowardsTarget()
    {
        // // Smoothly interpolate position
        // transform.position = Vector3.MoveTowards(transform.position, _targetPosition, Speed * Time.deltaTime);
        //
        // // Smoothly interpolate rotation
        // Quaternion targetRotation = Quaternion.LookRotation(_movementDirection, _targetNormal);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Speed * Time.deltaTime);
        //
        // // Check if reached the center of the target tile
        // if (Vector3.Distance(transform.position, _targetPosition) <= 0.01f)
        // {
        //     //_isMoving = false;
        // }
    }

    // private System.Collections.IEnumerator FallForward()
    // {
    //     float fallDistance = 0.0f;
    //     Vector3 fallStart = transform.position;
    //
    //     while (fallDistance < Tile.TileSize / 2)
    //     {
    //         fallDistance += Speed * Time.deltaTime;
    //         transform.position = fallStart + _movementDirection * fallDistance;
    //         yield return null;
    //     }
    //
    //     // Final check for a tile before falling off
    //     var sphereHits = Physics.OverlapSphere(transform.position, Tile.TileSize / 2);
    //     foreach (var hit in sphereHits)
    //     {
    //         var tile = hit.GetComponent<Tile>();
    //         if (tile != null)
    //         {
    //             Init(tile);
    //             yield break;
    //         }
    //     }
    //
    //     // No tile found, complete fall
    //     Debug.Log("Walker fell off!");
    // }
}
