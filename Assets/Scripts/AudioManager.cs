using UnityEngine;
using System; // Array.Find için gereklidir

// Bu [System.Serializable] etiketi sayesinde Sound sınıfı Inspector'da görünebilir hale gelir.
[System.Serializable]
public class Sound
{
    public string name; // Sesin adı (Launch, Death gibi)

    public AudioClip clip; // Ses dosyası

    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;

    // Bu bileşeni kodun içinde saklayacağız, Inspector'da görünmesine gerek yok.
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    // Singleton Pattern: Bu script'e projenin her yerinden kolayca erişmemizi sağlar.
    public static AudioManager instance;

    // Inspector'da yöneteceğimiz seslerin listesi
    public Sound[] sounds;

    private void Awake()
    {
        // Singleton kontrolü
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Oyun sahneleri arasında bu nesnenin kaybolmamasını sağlar.
        DontDestroyOnLoad(gameObject);

        // Her bir ses için bir AudioSource bileşeni oluştur ve ayarlarını yap.
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // Ses çalmak için çağıracağımız fonksiyon
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }
}