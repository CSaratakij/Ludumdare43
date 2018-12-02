using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Ludumdare43
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 3)]
        int playerIndex;

        [SerializeField]
        float walkSpeed;

        [SerializeField]
        float runSpeed;

        [SerializeField]
        float tackleSpeed;

        [SerializeField]
        Timer runTimer;

        [SerializeField]
        Timer runCooldown;

        [SerializeField]
        Timer stuntTimer;

        [SerializeField]
        Timer tackleTimer;

        [SerializeField]
        Timer tackleCooldown;

        [SerializeField]
        Status health;

        public bool IsTarget { get { return isTarget; } }
        public bool IsTackling { get { return isToggleTackle; } }

        bool isToggleRun;
        bool isToggleTackle;
        bool isTarget;
        bool isCarrySomeone; //if carrying someone, get tackle -> throw target up, nagative face dir
        bool isPressedCarry;
        bool isStunt;

        bool isCanTackle = true;
        bool isCanToggleRun = true;

        Vector2 inputVector;
        Vector2 lastInputVector;
        Vector2 tackleDirection;

        Vector3 velocity;
        Vector3 lastInputDir;
        Vector3 relativeVector;

        Quaternion targetRotation;
        Rigidbody rigid;

        Transform carryTarget;


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            //Gizmos.DrawWireCube(transform.position + new Vector3(offset.x, offset.y, 0.0f), size);
            //Handles.Label(transform.position, "Require Coin : " + requireCoin);
        }
#endif
        void Awake()
        {
            Initialize();
            SubscribeEvents();
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
        }

        void Update()
        {
            InputHandler();
        }

        void LateUpdate()
        {
            LookToInputDirection();
        }

        void FixedUpdate()
        {
            MovementHandler();
            CarryPlayerHandler();
        }

        void Initialize()
        {
            rigid = GetComponent<Rigidbody>();
        }

        void InputHandler()
        {
            if (isStunt) {
                inputVector = Vector2.zero;
                return;
            }

            if (inputVector.x != 0.0f || inputVector.y != 0.0f)
                lastInputVector = inputVector;

            inputVector.x = Input.GetAxisRaw("Joy" + playerIndex + "X");
            inputVector.y = Input.GetAxisRaw("Joy" + playerIndex + "Y");

            if (inputVector.x > 0.0f) {
                inputVector.x = 1.0f;
            }
            else if (inputVector.x < 0.0f) {
                inputVector.x = -1.0f;
            }

            if (inputVector.y > 0.0f) {
                inputVector.y = 1.0f;
            }
            else if (inputVector.y < 0.0f) {
                inputVector.y = -1.0f;
            }

            if (inputVector.magnitude > 1.0f)
                inputVector = inputVector.normalized;

            if (isCanToggleRun && Input.GetButtonDown("Joy" + playerIndex + "ToggleRun")) {
                runTimer.Countdown();
            }

            //if only 2 player left -> make remains player a target an exception to use tackle ability
            if (!isTarget && isCanTackle && Input.GetButtonDown("Joy" + playerIndex + "Tackle")) {
                runTimer.Stop();
                tackleTimer.Countdown();
            }

            isPressedCarry = Input.GetButton("Joy" + playerIndex + "Carry");
        }

        void MovementHandler()
        {
            if (isStunt || !isCanTackle) {
                rigid.velocity = Vector3.zero;
                return;
            }

            if (isToggleRun) {
                velocity.x = (inputVector.x * runSpeed) * Time.fixedDeltaTime;
                velocity.z = (inputVector.y * runSpeed) * Time.fixedDeltaTime;
            }
            else if (isToggleTackle) {
                velocity.x = (tackleDirection.x * tackleSpeed) * Time.fixedDeltaTime;
                velocity.z = (tackleDirection.y * tackleSpeed) * Time.fixedDeltaTime;
            }
            else {
                velocity.x = (inputVector.x * walkSpeed) * Time.fixedDeltaTime;
                velocity.z = (inputVector.y * walkSpeed) * Time.fixedDeltaTime;
            }

            rigid.AddForce(velocity, ForceMode.Impulse);
        }

        void CarryPlayerHandler()
        {
            if (isStunt)
                return;

            if (isTarget)
                return;

            //overlap circle,  check if that player is stunt...check if that player is a target, check if this player pressed carry, then carry a target...
            //when target has carried by someone, set rigidbody to kinematic? and follow carry target point, when carrier release, change back to dynamic

            //if catch fail, make player unable to move for a short period of time, If catch pass, make player catch other player immediately (make player movement slower a little bit..)

            //if can catch target -> don't stop on tackle
        }

        void LookToInputDirection()
        {
            lastInputDir.x = lastInputVector.x;
            lastInputDir.y = 0.0f;
            lastInputDir.z = lastInputVector.y;
            
            relativeVector = (lastInputDir + transform.position) - transform.position;

            if (relativeVector != Vector3.zero) {
                targetRotation = Quaternion.LookRotation(relativeVector, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
            }
        }

        void SubscribeEvents()
        {
            runTimer.OnTimerStart += runTimer_OnTimerStart;
            runTimer.OnTimerStop += runTimer_OnTimerStop;

            runCooldown.OnTimerStop += runCooldown_OnTimerStop;

            stuntTimer.OnTimerStart += stuntTimer_OnTimerStart;
            stuntTimer.OnTimerStop += stuntTimer_OnTimerStop;

            tackleTimer.OnTimerStart += tackleTimer_OnTimerStart;
            tackleTimer.OnTimerStop += tackleTimer_OnTimerStop;

            tackleCooldown.OnTimerStop += tackleCooldown_OnTimerStop;
        }

        void UnsubscribeEvents()
        {
            runTimer.OnTimerStart -= runTimer_OnTimerStart;
            runTimer.OnTimerStop -= runTimer_OnTimerStop;

            runCooldown.OnTimerStop -= runCooldown_OnTimerStop;

            stuntTimer.OnTimerStart -= stuntTimer_OnTimerStart;
            stuntTimer.OnTimerStop -= stuntTimer_OnTimerStop;

            tackleTimer.OnTimerStart -= tackleTimer_OnTimerStart;
            tackleTimer.OnTimerStop -= tackleTimer_OnTimerStop;

            tackleCooldown.OnTimerStop -= tackleCooldown_OnTimerStop;
        }

        void runTimer_OnTimerStart()
        {
            isToggleRun = true;
        }

        void runTimer_OnTimerStop()
        {
            isCanToggleRun = false;
            isToggleRun = false;
            runCooldown.Countdown();
        }

        void runCooldown_OnTimerStop()
        {
            isCanToggleRun = true;
        }

        void stuntTimer_OnTimerStart()
        {
            isStunt = true;
        }

        void stuntTimer_OnTimerStop()
        {
            isStunt = false;
        }

        void tackleTimer_OnTimerStart()
        {
            tackleDirection = lastInputVector;
            isToggleTackle = true;
        }

        void tackleTimer_OnTimerStop()
        {
            isCanTackle = false;
            isToggleTackle = false;
            tackleCooldown.Countdown();
        }

        void tackleCooldown_OnTimerStop()
        {
            isCanTackle = true;
        }

        public void SetTarget(bool value)
        {
            isTarget = value;
        }
    }
}
