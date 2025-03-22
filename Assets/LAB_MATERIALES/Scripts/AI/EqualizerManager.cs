using UnityEngine;

public class EqualizerManager : MonoBehaviour
{
    public AudioSource audioSource;       
    public GameObject barPrefab;          
    public int numberOfBars = 5;          
    public int sampleSize = 256;          
    public float scaleMultiplier = 50f;   
    public float barSpacing = 2f;       


    public float[] individualMultipliers = new float[] {0.5f, 1f, 1f, 1f, 1f};

    // Configuración para detectar silencio
    public float silenceThreshold = 0.01f; 
    public float silenceDuration = 0.5f;  
    private float silenceTimer = 0f;

    private GameObject[] bars;            
    private float[] spectrumData;         
    void Start()
    {
        bars = new GameObject[numberOfBars];
        for (int i = 0; i < numberOfBars; i++)
        {

            Vector3 pos = new Vector3(i * barSpacing, 0, 0);
            

            bars[i] = Instantiate(barPrefab, transform);
            

            bars[i].transform.localPosition = pos;
        }
        spectrumData = new float[sampleSize];
    }

    void Update()
    {

        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        int samplesPerBar = sampleSize / numberOfBars;
        float overallIntensity = 0f;

 
        for (int i = 0; i < numberOfBars; i++)
        {
            float sum = 0f;
            for (int j = 0; j < samplesPerBar; j++)
            {
                int index = i * samplesPerBar + j;
                sum += spectrumData[index];
            }
            float intensity = (sum / samplesPerBar) * scaleMultiplier * individualMultipliers[i];
            overallIntensity += intensity;


            Vector3 newScale = bars[i].transform.localScale;
            newScale.y = Mathf.Lerp(newScale.y, intensity, Time.deltaTime * 30f);
            bars[i].transform.localScale = newScale;
        }


        overallIntensity /= numberOfBars;


        if (overallIntensity < silenceThreshold)
        {
            silenceTimer += Time.deltaTime;
        }
        else
        {
            silenceTimer = 0f;

            foreach (var bar in bars)
            {
                if (!bar.activeSelf)
                    bar.SetActive(true);
            }
        }


        if (silenceTimer > silenceDuration)
        {
            foreach (var bar in bars)
            {
                if (bar.activeSelf)
                    bar.SetActive(false);
            }
        }
    }
}
