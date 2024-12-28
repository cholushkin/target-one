using GameLib;
using GameLib.Alg;
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

        public float CurrentSpeed => Speed * GameSession.Instance.GameSpeed;


        private Tile _targetTile;

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

            // Draw the body as a solid sphere
            Gizmos.color = _quiting ? Color.red : Color.green;
            Gizmos.DrawSphere(transform.position, 0.2f);

            // Draw SubDestinationPointer position as a wire sphere
            if (SubDestinationPointer != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(SubDestinationPointer.transform.position, 0.2f);
            }
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
            transform.rotation =
                Quaternion.LookRotation(GetClosestTileDirection(CurrentTile, transform.forward), CurrentTile.Up);
            transform.SetParent(tile.transform);
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
            float distanceToMove = CurrentSpeed * Time.deltaTime;

            // Calculate the remaining distance to the destination
            float remainingDistance = Vector3.Distance(transform.position, SubDestinationPointer.transform.position);

            // Check if moving would overshoot the destination
            if (distanceToMove >= remainingDistance)
            {
                if (_quiting)
                {
                    // On reach tile edge
                    var tileChanged = StartMoveToNextTile();
                    // if(!tileChanged)
                    //     Debug.Log("waking up state");
                    
                }
                else
                {
                    // On reach center
                    CurrentTile.GetComponent<TriggerTileReachCenter>()?.HitTriggerReachCenter(this);
                    StartQuitingTile();
                }
            }

            // Move towards the destination
            transform.position += direction * distanceToMove;
        }



        #endregion

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

                var tileDirection = GetClosestTileDirection(tile, transform.forward);
                

                _quiting = false;
                var prevCurrentTile = CurrentTile;
                SetCurrentTile(tile);
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
            transform.rotation = Quaternion.LookRotation(closestDirection, CurrentTile.Up);

            _quiting = true;
            SetSubdestinationPointer(CurrentTile,
                CurrentTile.transform.position + closestDirection * Tile.TileSize * 0.5f);
        }

        private Vector3 GetClosestTileDirection(Tile tile, Vector3 direction)
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

        internal float GetTimeToTileCenter()
        {
            if (_quiting) 
                return 0f;
            var distance = Vector3.Distance(SubDestinationPointer.transform.position, transform.position);
            return distance / CurrentSpeed;
        }

        internal float GetTimeToTileEdge()
        {
            return Tile.TileSize * 0.5f / CurrentSpeed;
        }
        
    }

    public static class TileWalkerHelper
    {
        public static float GetTimeLeftToQuitCurrentTile(this TileWalker tileWalker)
        {
            return tileWalker.GetTimeToTileCenter() + tileWalker.GetTimeToTileEdge();
        }
    }
}