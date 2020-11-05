using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private int playerIndex = 0;
    private CharacterAnimator characterAnimator = null;
    private Transform enemyTransform = null;
    private DamageTrigger[] damageTriggers = null;

    private float takenDamageAt = 0f;
    private float damageCooldown = 0.4f;

    private const float movementSpeed = 1.5f;

    #region UNITY
    private void Start()
    {
        InitializeCharacter(0, null);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        ProcessPlayerInput();
    }
    #endregion

    #region PUBLIC API

    public void InitializeCharacter(int _playerIndex, Transform _enemyTransform)
    {
        playerIndex = _playerIndex;
        enemyTransform = _enemyTransform;

        damageTriggers = GetComponentsInChildren<DamageTrigger>();
        characterAnimator = GetComponent<CharacterAnimator>();
        SubscribeToEvents();
    }

    #endregion

    #region PRIVATE
    void SubscribeToEvents()
    {
        SubscribeToTriggerEvents();
    }

    void UnsubscribeFromEvents()
    {
        UnsubscribeFromTriggerEvents();
    }

    void SubscribeToTriggerEvents()
    {
        for (int i = 0; i < damageTriggers.Length; i++)
        {
            damageTriggers[i].OnTrigger += OnTrigger;
        }
    }

    void UnsubscribeFromTriggerEvents()
    {
        if (damageTriggers == null) return;

        for (int i = 0; i < damageTriggers.Length; i++)
        {
            damageTriggers[i].OnTrigger -= OnTrigger;
        }
    }

    void OnTrigger(Collider _other)
    {
        if (_other.tag.Contains(playerIndex.ToString())) return;
        if (!_other.tag.Contains("Damage")) return;

        Debug.Log("Trigger enter! " + _other.tag);
        ProcessDamage();
    }

    void ProcessDamage()
    {
        if (takenDamageAt + damageCooldown > Time.time) return;

        takenDamageAt = Time.time;
        characterAnimator.SetTrigger(AnimatorParameter.hit);
        // TODO: Calculate damage.
    }

    #region INPUT
    void ProcessPlayerInput()
    {
        ProcessMovement();
        ProcessJump();
        ProcessBlock();
        ProcessCrouch();
        ProcessPunch();
        ProcessKick();
        ProcessFireball();
        ProcessTaunt();
    }

    void ProcessMovement()
    {
        float moveInput = Input.GetAxis("LS_X" + playerIndex);

        if (moveInput == 0f || !characterAnimator.CanMove())
        {
            characterAnimator.SetBool(AnimatorParameter.walk, false);
            return;
        }

        if (moveInput > 0.5f)
        {
            MovePlayer(1);
        }
        else if (moveInput < -0.5f)
        {
            MovePlayer(-1);
        }
        else
        {
            characterAnimator.SetBool(AnimatorParameter.walk, false);
        }
    }

    void MovePlayer(int _direction)
    {
        characterAnimator.SetBool(AnimatorParameter.reverse, _direction * OrientationDirection() < 0);
        characterAnimator.SetBool(AnimatorParameter.walk, true);

        Vector2 currentPos = transform.position;
        currentPos.x += movementSpeed * _direction * Time.deltaTime;
        transform.position = currentPos;
    }

    void ProcessJump()
    {
        if (Input.GetButtonDown("A" + playerIndex))
        {
            characterAnimator.SetBool(AnimatorParameter.jump, true);
        }
        else if (Input.GetButtonUp("A" + playerIndex))
        {
            characterAnimator.SetBool(AnimatorParameter.jump, false);
        }
    }

    void ProcessBlock()
    {
        if (Input.GetButtonDown("RShoulder1" + playerIndex))
        {
            characterAnimator.SetTrigger(AnimatorParameter.block);
        }
    }

    void ProcessCrouch()
    {
        characterAnimator.SetBool(AnimatorParameter.crouch, Input.GetButton("LShoulder1" + playerIndex));
    }

    void ProcessPunch()
    {
        if (Input.GetButtonDown("B" + playerIndex))
        {
            characterAnimator.SetTrigger(AnimatorParameter.punch);
        }
    }

    void ProcessKick()
    {
        if (Input.GetButtonDown("X" + playerIndex))
        {
            characterAnimator.SetTrigger(AnimatorParameter.kick);
        }
    }

    void ProcessFireball()
    {
        if (Input.GetButtonDown("Y" + playerIndex))
        {
            characterAnimator.SetTrigger(AnimatorParameter.fire);
        }
    }

    void ProcessTaunt()
    {
        float dpadInput = Input.GetAxis("DPX" + playerIndex);

        if (dpadInput == 0) return;

        characterAnimator.SetTrigger(AnimatorParameter.taunt);
    }
    #endregion

    int OrientationDirection()
    {
        int direction = 1;

        if (transform.rotation.eulerAngles.y > 180) direction = -1;

        return direction;
    }
    #endregion
}