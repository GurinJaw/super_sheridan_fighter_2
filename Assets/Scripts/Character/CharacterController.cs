using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    public delegate void CharacterDamageDelegate(int _playerIndex, int _characterHealth);
    public CharacterDamageDelegate OnTakenDamage = null;

    [SerializeField] private string characterName = "";

    [SerializeField] private GameObject fireOrbPrefab = null;

    [SerializeField] private Transform fireOrbSpawnPos = null;

    private int playerIndex = 0;
    private CharacterAnimator characterAnimator = null;
    private Transform enemyTransform = null;
    private DamageTrigger[] damageTriggers = null;

    private List<GameObject> fireOrbs = null;

    private float takenDamageAt = 0f;
    private float firedAt = 0f;

    private int health = 0;

    private bool locked = true;
    private bool initialized = false;

    private int maxHealth = 0;

    private const float damageCooldown = 0.4f;
    private const float fireballCooldown = 1.5f;

    private const float fireballSpeed = 5f;

    private const int damage = 5;
    private const float movementSpeed = 1.5f;

    #region UNITY
    private void OnDestroy()
    {
        if (!initialized) return;

        UnsubscribeFromEvents();

        StopAllCoroutines();
    }

    private void Update()
    {
        if (!initialized) return;

        if (health <= 0) return;

        ProcessPlayerOrientation();

        if (locked) return;

        ProcessPlayerInput();
    }
    #endregion

    #region PUBLIC API
    /// <summary>
    /// Initializes the character.
    /// </summary>
    /// <param name="_playerIndex">The player index.</param>
    /// <param name="_enemyTransform">The enemy transform.</param>
    /// <param name="_maxHealth">The max health.</param>
    public void InitializeCharacter(int _playerIndex, Transform _enemyTransform, int _maxHealth)
    {
        playerIndex = _playerIndex;
        enemyTransform = _enemyTransform;


        damageTriggers = GetComponentsInChildren<DamageTrigger>();
        characterAnimator = GetComponent<CharacterAnimator>();

        fireOrbs = new List<GameObject>();

        characterAnimator.SetBool(AnimatorParameter.start, true);

        maxHealth = _maxHealth;
        health = maxHealth;

        SubscribeToEvents();
        initialized = true;
    }

    /// <summary>
    /// The character name.
    /// </summary>
    /// <returns></returns>
    public string GetCharacterName()
    {
        return characterName;
    }


    /// <summary>
    /// Locks the character.
    /// </summary>
    /// <param name="_lock"></param>
    public void LockCharacter(bool _lock)
    {
        locked = _lock;
    }

    /// <summary>
    /// Resets the character.
    /// </summary>
    public void ResetCharacter()
    {
        health = maxHealth;
        // Update bar
        characterAnimator.SetBool(AnimatorParameter.lose, false);
        characterAnimator.Play("Idle");
        ResetFireOrbs();
    }

    /// <summary>
    /// Destroys all present fireorbs instantiated by this character.
    /// </summary>
    public void ResetFireOrbs()
    {
        for (int i = 0; i < fireOrbs.Count; i++)
        {
            if (fireOrbs[i] != null)
            {
                Destroy(fireOrbs[i].gameObject);
            }
        }

        fireOrbs.Clear();
    }

    public void ResetAnimator()
    {
        if (health > 0)
            characterAnimator.Play("Idle");

        characterAnimator.SetBool(AnimatorParameter.walk, false);
        characterAnimator.SetBool(AnimatorParameter.crouch, false);
        characterAnimator.SetBool(AnimatorParameter.jump, false);
    }

    /// <summary>
    /// Current player health.
    /// </summary>
    public int CurrentHealth { get { return health; } }
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
        if (health <= 0) return;

        if (_other.tag.Contains(playerIndex.ToString())) return;
        if (!_other.tag.Contains("Damage")) return;

        if (null != _other.GetComponent<FireOrb>()) Destroy(_other.gameObject);

        ProcessDamage();
    }

    void ProcessDamage()
    {
        if (locked) return;

        if (takenDamageAt + damageCooldown > Time.time) return;

        takenDamageAt = Time.time;
        characterAnimator.SetTrigger(AnimatorParameter.hit);
        health = Mathf.Clamp(health - damage, 0, maxHealth);

        if (OnTakenDamage != null)
            OnTakenDamage(playerIndex, health);

        if (health == 0)
        {
            characterAnimator.SetBool(AnimatorParameter.lose, true);
        }
    }

    void ProcessPlayerOrientation()
    {
        int targetAngle = 90;

        if (transform.position.x > enemyTransform.position.x) targetAngle = 270;

        if (transform.rotation.eulerAngles.y == targetAngle) return;

        Vector3 targetRotation = new Vector3(0, targetAngle, 0);
        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, Time.deltaTime * 5f);
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

        // Input for player 2 keyboard debugging.
        if (playerIndex == 1 && moveInput == 0f)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                moveInput = 1f;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                moveInput = -1f;
            }
        }


        if (moveInput == 0f || !characterAnimator.CanMove())
        {
            characterAnimator.SetBool(AnimatorParameter.walk, false);
            return;
        }

        if (moveInput > 0.5f)
        {
            MoveCharacter(1);
        }
        else if (moveInput < -0.5f)
        {
            MoveCharacter(-1);
        }
        else
        {
            characterAnimator.SetBool(AnimatorParameter.walk, false);
        }
    }

    void MoveCharacter(int _direction)
    {
        characterAnimator.SetBool(AnimatorParameter.reverse, _direction * OrientationDirection() < 0);
        characterAnimator.SetBool(AnimatorParameter.walk, true);

        Vector3 currentPos = transform.position;
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
        if (firedAt + fireballCooldown > Time.time) return;

        if (Input.GetButtonDown("Y" + playerIndex))
        {
            firedAt = Time.time;

            characterAnimator.SetTrigger(AnimatorParameter.fire);

            StartCoroutine(FireballRoutine());
        }
    }

    IEnumerator FireballRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        GameObject fireOrb = Instantiate(fireOrbPrefab);
        fireOrb.transform.position = fireOrbSpawnPos.position;
        Rigidbody orbRigidBody = fireOrb.GetComponent<Rigidbody>();
        orbRigidBody.AddForce(Vector3.right * OrientationDirection() * fireballSpeed, ForceMode.Impulse);
        fireOrbs.Add(fireOrb);
    }

    void ProcessTaunt()
    {
        float dpadInput = Input.GetAxis("DPX" + playerIndex);

        // Input for player 2 keyboard debugging.
        if (playerIndex == 1 && dpadInput == 0f)
        {
            if (Input.GetKey(KeyCode.T))
            {
                dpadInput = 1f;
            }
        }

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