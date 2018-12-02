using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludumdare43
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        float walkSpeed;

        [SerializeField]
        float runSpeed;

        [SerializeField]
        Timer runTimer;

        [SerializeField]
        Status health;

        bool isToggleRun;
        bool isTarget;

        Vector2 inputVector;
        Vector2 lastInputVector;

        Vector3 velocity;
        Vector3 lastInputDir;
        Vector3 relativeVector;

        Quaternion targetRotation;
        Rigidbody rigid;


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
        }

        void Initialize()
        {
            rigid = GetComponent<Rigidbody>();
        }

        void InputHandler()
        {
            if (Input.GetButtonDown("ToggleRun"))
                runTimer.Countdown();

            if (inputVector.x != 0.0f || inputVector.y != 0.0f)
                lastInputVector = inputVector;

            inputVector.x = Input.GetAxisRaw("Horizontal");
            inputVector.y = Input.GetAxisRaw("Vertical");

            if (inputVector.magnitude > 1.0f)
                inputVector = inputVector.normalized;
        }

        void MovementHandler()
        {
            if (isToggleRun) {
                velocity.x = (inputVector.x * runSpeed) * Time.fixedDeltaTime;
                velocity.z = (inputVector.y * runSpeed) * Time.fixedDeltaTime;
            }
            else {
                velocity.x = (inputVector.x * walkSpeed) * Time.fixedDeltaTime;
                velocity.z = (inputVector.y * walkSpeed) * Time.fixedDeltaTime;
            }

            rigid.AddForce(velocity, ForceMode.Impulse);
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
            runTimer.OnTimerStart += OnTimerStart;
            runTimer.OnTimerStop += OnTimerStop;
        }

        void UnsubscribeEvents()
        {
            runTimer.OnTimerStart -= OnTimerStart;
            runTimer.OnTimerStop -= OnTimerStop;
        }

        void OnTimerStart()
        {
            isToggleRun = true;
        }

        void OnTimerStop()
        {
            isToggleRun = false;
        }
    }
}
