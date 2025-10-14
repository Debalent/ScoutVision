using System.Globalization;
using Microsoft.Extensions.Localization;
using Blazored.LocalStorage;

namespace ScoutVision.Web.Services;

public interface ILocalizationService
{
    event Action? LanguageChanged;
    Task<string> GetTextAsync(string key, params object[] args);
    Task SetLanguageAsync(string culture);
    Task<string> GetCurrentLanguageAsync();
    Task<List<SupportedLanguage>> GetSupportedLanguagesAsync();
}

public interface IThemeService
{
    event Action? ThemeChanged;
    Task<bool> GetIsDarkModeAsync();
    Task SetDarkModeAsync(bool isDark);
    Task ToggleThemeAsync();
    string GetCurrentTheme();
}

public class LocalizationService : ILocalizationService
{
    private readonly IStringLocalizer<LocalizationService> _localizer;
    private readonly ILocalStorageService _localStorage;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private string _currentLanguage = "en";

    public event Action? LanguageChanged;

    public LocalizationService(IStringLocalizer<LocalizationService> localizer, ILocalStorageService localStorage)
    {
        _localizer = localizer;
        _localStorage = localStorage;
        _translations = InitializeTranslations();
        LoadCurrentLanguage();
    }

    public async Task<string> GetTextAsync(string key, params object[] args)
    {
        var currentLang = await GetCurrentLanguageAsync();
        
        if (_translations.ContainsKey(currentLang) && _translations[currentLang].ContainsKey(key))
        {
            var text = _translations[currentLang][key];
            return args.Length > 0 ? string.Format(text, args) : text;
        }
        
        // Fallback to English
        if (_translations.ContainsKey("en") && _translations["en"].ContainsKey(key))
        {
            var text = _translations["en"][key];
            return args.Length > 0 ? string.Format(text, args) : text;
        }

        return key; // Return key if translation not found
    }

    public async Task SetLanguageAsync(string culture)
    {
        _currentLanguage = culture;
        await _localStorage.SetItemAsync("selectedLanguage", culture);
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
        LanguageChanged?.Invoke();
    }

    public async Task<string> GetCurrentLanguageAsync()
    {
        if (string.IsNullOrEmpty(_currentLanguage))
        {
            _currentLanguage = await _localStorage.GetItemAsync<string>("selectedLanguage") ?? "en";
        }
        return _currentLanguage;
    }

    public async Task<List<SupportedLanguage>> GetSupportedLanguagesAsync()
    {
        return new List<SupportedLanguage>
        {
            new() { Code = "en", Name = "English", Flag = "🇺🇸" },
            new() { Code = "es", Name = "Español", Flag = "🇪🇸" },
            new() { Code = "fr", Name = "Français", Flag = "🇫🇷" },
            new() { Code = "de", Name = "Deutsch", Flag = "🇩🇪" },
            new() { Code = "it", Name = "Italiano", Flag = "🇮🇹" },
            new() { Code = "pt", Name = "Português", Flag = "🇵🇹" },
            new() { Code = "ru", Name = "Русский", Flag = "🇷🇺" },
            new() { Code = "ja", Name = "日本語", Flag = "🇯🇵" },
            new() { Code = "ko", Name = "한국어", Flag = "🇰🇷" },
            new() { Code = "zh", Name = "中文", Flag = "🇨🇳" },
            new() { Code = "ar", Name = "العربية", Flag = "🇸🇦" },
            new() { Code = "hi", Name = "हिन्दी", Flag = "🇮🇳" }
        };
    }

    private async void LoadCurrentLanguage()
    {
        _currentLanguage = await _localStorage.GetItemAsync<string>("selectedLanguage") ?? "en";
    }

    private Dictionary<string, Dictionary<string, string>> InitializeTranslations()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                // Navigation
                ["nav.dashboard"] = "Dashboard",
                ["nav.players"] = "Players",
                ["nav.search"] = "Search",
                ["nav.analytics"] = "Analytics",
                ["nav.reports"] = "Reports",
                ["nav.settings"] = "Settings",
                ["nav.help"] = "Help",
                ["nav.user_manual"] = "User Manual",
                
