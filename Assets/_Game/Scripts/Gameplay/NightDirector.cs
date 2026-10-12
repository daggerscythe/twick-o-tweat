using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// runs the whole level, start to finish
// lives on Managers

// the settings for one phase of the night
[Serializable]
public class PhaseSettings
{
    public string name = "Phase";

    [Header("Kids")]
    [Min(1)] public int minItems = 2;
    [Min(1)] public int maxItems = 3;
    [Range(1, 2)] public int kidSlots = 1;

    [Header("Ghosts")]
    public bool thiefGhosts;
    public bool tankGhosts;
    [Min(1)] public int ghostHealth = 2; // normal ghost HP; tanks are x3, thieves have their own fixed HP
    [Min(0.5f)] public float ghostSpeed = 2f;
    [Min(1f)] public float spawnInterval = 8f; // seconds between spawns
    [Min(1)] public int maxGhosts = 3; // alive at once

    [Header("Level")]
    public bool lightsOut;
    public string popupTitle;
    [TextArea(3, 8)] public string popupBody;
}

public class NightDirector : MonoBehaviour
{
    private enum Step { Intro, TutorialBasics, TutorialTimer, Night, Done }

    [Header("References")]
    [SerializeField] private PopupUI popup;
    [SerializeField] private KidSpawner kids;
    [SerializeField] private GhostSpawner ghosts;
    [SerializeField] private LightsOut lightsOut;
    [SerializeField] private PlayerBag playerBag;

    [Header("Clock")]
    [SerializeField] private float phaseLength = 90f; // real seconds per phase (= 45 in-game minutes)
    [SerializeField] private int startHour = 6; // 6 PM
    [SerializeField] private int nightHours = 3; // until 9 PM

    [Header("Tutorial")]
    [SerializeField] private int tutorialMinItems = 1;
    [SerializeField] private int tutorialMaxItems = 2;

    [Header("Phases (in order)")]
    [SerializeField] private PhaseSettings[] phases =
    {
        new PhaseSettings
        {
            name = "Phase 1", minItems = 2, maxItems = 3, kidSlots = 1,
            ghostHealth = 2, ghostSpeed = 2f, spawnInterval = 8f, maxGhosts = 3,
            popupTitle = "The night begins!",
            popupBody = "Survive until <b>9 PM</b>. Orders get bigger and the ghosts get tougher as the night goes on.\n\n" +
                        "The night ends early if your <b>health hits 0</b> or <b>3 kids</b> leave unhappy.\n\n" +
                        "Tip: bonus points only come from <b>perfect</b> orders."
        },
        new PhaseSettings
        {
            name = "Phase 2", minItems = 2, maxItems = 4, kidSlots = 2, thiefGhosts = true,
            ghostHealth = 3, ghostSpeed = 2.2f, spawnInterval = 7f, maxGhosts = 4,
            popupTitle = "Two at the door!",
            popupBody = "Word got out: kids now wait at <b>two spots</b> on the porch. Pick which order to fill first, and which bag to use.\n\n" +
                        "Watch out for <color=#B266FF><b>purple Thief Ghosts</b></color>. They don't hurt you: they <b>steal a candy</b> from your bag and fly away. " +
                        "Shoot one before it leaves the house and the candy drops back into your bag."
        },
        new PhaseSettings
        {
            name = "Phase 3", minItems = 3, maxItems = 5, kidSlots = 2, thiefGhosts = true, tankGhosts = true,
            ghostHealth = 3, ghostSpeed = 2.4f, spawnInterval = 7f, maxGhosts = 4,
            popupTitle = "Tank Ghost!",
            popupBody = "A <b>Tank Ghost</b> has woken up: big, slow, and it hits extra hard. It takes a lot of shots to bring down.\n\n" +
                        "Only one roams at a time."
        },
        new PhaseSettings
        {
            name = "Phase 4", minItems = 4, maxItems = 6, kidSlots = 2, thiefGhosts = true, tankGhosts = true, lightsOut = true,
            ghostHealth = 4, ghostSpeed = 2.5f, spawnInterval = 6f, maxGhosts = 4,
            popupTitle = "Lights out!",
            popupBody = "The power just went out! Your <b>flashlight</b> points wherever you look.\n\n" +
                        "Ghosts glow faintly in the dark. Hold on until 9 PM!"
        },
    };

    [Header("Debug")]
    [SerializeField] private bool debugKeys = true; // N = skip to the next phase

    // the HUD reads these
    public string ClockText { get; private set; } = "";
    public string PhaseLabel { get; private set; } = "";
    public string Hint { get; private set; } = "";

    private const string IntroTitle = "Welcome to University Ave";
    private const string IntroBody =
        "You just moved into the cheapest house in Oxford, and now you know why: it's <b>haunted</b>. " +
        "Tonight the neighborhood kids are coming for candy, and the house's ghosts are hungry too.\n\n" +
        "<b>Goal:</b> fill every kid's candy order and survive until <b>9 PM</b>.\n\n" +
        "<b>WASD</b> move    <b>Mouse</b> look    <b>E</b> interact\n<b>Left Click</b> shoot    <b>Esc</b> pause";

