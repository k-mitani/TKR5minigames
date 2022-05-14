using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void PlayGun() => SceneManager.LoadScene("GunScene");
    public void PlayArchery() => SceneManager.LoadScene("ArcheryScene");
    public void PlayRiding() => SceneManager.LoadScene("RidingScene");
    public void PlaySpeech() => SceneManager.LoadScene("SpeechScene");
    public void PlayMath() => SceneManager.LoadScene("MathScene");
    public void PlayNinjutsu() => SceneManager.LoadScene("NinjutsuScene");
    public void PlayTea() => SceneManager.LoadScene("TeaScene");
    public void PlayMining() => SceneManager.LoadScene("MiningScene");
    public void PlayBuilding() => SceneManager.LoadScene("BuildingScene");
    public void PlayTactics() => SceneManager.LoadScene("TacticsScene");
    public void PlayFarming() => SceneManager.LoadScene("FarmingScene");
    public void PlayManner() => SceneManager.LoadScene("MannerScene");
    public void PlayMedicine() => SceneManager.LoadScene("MedicineScene");
    public void PlayMartial() => SceneManager.LoadScene("MartialScene");
    public void PlaySabotage() => SceneManager.LoadScene("SabotageScene");
    public void PlayNegotiation() => SceneManager.LoadScene("NegotiationScene");
}