                // Search
                ["search.title"] = "Search",
                ["search.placeholder"] = "Search players, footage, statistics...",
                ["search.button"] = "Search",
                ["search.filters"] = "Search Filters",
                ["search.no_results"] = "No results found",
                ["search.no_results_desc"] = "Try adjusting your search terms or filters",
                ["search.discover_title"] = "Discover Players, Footage, and Statistics",
                ["search.discover_desc"] = "Use our powerful search to find exactly what you're looking for",
                ["search.results_count"] = "Found {0} results for \"{1}\"",
                
                // Common
                ["common.loading"] = "Loading...",
                ["common.save"] = "Save",
                ["common.cancel"] = "Cancel",
                ["common.delete"] = "Delete",
                ["common.edit"] = "Edit",
                ["common.view"] = "View",
                ["common.clear"] = "Clear",
                ["common.apply"] = "Apply",
                ["common.close"] = "Close",
                ["common.yes"] = "Yes",
                ["common.no"] = "No",
                
                // Player
                ["player.name"] = "Name",
                ["player.position"] = "Position",
                ["player.team"] = "Team",
                ["player.age"] = "Age",
                ["player.nationality"] = "Nationality",
                ["player.status"] = "Status",
                ["player.details"] = "Player Details",
                ["player.analytics"] = "Analytics",
                
                // Theme
                ["theme.light"] = "Light Mode",
                ["theme.dark"] = "Dark Mode",
                ["theme.toggle"] = "Toggle Theme",
                
                // User Manual
                ["manual.title"] = "ScoutVision User Manual",
                ["manual.getting_started"] = "Getting Started",
                ["manual.navigation"] = "Navigation Guide",
                ["manual.search_guide"] = "Search Guide",
                ["manual.analytics_guide"] = "Analytics Guide",
                ["manual.troubleshooting"] = "Troubleshooting",
                ["manual.faq"] = "Frequently Asked Questions",
                