    private const string TimerTitle = "Kids won't wait forever";
    private const string TimerBody =
        "From now on every kid has a <b>timer</b> (the bar under their bubble). If it runs out, the kid leaves unhappy " +
        "and you get a <b>strike</b>. 3 strikes and the night is over.\n\n" +
        "And you're not alone... <b>Ghosts</b> float through the walls and hurt you when they touch you. " +
        "Aim with the mouse and <b>Left Click</b> to blast them. Your health comes back if you avoid hits for a few seconds.\n\n";

    private Step step = Step.Intro;
    private float nightTime; // real seconds since 6 PM
    private int currentPhase = -1;

    private bool basicsMissed; // step 1: last bag was not perfect
    private bool tutorialKidServed; // step 2: one kid delivered
    private bool tutorialGhostGone; // step 2: the 1 HP ghost was shot
    private bool colorPopupShown;

    private void Awake()
    {
        // subscribe in Awake so we see the very first kid the spawner makes in its Start
        kids.OnKidSpawned += HandleKidSpawned;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ghosts.SpawningEnabled = false; // no ghosts until tutorial step 2
        GameManager.Instance.PracticeMode = true; // no strikes in the tutorial
        GameManager.Instance.OnDelivery += HandleDelivery;
        GameManager.Instance.OnStrike += HandleStrike;
        kids.SetOrderSize(tutorialMinItems, tutorialMaxItems);

        UpdateClock();
        popup.Show(IntroTitle, IntroBody, "Start", StartBasics);
    }

    private void OnDestroy()
    {
        if (kids != null) kids.OnKidSpawned -= HandleKidSpawned;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDelivery -= HandleDelivery;
            GameManager.Instance.OnStrike -= HandleStrike;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (debugKeys && GameManager.IsPlaying && Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) DebugSkip();

        UpdateHint();

        if (step != Step.Night) return;

        nightTime += Time.deltaTime; // 0 while paused

        float nightLength = phaseLength * phases.Length;
        if (nightTime >= nightLength)
        {
            nightTime = nightLength;
            UpdateClock();
            step = Step.Done;
            GameManager.Instance.EndGame(GameOverReason.Survived);
            return;
        }

        int phase = Mathf.Min(Mathf.FloorToInt(nightTime / phaseLength), phases.Length - 1);
        if (phase != currentPhase) StartPhase(phase);

        UpdateClock();
    }

    // ---------- tutorial ----------

    private void StartBasics()
    {
        step = Step.TutorialBasics;
    }

    private void HandleKidSpawned(Kid kid)
    {
        // step 1 has no time limit
        kid.TimerRunning = step == Step.TutorialTimer || step == Step.Night;
    }

    private void HandleDelivery(DeliveryResult r)
    {
        // first color match ever: explain the bonus and list all 4 effects
        if (r.ColorBonus > 0 && !colorPopupShown)
        {
            colorPopupShown = true;
            popup.Show("Color match!", ColorMatchText(), "Nice!");
        }

        if (step == Step.TutorialBasics)
        {
            if (!r.Perfect)
            {
                basicsMissed = true; // the next kid (still no timer) is another try
                return;
            }
            // switch BEFORE the kid leaves so next kid spawns with its timer on
            step = Step.TutorialTimer;
            popup.Show(TimerTitle, TimerBody, "Got it", StartTimerStep);
        }
        else if (step == Step.TutorialTimer)
        {
            tutorialKidServed = true;
            CheckTutorialDone();
        }
    }

    private void HandleStrike()
    {
        if (step != Step.TutorialTimer) return;
        tutorialKidServed = true;
        CheckTutorialDone();
    }

    private void StartTimerStep()
    {
        Ghost ghost = ghosts.SpawnTutorialGhost();
        ghost.OnRemoved += HandleTutorialGhostRemoved;
    }

    private void HandleTutorialGhostRemoved(Ghost ghost)
    {
        tutorialGhostGone = true;
        CheckTutorialDone();
    }

    private void CheckTutorialDone()
    {
        if (step == Step.TutorialTimer && tutorialKidServed && tutorialGhostGone) StartNight();
    }

    // ---------- the night ----------

    private void StartNight()
    {
        step = Step.Night;
        GameManager.Instance.PracticeMode = false;
        ghosts.SpawningEnabled = true;

        // kids already at the door get their timers now
        for (int i = 0; i < 2; i++)
        {
            Kid kid = kids.GetKid(i);
            if (kid != null) kid.TimerRunning = true;
        }

        nightTime = 0f;
        StartPhase(0);
    }

