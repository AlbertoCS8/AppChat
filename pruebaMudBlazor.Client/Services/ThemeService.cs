public class ThemeService
{
    public bool _isDarkMode= true;

    public ThemeService()
    {
    }

    public ThemeService(bool isDarkMode)
    {
        _isDarkMode = isDarkMode;
    }

    public bool GetTheme() //devuelve true si es dark mode
    {
        if (this._isDarkMode)
        {
            return true;
        }else
        {
            return false;
        }
    }

    public void SetTheme(bool isDarkMode)
    {
        this._isDarkMode = isDarkMode;
    }
}