using UnityEngine;

public class DayNightVFXController : MonoBehaviour
{
    [SerializeField] ParticleSystem[] _dayParticleList;
    [SerializeField] ParticleSystem[] _nightParticleList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideNightParticles();
        ShowDayParticles();
    }

    public void ShowDayParticles()
    {
        foreach (ParticleSystem particle in _dayParticleList)
        {
            particle.Play();
        }
    }

    public void HideDayParticles()
    {
        foreach (ParticleSystem particle in _dayParticleList)
        {
            particle.Stop();
        }
    }

    public void ShowNightParticles()
    {
        foreach (ParticleSystem particle in _nightParticleList)
        {
            particle.Play();
        }
    }

    public void HideNightParticles()
    {
        foreach (ParticleSystem particle in _nightParticleList)
        {
            particle.Stop();
        }
    }
}
