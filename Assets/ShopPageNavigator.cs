using UnityEngine;
using TMPro;

public class ShopPageNavigator : MonoBehaviour
{
    [Header("Main Containers")]
    [SerializeField] private GameObject shopRoot;
    [SerializeField] private GameObject landingPage;
    [SerializeField] private GameObject homePage;
    [SerializeField] private GameObject tabBar;

    [Header("Shared Title")]
    [SerializeField] private TMP_Text shopTitleText;
    [SerializeField] private string landingTitle = "SHOP";
    [SerializeField] private string outfitsTitle = "OUTFITS";
    [SerializeField] private string songsTitle = "SONGS";
    [SerializeField] private string stagesTitle = "STAGES";
    [SerializeField] private string charactersTitle = "CHARACTERS";

    [Header("Category Pages")]
    [SerializeField] private GameObject outfitsPage;
    [SerializeField] private GameObject songsPage;
    [SerializeField] private GameObject stagesPage;
    [SerializeField] private GameObject charactersPage;

    private GameObject[] allCategoryPages;

    private void Awake()
    {
        if (shopRoot == null && landingPage != null && landingPage.transform.parent != null)
        {
            shopRoot = landingPage.transform.parent.gameObject;
        }

        allCategoryPages = new[]
        {
            outfitsPage,
            songsPage,
            stagesPage,
            charactersPage
        };

        ShowLandingPage();
    }

    public void ShowLandingPage()
    {
        if (landingPage != null)
        {
            landingPage.SetActive(true);
        }

        HideAllCategoryPages();
        SetTabBarVisible(false);
        SetShopTitle(landingTitle);
    }

    public void OpenOutfitsPage()
    {
        OpenCategoryPage(outfitsPage);
        SetShopTitle(outfitsTitle);
    }

    public void OpenSongsPage()
    {
        OpenCategoryPage(songsPage);
        SetShopTitle(songsTitle);
    }

    public void OpenStagesPage()
    {
        OpenCategoryPage(stagesPage);
        SetShopTitle(stagesTitle);
    }

    public void OpenCharactersPage()
    {
        OpenCategoryPage(charactersPage);
        SetShopTitle(charactersTitle);
    }

    // Wire your BackButton to this method.
    // If user is inside a category page, go back to shop landing.
    // If user is already on landing page, exit shop and go to home page.
    public void OnBackPressed()
    {
        if (IsAnyCategoryPageOpen())
        {
            ShowLandingPage();
            return;
        }

        GoToHomePage();
    }

    private void OpenCategoryPage(GameObject targetPage)
    {
        if (landingPage != null)
        {
            landingPage.SetActive(false);
        }

        HideAllCategoryPages();

        if (targetPage != null)
        {
            targetPage.SetActive(true);
        }

        SetTabBarVisible(true);
    }

    private void HideAllCategoryPages()
    {
        if (allCategoryPages == null)
        {
            return;
        }

        for (int i = 0; i < allCategoryPages.Length; i++)
        {
            if (allCategoryPages[i] != null)
            {
                allCategoryPages[i].SetActive(false);
            }
        }
    }

    private bool IsAnyCategoryPageOpen()
    {
        if (allCategoryPages == null)
        {
            return false;
        }

        for (int i = 0; i < allCategoryPages.Length; i++)
        {
            if (allCategoryPages[i] != null && allCategoryPages[i].activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void GoToHomePage()
    {
        // Home page may be built later by another teammate.
        // In that case, keep the player on landing instead of hiding the shop UI.
        if (homePage == null)
        {
            ShowLandingPage();
            return;
        }

        if (shopRoot != null)
        {
            shopRoot.SetActive(false);
        }

        SetTabBarVisible(false);
        homePage.SetActive(true);
    }

    private void SetTabBarVisible(bool isVisible)
    {
        if (tabBar != null)
        {
            tabBar.SetActive(isVisible);
        }
    }

    private void SetShopTitle(string title)
    {
        if (shopTitleText != null)
        {
            shopTitleText.text = title;
        }
    }
}