                // Hybrid Analytics
                ["HybridAnalytics"] = "Hybrid Analytics",
                ["NewSession"] = "New Session",
                ["HybridAnalyticsDescription"] = "Combine web-based analytics with 3D GMod visualization for comprehensive insights",
                ["3DVisualization"] = "3D Visualization",
                ["SyncWithGMod"] = "Sync with GMod",
                ["WebAnalytics"] = "Web Analytics",
                ["GModVisualization"] = "GMod 3D Visualization",
                ["HybridMode"] = "Hybrid Mode",
                ["ActiveSessions"] = "Active Sessions",
                ["SessionCreated"] = "Session {0} created successfully",
                ["ConnectionStatus"] = "GMod Connection Status",
                ["Connected"] = "Connected",
                ["NotConnected"] = "Not Connected",
                ["CheckConnection"] = "Check Connection"
            },
            
            ["es"] = new Dictionary<string, string>
            {
                // Navigation
                ["nav.dashboard"] = "Panel de Control",
                ["nav.players"] = "Jugadores",
                ["nav.search"] = "Buscar",
                ["nav.analytics"] = "Análisis",
                ["nav.reports"] = "Informes",
                ["nav.settings"] = "Configuración",
                ["nav.help"] = "Ayuda",
                ["nav.user_manual"] = "Manual de Usuario",
                
                // Search
                ["search.title"] = "Buscar",
                ["search.placeholder"] = "Buscar jugadores, grabaciones, estadísticas...",
                ["search.button"] = "Buscar",
                ["search.filters"] = "Filtros de Búsqueda",
                ["search.no_results"] = "No se encontraron resultados",
                ["search.no_results_desc"] = "Intenta ajustar tus términos de búsqueda o filtros",
                ["search.discover_title"] = "Descubre Jugadores, Grabaciones y Estadísticas",
                ["search.discover_desc"] = "Usa nuestra poderosa búsqueda para encontrar exactamente lo que buscas",
                ["search.results_count"] = "Se encontraron {0} resultados para \"{1}\"",
                
                // Common
                ["common.loading"] = "Cargando...",
                ["common.save"] = "Guardar",
                ["common.cancel"] = "Cancelar",
                ["common.delete"] = "Eliminar",
                ["common.edit"] = "Editar",
                ["common.view"] = "Ver",
                ["common.clear"] = "Limpiar",
                ["common.apply"] = "Aplicar",
                ["common.close"] = "Cerrar",
                ["common.yes"] = "Sí",
                ["common.no"] = "No",
                
                // Player
                ["player.name"] = "Nombre",
                ["player.position"] = "Posición",
                ["player.team"] = "Equipo",
                ["player.age"] = "Edad",
                ["player.nationality"] = "Nacionalidad",
                ["player.status"] = "Estado",
                ["player.details"] = "Detalles del Jugador",
                ["player.analytics"] = "Análisis",
                
                // Theme
                ["theme.light"] = "Modo Claro",
                ["theme.dark"] = "Modo Oscuro",
                ["theme.toggle"] = "Cambiar Tema",
                
                // User Manual
                ["manual.title"] = "Manual de Usuario de ScoutVision",
                ["manual.getting_started"] = "Primeros Pasos",
                ["manual.navigation"] = "Guía de Navegación",
                ["manual.search_guide"] = "Guía de Búsqueda",
                ["manual.analytics_guide"] = "Guía de Análisis",
                ["manual.troubleshooting"] = "Solución de Problemas",
                ["manual.faq"] = "Preguntas Frecuentes"
            },
            
            ["fr"] = new Dictionary<string, string>
            {
                // Navigation
                ["nav.dashboard"] = "Tableau de Bord",
                ["nav.players"] = "Joueurs",
                ["nav.search"] = "Rechercher",
                ["nav.analytics"] = "Analyses",
                ["nav.reports"] = "Rapports",
                ["nav.settings"] = "Paramètres",
                ["nav.help"] = "Aide",
                ["nav.user_manual"] = "Manuel Utilisateur",
                
                // Search
                ["search.title"] = "Rechercher",
                ["search.placeholder"] = "Rechercher des joueurs, des vidéos, des statistiques...",
                ["search.button"] = "Rechercher",
                ["search.filters"] = "Filtres de Recherche",
                ["search.no_results"] = "Aucun résultat trouvé",
                ["search.no_results_desc"] = "Essayez d'ajuster vos termes de recherche ou filtres",
                ["search.discover_title"] = "Découvrez Joueurs, Vidéos et Statistiques",
                ["search.discover_desc"] = "Utilisez notre recherche puissante pour trouver exactement ce que vous cherchez",
                ["search.results_count"] = "{0} résultats trouvés pour \"{1}\"",
                
                // Common
                ["common.loading"] = "Chargement...",
                ["common.save"] = "Sauvegarder",
                ["common.cancel"] = "Annuler",
                ["common.delete"] = "Supprimer",
                ["common.edit"] = "Modifier",
                ["common.view"] = "Voir",
                ["common.clear"] = "Effacer",
                ["common.apply"] = "Appliquer",
                ["common.close"] = "Fermer",
                ["common.yes"] = "Oui",
                ["common.no"] = "Non",
                
                // Player
                ["player.name"] = "Nom",
                ["player.position"] = "Position",
                ["player.team"] = "Équipe",
                ["player.age"] = "Âge",
                ["player.nationality"] = "Nationalité",
                ["player.status"] = "Statut",
                ["player.details"] = "Détails du Joueur",
                ["player.analytics"] = "Analyses",
                
                // Theme
                ["theme.light"] = "Mode Clair",
                ["theme.dark"] = "Mode Sombre",
                ["theme.toggle"] = "Changer de Thème",
                
                // User Manual
                ["manual.title"] = "Manuel Utilisateur ScoutVision",
                ["manual.getting_started"] = "Premiers Pas",
                ["manual.navigation"] = "Guide de Navigation",
                ["manual.search_guide"] = "Guide de Recherche",
                ["manual.analytics_guide"] = "Guide d'Analyse",
                ["manual.troubleshooting"] = "Dépannage",
                ["manual.faq"] = "Questions Fréquemment Posées"
            },
            
            ["de"] = new Dictionary<string, string>
            {
                // Navigation
                ["nav.dashboard"] = "Dashboard",
                ["nav.players"] = "Spieler",
                ["nav.search"] = "Suchen",
                ["nav.analytics"] = "Analytik",
                ["nav.reports"] = "Berichte",
                ["nav.settings"] = "Einstellungen",
                ["nav.help"] = "Hilfe",
                ["nav.user_manual"] = "Benutzerhandbuch",
                
                // Search
                ["search.title"] = "Suchen",
                ["search.placeholder"] = "Spieler, Videos, Statistiken suchen...",
                ["search.button"] = "Suchen",
                ["search.filters"] = "Suchfilter",
                ["search.no_results"] = "Keine Ergebnisse gefunden",
                ["search.no_results_desc"] = "Versuchen Sie, Ihre Suchbegriffe oder Filter anzupassen",
                ["search.discover_title"] = "Entdecken Sie Spieler, Videos und Statistiken",
                ["search.discover_desc"] = "Nutzen Sie unsere leistungsstarke Suche, um genau das zu finden, was Sie suchen",
                ["search.results_count"] = "{0} Ergebnisse für \"{1}\" gefunden",
                
                // Common
                ["common.loading"] = "Wird geladen...",
                ["common.save"] = "Speichern",
                ["common.cancel"] = "Abbrechen",
                ["common.delete"] = "Löschen",
                ["common.edit"] = "Bearbeiten",
                ["common.view"] = "Anzeigen",
                ["common.clear"] = "Löschen",
                ["common.apply"] = "Anwenden",
                ["common.close"] = "Schließen",
                ["common.yes"] = "Ja",
                ["common.no"] = "Nein",
                
                // Player
                ["player.name"] = "Name",
                ["player.position"] = "Position",
                ["player.team"] = "Team",
                ["player.age"] = "Alter",
                ["player.nationality"] = "Nationalität",
                ["player.status"] = "Status",
                ["player.details"] = "Spielerdetails",
                ["player.analytics"] = "Analytik",
                
                // Theme
                ["theme.light"] = "Heller Modus",
                ["theme.dark"] = "Dunkler Modus",
                ["theme.toggle"] = "Thema Wechseln",
                
                // User Manual
                ["manual.title"] = "ScoutVision Benutzerhandbuch",
                ["manual.getting_started"] = "Erste Schritte",
                ["manual.navigation"] = "Navigationsführer",
                ["manual.search_guide"] = "Suchführer",
                ["manual.analytics_guide"] = "Analytik-Leitfaden",
                ["manual.troubleshooting"] = "Fehlerbehebung",
                ["manual.faq"] = "Häufig Gestellte Fragen"
            }
        };
    }
}

public class ThemeService : IThemeService
{
    private readonly ILocalStorageService _localStorage;
    private bool _isDarkMode = false;

    public event Action? ThemeChanged;

    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        LoadCurrentTheme();
    }

    public async Task<bool> GetIsDarkModeAsync()
    {
        _isDarkMode = await _localStorage.GetItemAsync<bool>("isDarkMode");
        return _isDarkMode;
    }

    public async Task SetDarkModeAsync(bool isDark)
    {
        _isDarkMode = isDark;
        await _localStorage.SetItemAsync("isDarkMode", isDark);
        ThemeChanged?.Invoke();
    }

    public async Task ToggleThemeAsync()
    {
        var currentMode = await GetIsDarkModeAsync();
        await SetDarkModeAsync(!currentMode);
    }

    public string GetCurrentTheme()
    {
        return _isDarkMode ? "dark" : "light";
    }

    private async void LoadCurrentTheme()
    {
        _isDarkMode = await _localStorage.GetItemAsync<bool>("isDarkMode");
    }
}

public class SupportedLanguage
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
}