    private void StartPhase(int index)
    {
        currentPhase = index;
        PhaseSettings p = phases[index];
        Debug.Log($"{p.name} starts at {ClockText}");

        kids.SetOrderSize(p.minItems, p.maxItems);
        kids.SetActiveSlots(p.kidSlots);
        ghosts.SetDifficulty(p.ghostHealth, p.ghostSpeed, p.spawnInterval, p.maxGhosts);

        // a ghost type that is new this phase shows up right after its pop up
        if (p.thiefGhosts && !ghosts.ThievesEnabled) ghosts.ForceNext(GhostType.Thief);
        if (p.tankGhosts && !ghosts.TanksEnabled) ghosts.ForceNext(GhostType.Tank);
        ghosts.ThievesEnabled = p.thiefGhosts;
        ghosts.TanksEnabled = p.tankGhosts;

        if (p.lightsOut && !LightsOut.Active) lightsOut.TurnOff(() => ShowPhasePopup(p)); // pop up after the flicker
        else ShowPhasePopup(p);
    }

    private void ShowPhasePopup(PhaseSettings p)
    {
        if (!string.IsNullOrEmpty(p.popupTitle)) popup.Show(p.popupTitle, p.popupBody);
    }

    private void DebugSkip()
    {
        if (step == Step.TutorialBasics || step == Step.TutorialTimer) StartNight();
        else if (step == Step.Night) nightTime = (currentPhase + 1) * phaseLength; // Update starts the next phase
    }

    // ---------- text for the HUD ----------

    private void UpdateClock()
    {
        float nightLength = phaseLength * phases.Length;
        int minutes = Mathf.FloorToInt(nightTime / nightLength * nightHours * 60f);
        minutes -= minutes % 5; // tick in 5 min steps like a real clock face

        int hour = startHour + minutes / 60;
        ClockText = $"{hour}:{minutes % 60:00} PM";

        if (step == Step.Night) PhaseLabel = $"Phase {currentPhase + 1} of {phases.Length}";
        else if (step == Step.Done) PhaseLabel = "";
        else PhaseLabel = "Tutorial";
    }

    private void UpdateHint()
    {
        if (step == Step.TutorialBasics) Hint = BasicsHint();
        else if (step == Step.TutorialTimer)
        {
            if (!tutorialGhostGone && !tutorialKidServed) Hint = "Shoot the ghost  [Left Click]  and fill the order before the timer runs out";
            else if (!tutorialGhostGone) Hint = "Kid served! Now shoot the ghost  [Left Click]";
            else Hint = "Ghost down! Now fill the order before the timer runs out";
        }
        else Hint = "";
    }

    // step 1 hint changes with what the player is doing
    private string BasicsHint()
    {
        Kid kid = kids.GetKid(0);
        if (kid == null) return "";

        if (!playerBag.HasBag)
        {
            return basicsMissed
                ? "Not quite: the bag has to match the bubble exactly. Grab a new bag and try again  [E]"
                : "A kid is at the door! Grab any bag from the bag table  [E]";
        }
        if (HasWrongCandy(kid.Order)) return "Oops, wrong candy! Empty the bag at the trash can between the tables  [E]";
        if (playerBag.Contents.Count < kid.Order.TotalCount) return "Add the candy from the kid's bubble at the bowls  [E]     Wrong one? The trash can empties your bag"; 
        return "That matches! Give the bag to the kid  [E]";
    }

    // true if the bag has a candy the kid didn't ask for, or too many of one
    private bool HasWrongCandy(Order order)
    {
        Dictionary<CandyData, int> counts = new Dictionary<CandyData, int>();
        foreach (CandyData c in playerBag.Contents)
        {
            counts.TryGetValue(c, out int n);
            counts[c] = n + 1;

            order.Items.TryGetValue(c, out int wanted);
            if (n + 1 > wanted) return true;
        }
        return false;
    }

    private string ColorMatchText()
    {
        string text = "That bag matched the kid's color! A <b>perfect order</b> in a <b>matching bag</b> gives " +
               "<b>+100 points</b> and a <b>5-second power-up</b>. Different colors <b>stack</b>!\n\n";

        BagColor[] all = { BagColor.LightBlue, BagColor.Yellow, BagColor.Red, BagColor.Green };
        foreach (BagColor c in all)
        {
            string hex = ColorUtility.ToHtmlStringRGB(GameColors.ToColor(c));
            text += $"<color=#{hex}><b>{GameColors.ToName(c)}</b></color>: {ColorEffects.EffectName(c)} ({EffectDescription(c)})\n";
        }
        return text;
    }

    private static string EffectDescription(BagColor c)
    {
        switch (c)
        {
            case BagColor.LightBlue: return "move faster";
            case BagColor.Yellow: return "bullets do 2x damage";
            case BagColor.Red: return "health refills";
            case BagColor.Green: return "ghosts can't hurt you";
            default: return "";
        }
    }
}
