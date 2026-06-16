using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideo : MonoBehaviour
{
    void Start()
    {
        var vp = GetComponent<VideoPlayer>();
        vp.url = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "thunderstorm.mp4");
        vp.isLooping = true;
        vp.Play();
    }
}