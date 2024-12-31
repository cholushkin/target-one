using GameLib;
using GameLib.Log;
using UnityEngine;
using UnityEngine.Assertions;


namespace Core
{
    public class TileWalker : MonoBehaviour
    {
        public enum State
        {
            Walking,
            Standing,
            Falling
        }

        public enum TileState
        {
            Entering, 
            Quiting,
            Falling
        }

        #region Inspector-----------------------------------------------------------------------------------------------
        public LogChecker LogChecker;
        public GameObject SubDestinationPointer;
        public SmoothTileFollower SmoothTileFollower;

        [Tooltip("Movement speed (units per second).")]
        public float Speed = 2.0f;

        [Tooltip("Maximum elevation difference to consider a tile reachable.")]
        public float MaxElevationThreshold = 0.5f;

        [Tooltip("Maximum angular difference (degrees) to consider a tile reachable.")]
        public float MaxAngleThreshold = 20f;

        public Tile CurrentTile;

        #endregion -----------------------------------------------------------------------------------------------------

        public float CurrentSpeed => Speed * GameSession.Instance.GameSpeed;
        public bool StickToTile { get; set; }
        public TileState CurrentTileState { get; set; }

        private Tile _targetTile;

        private State _currentState;
        
        private StateMachine<State> _stateMachine;

        #region Unity callbacks ----------------------------------------------------------------------------------------
        public void Awake()
        {
            Init(CurrentTile);
            _stateMachine = new StateMachine<State>(this, State.Standing);
        }

        public void Start()
        {
            _stateMachine.GoTo(State.Walking);
        }
        
        private void Update()
        {
            _stateMachine.Update();
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) 
                return;

            // Draw 3 axes (forward, up, right) as arrows
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f);
            Gizmos.DrawSphere(transform.position + transform.forward * 1.5f, 0.1f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);
            Gizmos.DrawSphere(transform.position + transform.up * 1.5f, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * 1.5f);
            Gizmos.DrawSphere(transform.position + transform.right * 1.5f, 0.1f);

            // Red color for falling state
            Gizmos.color = Color.red;
            if(CurrentTileState == TileState.Quiting)
                Gizmos.color = Color.yellow;
            else if(CurrentTileState == TileState.Entering)
                Gizmos.color = Color.green;
            
            Gizmos.DrawSphere(transform.position, 0.2f);

