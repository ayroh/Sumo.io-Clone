using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using System.Linq;

public class GameManagerScript : MonoBehaviour {

    public static GameManagerScript instance;

    [HeaderAttribute("Exposed Variables")]
    public float collisionBaseForce = 125f;
    public float moveForce = 5f;
    public float AIMoveForce = 5f;
    public float lerpConstant = .05f;
    public float scaleCoefficient = .75f;

    [HeaderAttribute("Scriptable Objects")]
    [SerializeField] private AISO aiSO;

    [HeaderAttribute("Parents")]
    [SerializeField] private Transform ediblesParent;
    [SerializeField] private Transform playersParent;


    [HeaderAttribute("Dependency Injection")]
    [SerializeField] private TouchManagerScript touchManagerScript;

    [HeaderAttribute("UIPanels")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Transform scorePanel;

    [HeaderAttribute("Prefabs")]
    [SerializeField] private GameObject edible;
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject AIPrefab;

    [HeaderAttribute("Text and Button")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject pauseButton;

    // For instantiating and injecting
    private PlayerScript player;
    private List<AIScript> AIs;

    // Score and Main camera to stay away from Camera.main more than one time
    private Dictionary<Color, int> scores;
    private Camera cam;

    // Timer Numerator to stop it on endgame
    private IEnumerator countdownCoroutine;


    // No need for singleton, hand habit
    void Awake() {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }



    public void StartGame() {
        // Instantiate main player
        player = Instantiate(playerPrefab, aiSO.positions[^1], Quaternion.identity, playersParent).GetComponent<PlayerScript>();
        player.SetTouchManager(touchManagerScript);
        player.SetColor(aiSO.colors[^1]);

        // Keep scores
        scores = new Dictionary<Color, int>();
        scores.Add(aiSO.colors[^1], 0);

        // Main camera to stay away from Camera.main more than one time
        cam = Camera.main;
        CameraScript camScript = cam.GetComponent<CameraScript>();

        // Injection
        camScript.target = player.transform;
        camScript.enabled = true;

        // Instantiate 7 AI's
        AIs = new List<AIScript>();
        for(int i = 0;i < 7;++i) {
            AIs.Add(Instantiate(AIPrefab, aiSO.positions[i], Quaternion.identity, playersParent).GetComponent<AIScript>());
            AIs[i].SetColor(aiSO.colors[i]);
            scores.Add(aiSO.colors[i], 0);
        }
        // Create 5 Edibles which replace themselves after getting eaten
        CreateEdibles();

        // Start Game
        StartCoroutine(countdownCoroutine = Countdown());

    }


    private IEnumerator Countdown() {
        // Countdown from 3 and start game. Little animation with fontsize 

        countdownText.text = 3.ToString();
        countdownText.gameObject.SetActive(true);
        DOTween.To(() => countdownText.fontSize, x => countdownText.fontSize = x, 400, 1f);
        yield return new WaitForSecondsRealtime(1);
        countdownText.fontSize = 300;
        countdownText.text = 2.ToString();
        DOTween.To(() => countdownText.fontSize, x => countdownText.fontSize = x, 400, 1f);
        yield return new WaitForSecondsRealtime(1);
        countdownText.fontSize = 300;
        countdownText.text = 1.ToString();
        DOTween.To(() => countdownText.fontSize, x => countdownText.fontSize = x, 400, 1f);
        yield return new WaitForSecondsRealtime(1);
        countdownText.transform.gameObject.SetActive(false);

        // Open inputs and AI's
        EnablePlayerScripts();

        pauseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;

        // 60 seconds timer
        int timer = 60;
        timerText.text = timer.ToString();
        while (true) {
            yield return new WaitForSeconds(1);
            timerText.text = (--timer).ToString();
            if (timer == 0f) {
                EndGame();
                yield break;
            }

        }
    }

    public void EndGame() {
        StopCoroutine(countdownCoroutine);

        // Stop physics with timeScale
        Time.timeScale = 0f;

        // Make pause button not interactable
        pauseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;


        // Order by values descending
        scores = scores.OrderByDescending(value => value.Value).ToDictionary(x => x.Key, x => x.Value);
        foreach(var score in scores) {
            // Instantiate UI panel for all players
            // You can scroll all players scores
            GameObject obj = Instantiate(scorePrefab, scorePanel.transform);
            obj.transform.Find("ColorImage").GetComponent<UnityEngine.UI.Image>().color = score.Key;
            obj.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text += score.Value;
        }

        // Open last panel
        endGamePanel.SetActive(true);
    }

    private void EnablePlayerScripts() {
        // Enable all scripts
        player.enabled = true;
        for (int i = 0;i < AIs.Count;++i)
            AIs[i].enabled = true;
    }


    #region Score

    private int dieCount = 0;

    public void AddScore(int score, GameObject obj = null, bool died = false) {
        if(obj != null) {
            PlayerScript objScript = obj.GetComponent<PlayerScript>();


            if (died) {
                // Ensure that there aren't any tweens
                objScript.transform.DOComplete();
                objScript.transform.DOScale(new Vector3(objScript.transform.localScale.x + .2f, objScript.transform.localScale.y + .2f, objScript.transform.localScale.z + .2f), .1f);
            }


            // Add score to dictionary by objects color
            scores[objScript.color] += score;

            // If its our player, add score to UI
            if (objScript.color == Color.white) {
                RaiseCamera(.5f);
                scoreText.DOComplete();
                Tween tw;
                (tw = DOTween.To(() => scoreText.fontSize, x => scoreText.fontSize = x, 120, .25f)).SetAutoKill(false).OnComplete(() => tw.Rewind());
                scoreText.text = (int.Parse(scoreText.text) + score).ToString();
            }
        }

        // End game if only 1 player lefts
        if (died && ++dieCount == scores.Count - 1)
            EndGame();
    }

    // Raise camera when main player eats edible
    public void RaiseCamera(float multiplier) => cam.GetComponent<CameraScript>().AddOffset(new Vector3(0f, 1f * multiplier, -1f * multiplier));

    #endregion

    #region Edible Management

    [HeaderAttribute("Game Arena")]
    [SerializeField] private Transform arena;
    private float arenaX, arenaZ;

    private void CreateEdibles() {

        // Create 5 edibles at 5 random places that doesn't collide with players
        arenaX = arena.transform.localScale.x / 3;
        arenaZ = arena.transform.localScale.z / 3;
        float randomX, randomZ;
        Collider[] edibleAlloc = new Collider[1];
        
        for (int i = 0;i < 5;++i) {
            randomX = Random.Range(-arenaX, arenaX);
            randomZ = Random.Range(-arenaZ, arenaZ);

            // Doesn't spawns if there is a player in that point
            if (Physics.OverlapSphereNonAlloc(new Vector3(randomX, arena.transform.position.y, randomZ), 1.5f, edibleAlloc, LayerMask.GetMask("Panda")) != 0) {
                --i;
                continue;
            }

            // Instantiate and start rotating forever
            Instantiate(edible, new Vector3(randomX, arena.transform.position.y + .6f, randomZ), Quaternion.Euler(new Vector3(0f, 0f, 0f)), ediblesParent)
                .transform.DORotate(new Vector3(0f, 360f, 0f), 2f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);

        }
    }


    public void ReleaseEdible(GameObject edibleObj) => StartCoroutine(ReleaseEdibleCoroutine(edibleObj));

    // Instead of destroying edible, I deactivate it and use it elsewhere after 5 seconds 
    private IEnumerator ReleaseEdibleCoroutine(GameObject edibleObj) {
        edibleObj.SetActive(false);
        yield return new WaitForSeconds(5f);
        edibleObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        edibleObj.transform.position = new Vector3(Random.Range(-arenaX, arenaX), arena.transform.position.y + .6f, Random.Range(-arenaZ, arenaZ));
        edibleObj.SetActive(true);
        edibleObj.GetComponent<Collider>().enabled = true;
        edibleObj.transform.DORotate(new Vector3(0f, 360f, 0f), 2f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
    }

    #endregion

    #region Buttons
    public void RestartGame() {
        if(Time.timeScale == 0f)
            Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene("GameScene");
    }

    public void PauseGame() => Time.timeScale = 0f;

    public void ResumeGame() => Time.timeScale = 1f;

    #endregion
}