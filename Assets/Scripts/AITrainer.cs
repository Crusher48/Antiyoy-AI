using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//placeholder neural network class
[System.Serializable]
public class NeuralNetworkPlaceholder
{
    //processes the strategic neural network
    public List<float> ProcessStrategicNetwork(List<float> inputs)
    {
        return new List<float>();
    }
    //processes the weights neural network (for each tile)
    public List<float> ProcessWeightsNetwork(List<float> inputs)
    {
        return new List<float>();
    }
}

//Metagame-level AI trainer
public class AITrainer : MonoBehaviour
{
    //buttons that the human can use
    [SerializeField] Button ViewGameButton;
    [SerializeField] Button Train100Button;
    [SerializeField] Button TrainPerpetuallyButton;
    [SerializeField] Button PlayGameButton;
    //singleton reference
    public static AITrainer Main;
    //the list of all AI data currently in the pool
    List<NeuralNetworkPlaceholder> AIData;

    void Awake()
    {
        Main = this;
        DontDestroyOnLoad(gameObject); //the trainer is a persistent object
        ViewGameButton.onClick.AddListener(ViewGame);
        Train100Button.onClick.AddListener(Train100Rounds);
        TrainPerpetuallyButton.onClick.AddListener(TrainContinuously);
        PlayGameButton.onClick.AddListener(PlayGame);
        SceneManager.LoadScene("MainScene");
    }
    //sends AIs into the training environment
    public IEnumerator StartAITrainingRound(bool viewable = false, bool playable = false)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        while (!asyncLoad.isDone)
            yield return null;
        //after the scene is loaded, do things
        transform.position = new Vector3(0, 0, -10);
        GameObject playerControlsObject = GameObject.Find("PlayerControls");
        if (playable)
        {
            playerControlsObject.SetActive(true);
        }
        else
        {
            playerControlsObject.SetActive(false);
        }
    }
    //trains all AIs
    public void EndAITrainingRound()
    {

    }
    void ViewGame()
    {

    }
    void Train100Rounds()
    {

    }
    void TrainContinuously()
    {

    }
    void PlayGame()
    {
        StartCoroutine(StartAITrainingRound());
    }
}
