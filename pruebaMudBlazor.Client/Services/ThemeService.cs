public class ThemeService
{
    public bool _isDarkMode= true;
    public event Action? OnThemeChanged; //valor que cambio para poder avisar a las otras plantillas que el tema cambio
    

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
        this.OnThemeChanged?.Invoke(); //avisa a las otras plantillas que el tema cambio
    }
}