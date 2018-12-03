using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludumdare43
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 3)]
        int playerIndex;

        [SerializeField]
        [Range(1, 30)]
        int maxGetup;

        [SerializeField]
        float gravity;

        [SerializeField]
        float breakFreeForce;

        [SerializeField]
        float walkSpeed;

        [SerializeField]
        float runSpeed;

        [SerializeField]
        float carrySomeoneWalkSpeed;

        [SerializeField]
        float carrySomeoneRunSpeed;

        [SerializeField]
        float tackleSpeed;

        [SerializeField]
        Animator anim;

        [SerializeField]
        GameObject model;

        [SerializeField]
        Material[] playerMaterial;

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
        Timer gainInputControlCooldown;

        [SerializeField]
        Status health;

        [SerializeField]
        Transform carryPoint;

        [SerializeField]
        LayerMask targetLayer;

        [SerializeField]
        LayerMask groundLayer;

        [SerializeField]
        Transform groundPoint;

        public int PlayerIndex { get { return playerIndex; } }

        public bool IsTarget { get { return isTarget; } }
        public bool IsTackling { get { return isToggleTackle; } }
        public bool IsPickedUp { get { return isPickedUp; } }
        public bool IsBreakFree { get { return isBreakFree; } }
        public bool IsCarrySomeone { get { return isCarrySomeone; } }

        public PlayerController LastCarrier { get { return lastCarrier; } }
        public PlayerController CarryPlayer { get { return carryPlayer; } }

        public Status Health { get { return health; } }
        public Color Color { get { return playerMaterial[PlayerIndex].color; } }

        bool isToggleRun;
        bool isToggleTackle;
        bool isTarget;
        bool isCarrySomeone;
        bool isStunt;
        bool isPickedUp;
        bool isBreakFree = true;
        bool isThrowSomeone = false;
        bool isHasBeenThrowed = false;

        bool isCanTackle = true;
        bool isCanToggleRun = true;

        int getupProgress;

        Vector2 inputVector;
        Vector2 lastInputVector;

        Vector3 velocity;
        Vector3 lastInputDir;
        Vector3 relativeVector;
        Vector3 throwDirection;

        Quaternion targetRotation;

        Rigidbody rigid;
        Rigidbody carryRigid;

        Transform carryTarget;

        PlayerController lastCarrier;
        PlayerController carryPlayer;

        Collider[] hits;


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
            AnimationHandler();
        }

        void LateUpdate()
        {
            LookToInputDirection();
            CarrySomeoneHandler();
        }

        void FixedUpdate()
        {
            MovementHandler();
            ReceiveThrowHandler();
        }

        void Initialize()
        {
            rigid = GetComponent<Rigidbody>();
            model.GetComponent<Renderer>().material = playerMaterial[playerIndex];
        }

        void InputHandler()
        {
            if (!GameController.IsGameStart)
                return;

            if (isHasBeenThrowed)
                return;

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

            if (isCanTackle && !isCarrySomeone && Input.GetButtonDown("Joy" + playerIndex + "Tackle")) {
                runTimer.Stop();
                tackleTimer.Countdown();
            }

            if (isCarrySomeone && Input.GetButtonDown("Joy" + playerIndex + "Throw")) {
                isCarrySomeone = false;
                isThrowSomeone = true;
            }

            if (isPickedUp)
            {
                bool isPressedGetUp = Input.GetButtonUp("Joy" + playerIndex + "GetUp");

                if (isPressedGetUp) {
                    if (getupProgress < maxGetup) {
                        getupProgress += 1;
                        anim.SetTrigger("strugle");
                    }
                    else {
                        getupProgress = 0;
                        isPickedUp = false;
                        Pickup(this, false);
                    }
                }
            }
        }

        void AnimationHandler()
        {
            if (inputVector.x != 0.0f || inputVector.y != 0.0f) {
                anim.SetFloat("Speed", (isToggleRun) ? 2.0f : 1.0f);
            }
            else {
                anim.SetFloat("Speed", 0.0f);
            }

            anim.SetBool("holdingTarget", isCarrySomeone);
            anim.SetBool("IsCanTackle", isCanTackle);
            anim.SetBool("IsPickedUp", isPickedUp);
            anim.SetBool("IsToggleTackle", isToggleTackle);
        }

        void MovementHandler()
        {
            if (isStunt || (!isCanTackle && !isCarrySomeone)) {
                rigid.velocity = Vector3.zero;
                return;
            }

            if (isCarrySomeone)
            {
                if (isToggleRun) {
                    velocity.x = (inputVector.x * carrySomeoneRunSpeed) * Time.fixedDeltaTime;
                    velocity.z = (inputVector.y * carrySomeoneRunSpeed) * Time.fixedDeltaTime;
                }
                else if (isToggleTackle) {
                    velocity.x = (transform.forward.x * tackleSpeed * 0.8f) * Time.fixedDeltaTime;
                    velocity.z = (transform.forward.z * tackleSpeed * 0.8f) * Time.fixedDeltaTime;
                }
                else {
                    velocity.x = (inputVector.x * carrySomeoneWalkSpeed) * Time.fixedDeltaTime;
                    velocity.z = (inputVector.y * carrySomeoneWalkSpeed) * Time.fixedDeltaTime;
                }
            }
            else
            {
                if (isToggleRun) {
                    velocity.x = (inputVector.x * runSpeed) * Time.fixedDeltaTime;
                    velocity.z = (inputVector.y * runSpeed) * Time.fixedDeltaTime;
                }
                else if (isToggleTackle) {
                    velocity.x = (transform.forward.x * tackleSpeed) * Time.fixedDeltaTime;
                    velocity.z = (transform.forward.z * tackleSpeed) * Time.fixedDeltaTime;
                }
                else {
                    velocity.x = (inputVector.x * walkSpeed) * Time.fixedDeltaTime;
                    velocity.z = (inputVector.y * walkSpeed) * Time.fixedDeltaTime;
                }
            }

            if (isHasBeenThrowed) {

                velocity.x = (throwDirection.x * walkSpeed) * Time.fixedDeltaTime;
                velocity.z = (throwDirection.z * walkSpeed) * Time.fixedDeltaTime;

                velocity.y = rigid.velocity.y + (throwDirection.y * walkSpeed) * Time.fixedDeltaTime;
                velocity.y = rigid.velocity.y + (-gravity * Time.fixedDeltaTime);

                rigid.velocity = velocity;
            }
            else {
                velocity.y = rigid.velocity.y + (-gravity * Time.fixedDeltaTime);
                rigid.velocity = velocity;
            }
        }

        void ReceiveThrowHandler()
        {
            if (!isHasBeenThrowed)
                return;

            bool isGround = Physics.BoxCast(groundPoint.position, new Vector3(3.0f, 1.0f, 3.0f), Vector3.down, Quaternion.identity, 1.0f, groundLayer);

            if (isGround && !gainInputControlCooldown.IsStart) {
                gainInputControlCooldown.Countdown();
            }
        }

        void LookToInputDirection()
        {
            if (isStunt)
                return;

            if (isToggleTackle) {
                return;
            }

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

        void CarrySomeoneHandler()
        {
            if (carryPlayer == null || carryRigid == null || carryTarget == null)
                return;

            if (isCarrySomeone && !carryPlayer.IsBreakFree) {
                carryTarget.position = carryPoint.position;
                carryTarget.Rotate(Vector3.up * 20.0f * Time.deltaTime, Space.World);
            }
            else {
                isCarrySomeone = false;

                if (isThrowSomeone) {
                    var direction = (transform.forward * 2.0f) + (Vector3.up * 0.8f);
                    var velocity = (direction * breakFreeForce) * Time.fixedDeltaTime;

                    carryRigid.velocity = velocity;
                    carryPlayer.MarkThrowed(true, direction);

                    UnPickOldOne();
                    isThrowSomeone = false;
                }
                else {
                    carryRigid.velocity = (Vector3.up * breakFreeForce) * Time.fixedDeltaTime;
                }

                carryPlayer = null;
                carryTarget = null;
                carryRigid = null;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player")) {
                PlayerController collidePlayer = collision.gameObject.GetComponent<PlayerController>();

                if (collidePlayer == null)
                    return;

                if (isToggleTackle && !isCarrySomeone && !collidePlayer.IsTackling) {
                    if (collidePlayer.IsCarrySomeone) {
                        var p = collidePlayer.CarryPlayer;
                        collidePlayer.UnPickOldOne();
                        Pickup(p, true);
                        collidePlayer.Stunt();
                    }
                    else {
                        UnPickOldOne();
                        Pickup(collidePlayer, true);
                    }
                }
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
            gainInputControlCooldown.OnTimerStop += gainInputControlCooldown_OnTimerStop;
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
            gainInputControlCooldown.OnTimerStop += gainInputControlCooldown_OnTimerStop;
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
            isToggleTackle = true;
            anim.SetTrigger("Tackle");
        }

        void tackleTimer_OnTimerStop()
        {
            isToggleTackle = false;
            isCanTackle = false;

            tackleCooldown.Countdown();
        }

        void tackleCooldown_OnTimerStop()
        {
            isCanTackle = true;
        }

        void gainInputControlCooldown_OnTimerStop()
        {
            isHasBeenThrowed = false;
        }

        public void SetTarget(bool value)
        {
            isTarget = value;
        }

        public void Stunt()
        {
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

            if (value) {
                carryTarget.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
                carryPlayer.SetLastCarrier(this);
            }

            target.MarkPickup(value);
        }

        public void UnPickOldOne()
        {
            if (carryPlayer == null)
                return;

            Pickup(carryPlayer, false);
        }

        public void MarkPickup(bool value)
        {
            isPickedUp = value;
            isBreakFree = !value;

            if (value) {
                getupProgress = 0;
            }
            else {
                lastInputDir = Vector3.zero;
            }
        }

        public void MarkThrowed(bool value, Vector3 direction)
        {
            if (value) {
                inputVector = Vector2.zero;
                throwDirection = direction;
            }

            isHasBeenThrowed = value;
        }

        public void ClearThrow()
        {
            inputVector = Vector2.zero;
            isHasBeenThrowed = false;
        }

        public void SetLastCarrier(PlayerController carrier)
        {
            lastCarrier = carrier;
        }
    }
}
