using UnityEngine;

public enum AnimatorParameter
{
    punch = 0,
    crouch = 1,
    walk = 2,
    kick = 3,
    reverse = 4,
    taunt = 5,
    block = 6,
    jump = 7,
    fire = 8,
    hit = 9,
    lose = 10,
    start = 11
}

public class CharacterAnimator : MonoBehaviour
{

    private Animator myAnimator = null;
    private string[] animParameterStrings = { "punch", "crouch", "walk", "kick", "reverse", "taunt", "block", "jump", "fire", "hit", "lose", "start" };

    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    #region PUBLIC API

    public void SetBool(AnimatorParameter _parameter, bool _val)
    {
        myAnimator.SetBool(animParameterStrings[(int)_parameter], _val);
    }

    public void SetTrigger(AnimatorParameter _parameter)
    {
        myAnimator.SetTrigger(animParameterStrings[(int)_parameter]);
    }

    public bool CanMove()
    {
        bool canMove = true;

        AnimatorClipInfo[] currentClip = myAnimator.GetCurrentAnimatorClipInfo(0);
        string clipName = currentClip[0].clip.name;

        if (clipName != "Fighting Idle" && clipName != "Crouch" && clipName != "Walking" && clipName != "Crouch_Walk" && clipName != "Walk Back" && clipName != "Jump")
        {
            canMove = false;
        }

        return canMove;
    }

    #endregion
}
