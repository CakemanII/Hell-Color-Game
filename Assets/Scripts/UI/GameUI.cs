using UnityEngine;
using TMPro;

public enum UIType
{
    Inventory,
    Quests,
    Cheats,
    MainHUD,
    Settings
}

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { private set; get; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryUIObject;
    [SerializeField] private GameObject questsUIObject;
    [SerializeField] private GameObject cheatsUIObject;
    [SerializeField] private GameObject MainHUDObject;
    [SerializeField] private GameObject settingsUIObject;

    [SerializeField] private GameObject secondaryUI;

    [Header("Tab Button Texts")]
    [SerializeField] private TextMeshProUGUI inventoryButtonText;
    [SerializeField] private TextMeshProUGUI questsButtonText;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenObject;

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
        bool isSettings = uiType == UIType.Settings;

        playerInput.SetMouseLocked(isMainHUD);
        secondaryUI.SetActive(!isMainHUD && !isSettings);

        SetInventoryUIEnabled(uiType == UIType.Inventory);
        SetQuestsUIEnabled(uiType == UIType.Quests);
        SetCheatsUIEnabled(uiType == UIType.Cheats);
        SetSettingsUIEnabled(isSettings);
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

    public void SetLoadingScreen(bool active)
    {
        if (loadingScreenObject != null) loadingScreenObject.SetActive(active);
    }

    private void SetInventoryUIEnabled(bool enabled) => inventoryUIObject.SetActive(enabled);
    private void SetQuestsUIEnabled(bool enabled) => questsUIObject.SetActive(enabled);
    private void SetCheatsUIEnabled(bool enabled) => cheatsUIObject.SetActive(enabled);
    private void SetSettingsUIEnabled(bool enabled) => settingsUIObject.SetActive(enabled);
    private void SetMainHUDUIEnabled(bool enabled) => MainHUDObject.SetActive(enabled);
}
