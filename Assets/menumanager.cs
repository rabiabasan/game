using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject settingsPanel;

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // ✅ ŞİMDİLİK: Sahne geçmeden sadece test mesajı
    public void PlayPlaceholder()
    {
        Debug.Log("OYUN BASLAT: (şimdilik placeholder) - Sonra sahne yükleme bağlanacak");
        // İstersen burada SettingsPanel açıksa kapatıp ana paneli gösterebilirsin
    }

    // ✅ ŞİMDİLİK: Editörde kapatmaz, build alınca kapatır
    public void QuitGame()
    {
        Debug.Log("CIKIS (Editor'da kapanmaz, build'de kapanır)");
        Application.Quit();
    }
}