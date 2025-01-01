using GameLib;
using GameLib.Log;
using NaughtyAttributes;
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

        public class EventWalkerAttachToTile
        {
            public TileWalker TileWalker;
            public Tile PrevTile;
            public Tile CurrentTile;
        }
        
        public class EventWalkerReachTileCenter
        {
            public TileWalker TileWalker;
            public Tile Tile;
        }

        #region Inspector-----------------------------------------------------------------------------------------------
        public LogChecker LogChecker;
        public bool MoveOnStart;
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

        private StateMachine<State> _stateMachine;
        private float _targetSpeed;
        
        // Reuse frequent event
        private static readonly EventWalkerAttachToTile _eventWalkerAttachToTile = new EventWalkerAttachToTile();
        private static readonly EventWalkerReachTileCenter _eventWalkerReachTileCenter = new EventWalkerReachTileCenter();
        

        #region Unity callbacks ----------------------------------------------------------------------------------------
        public void Awake()
        {
            Assert.IsNotNull(CurrentTile);
            Init(CurrentTile);
            _stateMachine = new StateMachine<State>(this, State.Standing);
        }

        public void Start()
        {
            if(MoveOnStart)
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

        #region State machine ------------------------------------------------------------------------------------------
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
            
            // Move towards the destination
            transform.localPosition += direction * distanceToMove;

            // Check if moving would overshoot the destination
            if (distanceToMove >= remainingDistance)
            {
                //  On reach tile edge
                if (CurrentTileState == TileState.Quiting)
                {
                    if (StickToTile)
                    {
                        Debug.LogWarning($"still rotating, left time: {GetTimeLeftToQuitCurrentTile()}");
                    }
                    else
                    {
                        var tileChanged = StartMoveToNextTile();
                        // if(!tileChanged)
                        //     Debug.Log("waking up state");
                    }
                }
                // On reach center
                else if (CurrentTileState == TileState.Entering)
                {
                    PostWalkerReachTileCenterEvent(this, CurrentTile);
                    StartQuitingTile();
                }
            }
        }
        #endregion
        
        public void Init(Tile startTile)
        {
            // Set current tile
            Assert.IsNotNull(startTile);
            CurrentTile = startTile;
            
            // Set position and parents
            transform.position = CurrentTile.transform.position;
            transform.SetParent(CurrentTile.transform);
            if(SmoothTileFollower)
                SmoothTileFollower.transform.SetParent(CurrentTile.transform);
            
            // Set rotation to tile forward
            transform.rotation = Quaternion.LookRotation(CurrentTile.Forward, CurrentTile.Up);
            if (SmoothTileFollower)
                SmoothTileFollower.Init(transform.position, transform.rotation);
        }
        
        private Tile GetNextTile()
        {
            var sphereHits = Physics.OverlapSphere(SubDestinationPointer.transform.position, Tile.TileSize / 2);
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
            var nextTile = GetNextTile();

            if (!nextTile) 
                return false;
            
            var prevCurrentTile = CurrentTile;
            CurrentTile = nextTile;
            CurrentTileState = TileState.Entering;
                
            SetSubdestinationPointer(CurrentTile, CurrentTile.transform.position);
            
            Vector3 closestDirection = GetClosestTileDirection(CurrentTile, transform.forward);
            transform.rotation = Quaternion.LookRotation(closestDirection, CurrentTile.Up); // For walker: same look direction as a next tile 
                
            // Set new parents
            transform.SetParent(CurrentTile.transform);
            if (SmoothTileFollower)
            {
                // Reparent SmoothTileFollower rotation pointers
                SmoothTileFollower.ReparentRotationPointers(CurrentTile.transform);
                SmoothTileFollower.transform.SetParent(CurrentTile.transform);
            }

            PostWalkerAttachToTileEvent(this, prevCurrentTile, CurrentTile);
            return true;
        }


        private void StartQuitingTile()
        {
            CurrentTileState = TileState.Quiting;
            
            // Set SubdestinationPointer 
            Vector3 closestDirection = GetClosestTileDirection(CurrentTile, transform.forward);
            SetSubdestinationPointer(CurrentTile, CurrentTile.transform.position + closestDirection * Tile.TileSize * 0.5f);
            
            // Set rotation
            transform.rotation = Quaternion.LookRotation(closestDirection, CurrentTile.Up); // For walker: look to tile exit
            if (SmoothTileFollower && !StickToTile)
            {
                // For SmoothTileFollower we need to set TargetRotation to next tile rotation
                var nextTile = GetNextTile();
                if (nextTile)
                {
                    // Minimum duration to reach next tile center from current tile center (in fact it could be greater but for ERP purposes it's OK)
                    var minDuration = Tile.TileSize / CurrentSpeed;
                    var nextTileCharMovementDirection = GetClosestTileDirection(nextTile, transform.forward);
                    SmoothTileFollower.SetInterpolationSegment(
                        Quaternion.LookRotation(nextTileCharMovementDirection, nextTile.Up),
                        CurrentTile.transform,
                        nextTile.transform.position, 
                        minDuration);
                }
                else
                {
                    // Probably we're going to fall, set as if the next tile has same rotation (don't need to update TargetRotation)
                }
                
            }
        }
        
        public void RecalculateSmoothRotationTarget()
        {
            if (!SmoothTileFollower)
                return;
            
            var nextTile = GetNextTile();
            if (nextTile)
            {
                // Minimum duration to reach next tile center from current tile center (in fact it could be greater but for ERP purposes it's OK)
                var minDuration = Tile.TileSize * 0.5f / CurrentSpeed;
                var nextTileCharMovementDirection = GetClosestTileDirection(nextTile, transform.forward);
                SmoothTileFollower.SetInterpolationSegment(
                    Quaternion.LookRotation(nextTileCharMovementDirection, nextTile.Up),
                    CurrentTile.transform,
                    nextTile.transform.position, 
                    minDuration);
            }

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

        private void SetSubdestinationPointer(Tile parent, Vector3 position)
        {
            SubDestinationPointer.transform.position = position;
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
        
        private static void PostWalkerAttachToTileEvent(TileWalker tileWalker, Tile prevTile, Tile currentTile)
        {
            _eventWalkerAttachToTile.TileWalker = tileWalker;
            _eventWalkerAttachToTile.PrevTile = prevTile;
            _eventWalkerAttachToTile.CurrentTile = currentTile;
            GlobalEventAggregator.EventAggregator.Publish(_eventWalkerAttachToTile);
        }

        private static void PostWalkerReachTileCenterEvent(TileWalker tileWalker, Tile tile)
        {
            _eventWalkerReachTileCenter.TileWalker = tileWalker;
            _eventWalkerReachTileCenter.Tile = tile;
            GlobalEventAggregator.EventAggregator.Publish(_eventWalkerReachTileCenter);
        }
    }
}