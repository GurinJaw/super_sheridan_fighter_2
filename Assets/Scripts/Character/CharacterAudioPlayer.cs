using UnityEngine;

public class CharacterAudioPlayer : MonoBehaviour
{
    public enum CharacterSFX
    {
        Damage = 0,
        Fireball = 1,
        Jump = 2,
        Land = 3,
        Lose = 4,
        Taunt = 5
    }

    [SerializeField] private AudioClip[] sfxClips = null;
    private AudioSource myAudioSource = null;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(CharacterSFX _sfx, float? _randomPitchOffset = null)
    {
        if (_randomPitchOffset != null)
        {
            myAudioSource.pitch = Random.Range((1f - _randomPitchOffset.Value), (1f + _randomPitchOffset.Value));
        }

        myAudioSource.PlayOneShot(sfxClips[(int)_sfx]);
    }
}
