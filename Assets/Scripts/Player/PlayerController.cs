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

        [SerializeField]
        Transform carryPoint;

        [SerializeField]
        LayerMask targetLayer;

        public bool IsTarget { get { return isTarget; } }
        public bool IsTackling { get { return isToggleTackle; } }
        public bool IsPickedUp { get { return isPickedUp; } }

        bool isToggleRun;
        bool isToggleTackle;
        bool isTarget;
        bool isCarrySomeone;
        bool isStunt;
        bool isPickedUp;

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
        PlayerController carryPlayer;
        Rigidbody carryRigid;

        Collider[] hits;


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + transform.forward * 3.2f, new Vector3(1.0f, 0.5f, 1.5f) * 2.0f);
        }
#endif
        void Awake()
        {
            Initialize();
            SubscribeEvents();
        }

        //Test
        void Start()
        {
            if (playerIndex == 0)
                SetTarget(true);
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
        }

        void Update()
        {
            InputHandler();

            if (!isCanTackle && !isCarrySomeone && !isStunt)
                stuntTimer.Countdown();
        }

        void LateUpdate()
        {
            LookToInputDirection();
            CarrySomeone();
        }

        void FixedUpdate()
        {
            MovementHandler();
            TacklePlayerHandler();
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

            if (isCanTackle && Input.GetButtonDown("Joy" + playerIndex + "Tackle")) {
                runTimer.Stop();
                tackleTimer.Countdown();
            }
        }

        void MovementHandler()
        {
            if (isStunt) {
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
            rigid.AddForce(Vector3.up * -400 * Time.deltaTime, ForceMode.Force);
        }

        void TacklePlayerHandler()
        {
            if (isStunt)
                return;

            if (isTarget)
                return;

            if (!isToggleTackle)
                return;

            hits = Physics.OverlapBox(rigid.position + Vector3.forward * 3.0f, new Vector3(1.0f, 0.5f, 1.5f), Quaternion.identity, targetLayer);

            foreach (Collider collider in hits)
            {
                PlayerController player = collider.GetComponent<PlayerController>();

                if (player == null)
                    continue;

                if (!player.IsTarget)
                    continue;

                if (player.IsPickedUp)
                    continue;

                player.Stunt();
                Pickup(player, true);
            }

            //if catch fail, make player unable to move for a short period of time, If catch pass, make player catch other player immediately (make player movement slower a little bit..)

            //if can catch target -> don't stop on tackle
            //if player 1 and player 2 has an opposite forward vector to each other when tackle, (cancel out)
        }

        void LookToInputDirection()
        {
            if (isStunt)
                return;

            if (isPickedUp) {
                return;
            }

            lastInputDir.x = lastInputVector.x;
            lastInputDir.y = 0.0f;
            lastInputDir.z = lastInputVector.y;
            
            relativeVector = (lastInputDir + transform.position) - transform.position;

            if (relativeVector != Vector3.zero) {
                targetRotation = Quaternion.LookRotation(relativeVector, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
            }
        }

        void CarrySomeone()
        {
            if (carryPlayer == null || carryRigid == null || carryTarget == null)
                return;

            if (isCarrySomeone && carryPlayer.isTarget) {
                carryTarget.position = carryPoint.position;
                carryTarget.Rotate(Vector3.up * 60.0f * Time.deltaTime, Space.World);
            }
            else {
                Pickup(carryPlayer, false);
                carryRigid.AddForce(Vector3.up * 800.0f * Time.deltaTime, ForceMode.Impulse);

                carryPlayer = null;
                carryTarget = null;
                carryRigid = null;
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
            //Test
            isCarrySomeone = false;
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
            //check if hit target, if hit target -> don't stunt
            /*
            if (!isCarrySomeone) {
                isStunt = true;
                stuntTimer.Countdown();
            }
            */

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

        public void Stunt()
        {
            isCarrySomeone = false;
            stuntTimer.Reset();
            stuntTimer.Countdown();
        }

        public void Pickup(PlayerController target, bool value)
        {
            isCarrySomeone = value;

            carryPlayer = target;
            carryTarget = target.transform;
            carryRigid = target.rigid;

            carryRigid.isKinematic = value;
            target.GetComponent<CapsuleCollider>().enabled = !value;

            if (value)
                carryTarget.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

            target.MarkPickup(value);

            /*
            else
                carryTarget.rotation = Quaternion.Euler(Vector3.zero); //need button mash to get up
            */
        }

        public void MarkPickup(bool value)
        {
            isPickedUp = value;

            if (!value)
                lastInputDir = Vector3.zero;
        }
    }
}
