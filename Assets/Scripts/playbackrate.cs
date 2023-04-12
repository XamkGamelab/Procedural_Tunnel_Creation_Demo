using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playbackrate : MonoBehaviour
{
    [Range(0.00001f, 100f)]
    public float waitTime = 1;

    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoopPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator LoopPlayer()
    {
        while (this.enabled)
        {
            if (!audioSource.isPlaying)
            {
                yield return null;
            }
            audioSource.Play();
            yield return new WaitForSeconds(waitTime);


        }


    }
}
