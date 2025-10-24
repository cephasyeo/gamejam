using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private PlayerOrbManager orbManager;
        [SerializeField] private PlayerInputHandler inputHandler;
        
        private Rigidbody2D _rb;
        private Collider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            
            // Freeze rotation to prevent rolling
            _rb.freezeRotation = true;
            
            
            // Get PlayerOrbManager if not assigned
            if (orbManager == null)
                orbManager = GetComponent<PlayerOrbManager>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            if (inputHandler == null) return;
            
            _frameInput = new FrameInput
            {
                JumpDown = inputHandler.GetJumpPressed(),
                JumpHeld = inputHandler.GetJumpHeld(),
                Move = inputHandler.GetMoveInput()
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();
            
            ApplyMovement();
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.BoxCast(_col.bounds.center, _col.bounds.size, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.BoxCast(_col.bounds.center, _col.bounds.size, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                
                // Reset air jumps and dashes when landing
                if (orbManager != null)
                {
                    orbManager.ResetAirJumps();
                    orbManager.ResetDashCount();
                }
                
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            // Allow jumping if grounded, can use coyote, OR if player has air jumps remaining
            bool canJump = _grounded || CanUseCoyote;
            
            // Check if player has air jumps remaining (only if not grounded and not coyote)
            if (!canJump && orbManager != null && orbManager.CanAirJump())
            {
                canJump = true;
            }

            if (canJump) 
            {
                if (orbManager != null && orbManager.debugMode)
                {
                    Debug.Log($"Jumping! Grounded: {_grounded}, Coyote: {CanUseCoyote}, Air jumps: {orbManager.GetRemainingAirJumps()}");
                }
                ExecuteJump();
            }

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            
            // Calculate jump power based on orb abilities
            float jumpPower = _stats.JumpPower;
            if (orbManager != null && orbManager.CanJump())
            {
                jumpPower *= orbManager.GetJumpPowerMultiplier();
            }
            
            // Consume air jump if not grounded
            if (!_grounded && orbManager != null)
            {
                orbManager.ConsumeAirJump();
            }
            
            _frameVelocity.y = jumpPower;
            Jumped?.Invoke();
            
            // Play jump SFX
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJumpSFX();
            }
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            // Don't handle horizontal movement during dash
            if (orbManager != null && orbManager.IsDashing())
            {
                return; // Let PlayerOrbManager handle horizontal movement during dash
            }
            
            
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            // Don't apply gravity during dash
            if (orbManager != null && orbManager.IsDashing())
            {
                return; // Let PlayerOrbManager handle movement during dash
            }
            
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        
        private void ApplyMovement() 
        {
            // Don't override velocity if player is dashing
            if (orbManager != null && orbManager.IsDashing())
            {
                return; // Let PlayerOrbManager handle velocity during dash
            }
            
            
            _rb.linearVelocity = _frameVelocity;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}