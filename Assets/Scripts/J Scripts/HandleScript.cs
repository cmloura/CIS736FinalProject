using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class HandleScript : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Image shocked;
    public Image shockedRight;
    public Image disappointed;
    public Image angry;
    public Image thumbsDown;

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
        if (volume > -60 && volume < -40)
        {
            shockedRight.gameObject.SetActive(true);
        }
        else if (volume > -40 && volume < 0)
        {
            shockedRight.gameObject.SetActive(false);
            shocked.gameObject.SetActive(true);
        }
        else if (volume == 0)
        {
            shocked.gameObject.SetActive(false);
            disappointed.gameObject.SetActive(true);
            StartCoroutine(getMad());
        }
    }
    private IEnumerator getMad()
    {
        yield return new WaitForSeconds(3);
        disappointed.gameObject.SetActive(false);
        angry.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        angry.gameObject.SetActive(false);
        thumbsDown.gameObject.SetActive(true);
    }
}