            // Draw SubDestinationPointer position as a wire sphere
            if (SubDestinationPointer != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(SubDestinationPointer.transform.position, 0.2f);
            }
        }
        #endregion -----------------------------------------------------------------------------------------------------

        public void Init(Tile startTile)
        {
            Assert.IsNotNull(startTile);
            SetCurrentTile(startTile);
            transform.position = CurrentTile.gameObject.transform.position;
            transform.rotation = Quaternion.LookRotation(CurrentTile.Forward, CurrentTile.Up);
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
            Vector3 direction = (SubDestinationPointer.transform.localPosition - transform.localPosition).normalized;

            // Calculate the distance to move this frame
            float distanceToMove = CurrentSpeed * Time.deltaTime;

            // Calculate the remaining distance to the destination
            float remainingDistance = Vector3.Distance(transform.localPosition, SubDestinationPointer.transform.localPosition);

            // Check if moving would overshoot the destination
            if (distanceToMove >= remainingDistance)
            {
                // --- On reach tile edge
                if (CurrentTileState == TileState.Quiting)
                {
                    
                    if(StickToTile)
                        Debug.LogWarning($"still rotating, left time: {GetTimeLeftToQuitCurrentTile()}");
                    var tileChanged = StartMoveToNextTile();
                    // if(!tileChanged)
                    //     Debug.Log("waking up state");
                    
                }
                // --- On reach center
                else if (CurrentTileState == TileState.Entering)
                {
                    
                    CurrentTile.GetComponent<TriggerTileReachCenter>()?.HitTriggerReachCenter(this);
                    StartQuitingTile();

                    // Set smooth look direction target
                    if(SmoothTileFollower && !StickToTile)
                    {
                        var nextTile = GetNextTile();
                        if (nextTile)
                        {
                            // Minimum duration to reach next tile center from current tile center (in fact it could be greater but for ERP purposes it's OK)
                            var minDuration = Tile.TileSize / CurrentSpeed;
                            var nextTileCharMovementDirection = GetClosestTileDirection(nextTile, transform.forward);
                            SmoothTileFollower.transform.rotation = Quaternion.LookRotation(GetClosestTileDirection(CurrentTile,transform.forward), CurrentTile.Up);
                            SmoothTileFollower.SetErpTarget(Quaternion.LookRotation(nextTileCharMovementDirection, nextTile.Up), minDuration);
                        }
                        else
                        {
                            SmoothTileFollower.transform.rotation = Quaternion.LookRotation(GetClosestTileDirection(CurrentTile,transform.forward), CurrentTile.Up);
                            SmoothTileFollower.SetErpTarget(Quaternion.LookRotation(transform.forward, CurrentTile.Up), Tile.TileSize * 0.5f / CurrentSpeed);
                        }
                        
                    }
                }
            }

            // Move towards the destination
            transform.localPosition += direction * distanceToMove;
        }
        #endregion

        // public void SetErpForRotation(float duration)
        // {
        //     OnSetErpTarget(Quaternion.LookRotation(transform.forward, CurrentTile.Up), Tile.TileSize * 0.5f / CurrentSpeed);
        // }
        
        private void SetCurrentTile(Tile tile)
        {
            CurrentTile = tile;
            ChangeDirection(Quaternion.LookRotation(GetClosestTileDirection(CurrentTile, transform.forward), CurrentTile.Up));
            transform.SetParent(tile.transform);
            // if(SmoothTileFollower)
            //     SmoothTileFollower.transform.SetParent(tile.transform);
        }

        
        // Note: works only when SubDestinationPointer is set on the exit of the tile
        private Tile GetNextTile()
        {
            var sphereHits = Physics.OverlapSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2);
            
            // if (LogChecker.Gizmos)
            //      GameObjectExtensions.CreateDebugSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2, 1f);

            foreach (var hit in sphereHits)
            {
                var tile = hit.GetComponent<Tile>();
                if (tile == null || tile == CurrentTile)
                    continue;
                return tile;
            }

            return null;
        }

        private bool StartMoveToNextTile()
        {
            // Find next tile
            var sphereHits = Physics.OverlapSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2);

            if (LogChecker.Gizmos)
            {
                // GameObjectExtensions.CreateDebugSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2, 1f);
            }

            foreach (var hit in sphereHits)
            {
                var tile = hit.GetComponent<Tile>();
                if (tile == null || tile == CurrentTile)
                    continue;

                //var walkerEntryDirectionAligned = GetClosestTileDirection(tile, transform.forward);
                
                var prevCurrentTile = CurrentTile;
                SetCurrentTile(tile);
                CurrentTileState = TileState.Entering;
                SetSubdestinationPointer(CurrentTile, tile.transform.position);

                // On attach to a new tile
                {
                    CurrentTile.GetComponent<TileWheelRotation>()
                        ?.RegisterEntering(prevCurrentTile);

                    prevCurrentTile.GetComponent<TriggerTileExit>()
                        ?.HitTriggerExit(this);

                    CurrentTile.GetComponent<TriggerTileEnter>()
                        ?.HitTriggerEnter(this);
                }

                return true;
            }

            return false;
        }

        private void StartQuitingTile()
        {
            Vector3 closestDirection = GetClosestTileDirection(CurrentTile, transform.forward);
            ChangeDirection(Quaternion.LookRotation(closestDirection, CurrentTile.Up));

            CurrentTileState = TileState.Quiting;
            SetSubdestinationPointer(CurrentTile,
                CurrentTile.transform.position + closestDirection * Tile.TileSize * 0.5f);
        }

        private void ChangeDirection(Quaternion q)
        {
            transform.rotation = q;
        }

        public Vector3 GetClosestTileDirection(Tile tile, Vector3 direction)
        {
            Vector3[] directions =
            {
                tile.Forward,
                tile.Right,
                -tile.Right,
                -tile.Forward
            };

            Vector3 closestDirection = directions[0];
            float maxDot = Vector3.Dot(closestDirection, direction);

            // Loop through the remaining directions
            for (int i = 1; i < directions.Length; i++)
            {
                float dot = Vector3.Dot(directions[i], direction);
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

        public float GetTimeLeftToQuitCurrentTile()
        {
            if (CurrentTileState == TileState.Entering)
            {
                var distanceToCenter = Vector3.Distance(SubDestinationPointer.transform.position, transform.position);
                return (distanceToCenter + Tile.TileSize * 0.5f ) / CurrentSpeed;
            }
            else if (CurrentTileState == TileState.Quiting)
            {
                var distanceToEdge = Vector3.Distance(SubDestinationPointer.transform.position, transform.position);
                return distanceToEdge / CurrentSpeed;
            }
            return 0f;
        }
    }

    public static class TileWalkerHelper
    {
       
    }
}