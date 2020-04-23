
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class CycleController : UdonSharpBehaviour
{
    public Light Sun;

    public Slider SpeedSlider;
    public Slider TimeSlider;

    public Material LowCloud;
    public Material HighCloud;

    public Material Stars;
    public GameObject StarsObject;
    public Material Moon;
    public GameObject SkyObject;

    public GameObject FireFX;

    public AudioSource Birds;
    public AudioSource Cicadas;

    [UdonSynced]
    public float SetTime = 0f;
    [UdonSynced]
    public int syncid = 0;
    public int lastreceivedid = 0;
    [UdonSynced]
    public float Speed = 1 / 600f;


    [Range(0, 1)]
    public float CurrentTimeOfDay = 0.2f;
    [HideInInspector]
    public float TimeMultiplier = 1f;

    public Color SunColor1;
    public Color SunColor2;
    public float SunPoint1 = 0.25f;
    public float SunPoint2 = 0.35f;

    public Color AmbientColor1;
    public Color AmbientColor2;
    public Color AmbientColor3;
    public float AmbientPoint1 = 0.2f;
    public float AmbientPoint2 = 0.25f;
    public float AmbientPoint3 = 0.35f;

    public Color CloudColor1;
    public Color CloudColor2;
    public Color CloudColor3;
    public float CloudPoint1 = 0.2f;
    public float CloudPoint2 = 0.25f;
    public float CloudPoint3 = 0.35f;

    public Color StarColor1;
    public Color StarColor2;
    public float StarPoint1 = 0.2f;
    public float StarPoint2 = 0.25f;
    public float StarCutoff = 0.3f;

    public Color MoonColor1;
    public Color MoonColor2;
    public float MoonPoint1 = 0.2f;
    public float MoonPoint2 = 0.25f;

    public float AudioPoint1 = 0.25f;
    public float AudioPoint2 = 0.35f;

    public float SunIntensityPoint1 = 0.23f;
    public float SunIntensityPoint2 = 0.25f;

    public float FirePoint = 0.2f;

    float SunInitialIntensity;
    float BirdsInitialVolume;
    float CicadasInitialVolume;

    void Start()
    {
        SunInitialIntensity = Sun.intensity;
        BirdsInitialVolume = Birds.volume;
        CicadasInitialVolume = Cicadas.volume;
        Random.InitState((int)Time.time);
        TimeSlider.value = CurrentTimeOfDay;
        SpeedSlider.value = Speed;
    }


    void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        SetTime = CurrentTimeOfDay + 0.083f;
        syncid = GetID();
    }

    public void SpeedSliderUpdated()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        SetTime = TimeSlider.value;
        Speed = SpeedSlider.value;
        syncid = GetID();
    }

    public void TimeSliderUpdated()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        SetTime = TimeSlider.value;
        syncid = GetID();
    }

    private int GetID()
    {
        return Random.Range(-2000000000, 2000000000);
    }

    void Update()
    {
        if (syncid != lastreceivedid)
        {
            CurrentTimeOfDay = SetTime;
            lastreceivedid = syncid;
            TimeSlider.value = CurrentTimeOfDay;
            SpeedSlider.value = Speed;
        }
        else if (TimeSlider.gameObject.activeInHierarchy && Time.time % 1 < 0.01f)
        {
            TimeSlider.value = CurrentTimeOfDay;
            SpeedSlider.value = Speed;
        }

        Sun.transform.localRotation = Quaternion.Euler((CurrentTimeOfDay * 360f) - 90, 140, 30);
        SkyObject.transform.localRotation = Quaternion.Euler((CurrentTimeOfDay * 360f) - 90, 140, 30);

        Sun.color = TwoPoint(SunPoint1, SunPoint2, SunColor1, SunColor2);
        RenderSettings.ambientLight = ThreePoint(AmbientPoint1, AmbientPoint2, AmbientPoint3, AmbientColor1, AmbientColor2, AmbientColor3);
        Color c = ThreePoint(CloudPoint1, CloudPoint2, CloudPoint3, CloudColor1, CloudColor2, CloudColor3);
        LowCloud.SetColor("_CloudColor", c);
        HighCloud.SetColor("_CloudColor", c);

        c = TwoPoint(StarPoint1, StarPoint2, StarColor1, StarColor2);
        Stars.SetColor("_EmissionColor", c);

        if (c.a <= StarCutoff)
        {
            if (StarsObject.activeInHierarchy)
            {
                StarsObject.SetActive(false);
            }
        }
        else if (!StarsObject.activeInHierarchy)
        {
            StarsObject.SetActive(true);
        }


        if (CurrentTimeOfDay > FirePoint && CurrentTimeOfDay < 1 - FirePoint)
        {
            if (FireFX.activeSelf)
            {
                FireFX.SetActive(false);
            }
        }
        else
        {
            if (!FireFX.activeSelf)
            {
                FireFX.SetActive(true);
            }
        }

        Moon.color = TwoPoint(MoonPoint1, MoonPoint2, MoonColor1, MoonColor2);


        float audiovolume = TwoPointFloat(AudioPoint1, AudioPoint2);

        Birds.volume = BirdsInitialVolume * audiovolume;
        Cicadas.volume = CicadasInitialVolume * Mathf.Clamp(1f - audiovolume, 0f, 1f);

        float sunintensity = TwoPointFloat(SunIntensityPoint1, SunIntensityPoint2);
        Sun.intensity = (SunInitialIntensity * sunintensity) + 0.001f;


        CurrentTimeOfDay += (Time.deltaTime * Speed) * TimeMultiplier;

        if (CurrentTimeOfDay >= 1)
        {
            CurrentTimeOfDay = 0;
        }
    }


    public float TwoPointFloat(float p1, float p2)
    {
        float p3 = 1 - p2;
        float p4 = 1 - p1;

        float ret = 1f;

        if (CurrentTimeOfDay < p1)
        {
            ret = 0f;
        }
        else if (CurrentTimeOfDay < p2)
        {
            ret = (CurrentTimeOfDay - p1) / (p2 - p1);
        }
        else if (CurrentTimeOfDay < p3)
        {
            ret = 1f;
        }
        else if (CurrentTimeOfDay < p4)
        {
            ret = 1 - ((CurrentTimeOfDay - p3) / (p4 - p3));
        }
        else
        {
            ret = 0f;
        }

        return ret;
    }

    public Color TwoPoint(float p1, float p2, Color c1, Color c2)
    {
        Color ret = new Color(0f, 0f, 0f);

        float p3 = 1 - p2;
        float p4 = 1 - p1;

        if (CurrentTimeOfDay < p1)
        {
            ret = c1;
        }
        else if (CurrentTimeOfDay < p2)
        {
            float v = (CurrentTimeOfDay - p1) / (p2 - p1);
            ret = Color.Lerp(c1, c2, v);
        }
        else if (CurrentTimeOfDay < p3)
        {
            ret = c2;
        }
        else if (CurrentTimeOfDay < p4)
        {
            float v = (CurrentTimeOfDay - p3) / (p4 - p3);
            ret = Color.Lerp(c2, c1, v);
        }
        else
        {
            ret = c1;
        }

        return ret;
    }

    public Color ThreePoint(float p1, float p2, float p3, Color c1, Color c2, Color c3)
    {
        Color ret = new Color(1f, 1f, 1f);

        float p4 = 1 - p3;
        float p5 = 1 - p2;
        float p6 = 1 - p1;

        if (CurrentTimeOfDay < p1)
        {
            ret = c1;
        }
        else if (CurrentTimeOfDay < p2)
        {
            float v = (CurrentTimeOfDay - p1) / (p2 - p1);
            ret = Color.Lerp(c1, c2, v);
        }
        else if (CurrentTimeOfDay < p3)
        {
            float v = (CurrentTimeOfDay - p2) / (p3 - p2);
            ret = Color.Lerp(c2, c3, v);
        }
        else if (CurrentTimeOfDay < p4)
        {
            ret = c3;
        }
        else if (CurrentTimeOfDay < p5)
        {
            float v = (CurrentTimeOfDay - p4) / (p5 - p4);
            ret = Color.Lerp(c3, c2, v);
        }
        else if (CurrentTimeOfDay < p6)
        {
            float v = (CurrentTimeOfDay - p5) / (p6 - p5);
            ret = Color.Lerp(c2, c1, v);
        }
        else
        {
            ret = c1;
        }

        return ret;
    }
}
