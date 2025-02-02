using System;
using Cysharp.Threading.Tasks;
using GameLib.Alg;
using GameLib.Log;
using MoonSharp.Interpreter;
using NaughtyAttributes;
using uconsole;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Core
{
    public class TriggerBase : MonoBehaviour
    {
        #region Inspector
        [Header("Trigger Base")]
        public LogChecker LogChecker;
        [Tooltip("-1 for infinity")] public int MaxHitCount; // Maximum number of allowed trigger hits (-1 for no limit)
        public bool IsActive; // Determines if the trigger is active
        public UnityEvent Handlers; // Unity events triggered on activation
        [ResizableTextArea] public string ScriptHandler; // Lua script to execute on activation (after Unity events)
        #endregion

        [Tooltip(
            " Example of coroutine script:\n WaitUntil(function() return not tileexist('Tile') end)\n WaitUntil(not tileexist('Tile.002')) ")]
        // Example of one lua script:
        // WaitUntil(function() return not tileexist('Tile') end) -- first wait using function
        // WaitUntil(not tileexist('Tile.002')) -- then wait using boolean value

        public bool RunLuaCoroutine; // Flag to enable running Lua scripts as coroutines

        public int HitCount; // Tracks the current hit count

        private DynValue _luaCoroutine; // Reference to the active Lua coroutine
        private static ulong _functionUID = 0; // Unique ID for Lua functions
        private string _currentLuaFunctionName; // Name of the current Lua function


        public virtual void Reset()
        {
            MaxHitCount = -1;
            IsActive = true;
        }

        void Awake()
        {
            // Validate that MaxHitCount is not invalid
            Assert.IsFalse(MaxHitCount == 0,$"{transform.GetDebugName()}");
            Assert.IsFalse(MaxHitCount < -1, $"{transform.GetDebugName()}");
        }

        protected void Activate()
        {
            // Increment the hit count
            ++HitCount;

            // Handle Lua coroutine execution
            if (RunLuaCoroutine)
            {
                // Ignore if a Lua coroutine is already running
                if (_luaCoroutine != null)
                {
                    LogChecker.PrintWarning(LogChecker.Level.Important,
                        $"Lua coroutine for trigger {gameObject.name} is still running. Ignoring HitTrigger!");
                    return;
                }

                // Invoke Unity event handlers
                Handlers?.Invoke();

                // Create a unique function name for the Lua script
                _currentLuaFunctionName = $"luaCoroutineFunction{++_functionUID}";

                // Inject the Lua script into a function
                ConsoleSystem.Instance.Executor.ExecuteString($@"
                function {_currentLuaFunctionName}()
                    {ScriptHandler}
                end
            ");

                // Create a Lua coroutine from the defined function
                _luaCoroutine = ConsoleSystem.Instance.Executor.Script.CreateCoroutine(
                    ConsoleSystem.Instance.Executor.Script.Globals[_currentLuaFunctionName]);

                // Start the coroutine execution
                _luaCoroutine.Coroutine.Resume();
                ScriptCoroutineProcess().Forget();
            }
            else
            {
                // Invoke Unity event handlers
                Handlers?.Invoke();

                // Directly execute the Lua script without coroutine
                ConsoleSystem.Instance.Executor.ExecuteString(ScriptHandler);
            }
        }

        public void ResetTriggerCount()
        {
            HitCount = 0;
        }

        public bool WillActivate()
        {
            if (!IsActive)
                return false;
            if (MaxHitCount != -1 && HitCount >= MaxHitCount)
                return false;
            if (_luaCoroutine != null)
                return false;
            return true;
        }
        
        public bool Trigger()
        {
            if (!WillActivate())
                return false;
            Activate();
            return true;
        }


        private async UniTask ScriptCoroutineProcess()
        {
            // Keep running while the Lua coroutine is suspended
            while (_luaCoroutine != null && _luaCoroutine.Coroutine.State == CoroutineState.Suspended)
            {
                // Execute the next step of the Lua coroutine
                var result = _luaCoroutine.Coroutine.Resume();

                // If the Lua coroutine yields a number, interpret it as wait time
                if (result.Type == DataType.Number)
                {
                    float waitTime = (float)result.Number;
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime));
                }
                else
                {
                    // If no wait time is specified, continue in the next frame
                    await UniTask.Yield();
                }
            }

            // Log coroutine completion
            if (_luaCoroutine?.Coroutine.State == CoroutineState.Dead)
            {
                LogChecker.Print(LogChecker.Level.Normal, $"Lua coroutine for trigger {gameObject.name} has completed");
            }

            // Clean up the Lua function from the global scope
            ConsoleSystem.Instance.Executor.Script.Globals[_currentLuaFunctionName] = DynValue.Nil;

            // Reset the coroutine reference
            _luaCoroutine = null;
        }
    }
}