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
        [SerializeField] private Sprite[] characterSprites = new Sprite[4]; // Array of 4 character sprites
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private Rigidbody2D _rb;
        private Collider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        
        // Character switching
        private int currentCharacterIndex = 0;

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
            
            // Get SpriteRenderer if not assigned
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
            HandleCharacterSwitch();
        }

        private void GatherInput()
        {
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;
            
            bool jumpDown = false;
            bool jumpHeld = false;
            Vector2 moveInput = Vector2.zero;
            
            // Keyboard input
            if (keyboard != null)
            {
                jumpDown = keyboard.spaceKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame;
                jumpHeld = keyboard.spaceKey.isPressed || keyboard.cKey.isPressed;
                
                float horizontal = 0f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
                
                float vertical = 0f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
                
                moveInput = new Vector2(horizontal, vertical);
            }
            
            // Gamepad input (if available)
            if (gamepad != null)
            {
                jumpDown = jumpDown || gamepad.buttonSouth.wasPressedThisFrame;
                jumpHeld = jumpHeld || gamepad.buttonSouth.isPressed;
                
                Vector2 gamepadMove = gamepad.leftStick.ReadValue();
                if (gamepadMove.magnitude > moveInput.magnitude)
                {
                    moveInput = gamepadMove;
                }
            }
            
            _frameInput = new FrameInput
            {
                JumpDown = jumpDown,
                JumpHeld = jumpHeld,
                Move = moveInput
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

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
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

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;
        
        #region Character Switching
        
        private void HandleCharacterSwitch()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            
            // Check for Shift key press
            if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
            {
                SwitchToNextCharacter();
            }
        }
        
        private void SwitchToNextCharacter()
        {
            // Cycle through characters
            currentCharacterIndex = (currentCharacterIndex + 1) % characterSprites.Length;
            
            // Change sprite if available
            if (characterSprites[currentCharacterIndex] != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = characterSprites[currentCharacterIndex];
                Debug.Log($"Switched to character {currentCharacterIndex + 1}");
            }
        }
        
        public void SetCharacter(int characterIndex)
        {
            if (characterIndex >= 0 && characterIndex < characterSprites.Length)
            {
                currentCharacterIndex = characterIndex;
                if (characterSprites[currentCharacterIndex] != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = characterSprites[currentCharacterIndex];
                }
            }
        }
        
        public int GetCurrentCharacterIndex()
        {
            return currentCharacterIndex;
        }
        
        #endregion

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