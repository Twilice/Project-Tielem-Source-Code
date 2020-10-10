using UnityEngine;
using System.Collections;

using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GraphicsDebug : MonoBehaviour
{
    public float fpsMeasurePeriod = 3f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    const string display = "{0} FPS";
    private Text m_Text;
    private string infoString;

    private void Start()
    {
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        m_Text = GetComponent<Text>();
        UpdateInfo();
    }


    private void Update()
    {
        // measure average frames per second
        m_FpsAccumulator++;
        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
            m_Text.text = "FPS: " + string.Format(display, m_CurrentFps) + "\n" + infoString;
        }
    }

    public void UpdateInfo()
    {
        infoString = QualitySettings.names[QualitySettings.GetQualityLevel()] +
            "\nCPUx" + SystemInfo.processorCount + ":" + SystemInfo.processorFrequency +
            "\nShaderLevel:" + SystemInfo.graphicsShaderLevel +
            "\nGMem:" + SystemInfo.graphicsMemorySize +
            "\nMem:" + SystemInfo.systemMemorySize;
    }

}
