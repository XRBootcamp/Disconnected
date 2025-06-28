using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class RecordToggleButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color recordingColor = Color.red;

    [Header("Animation")]
    [SerializeField] private float colorDuration = 0.3f;
    [SerializeField] private float scalePunch = 0.2f;

    private Color idleColor = Color.gray;
    private bool isRecording = false;

    private void Awake()
    {
        idleColor = background.color;
    }

    public void ToggleUiRecordingChanges()
    {
        isRecording = !isRecording;
        Color targetColor = isRecording ? recordingColor : idleColor;

        // Animate color
        background.DOColor(targetColor, colorDuration);

        // Optional: Add a punch scale animation
        background.transform.DOPunchScale(Vector3.one * scalePunch, 0.3f, vibrato: 4, elasticity: 0.7f);

        
        // change color
        //var nextButtonColor = isRecording ? button.colors.pressedColor : button.colors.normalColor;
        //button.targetGraphic.DOColor(nextButtonColor, colorDuration);
        // NOTE: most important bit of all
    }

}
