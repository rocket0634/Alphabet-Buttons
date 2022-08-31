using System.Collections;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

public class AlphabetButtonsScript : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;
    public KMBombModule module;
    public KMAudio audio;
    public KMBombInfo bomb;
    public KMSelectable[] buttons;
    public TextMesh[] texts;
    public KMColorblindMode colorblind;

    private bool[] pressed = new bool[26];
    private Coroutine[] buttonAnims = new Coroutine[26];
    private List<char> shuffledAlphabet = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private List<char> colorNames = new List<char>() { 'R', 'O', 'Y', 'G', 'C', 'B', 'M', };
    private List<Color> possibleColors = new List<Color>() { new Color(1, 0.5f, 0.5f), new Color(1, 0.75f, 0.5f), new Color(1, 1, 0.5f), new Color(0.5f, 1, 0.5f), new Color(0.5f, 1, 1), new Color(0.5f, 0.5f, 1), new Color(1, 0.5f, 1) };
    private List<int> colors = new List<int>();
    private bool colorblindEnabled;
    private bool[] trueButtons = new bool[26];
    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        colorblindEnabled = colorblind.ColorblindModeActive;
        texts[26].text = "";
        shuffledAlphabet.Shuffle();
        for (int i = 0; i < buttons.Length; i++)
        {
            int x = i;
            buttons[x].OnInteract += delegate { ButtonPress(x); return false; };
            buttons[x].OnHighlight += delegate { if (colorblindEnabled) texts[26].text = colorNames[colors[x]].ToString(); };
            buttons[x].OnHighlightEnded += delegate { texts[26].text = ""; };
            texts[x].text = shuffledAlphabet[x].ToString();
            colors.Add(Rnd.Range(0, possibleColors.Count()));
            buttons[x].GetComponent<MeshRenderer>().material.color = possibleColors[colors[x]];
        }
    }

    // Use this for initialization
    void Start()
    {
        Calculate();
    }

    // Update is called once per frame
    void Update()
    {
    }
    void Calculate()
    {
        trueButtons[0] = bomb.IsPortPresent(Port.RJ45);
        trueButtons[1] = new[] { 0, 3, 6, 9 }.Contains(bomb.GetSerialNumberNumbers().Last());
        trueButtons[2] = bomb.IsPortPresent(Port.Parallel);
        trueButtons[3] = bomb.IsPortPresent(Port.Serial);
        trueButtons[4] = new[] { 0, 5 }.Contains(bomb.GetSerialNumberNumbers().Last());
        trueButtons[5] = bomb.IsIndicatorPresent("CAR");
        trueButtons[6] = bomb.IsIndicatorOn("SIG");
        trueButtons[7] = bomb.IsPortPresent(Port.PS2) && bomb.IsPortPresent(Port.DVI);
        trueButtons[8] = bomb.GetPortPlates().Any(x => x.Length == 0);
        trueButtons[9] = colors[shuffledAlphabet.IndexOf('J')] % 2 == 0;
        trueButtons[10] = bomb.GetBatteryCount() >= 4;
        trueButtons[11] = colors[shuffledAlphabet.IndexOf('R')] == 0 || colors[shuffledAlphabet.IndexOf('O')] == 1 || colors[shuffledAlphabet.IndexOf('Y')] == 2 || colors[shuffledAlphabet.IndexOf('G')] == 3 || colors[shuffledAlphabet.IndexOf('C')] == 4 || colors[shuffledAlphabet.IndexOf('B')] == 5 || colors[shuffledAlphabet.IndexOf('M')] == 6;
        trueButtons[12] = colors[shuffledAlphabet.IndexOf('M')] % 6 == 0;
        trueButtons[13] = bomb.GetSerialNumber().Contains('N');
        trueButtons[14] = colors[shuffledAlphabet.IndexOf('O')] <= 2;
        trueButtons[15] = colors[shuffledAlphabet.IndexOf('P')] == 0 || colors[shuffledAlphabet.IndexOf('P')] >= 5;
        trueButtons[16] = bomb.GetSerialNumber()[1] == 'Q';
        trueButtons[17] = colors[shuffledAlphabet.IndexOf('R')] <= 1 || colors[shuffledAlphabet.IndexOf('R')] == 4 || colors[shuffledAlphabet.IndexOf('R')] == 6;
        trueButtons[18] = shuffledAlphabet.IndexOf('S') <= 4;
        trueButtons[19] = shuffledAlphabet.IndexOf('T') % 5 == 0;
        trueButtons[20] = colors[shuffledAlphabet.IndexOf('U')] == 6;
        trueButtons[21] = colors[shuffledAlphabet.IndexOf('V')] % 2 == 1;
        trueButtons[22] = bomb.GetSerialNumber().Contains('2');
        trueButtons[23] = bomb.GetBatteryHolderCount() >= 4;
        trueButtons[24] = colors[shuffledAlphabet.IndexOf('Y')] == 2;
        trueButtons[25] = shuffledAlphabet.IndexOf('Z') == 25;
        if (trueButtons.Where(x => x).Count() == 0)
        {
            trueButtons["ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(bomb.GetSerialNumberLetters().First())] = true;
        }
        Debug.LogFormat("[Alphabet Buttons #{0}] The buttons that should be pressed are: {1}.", _moduleID, trueButtons.Select((x, ix) => x ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[ix] : '#').Where(x => x != '#').Join(", "));
    }
    void ButtonPress(int pos)
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[pos].transform);

        try { StopCoroutine(buttonAnims[pos]); } catch { }
        buttonAnims[pos] = StartCoroutine(ButtonAnim(pos));
        var alphabet = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        if (trueButtons[alphabet.IndexOf(texts[pos].text[0])])
        {
            pressed[pos] = true;
            texts[pos].color = new Color(1, 1, 1, 1);
            if (trueButtons.Where(x => x).Count() == pressed.Where(x => x).Count())
            {
                module.HandlePass();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, buttons[pos].transform);
                Debug.LogFormat("[Alphabet Buttons #{0}] All correct buttons pressed, module solved!", _moduleID);
            }
            else
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TitleMenuPressed, buttons[pos].transform);

            }
        }
        else
        {
            module.HandleStrike();
            Debug.LogFormat("[Alphabet Buttons #{0}] You pressed {1}, it was not a correct button.", _moduleID, texts[pos].text);
        }
    }
    private IEnumerator ButtonAnim(int pos, float depression = 0.005f, float duration = 0.05f)

    {
        float startPos = buttons[pos].transform.localPosition.y;
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            buttons[pos].transform.localPosition = Vector3.Lerp(new Vector3(buttons[pos].transform.localPosition.x, startPos, buttons[pos].transform.localPosition.z), new Vector3(buttons[pos].transform.localPosition.x, startPos - depression, buttons[pos].transform.localPosition.z), timer / duration);
        }

        timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            buttons[pos].transform.localPosition = Vector3.Lerp(new Vector3(buttons[pos].transform.localPosition.x, startPos - depression, buttons[pos].transform.localPosition.z), new Vector3(buttons[pos].transform.localPosition.x, startPos, buttons[pos].transform.localPosition.z), timer / duration);
        }
        buttons[pos].transform.localPosition = new Vector3(buttons[pos].transform.localPosition.x, startPos, buttons[pos].transform.localPosition.z);
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} press 'A B C' to press the buttons with those labels.";
#pragma warning restore 414
    public IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToLowerInvariant().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (split.Length >= 2 && split[0] == "press")
        {
            for (int j = 1; j < split.Length; j++)
            {
                for (int i = 0; i < 26; i++)
                {
                    string text = texts[i].text.ToLowerInvariant();
                    if (text == split[j])
                    {
                        yield return null;
                        buttons[i].OnInteract();
                    }
                }
            }
        }
    }
}