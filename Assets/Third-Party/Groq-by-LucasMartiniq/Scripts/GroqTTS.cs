using NaughtyAttributes;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public enum PlayAIVoice
{
    Aaliyah_PlayAI,
    Adelaide_PlayAI,
    Angelo_PlayAI,
    Arista_PlayAI,
    Atlas_PlayAI,
    Basil_PlayAI,
    Briggs_PlayAI,
    Calum_PlayAI,
    Celeste_PlayAI,
    Cheyenne_PlayAI,
    Chip_PlayAI,
    Cillian_PlayAI,
    Deedee_PlayAI,
    Eleanor_PlayAI,
    Fritz_PlayAI,
    Gail_PlayAI,
    Indigo_PlayAI,
    Jennifer_PlayAI,
    Judy_PlayAI,
    Mamaw_PlayAI,
    Mason_PlayAI,
    Mikail_PlayAI,
    Mitch_PlayAI,
    Nia_PlayAI,
    Quinn_PlayAI,
    Ruby_PlayAI,
    Thunder_PlayAI
}

public class GroqTTS : MonoBehaviour
{
    [SerializeField] private FileEnumPath storeGeneratedWavFiles;
    [SerializeField] public AudioSource audioSource;

    private const string apiUrl = "https://api.groq.com/openai/v1/audio/speech";

    private const string model = "playai-tts";
    [SerializeField] private PlayAIVoice selectedVoice = PlayAIVoice.Fritz_PlayAI;
    // Add a virtual property
    protected virtual PlayAIVoice SelectedVoice
    {
        get => selectedVoice;
        set => selectedVoice = value;
    }
    private const string responseFormat = "wav";
    [SerializeField] private string prompt = "I love building and shipping new features for our students!";

    public UnityEvent onCompletedTTS;

    public bool HasFinishedGeneratingClip { get; private set; }

    [Button]
    private async void Generate()
    {
        await GenerateAndPlaySpeech(prompt);
    }

    /// <summary>
    /// Generates TTS audio from the given text, but does not play it. Sets IsGenerated and audioSource.clip.
    /// </summary>
    /// <param name="text">The text to synthesize.</param>
    /// <returns>Task</returns>
    public async Task GenerateTTS(string text)
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            return;
        }

        var json = $"{{\"model\":\"{model}\",\"voice\":\"{GetVoiceName(SelectedVoice)}\",\"input\":\"{text}\",\"response_format\":\"{responseFormat}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HasFinishedGeneratingClip = false;

            var response = await APIKeyLoader.Instance.GroqHttpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"TTS API call failed: {response.StatusCode}");
                Debug.LogError(await response.Content.ReadAsStringAsync());
                return;
            }

            byte[] audioData = await response.Content.ReadAsByteArrayAsync();


            AudioClip clip = CreateClipFromWav(audioData);
            audioSource.clip = clip;
            if (storeGeneratedWavFiles != FileEnumPath.None)
            {
                string filePath = FileManagementExtensions.GenerateFilePath(storeGeneratedWavFiles, FilePaths.TEXT_TO_SPEECH, "tts", FileExtensions.WAV, true);
                SavWav.Save(filePath, clip, false);
            }
            prompt = text;
            HasFinishedGeneratingClip = true;

            onCompletedTTS.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error generating TTS: " + ex.Message);
        }
    }

    /// <summary>
    /// Generates TTS audio and plays it if successful.
    /// </summary>
    /// <param name="text">The text to synthesize and play.</param>
    /// <returns>Task</returns>
    public async Task<GroqTTS> GenerateAndPlaySpeech(string text)
    {
        await GenerateTTS(text);
        if (HasFinishedGeneratingClip)
        {
            audioSource.Play();
            return this;
        }
        return null;
    }

    private string GetVoiceName(PlayAIVoice voice)
    {
        return voice.ToString().Replace('_', '-');
    }

    private AudioClip CreateClipFromWav(byte[] wav)
    {
        int channels = BitConverter.ToInt16(wav, 22);
        int sampleRate = BitConverter.ToInt32(wav, 24);
        int bitsPerSample = BitConverter.ToInt16(wav, 34);

        int dataStart = FindDataChunk(wav) + 8;
        int sampleCount = (wav.Length - dataStart) / (bitsPerSample / 8);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            int sampleIndex = dataStart + i * 2; // assuming 16-bit PCM
            short sample = BitConverter.ToInt16(wav, sampleIndex);
            samples[i] = sample / 32768f;
        }

        AudioClip clip = AudioClip.Create("GroqTTS_Audio", sampleCount / channels, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private int FindDataChunk(byte[] wav)
    {
        for (int i = 12; i < wav.Length - 4; i++)
        {
            if (wav[i] == 'd' && wav[i + 1] == 'a' && wav[i + 2] == 't' && wav[i + 3] == 'a')
            {
                return i;
            }
        }
        throw new Exception("DATA chunk not found in WAV");
    }

    public void SetVoice(PlayAIVoice newVoice)
    {
        SelectedVoice = newVoice;
    }

    public void SetPrompt(string newPrompt)
    {
        prompt = newPrompt;
    }

    public void SetStoreGeneratedWavFiles(FileEnumPath newEnum)
    {
        storeGeneratedWavFiles = newEnum;
    }

    [Button]
    public void StopAudio()
    {
        if (!HasFinishedGeneratingClip) return;
        audioSource.Stop();
    }

    [Button]
    public void PlayAudio()
    {
        if (!HasFinishedGeneratingClip) return;
        audioSource.Play();
        
    }

    public void SetClip(AudioClip newClip)
    {
        audioSource.clip = newClip;
    }

#if UNITY_EDITOR
    public void ForceIsGenerated()
    {
        HasFinishedGeneratingClip = true;
    }
#endif
}
