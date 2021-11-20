using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PolygridScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] arrows;
    public KMSelectable[] scrolls;
    public KMSelectable reset;
    public Renderer[] arrends;
    public Renderer[] grends;
    public Renderer[] disprends;
    public Material[] mats;

    private int[] grid = new int[25];
    private int[][] disps = new int[10][] { new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5]};
    private bool[] placed = new bool[10];
    private bool[] arrowpressed = new bool[10];
    private int currentdisp;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        bool[][] blanks = new bool[2][] { new bool[25], new bool[25] };
        int[] startgrid = Enumerable.Range(0, 25).Select(x => Random.Range(1, 8)).ToArray();
        bool[] fix = new bool[25];
        for(int i = 0; i < 7; i++)
            for(int j = 0; j < 2; j++)
            {
                int p = Random.Range(0, 25);
                while (fix[p])
                    p = Random.Range(0, 25);
                fix[p] = true;
                startgrid[p] = i + 1;
            }
        int r = Random.Range(0, 25);
        int blanknum = Random.Range(3, 7);
        for (int i = 0; i < blanknum; i++)
        {
            while (blanks[0][r] || blanks[0].Where((x, k) => k / 5 == r / 5 && k != r).All(x => x == true) || blanks[0].Where((x, k) => k % 5 == r % 5 && k != r).All(x => x == true))
                r = Random.Range(0, 25);
            blanks[0][r] = true;
        }
        int[] remaining = Enumerable.Range(0, 25).Where(x => blanks[0][x] == false).ToArray();
        r = remaining[Random.Range(0, remaining.Length)];
        for (int i = 0; i < 9 - blanknum; i++)
        {
            while (blanks[1][r] || blanks[1].Where((x, k) => k / 5 == r / 5 && k != r).All(x => x == true) || blanks[1].Where((x, k) => k % 5 == r % 5 && k != r).All(x => x == true))
                r = remaining[Random.Range(0, remaining.Length)];
            blanks[1][r] = true;
        }
        r = Random.Range(0, 25);
        while (!blanks[0][r] && !blanks[1][r])
            r = Random.Range(0, 25);
        startgrid[r] = 8;
        for (int i = 0; i < 5; i++)
            disps[i] = startgrid.Select((x, k) => blanks[0][k] ? 0 : x).Where((x, k) => k / 5 == i).ToArray();
        for (int i = 0; i < 5; i++)
            disps[i + 5] = startgrid.Select((x, k) => blanks[1][k] ? 0 : x).Where((x, k) => k % 5 == i).ToArray();
        for (int i = 0; i < 10; i++)
            if (Random.Range(0, 2) == 0)
                disps[i] = disps[i].Reverse().ToArray();
        disps = disps.Shuffle();
        for (int i = 0; i < 5; i++)
            disprends[i].material = mats[disps[0][i]];
        Debug.LogFormat("[Polygrid #{0}] The displayed rows/columns are:\n[Polygrid #{0}] {1}", moduleID, string.Join("\n[Polygrid #" + moduleID + "] ", disps.Select(x => "<" + string.Join("|", x.Select(y => new string[] { " ", "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665"}[y]).ToArray()) + ">").ToArray()));
        Debug.LogFormat("[Polygrid #{0}] The rows and columns fit into this 5\u00d75 grid: \n[Polygrid #{0}] {1}", moduleID, string.Join("\n[Polygrid #" + moduleID + "] ", Enumerable.Range(0, 5).Select(i => startgrid.Where((x, k) => k / 5 == i).Select(x => new string[] { "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665" }[x - 1]).ToArray().Join()).ToArray()));
        foreach (KMSelectable arrow in arrows)
        {
            int b = Array.IndexOf(arrows, arrow);
            arrow.OnInteract = delegate () { if (!moduleSolved && !arrowpressed[b % 10]) ArrowPress(b, disps[currentdisp]); return false; };
        }
        foreach(KMSelectable scroll in scrolls)
        {
            bool b = Array.IndexOf(scrolls, scroll) == 1;
            scroll.OnInteract = delegate () { if (placed.Count(x => x == false) > 1) {scrolls[b ? 1 : 0].AddInteractionPunch(); Audio.PlaySoundAtTransform("tick", scrolls[b ? 1 : 0].transform); Scroll(b); } return false; };
        }
        reset.OnInteract = delegate ()
        {
            if (!moduleSolved)
            {
                reset.AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, reset.transform);
                Debug.LogFormat("[Polygrid #{0}] Grid reset.", moduleID);
                if(placed.All(x => x == true))
                    for (int i = 0; i < 5; i++)
                        disprends[i].material = mats[disps[currentdisp][i]];
                for (int i = 0; i < 10; i++)
                {
                    placed[i] = false;
                    arrowpressed[i] = false;
                    arrends[i].material = mats[9];
                    arrends[i + 10].material = mats[9];
                }
                arrends[20].material = mats[9];
                arrends[21].material = mats[9];
                for (int i = 0; i < 25; i++)
                {
                    grid[i] = 0;
                    grends[i].material = mats[0];
                }
            }
            return false;
        };
    }

    private void Scroll(bool r)
    {
        currentdisp += r ? 1 : 9;
        currentdisp %= 10;
        while (placed[currentdisp])
        {
            currentdisp += r ? 1 : 9;
            currentdisp %= 10;
        }
        for (int i = 0; i < 5; i++)
            disprends[i].material = mats[disps[currentdisp][i]];
    }

    private void ArrowPress(int b, int[] c)
    {
        arrows[b].AddInteractionPunch(0.6f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrows[b].transform);
        int[] submission = Enumerable.Range(0, 5).Select(x => b > 9 ? c[4 - x] : c[x]).ToArray();
        bool success = true;
        b %= 10;
        if(b < 5)
        {
            for(int i = 0; i < 5; i++)
                if(grid[(i * 5) + b] != 0 && submission[i] != 0 && grid[(i * 5) + b] != submission[i])
                {
                    success = false;
                    Debug.LogFormat("[Polygrid #{0}] Attempt to place {1} into {2}{3}, which is already occupied by {4}", moduleID, new string[] { "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665" }[submission[i] - 1], "ABCDE"[i], b + 1, new string[] { "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665" }[grid[(i * 5) + b] - 1]);
                    break;
                }
        }
        else
        {
            for (int i = 0; i < 5; i++)
                if (grid[((b - 5) * 5) + i] != 0 && submission[i] != 0 && grid[((b - 5) * 5) + i] != submission[i])
                {
                    success = false;
                    Debug.LogFormat("[Polygrid #{0}] Attempt to place {1} into {2}{3}, which is already occupied by {4}", moduleID, new string[] { "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665" }[submission[i] - 1], "ABCDE"[b - 5], i + 1,new string[] { "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665" }[grid[((b - 5) * 5) + i] - 1]);
                    break;
                }
        }
        if (!success)
            module.HandleStrike();
        else
        {
            Debug.LogFormat("[Polygrid #{0}] Placed {1} into {2} {3}", moduleID, "<" + string.Join("|", submission.Select(y => new string[] { " ", "\u25b2", "\u25c6", "\u25cf", "\u2716", "\u25bc", "\u25a0", "+", "\u2665" }[y]).ToArray()) + ">", b < 5 ? "row" : "column", (b % 5) + 1);
            placed[currentdisp] = true;
            arrowpressed[b] = true;
            arrends[b].material = mats[0];
            arrends[b + 10].material = mats[0];
            for(int i = 0; i < 5; i++)
            {
                int d = b < 5 ? (i * 5) + b : (((b - 5) * 5) + i);
                if (grid[d] == 0)
                {
                    grid[d] = submission[i];
                    grends[d].material = mats[submission[i]];
                }
            }
            if (placed.All(x => x == true))
            {
                if (grid.All(x => x != 0))
                {
                    moduleSolved = true;
                    module.HandlePass();
                    Debug.LogFormat("[Polygrid #{0}] All displays placed. No empty cells remain. Module solved.", moduleID);
                }
                else
                    Debug.LogFormat("[Polygrid #{0}] All displays placed. {1} remain{2} empty.", moduleID, string.Join(" ", Enumerable.Range(0, 25).Where(x => grid[x] == 0).Select(x => "ABCDE"[x % 5] + ((x / 5) + 1).ToString()).ToArray()), grid.Count(x => x == 0) > 1 ? "" : "s");
                for (int i = 0; i < 5; i++)
                    disprends[i].material = mats[0];
            }
            else
            {
                if (placed.Count(x => x == false) == 1)
                    for (int i = 0; i < 2; i++)
                        arrends[i + 20].material = mats[0];
                Scroll(true);
            }
        }
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} scroll left/right [Changes display] | !{0} cycle | !{0} <L/R/U/D><1-5> [Selects the grid arrow pointing in the specified direction at the specified position from left to right/top to bottom] | !{0} reset";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if(command == "cycle")
        {
            int d = placed.Count(x => x == false);
            for(int i = 0; i < d; i++)
            {
                yield return new WaitForSeconds(1f);
                scrolls[1].OnInteract();
            }
            yield break;
        }
        if(command == "reset")
        {
            yield return null;
            reset.OnInteract();
            yield break;
        }
        string[] commands = command.Split(' ');
        if (commands.Length > 2)
        {
            yield return "sendtochaterror!f Too many arguments.";
            yield break;
        }
        if(commands[0] == "scroll")
        {
            int dir = Array.IndexOf(new string[] { "left", "right", "l", "r"}, commands[1]) % 2;
            if(dir < 0)
            {
                yield return "sendtochaterror!f Invalid scroll direction.";
                yield break;
            }
            yield return null;
            scrolls[dir].OnInteract();
        }
        else
        {
            if(command.Length < 2)
            {
                yield return "sendtochaterror!f Too few arguments.";
                yield break;
            }
            if (commands.Length == 1)
                commands = new string[] { command[0].ToString(), command[1].ToString() };
            int[] arr = new int[] { Array.IndexOf(new string[] { "r", "d", "l", "u"}, commands[0]), Array.IndexOf(new string[] { "1", "2", "3", "4", "5"}, commands[1])};
            if(arr[0] < 0 || arr[1] < 0)
            {
                yield return "sendtochaterror!f " + command + " is not a valid command.";
                yield break;
            }
            yield return null;
            arrows[(arr[0] * 5) + arr[1]].OnInteract();
        }
    }
}
