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
            Awake
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

        public class EventWalkerDetachFromTile
        {
            public TileWalker TileWalker;
            public Tile DetachedTile;
        }

        public class EventWalkerReachTileCenter
        {
            public TileWalker TileWalker;
            public Tile Tile;
        }

        public class EventStartFalling
        {
            public TileWalker TileWalker;
            public float Duration;
        }

        public class EventFall
        {
            public TileWalker TileWalker;
        }

        public class EventFallRecover
        {
            public TileWalker TileWalker;
            public float Duration;
        }


        #region Inspector-----------------------------------------------------------------------------------------------

        public LogChecker LogChecker;
        public bool MoveOnStart;
        public bool InitOnAwake;
        public GameObject SubDestinationPointer;
        public SmoothTileFollower SmoothTileFollower;
        public PlayerController PlayerController;

        [Tooltip("Movement speed (units per second).")]
        public float TargetSpeed = 2.0f;

        public float VelocitySmoothTime = 0.2f;

        [Tooltip("Maximum elevation difference to consider a tile reachable.")]
        public float MaxElevationThreshold = 0.5f;

        [Tooltip("Maximum angular difference (degrees) to consider a tile reachable.")]
        public float MaxAngleThreshold = 20f;

        public Tile CurrentTile;

        #endregion -----------------------------------------------------------------------------------------------------

        public float CurrentSpeed => TargetSpeed * GameSessionController.Instance.GameSpeed;
        public bool StickToTile { get; set; }
        public TileState CurrentTileState { get; set; }

        private Tile _targetTile;

        private StateMachine<State> _stateMachine;
        [ShowNonSerializedField] private float _velocity;
        private float _velocityChangeRate;
        private float _speedBeforeStartFalling;

        // Reuse frequent event
        private static readonly EventWalkerAttachToTile _eventWalkerAttachToTile = new();
        private static readonly EventWalkerDetachFromTile _eventWalkerDetachFromTile = new();
        private static readonly EventWalkerReachTileCenter _eventWalkerReachTileCenter = new();


        #region Unity callbacks ----------------------------------------------------------------------------------------

        public void Awake()
        {
            _stateMachine = new StateMachine<State>(this, State.Standing);
            if (InitOnAwake)
            {
                Assert.IsNotNull(CurrentTile);
                Init(CurrentTile);
            }
        }

        public void Reset()
        {
            TargetSpeed = 2f;
            VelocitySmoothTime = 0.3f;
        }

        public void Start()
        {
            if (MoveOnStart)
                GoToState(State.Walking);
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
            if (CurrentTileState == TileState.Quiting)
                Gizmos.color = Color.yellow;
            else if (CurrentTileState == TileState.Entering)
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
            // Update velocity
            _velocity = Mathf.SmoothDamp(_velocity, TargetSpeed, ref _velocityChangeRate, VelocitySmoothTime);
            var finalVelocity = _velocity * GameSessionController.Instance.GameSpeed;

            // Calculate the direction to the destination
            Vector3 direction = (SubDestinationPointer.transform.localPosition - transform.localPosition).normalized;

            // Calculate the distance to move this frame
            float distanceToMove = finalVelocity * Time.deltaTime;

            // Calculate the remaining distance to the destination
            float remainingDistance =
                Vector3.Distance(transform.localPosition, SubDestinationPointer.transform.localPosition);

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
                        // Note: here we already overshoot the target (SubDestinationPointer on the edge of the tile).
                        // Next step will be move back to the tile center direction then cross the edge again and so on
                        // SmoothFollower will fix wiggling effect. It should happen very rarely due to frame drop 

                        Debug.LogWarning(
                            $"Reach the edge while tile is still rotating. vel:{_velocity} tgSpeed:{TargetSpeed}");
                    }
                    else
                    {
                        var tileChanged = StartMoveToNextTile();
                        if (!tileChanged)
                        {
                            StartFalling();
                        }
                    }
                }
                // On reach center
                else if (CurrentTileState == TileState.Entering)
                {
                    PostWalkerReachTileCenterEvent(this, CurrentTile);
                    StartQuitingTile();
                }
                // On reach falling point
                else if (CurrentTileState == TileState.Falling)
                {
                    // Save check (in case there is some animation that brought the tile under the butt of the character
                    var saveTile = GetNextTile();
                    if (saveTile)
                    {
                        FallRecover(saveTile);
                    }
                    else // Final fall
                    {
                        PostWalkerDetachFromTile(this, CurrentTile);
                        _stateMachine.GoTo(State.Awake);
                    }
                }
            }
        }

        void OnEnterAwake()
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventFall()); // start lowering hovering height on visual
        }

        #endregion

        public void Init(Tile startTile)
        {
            PutOnTile(startTile);
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

        public void DetachFromTile()
        {
            transform.SetParent(null);
            SmoothTileFollower?.transform.SetParent(null);
            PostWalkerDetachFromTile(this, CurrentTile);
            CurrentTile = null;
        }

        public void SetActive(bool flag)
        {
            enabled = flag;
        }

        public void PutOnTile(Tile tile)
        {
            // Set current tile
            Assert.IsNotNull(tile);
            CurrentTile = tile;

            // Set position and parents
            transform.position = CurrentTile.transform.position;
            transform.SetParent(CurrentTile.transform);
            if (SmoothTileFollower)
                SmoothTileFollower.transform.SetParent(CurrentTile.transform);

            // Set rotation to tile forward
            transform.rotation = Quaternion.LookRotation(CurrentTile.Forward, CurrentTile.Up);
            if (SmoothTileFollower)
                SmoothTileFollower.Init(transform.position, transform.rotation);

            PostWalkerAttachToTileEvent(this, null, CurrentTile);
        }

        public void GoToState(State newState)
        {
            _stateMachine.GoTo(newState);
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
            transform.rotation =
                Quaternion.LookRotation(closestDirection,
                    CurrentTile.Up); // For walker: same look direction as a next tile 

            // Set new parents
            transform.SetParent(CurrentTile.transform);
            if (SmoothTileFollower)
            {
                // Reparent SmoothTileFollower rotation pointers
                SmoothTileFollower.ReparentRotationPointers(CurrentTile.transform);
                SmoothTileFollower.transform.SetParent(CurrentTile.transform);
            }

            PostWalkerDetachFromTile(this, prevCurrentTile);
            PostWalkerAttachToTileEvent(this, prevCurrentTile, CurrentTile);
            return true;
        }


        private void StartQuitingTile()
        {
            CurrentTileState = TileState.Quiting;

            // Set SubdestinationPointer 
            Vector3 closestDirection = GetClosestTileDirection(CurrentTile, transform.forward);
            SetSubdestinationPointer(CurrentTile,
                CurrentTile.transform.position + closestDirection * Tile.TileSize * 0.5f);

            // Set rotation
            transform.rotation =
                Quaternion.LookRotation(closestDirection, CurrentTile.Up); // For walker: look to tile exit
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

        private void StartFalling()
        {
            CurrentTileState = TileState.Falling;
            _speedBeforeStartFalling = TargetSpeed;
            TargetSpeed = 0f;
            var fallingPoint = CurrentTile.transform.position +
                               GetClosestTileDirection(CurrentTile, transform.forward) * Tile.TileSize;
            SetSubdestinationPointer(CurrentTile, fallingPoint);
            GlobalEventAggregator.EventAggregator.Publish(new EventStartFalling
            {
                TileWalker = this, Duration = Tile.TileSize / (_speedBeforeStartFalling * GameSessionController.Instance.GameSpeed)
            }); // start lowering hovering height on visual
        }

        private void FallRecover(Tile saveTile)
        {
            CurrentTile = saveTile;
            transform.SetParent(CurrentTile.transform);
            GlobalEventAggregator.EventAggregator.Publish(new EventFallRecover
            {
                TileWalker = this,
                Duration = Tile.TileSize / (_speedBeforeStartFalling * GameSessionController.Instance.GameSpeed)
            });
            StartQuitingTile();
            TargetSpeed = _speedBeforeStartFalling;
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
                return (distanceToCenter + Tile.TileSize * 0.5f) / CurrentSpeed;
            }
            else if (CurrentTileState == TileState.Quiting)
            {
                var distanceToEdge = Vector3.Distance(SubDestinationPointer.transform.position, transform.position);
                return distanceToEdge / CurrentSpeed;
            }

            return 0f;
        }

        public float GetTimeToPassThroughTile() => Tile.TileSize / CurrentSpeed;

        private static void PostWalkerAttachToTileEvent(TileWalker tileWalker, Tile prevTile, Tile currentTile)
        {
            _eventWalkerAttachToTile.TileWalker = tileWalker;
            _eventWalkerAttachToTile.PrevTile = prevTile;
            _eventWalkerAttachToTile.CurrentTile = currentTile;
            GlobalEventAggregator.EventAggregator.Publish(_eventWalkerAttachToTile);
        }

        private static void PostWalkerDetachFromTile(TileWalker tileWalker, Tile detachedTile)
        {
            _eventWalkerDetachFromTile.TileWalker = tileWalker;
            _eventWalkerDetachFromTile.DetachedTile = detachedTile;
            GlobalEventAggregator.EventAggregator.Publish(_eventWalkerDetachFromTile);
        }

        private static void PostWalkerReachTileCenterEvent(TileWalker tileWalker, Tile tile)
        {
            _eventWalkerReachTileCenter.TileWalker = tileWalker;
            _eventWalkerReachTileCenter.Tile = tile;
            GlobalEventAggregator.EventAggregator.Publish(_eventWalkerReachTileCenter);
        }
    }
}