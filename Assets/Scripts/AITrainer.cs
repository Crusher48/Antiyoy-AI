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
    [SerializeField] Button StopTrainingButton;
    [SerializeField] Button PlayGameButton;
    //singleton reference
    public static AITrainer Main;
    //the list of all AI data currently in the pool
    List<NeuralNetworkPlaceholder> AIData;
    int generation = 0;
    //for training loop
    int trainingRoundsRemaining = 0;
    bool nextRoundReady = true;

    void Awake()
    {
        Main = this;
        DontDestroyOnLoad(gameObject); //the trainer is a persistent object
        ViewGameButton.onClick.AddListener(ViewGame);
        Train100Button.onClick.AddListener(Train100Rounds);
        TrainPerpetuallyButton.onClick.AddListener(TrainContinuously);
        StopTrainingButton.onClick.AddListener(StopTraining);
        PlayGameButton.onClick.AddListener(PlayGame);
        SceneManager.LoadScene("MainScene");
    }
    void Update()
    {
        if (trainingRoundsRemaining > 0 && nextRoundReady)
        {
            print("Starting training round!");
            nextRoundReady = false;
            trainingRoundsRemaining--;
            StartCoroutine(StartAITrainingRound());
        }
    }
    //coroutine, sends AIs into the training environment and then does initial setup
    public IEnumerator StartAITrainingRound(bool viewable = false, bool playable = false)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        while (!asyncLoad.isDone)
            yield return null;
        //after the scene is loaded, do things
        transform.position = new Vector3(0, 0, -10);
        GameObject playerControlsObject = GameObject.Find("PlayerControls");
        if (viewable)
            GameManager.Main.AITurnDelayTimer = 0.1f;
        if (playable)
        {
            GameManager.Main.playableTeams.Add(1);
            gameObject.SetActive(false);
            playerControlsObject.SetActive(true);
        }
    }
    //trains all AIs based on the data, and sets up for the next round
    public void EndAITrainingRound()
    {
        gameObject.SetActive(true);
        generation++;
        nextRoundReady = true;
        //
    }
    void ViewGame()
    {
        StartCoroutine(StartAITrainingRound(true, false));
    }
    void Train100Rounds()
    {
        StartTrainingRegimin(100);
    }
    void TrainContinuously()
    {
        StartTrainingRegimin(10000);
    }
    void StartTrainingRegimin(int rounds)
    {
        trainingRoundsRemaining = rounds;
        TrainPerpetuallyButton.gameObject.SetActive(false);
        StopTrainingButton.gameObject.SetActive(true);
    }
    void StopTraining()
    {
        trainingRoundsRemaining = 0;
        TrainPerpetuallyButton.gameObject.SetActive(true);
        StopTrainingButton.gameObject.SetActive(false);
    }    
    void PlayGame()
    {
        StartCoroutine(StartAITrainingRound(true,true));
    }
}
