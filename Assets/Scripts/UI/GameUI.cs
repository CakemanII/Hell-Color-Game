using UnityEngine;
using TMPro;

public enum UIType
{
    Inventory,
    Quests,
    Cheats,
    MainHUD
}

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { private set; get; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryUIObject;
    [SerializeField] private GameObject questsUIObject;
    [SerializeField] private GameObject cheatsUIObject;
    [SerializeField] private GameObject MainHUDObject;

    [SerializeField] private GameObject secondaryUI;

    [Header("Tab Button Texts")]
    [SerializeField] private TextMeshProUGUI inventoryButtonText;
    [SerializeField] private TextMeshProUGUI questsButtonText;

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    public UIType uiDisplaying { private set; get; }  = UIType.MainHUD;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInput.OnPausePressed += OnPauseInput;
        SetUIBeingDisplayed(UIType.MainHUD);
    }

    private void OnDestroy()
    {
        playerInput.OnPausePressed -= OnPauseInput;
    }

    private void OnPauseInput()
    {
        if (uiDisplaying == UIType.MainHUD)
            SetUIBeingDisplayed(UIType.Inventory);
        else
            SetUIBeingDisplayed(UIType.MainHUD);
    }

    public void SetSecondaryUIEnabled(bool enabled)
    {
        secondaryUI.SetActive(enabled);
    }

    public void OnInventoryButtonClicked()
    {
        SetUIBeingDisplayed(UIType.Inventory);
    }

    public void OnQuestsButtonClicked()
    {
        SetUIBeingDisplayed(UIType.Quests);
    }

    public void SetUIBeingDisplayed(UIType uiType)
    {
        uiDisplaying = uiType;
        bool isMainHUD = uiType == UIType.MainHUD;

        playerInput.SetMouseLocked(isMainHUD);
        secondaryUI.SetActive(!isMainHUD);

        SetInventoryUIEnabled(uiType == UIType.Inventory);
        SetQuestsUIEnabled(uiType == UIType.Quests);
        SetCheatsUIEnabled(uiType == UIType.Cheats);
        SetMainHUDUIEnabled(isMainHUD);
        UpdateTabUnderlines();
    }

    private void UpdateTabUnderlines()
    {
        SetTextUnderline(inventoryButtonText, uiDisplaying == UIType.Inventory);
        SetTextUnderline(questsButtonText, uiDisplaying == UIType.Quests);
    }

    private static void SetTextUnderline(TextMeshProUGUI text, bool underline)
    {
        if (underline)
            text.fontStyle |= FontStyles.Underline;
        else
            text.fontStyle &= ~FontStyles.Underline;
    }

    private void SetInventoryUIEnabled(bool enabled) => inventoryUIObject.SetActive(enabled);
    private void SetQuestsUIEnabled(bool enabled) => questsUIObject.SetActive(enabled);
    private void SetCheatsUIEnabled(bool enabled) => cheatsUIObject.SetActive(enabled);
    private void SetMainHUDUIEnabled(bool enabled) => MainHUDObject.SetActive(enabled);
}
