using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    #region GAME VARIABLES
    private enum GAME_STATE { PREGAME, GAME, POSTGAME };
    private GAME_STATE currState;
    public static GameHandler singleton;
    public static GameObject iconPair1, iconPair2;
    public static bool canClick;

    [Tooltip("Check this if matching pairs have different images.")]
    public bool differentImageMatching;
    [Tooltip("Check this if game only starts on player click.")]
    public bool startOnClick;
    [Tooltip("Game has timer.")]
    public bool hasTimer;
    //private ref if timer is running
    private bool timerRun;

    [Tooltip("Place prefab for ButtonIcon here.")]
    public GameObject matchIconPrefab;
    [Tooltip("Place Object Holder from heirachy into the field.")]
    public Transform iconHolder;
    [Tooltip("Total number of matched pairs the game needs.")]
    public int numberOfPairs;
    //private ref for remaining matched pairs
    private int pairsLeft;
    [Tooltip("Insert max length of game.")]
    public float gameTimerMax;
    [Tooltip("Insert delay before game start. (Only applies if startOnClick is unchecked.)")]
    public float delayStartTimerMax;
    //private timer ref
    private float gameTimer, delayTimer;
    [Tooltip("Splits each pair into two seperate categories. Seperate each tag with comma. Leave blank if not using.")]
    public string pairTags;
    private List<string> tags;

    [Tooltip("Count value here to be the same as number of pairs value above. Insert sprites for icons here. (One per pair.)")]
    public List<Sprite> spriteList;
    [Tooltip("Use this only if differentImageMatching is marked true. Count value here to also be the same as number of pairs value above. " +
        "MAKE SURE THAT EACH MATCHED PICTURE IS THE SAME INDEX!")]
    public List<Sprite> spriteList2;
    //private ref for button placement
    private List<GameObject> gameButtons;
    #endregion

    #region UI ELEMENTS
    [Tooltip("Place relevant display elements here.")]
    public TextMeshProUGUI timerDisplay;
    #endregion

    private void Awake()
    {
        singleton = this;
    }

    private void Start()
    {
        currState = GAME_STATE.PREGAME;
        pairsLeft = numberOfPairs;
        canClick = false;
        timerRun = false;
        SetupPairs();
    }

    private void SetupPairs()
    {
        gameButtons = new List<GameObject>();
        tags = new List<string>();

        //category for pairs if needed
        if (!string.IsNullOrEmpty(pairTags))
        {
            string[] vars = pairTags.Split(',');
            foreach (string s in vars)
            {
                tags.Add(s);
            }
        }

        //make a temp sprite list for initial randomization
        List<Sprite> tempList = new List<Sprite>();

        foreach (var s in spriteList)
        {
            tempList.Add(s);
        }

        switch (differentImageMatching)
        {
            case true: //different image matching
                List<Sprite> tempList2 = new List<Sprite>();

                foreach (var s in spriteList2)
                {
                    tempList2.Add(s);
                }

                //instaniate each pair
                for (int i = 0; i < numberOfPairs; i++)
                {
                    int ranIndex = Random.Range(0, tempList.Count);
                    GameObject icon1 = Instantiate(matchIconPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    GameObject icon2 = Instantiate(matchIconPrefab, Vector3.zero, Quaternion.identity) as GameObject;

                    icon1.GetComponent<ButtonIconHandler>().frontImage = tempList[ranIndex];
                    icon1.GetComponent<ButtonIconHandler>().ID = i.ToString();
                    icon2.GetComponent<ButtonIconHandler>().frontImage = tempList2[ranIndex];
                    icon2.GetComponent<ButtonIconHandler>().ID = i.ToString();

                    if (tags.Count > 0)
                    {
                        icon1.GetComponent<ButtonIconHandler>().type = tags[0];
                        icon2.GetComponent<ButtonIconHandler>().type = tags[1];
                    }

                    tempList.RemoveAt(ranIndex);
                    tempList2.RemoveAt(ranIndex);

                    //put into gamebuttons list (this determines sibling index (and thus their placement in grid))
                    gameButtons.Add(icon1);
                    gameButtons.Add(icon2);
                    shuffleList(gameButtons);
                }
                break;


            case false: //same image matching

                //instaniate each pair
                for (int i = 0; i < numberOfPairs; i++)
                {
                    int ranIndex = Random.Range(0, tempList.Count);
                    GameObject icon1 = Instantiate(matchIconPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    GameObject icon2 = Instantiate(matchIconPrefab, Vector3.zero, Quaternion.identity) as GameObject;

                    icon1.GetComponent<ButtonIconHandler>().frontImage = tempList[ranIndex];
                    icon1.GetComponent<ButtonIconHandler>().ID = i.ToString();
                    icon2.GetComponent<ButtonIconHandler>().frontImage = tempList[ranIndex];
                    icon2.GetComponent<ButtonIconHandler>().ID = i.ToString();

                    if (tags.Count > 0)
                    {
                        icon1.GetComponent<ButtonIconHandler>().type = tags[0];
                        icon2.GetComponent<ButtonIconHandler>().type = tags[1];
                    }

                    tempList.RemoveAt(ranIndex);

                    //put into gamebuttons list (this determines sibling index (and thus their placement in grid))
                    gameButtons.Add(icon1);
                    gameButtons.Add(icon2);
                    shuffleList(gameButtons);
                }
                break;
        }

        shuffleList(gameButtons);
        foreach (var b in gameButtons)
        {
            b.transform.SetParent(iconHolder, false);
        }

        if (delayStartTimerMax > 0)
        {
            timerDisplay.text = "Time to Start: " + delayStartTimerMax.ToString();
            delayTimer = delayStartTimerMax;
            flipCards("front");
        } else
        {
            if (startOnClick)
            {
                flipCards("front");
            }
        }
    }

    private void shuffleList<E>(IList<E> list)
    {
        System.Random ran = new System.Random();

        if (list.Count > 1)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                E tmp = list[i];
                int ranIndex = ran.Next(i + 1);

                //swap elements
                list[i] = list[ranIndex];
                list[ranIndex] = tmp;
            }
        }
    }

    private void Update()
    {
        switch (currState)
        {
            #region PREGAME
            case GAME_STATE.PREGAME:
                if (startOnClick)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (delayStartTimerMax > 0)
                        {
                            timerRun = true;
                        } else
                        {
                            startGame();
                        }
                    }
                } else
                {
                    if (delayStartTimerMax > 0)
                    {
                        timerRun = true;
                    } else
                    {
                        startGame();
                    }
                }

                if (timerRun)
                {
                    if (delayTimer > 0)
                    {
                        delayTimer -= Time.deltaTime;
                        int time = (int)delayTimer;
                        timerDisplay.text = "Time to Start: " + time.ToString();
                    } else
                    {
                        startGame();
                    }
                }
                break;
            #endregion

            #region GAME
            case GAME_STATE.GAME:
                if (timerRun)
                {
                    if (gameTimer > 0)
                    {
                        gameTimer -= Time.deltaTime;
                        int time = (int)gameTimer;
                        timerDisplay.text = "Time Left: " + time.ToString();
                    } else
                    {
                        gameOver(false);
                    }
                }
                break;
                #endregion
        }
    }

    private void startGame()
    {
        timerRun = false;
        flipCards("back");

        if (hasTimer)
        {
            timerDisplay.text = "Time Left: " + gameTimerMax.ToString();
            gameTimer = gameTimerMax;
            timerRun = true;
        } else
        {
            timerDisplay.text = string.Empty;
        }

        currState = GAME_STATE.GAME;
        canClick = true;
    }

    private void flipCards(string side)
    {
        switch (side)
        {
            case "front":
                foreach (Transform c in iconHolder)
                {
                    c.GetComponent<Image>().sprite = c.GetComponent<ButtonIconHandler>().frontImage;
                }
                break;

            case "back":
                foreach (Transform c in iconHolder)
                {
                    c.GetComponent<Image>().sprite = c.GetComponent<ButtonIconHandler>().backImage;
                }
                break;
        }
    }

    private void gameOver(bool finished)
    {
        canClick = false;
        timerRun = false;

        if (finished)
        {
            Debug.Log("You win!");
        } else
        {
            Debug.Log("You lose...");
        }
    }

    public void checkPair()
    {
        canClick = false;

        if (iconPair1.GetComponent<ButtonIconHandler>().ID != iconPair2.GetComponent<ButtonIconHandler>().ID)
        {
            StartCoroutine(wrongDelay(0.5f));
        }
        else
        {
            StartCoroutine(rightDelay(0.2f));
        }
    }

    private IEnumerator wrongDelay (float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        iconPair1.GetComponent<Image>().sprite = iconPair1.GetComponent<ButtonIconHandler>().backImage;
        iconPair2.GetComponent<Image>().sprite = iconPair2.GetComponent<ButtonIconHandler>().backImage;
        iconPair1.GetComponent<ButtonIconHandler>().isClicked = false;
        iconPair2.GetComponent<ButtonIconHandler>().isClicked = false;
        iconPair1 = null;
        iconPair2 = null;
        canClick = true;
    }

    private IEnumerator rightDelay (float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        pairsLeft--;
        iconPair1 = null;
        iconPair2 = null;
        canClick = true;
        if (pairsLeft <= 0)
        {
            gameOver(true);
        }
    }
}
